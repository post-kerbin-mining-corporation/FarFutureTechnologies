using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Waterfall;

namespace FarFutureTechnologies
{
  public class ModulePulseEngine : PartModule
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

    // Transform containing the flare light
    [KSPField(isPersistant = false)]
    public string lightTransformName;

    // Light intensity along the pulse
    [KSPField(isPersistant = false)]
    public FloatCurve lightIntensityCurve = new FloatCurve();

    // FX intensity along the pulse
    [KSPField(isPersistant = false)]
    public FloatCurve plumeFXIntensityCurve = new FloatCurve();

    // 
    [KSPField(isPersistant = false)]
    public string plumeFXControllerID = "throttle";
    // FX intensity along the pulse
    [KSPField(isPersistant = false)]
    public FloatCurve flareFXIntensityCurve = new FloatCurve();

    // 
    [KSPField(isPersistant = false)]
    public string flareFXControllerID = "throttle";

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
    private ModuleColorAnimator emissiveAnimator;
    private ModuleEnginesFX engine;
    private MultiModeEngine multiEngine;
    private ModuleWaterfallFX waterfallEffect;
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
        waterfallEffect = this.GetComponents<ModuleWaterfallFX>().ToList().Find(x => x.engineID == engineID);
      }
      else
      {
        engine = this.GetComponents<ModuleEnginesFX>().ToList().First();
        waterfallEffect = this.GetComponents<ModuleWaterfallFX>().ToList().First();
      }
      savedThrust = engine.maxThrust;
      multiEngine = this.GetComponent<MultiModeEngine>();

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
      SetupLight();

      /// Reload nodes as needed
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (LaserAnimations && (laserFX == null || laserFX.Count == 0))
        {

          ConfigNode node = GameDatabase.Instance.GetConfigs("PART").
              Single(c => part.partInfo.name == c.name).config.
              GetNodes("MODULE").Single(n => n.GetValue("name") == moduleName);
          OnLoad(node);
        }
      }
    }

    void SetupLight()
    {
      if (lightTransformName != "")
      {
        Transform lightXform = part.FindModelTransform(lightTransformName);
        if (lightXform)
        {
          light = lightXform.gameObject.GetComponent<Light>();
          light.intensity = 0f;
        }
        if (light == null)
  
              Debug.LogError($"[ModulePulseEngine] No light was found on  {lightTransformName}");
            
      }
    }

    public override void OnLoad(ConfigNode node)
    {
      base.OnLoad(node);
      laserFX = new List<PulseEngineLaserEffect>();
      ConfigNode[] varNodes = node.GetNodes("LASERFX");

      for (int i = 0; i < varNodes.Length; i++)
      {
        laserFX.Add(new PulseEngineLaserEffect(varNodes[i]));
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
                  Utils.Log($"[ModulePulseEngine]: Pulse fired with impulse of {momentumPerFrame}, thrust {momentumPerFrame / TimeWarp.fixedDeltaTime} {engine.realIsp}, {PhysicsGlobals.GravitationalAcceleration}");

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
    void Update()
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
              part.Effect(engine.runningEffectName, 1f);
              pulseProgress = pulseProgress + TimeWarp.deltaTime;
              if (laserPulseDone)
              {
                laserPulseDone = false;

              }
            }
            // During pulse
            else if (pulseProgress < scaledPulseDuration)
            {
              foreach (AnimationState pulseState in pulseStates)
              {
                //pulseState.normalizedTime = Mathf.MoveTowards(pulseState.normalizedTime, 1.0f, TimeWarp.fixedDeltaTime * scaledPulseSpeed);
                pulseState.speed = 1.0f / scaledPulseSpeed;
              }
              part.Effect(engine.runningEffectName, 1);

              pulseProgress = pulseProgress + TimeWarp.deltaTime;
            }
            // after pulse but during wait period
            else if (pulseProgress >= scaledPulseDuration && pulseProgress < (scaledPulseDuration + scaledPulseInterval))
            {
              foreach (AnimationState pulseState in pulseStates)
              {
                pulseState.normalizedTime = 0f;
                pulseState.speed = 0f;
              }
              part.Effect(engine.runningEffectName, 0f);
              pulseProgress = pulseProgress + TimeWarp.deltaTime;

              
            }
            // After pulse wait period
            else
            {

              foreach (AnimationState pulseState in pulseStates)
              {
                pulseState.normalizedTime = 0f;
                pulseState.speed = 0f;
              }
              part.Effect(engine.runningEffectName, 0f);
              pulseProgress = 0f;
              if (!laserPulseDone)
              {
                laserPulseDone = true;
                laserAnimatorIndex++;
                if (laserAnimatorIndex > laserFX.Count - 1) laserAnimatorIndex = 0;
              }
            }
            waterfallEffect.SetControllerValue(plumeFXControllerID, plumeFXIntensityCurve.Evaluate(curveValue));
            waterfallEffect.SetControllerValue(flareFXControllerID, flareFXIntensityCurve.Evaluate(curveValue));
            light.intensity = lightIntensityCurve.Evaluate(curveValue);
            emissiveAnimator.SetScalar(lightIntensityCurve.Evaluate(curveValue));

            if (LaserAnimations)
            {
              laserFX[laserAnimatorIndex].Set(this.part, curveValue);
            }
          }
          else
          {
            foreach (AnimationState pulseState in pulseStates)
            {
              pulseState.normalizedTime = 0f;
              pulseState.speed = 0f;
            }
            part.Effect(engine.runningEffectName, 0f);
            waterfallEffect.SetControllerValue(flareFXControllerID, flareFXIntensityCurve.Evaluate(curveValue));
            waterfallEffect.SetControllerValue(plumeFXControllerID, plumeFXIntensityCurve.Evaluate(curveValue));
            pulseProgress = 0f;
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
          if (multiEngine == null || (multiEngine && multiEngine.runningPrimary && multiEngine.primaryEngineID == engineID))
          {
            foreach (AnimationState pulseState in pulseStates)
            {
              pulseState.normalizedTime = 0f;
              pulseState.speed = 0f;
            }
            pulseProgress = 0f;
            part.Effect(engine.runningEffectName, 0f);
            waterfallEffect.SetControllerValue(flareFXControllerID, flareFXIntensityCurve.Evaluate(curveValue));
            waterfallEffect.SetControllerValue(plumeFXControllerID, plumeFXIntensityCurve.Evaluate(curveValue));

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
