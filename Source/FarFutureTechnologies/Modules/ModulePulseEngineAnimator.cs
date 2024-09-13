using System;
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

    // Whether to apply thrust in pulses or not
    [KSPField(isPersistant = false)]
    public bool LaserAnimations = false;

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
    private List<PulseEngineLaserEffect> laserFX;
    private List<PulseEnginePlumeEffect> plumeFX;

    private ModuleEngineExhaustDamage engineDamage;
    private ModuleColorAnimator emissiveAnimator;
    private ModuleEnginesFX engine;
    private MultiModeEngine multiEngine;

    private Transform pulseVFXTransform;
    private Light light;
    private bool pulseFired = false;
    private int ticks = 0;

    private int laserAnimatorIndex = 0;
    private bool laserPulseDone = false;

    public void Start()
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
      if (LaserAnimations && (laserFX == null || laserFX.Count == 0))
      {
        laserFX = new List<PulseEngineLaserEffect>();
        ConfigNode[] varNodes = node.GetNodes("LASERFX");

        for (int i = 0; i < varNodes.Length; i++)
        {
          laserFX.Add(new PulseEngineLaserEffect(varNodes[i]));
        }
        Utils.Log($"Loaded {laserFX.Count} lasers");
      }
      if (node.GetNodes("PLUMEFX").Length > 0)
      {
        plumeFX = new List<PulseEnginePlumeEffect>();
        ConfigNode[] varNodes = node.GetNodes("PLUMEFX");

        for (int i = 0; i < varNodes.Length; i++)
        {
          plumeFX.Add(new PulseEnginePlumeEffect(varNodes[i]));
        }
        Utils.Log($"Loaded {plumeFX.Count} controlled plumes");
      }

    }

    void FixedUpdate()
    {
      if (PulsedThrust)
      {
        if (HighLogic.LoadedSceneIsFlight)
        {
          float totalImpulse = (scaledPulseDuration + scaledPulseInterval) * savedThrust;
          float momentumPerFrame = totalImpulse / (float)PulseThrustFrameCount;
          if (engine.EngineIgnited)
          {
            if (engine.requestedThrottle > 0f && !engine.flameout)
            {
              if (pulseProgress < PulseThrustTime * scaledPulseSpeed)
              {
                pulseFired = false;
                engine.maxThrust = 0f;
                engine.maxFuelFlow = 0f;
              }
              else if (pulseProgress >= PulseThrustTime * scaledPulseSpeed && !pulseFired)
              {

                engine.maxThrust = momentumPerFrame / TimeWarp.fixedDeltaTime;
                engine.maxFuelFlow = ((momentumPerFrame / TimeWarp.fixedDeltaTime) / (engine.realIsp * (float)PhysicsGlobals.GravitationalAcceleration));
                if (FarFutureTechnologySettings.DebugModules)
                  Utils.Log($"[ModulePulseEngineAnimator]: Pulse fired with impulse of {momentumPerFrame}, thrust {momentumPerFrame / TimeWarp.fixedDeltaTime} {engine.realIsp}, {PhysicsGlobals.GravitationalAcceleration}");

                ticks++;
                if (ticks >= PulseThrustFrameCount)
                {
                  ticks = 0;
                  pulseFired = true;
                }
              }
              else
              {
                engine.maxThrust = 0f;
                engine.maxFuelFlow = 0f;
              }
            }
            else
            {
              engine.maxThrust = savedThrust;
              engine.maxFuelFlow = ((savedThrust) / (engine.realIsp * (float)PhysicsGlobals.GravitationalAcceleration));
            }
          }
          else
          {
            engine.maxThrust = savedThrust;
            engine.maxFuelFlow = ((savedThrust) / (engine.realIsp * (float)PhysicsGlobals.GravitationalAcceleration));
          }
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
              if (PulseAnimation != "")
              {
                foreach (AnimationState pulseState in pulseStates)
                {
                  pulseState.normalizedTime = 0f;
                  pulseState.speed = 1.0f / scaledPulseSpeed;
                }
              }
              if (MovePulseVFX)
              {
                /// Raycast out from the thrustTransform and determine if anything was hit. If so, move the vfx transform to the hit point
                float moveDistance = PulseVFXDistance;
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(engine.thrustTransforms[0].position, engine.thrustTransforms[0].forward, out hit, PulseVFXDistance))
                {
                  moveDistance = hit.distance;
                }
                pulseVFXTransform.localPosition = engine.thrustTransforms[0].localPosition - Vector3.up * moveDistance;
              }

              if (PulsedExhaustDamage && engineDamage != null)
              {
                engineDamage.DoPulseBuildingDamageSpherical(pulseVFXTransform.position);
                engineDamage.DoPulsePartsDamageSpherical(pulseVFXTransform.position);
              }
              pulseProgress = pulseProgress + TimeWarp.deltaTime;
              if (laserPulseDone)
              {
                laserPulseDone = false;

              }
            }
            // During pulse
            else if (pulseProgress < scaledPulseDuration)
            {
              if (PulseAnimation != "")
                foreach (AnimationState pulseState in pulseStates)
                {
                  //pulseState.normalizedTime = Mathf.MoveTowards(pulseState.normalizedTime, 1.0f, TimeWarp.fixedDeltaTime * scaledPulseSpeed);
                  pulseState.speed = 1.0f / scaledPulseSpeed;
                }

              pulseProgress = pulseProgress + TimeWarp.deltaTime;
            }
            // after pulse but during wait period
            else if (pulseProgress >= scaledPulseDuration && pulseProgress < (scaledPulseDuration + scaledPulseInterval))
            {
              if (PulseAnimation != "")
                foreach (AnimationState pulseState in pulseStates)
                {
                  pulseState.normalizedTime = 0f;
                  pulseState.speed = 0f;
                }
              //part.Effect(engine.runningEffectName, 0f);
              pulseProgress = pulseProgress + TimeWarp.deltaTime;


            }
            // After pulse wait period
            else
            {
              if (PulseAnimation != "")
                foreach (AnimationState pulseState in pulseStates)
                {
                  pulseState.normalizedTime = 0f;
                  pulseState.speed = 0f;
                }
              // part.Effect(engine.runningEffectName, 0f);
              pulseProgress = 0f;
              if (LaserAnimations && !laserPulseDone)
              {
                laserPulseDone = true;
                laserAnimatorIndex++;
                if (laserAnimatorIndex > laserFX.Count - 1) laserAnimatorIndex = 0;
              }
            }
            part.Effect(pulseEffectName, soundIntensityCurve.Evaluate(curveValue));

            if (plumeFX != null)
            {
              for (int i = 0; i < plumeFX.Count; i++)
              {
                plumeFX[i].Set(this.part, curveValue);
              }
            }

            if (light != null)
              light.intensity = lightIntensityCurve.Evaluate(curveValue);
            emissiveAnimator.SetScalar(lightIntensityCurve.Evaluate(curveValue));

            if (LaserAnimations)
            {
              laserFX[laserAnimatorIndex].Set(this.part, curveValue);
            }
          }
          else
          {
            if (PulseAnimation != "")
              foreach (AnimationState pulseState in pulseStates)
              {
                pulseState.normalizedTime = 0f;
                pulseState.speed = 0f;
              }
            pulseProgress = 0f;
            part.Effect(pulseEffectName, 0f);
            if (plumeFX != null)
            {
              for (int i = 0; i < plumeFX.Count; i++)
              {
                plumeFX[i].Set(this.part, curveValue);
              }
            }
            if (light != null)
              light.intensity = lightIntensityCurve.Evaluate(curveValue);
            emissiveAnimator.SetScalar(lightIntensityCurve.Evaluate(curveValue));
            if (LaserAnimations)
            {
              laserFX[laserAnimatorIndex].Set(this.part, curveValue);
            }
          }
        }
        else
        {
          if (multiEngine == null || (multiEngine && multiEngine.runningPrimary && multiEngine.primaryEngineID == engineID) || (multiEngine && !multiEngine.runningPrimary && multiEngine.secondaryEngineID == engineID))
          {
            if (PulseAnimation != "")
              foreach (AnimationState pulseState in pulseStates)
              {
                pulseState.normalizedTime = 0f;
                pulseState.speed = 0f;
              }
            pulseProgress = 0f;
            part.Effect(pulseEffectName, soundIntensityCurve.Evaluate(curveValue));
            if (plumeFX != null)
            {
              for (int i = 0; i < plumeFX.Count; i++)
              {
                plumeFX[i].Set(this.part, curveValue);
              }
            }
            if (light != null)
              light.intensity = lightIntensityCurve.Evaluate(curveValue);

            emissiveAnimator.SetScalar(lightIntensityCurve.Evaluate(curveValue));
            if (LaserAnimations)
            {
              laserFX[laserAnimatorIndex].Set(this.part, curveValue);
            }
          }

        }
      }
    }
  }

  public class PulseEnginePlumeEffect
  {
    public string name = "";
    public string controllerName = "";
    public string moduleID = "";
    public FloatCurve fxCurve = new FloatCurve();

    public ModuleWaterfallFX waterfallModule;

    public PulseEnginePlumeEffect()
    {
    }
    /// <summary>
    /// Construct from confignode
    /// </summary>
    /// <param name="node"></param>
    ///
    public PulseEnginePlumeEffect(ConfigNode node)
    {
      OnLoad(node);
    }

    public void OnLoad(ConfigNode node)
    {
      // Process nodes
      node.TryGetValue("name", ref name);
      node.TryGetValue("moduleID", ref moduleID);
      node.TryGetValue("plumeControllerName", ref controllerName);
      fxCurve.Load(node.GetNode("plumeIntensityCurve"));
    }
    public void Set(Part part, float value)
    {
      if (waterfallModule == null)
      {
        if (moduleID != "")
        {
          waterfallModule = part.GetComponents<ModuleWaterfallFX>().ToList().Find(x => x.moduleID == moduleID);
        }
        else
        {
          waterfallModule = part.GetComponents<ModuleWaterfallFX>().ToList().First();
        }
      }
      waterfallModule.SetControllerValue(controllerName, fxCurve.Evaluate(value));
    }
  }
  public class PulseEngineLaserEffect
  {
    public string name;
    public string fxName;
    public FloatCurve fxCurve = new FloatCurve();

    public ModuleColorAnimator[] fxAnimator;
    private string[] fxNames;
    public PulseEngineLaserEffect()
    {
    }
    /// <summary>
    /// Construct from confignode
    /// </summary>
    /// <param name="node"></param>
    ///
    public PulseEngineLaserEffect(ConfigNode node)
    {
      OnLoad(node);
    }

    public void OnLoad(ConfigNode node)
    {
      // Process nodes
      node.TryGetValue("name", ref name);
      node.TryGetValue("laserFXControllerIDs", ref fxName);

      fxNames = fxName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

      fxCurve.Load(node.GetNode("laserFXIntensityCurve"));
    }
    public void Set(Part part, float value)
    {
      if (fxAnimator == null)
      {
        fxAnimator = new ModuleColorAnimator[fxNames.Length];
        for (int i = 0; i < fxNames.Length; i++)
        {
          fxAnimator[i] = part.GetComponents<ModuleColorAnimator>().ToList().Find(x => x.moduleID == fxNames[i]);
        }
      }
      for (int i = 0; i < fxAnimator.Length; i++)
      {
        fxAnimator[i].SetScalar(fxCurve.Evaluate(value));
      }

    }

  }
}
