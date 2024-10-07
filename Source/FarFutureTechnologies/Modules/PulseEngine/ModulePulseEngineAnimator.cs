using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Waterfall;

namespace FarFutureTechnologies
{
  public class ModulePulseEngineAnimator : PartModule
  {

    [KSPField(isPersistant = false)]
    public string engineID;


    // The cycle time between pulses 
    [KSPField(isPersistant = false)]
    public FloatCurve PulseInterval = new FloatCurve();

    // The speed of the pulse
    [KSPField(isPersistant = false)]
    public FloatCurve PulseSpeed = new FloatCurve();

    // The speed of the pulse
    [KSPField(isPersistant = false)]
    public float PulseDuration = 1.0f;

    // The KSP EFFECTS node to use
    [KSPField(isPersistant = false)]
    public string pulseEffectName = "running";

    // Whether to use pulsed exhaust damage
    [KSPField(isPersistant = false)]
    public bool PulsedExhaustDamage = false;

    // Whether to apply thrust in pulses or not
    [KSPField(isPersistant = false)]
    public bool PulsedThrust = false;


    // The time at which to apply thrust
    [KSPField(isPersistant = false)]
    public float PulseThrustTime = 1.0f;

    // The time at which to apply thrust
    [KSPField(isPersistant = false)]
    public int PulseThrustFrameCount = 5;

    // Animation that plays
    [KSPField(isPersistant = false)]
    public string PulseAnimation = "";

    // Transform containing the pulse vFX
    [KSPField(isPersistant = false)]
    public string PulseVFXTransformName;

    // Whether to move the pulse VFX 
    [KSPField(isPersistant = false)]
    public bool MovePulseVFX = false;

    // Whether to move the pulse VFX 
    [KSPField(isPersistant = false)]
    public float PulseVFXDistance = 10f;

    // Light intensity along the pulse
    [KSPField(isPersistant = false)]
    public FloatCurve lightIntensityCurve = new FloatCurve();

    // FX intensity along the pulse
    [KSPField(isPersistant = false)]
    public FloatCurve soundIntensityCurve = new FloatCurve();

    // 
    [KSPField(isPersistant = false)]
    public string emissiveColorAnimatorID;

    private float savedThrust = 0f;

    private float pulseProgress = 0f;
    private float scaledPulseInterval = 0f;
    private float scaledPulseSpeed = 1f;
    private float scaledPulseDuration = 1f;
    private AnimationState[] pulseStates;
    private List<PulseEngineEffect> pulseEffects;

    private ModuleEngineExhaustDamage engineDamage;
    private ModuleColorAnimator emissiveAnimator;
    private ModuleEnginesFX engine;
    private MultiModeEngine multiEngine;

    private Transform pulseVFXTransform;
    private Light light;
    private bool pulseFired = false;
    private int ticks = 0;

    private int pulseSequenceIndex = 0;
    private int pulseSequenceMax = 0;

    public void Start()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        if (engineID != "")
        {
          engine = this.GetComponents<ModuleEnginesFX>().ToList().Find(x => x.engineID == engineID);
        }
        else
        {
          engine = this.GetComponents<ModuleEnginesFX>().ToList().First();
        }
        savedThrust = engine.maxThrust;
        multiEngine = this.GetComponent<MultiModeEngine>();

        if (PulsedExhaustDamage)
        {
          engineDamage = this.GetComponent<ModuleEngineExhaustDamage>();
        }

        emissiveAnimator = this.GetComponents<ModuleColorAnimator>().ToList().Find(x => x.moduleID == emissiveColorAnimatorID);
        // Set up animations
        if (PulseAnimation != "")
        {
          pulseStates = Utils.SetUpAnimation(PulseAnimation, part);

          foreach (AnimationState pulseState in pulseStates)
          {
            pulseState.layer = 1;
          }
        }
        if (PulseVFXTransformName != "")
        {
          pulseVFXTransform = part.FindModelTransform(PulseVFXTransformName);

          if (pulseVFXTransform)
          {
            SetupLight(pulseVFXTransform);
          }
          else
          {
            Utils.LogError($"[ModulePulseEngineAnimator] No transform called  {PulseVFXTransformName} was found");
          }
        }

        /// Reload nodes as needed
        if (HighLogic.LoadedSceneIsFlight)
        {
          ConfigNode node = GameDatabase.Instance.GetConfigs("PART").
              Single(c => part.partInfo.name == c.name).config.
              GetNodes("MODULE").Single(n => n.GetValue("name") == moduleName && n.GetValue("engineID") == engineID);
          LoadVFX(node);

        }
      }
    }

    void SetupLight(Transform parent)
    {
      light = parent.gameObject.GetComponent<Light>();
      light.intensity = 0f;
      if (light == null)
      {
        Utils.LogError($"[ModulePulseEngineAnimator] No light was found on  {parent.name}");
      }
    }

    public void LoadVFX(ConfigNode node)
    {
      pulseEffects = new List<PulseEngineEffect>();
      ConfigNode[] animNodes = node.GetNodes("COLORANIMATORFX");
      ConfigNode[] plumeNodes = node.GetNodes("PLUMEFX");
      ConfigNode[] pulseNodes = node.GetNodes("PULSEFX");

      if (animNodes.Length > 0)
      {
        for (int i = 0; i < animNodes.Length; i++)
        {
          PulseEngineColorAnimatorEffect color = new PulseEngineColorAnimatorEffect(animNodes[i], part);

          pulseEffects.Add(color);
        }
      }
      if (plumeNodes.Length > 0)
      {
        for (int i = 0; i < plumeNodes.Length; i++)
        {
          PulseEnginePlumeEffect plume = new PulseEnginePlumeEffect(plumeNodes[i], part);
          pulseEffects.Add(plume);
        }
      }
      if (pulseNodes.Length > 0)
      {
        for (int i = 0; i < pulseNodes.Length; i++)
        {
          PulseEnginePulseEffect pulse = new PulseEnginePulseEffect(pulseNodes[i], part);
          pulseEffects.Add(pulse);
        }
      }
      // Figure out the highest sequence ID in the loaded FX
      for (int i = 0; i < pulseEffects.Count; i++)
      {
        if (pulseEffects[i].isSequenced)
        {
          pulseSequenceMax = Mathf.Max(pulseSequenceMax, pulseEffects[i].sequenceID);
        }
      }

    }

    void LateUpdate()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        if (!engine)
          return;

        scaledPulseInterval = PulseInterval.Evaluate(engine.requestedThrottle);
        scaledPulseSpeed = PulseSpeed.Evaluate(engine.requestedThrottle);
        scaledPulseDuration = scaledPulseSpeed * PulseDuration;
        float curveValue = pulseProgress / scaledPulseSpeed;

        if (engine.EngineIgnited)
        {
          if (engine.requestedThrottle > 0f && !engine.flameout)
          {
            // At start of pulse
            if (pulseProgress == 0f)
            {
              StartPulse();
              pulseProgress += pulseProgress + TimeWarp.deltaTime;
            }
            // During pulse
            else if (pulseProgress < scaledPulseDuration)
            {
              SetAnimationClips(1.0f / scaledPulseSpeed);
              pulseProgress += TimeWarp.deltaTime;
            }
            // after pulse but during wait period
            else if (pulseProgress >= scaledPulseDuration && pulseProgress < (scaledPulseDuration + scaledPulseInterval))
            {
              SetAnimationClips(0f, 0f);
              pulseProgress += TimeWarp.deltaTime;
            }
            // After pulse wait period
            else
            {
              CleanupPulse();
              pulseProgress = 0f;
            }
            SetEffects(curveValue);
          }
          else
          {
            SetAnimationClips(0f, 0f);
            SetEffects(0f);
            pulseProgress = 0f;
          }
        }
        else
        {

          if (IsRunningEngine())
          {
            SetAnimationClips(0f, 0f);
            SetEffects(0f);
            pulseProgress = 0f;
          }
        }
      }
    }
    protected bool IsRunningEngine()
    {
      /// This means... if we are not a multiengine, OR we are a multiengine and we are the running multiengine 
      return multiEngine == null || (multiEngine && multiEngine.runningPrimary && multiEngine.primaryEngineID == engineID) || (multiEngine && !multiEngine.runningPrimary && multiEngine.secondaryEngineID == engineID);
    }
    /// <summary>
    /// Start the pulse
    /// </summary>
    protected void StartPulse()
    {
      SetAnimationClips(0f, 1.0f / scaledPulseSpeed);
      PositionVFXTransform();

      if (PulsedExhaustDamage && engineDamage != null)
      {
        engineDamage.DoPulseBuildingDamageSpherical(pulseVFXTransform.position);
        engineDamage.DoPulsePartsDamageSpherical(pulseVFXTransform.position);
      }
      for (int i = 0; i < pulseEffects.Count; i++)
      {
        if (!pulseEffects[i].isSequenced || pulseEffects[i].isSequenced && pulseSequenceIndex == pulseEffects[i].sequenceID)
        {
          StartCoroutine(pulseEffects[i].Pulse());
        }
      }
    }
    /// <summary>
    /// Mark the pulse as done
    /// </summary>
    protected void CleanupPulse()
    {
      SetAnimationClips(0f, 0f);
      pulseSequenceIndex = pulseSequenceIndex < pulseSequenceMax ? pulseSequenceIndex + 1 : 0;

    }

    protected void PositionVFXTransform()
    {
      if (MovePulseVFX)
      {
        /// Raycast out from the thrustTransform and determine if anything was hit. If so, move the vfx transform to the hit point
        float moveDistance = PulseVFXDistance;
        if (Physics.Raycast(engine.thrustTransforms[0].position, engine.thrustTransforms[0].forward, out RaycastHit hit, PulseVFXDistance))
        {
          moveDistance = hit.distance;
        }
        pulseVFXTransform.localPosition = engine.thrustTransforms[0].localPosition - Vector3.up * moveDistance;
      }
    }

    /// <summary>
    /// Set the value of all effects on the part according to the progress through the pulse
    /// </summary>
    /// <param name="value"></param>
    protected void SetEffects(float value)
    {
      part.Effect(pulseEffectName, soundIntensityCurve.Evaluate(value));

      if (pulseEffects != null)
      {
        for (int i = 0; i < pulseEffects.Count; i++)
        {
          if (!pulseEffects[i].isSequenced || (pulseEffects[i].isSequenced && pulseSequenceIndex == pulseEffects[i].sequenceID))
          {
            pulseEffects[i].Update(value);
          }
        }
      }
      if (light != null)
      {
        light.intensity = lightIntensityCurve.Evaluate(value);
      }
      if (emissiveAnimator != null)
      {
        emissiveAnimator.SetScalar(lightIntensityCurve.Evaluate(value));
      }
    }
    /// <summary>
    /// Set the normalized time and speed of all animationclips on the part
    /// </summary>
    /// <param name="time"></param>
    /// <param name="speed"></param>
    protected void SetAnimationClips(float normTime, float speed)
    {
      if (PulseAnimation != "" && pulseStates != null)
      {
        foreach (AnimationState pulseState in pulseStates)
        {
          pulseState.normalizedTime = normTime;
          pulseState.speed = speed;
        }
      }
    }
    /// <summary>
    /// Set only the speed of all animation clips on the part
    /// </summary>
    /// <param name="speed"></param>
    protected void SetAnimationClips(float speed)
    {
      if (PulseAnimation != "" && pulseStates != null)
      {
        foreach (AnimationState pulseState in pulseStates)
        {
          pulseState.speed = speed;
        }
      }
    }

  }

}
