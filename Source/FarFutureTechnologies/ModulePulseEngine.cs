using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Waterfall;

namespace FarFutureTechnologies
{
  public class ModulePulseEngine: PartModule
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

    private ModuleColorAnimator emissiveAnimator;
    private ModuleEnginesFX engine;
    private MultiModeEngine multiEngine;
    private ModuleWaterfallFX waterfallEffect;
    private Light light;

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
      }
    }
    bool pulseFired = false;

    int ticks = 0;
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
                //Utils.Log($"[ModulePulseEngine]: Pulse fired with impulse of {momentumPerFrame}, thrust {momentumPerFrame / TimeWarp.fixedDeltaTime} {engine.realIsp}, {PhysicsGlobals.GravitationalAcceleration}");

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
                  pulseState.speed = 1.0f/scaledPulseSpeed;
                }
              }
              part.Effect(engine.runningEffectName, 1f);
              waterfallEffect.SetControllerValue(plumeFXControllerID, plumeFXIntensityCurve.Evaluate(pulseProgress / scaledPulseSpeed));
              waterfallEffect.SetControllerValue(flareFXControllerID, flareFXIntensityCurve.Evaluate(pulseProgress / scaledPulseSpeed));
              light.intensity = lightIntensityCurve.Evaluate(pulseProgress/ scaledPulseSpeed);
              emissiveAnimator.SetScalar(pulseProgress / scaledPulseSpeed);
              pulseProgress = pulseProgress + TimeWarp.deltaTime;
            }
            else if (pulseProgress <= scaledPulseDuration)
            {
              foreach (AnimationState pulseState in pulseStates)
              {
                //pulseState.normalizedTime = Mathf.MoveTowards(pulseState.normalizedTime, 1.0f, TimeWarp.fixedDeltaTime * scaledPulseSpeed);
                pulseState.speed = 1.0f / scaledPulseSpeed;
              }
              part.Effect(engine.runningEffectName, 1);
              waterfallEffect.SetControllerValue(plumeFXControllerID, plumeFXIntensityCurve.Evaluate(pulseProgress / scaledPulseSpeed));
              waterfallEffect.SetControllerValue(flareFXControllerID, flareFXIntensityCurve.Evaluate(pulseProgress / scaledPulseSpeed));
              light.intensity = lightIntensityCurve.Evaluate(pulseProgress / scaledPulseSpeed);
              emissiveAnimator.SetScalar(pulseProgress / scaledPulseSpeed);
              pulseProgress = pulseProgress + TimeWarp.deltaTime;
            }
            else if (pulseProgress >= scaledPulseDuration && pulseProgress <= (scaledPulseDuration + scaledPulseInterval))
            {
              foreach (AnimationState pulseState in pulseStates)
              {
                pulseState.normalizedTime = 0f;
                pulseState.speed = 0f;
              }
              part.Effect(engine.runningEffectName, 0f);
              waterfallEffect.SetControllerValue(plumeFXControllerID, plumeFXIntensityCurve.Evaluate(pulseProgress / scaledPulseSpeed));
              waterfallEffect.SetControllerValue(flareFXControllerID, flareFXIntensityCurve.Evaluate(pulseProgress / scaledPulseSpeed));
              pulseProgress = pulseProgress + TimeWarp.deltaTime;
              light.intensity = lightIntensityCurve.Evaluate(pulseProgress / scaledPulseSpeed);
              emissiveAnimator.SetScalar(pulseProgress / scaledPulseSpeed);
            } 
            else
            {
              
              foreach (AnimationState pulseState in pulseStates)
              {
                pulseState.normalizedTime = 0f;
                pulseState.speed = 0f;
              }
              part.Effect(engine.runningEffectName, 0f);
              waterfallEffect.SetControllerValue(plumeFXControllerID, plumeFXIntensityCurve.Evaluate(pulseProgress / scaledPulseSpeed));
              pulseProgress = 0f;
              light.intensity = lightIntensityCurve.Evaluate(pulseProgress/ scaledPulseSpeed);
              emissiveAnimator.SetScalar(pulseProgress / scaledPulseSpeed);
            }
          } else
          {
            foreach (AnimationState pulseState in pulseStates)
            {
              pulseState.normalizedTime = 0f;
              pulseState.speed = 0f;
            }
            part.Effect(engine.runningEffectName, 0f);
            waterfallEffect.SetControllerValue(flareFXControllerID, flareFXIntensityCurve.Evaluate(pulseProgress / scaledPulseSpeed));
            waterfallEffect.SetControllerValue(plumeFXControllerID, plumeFXIntensityCurve.Evaluate(pulseProgress / scaledPulseSpeed));
            pulseProgress = 0f;
            light.intensity = lightIntensityCurve.Evaluate(pulseProgress / scaledPulseSpeed);
            emissiveAnimator.SetScalar(pulseProgress / scaledPulseSpeed);
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
            waterfallEffect.SetControllerValue(flareFXControllerID, flareFXIntensityCurve.Evaluate(pulseProgress * scaledPulseSpeed));
            waterfallEffect.SetControllerValue(plumeFXControllerID, plumeFXIntensityCurve.Evaluate(pulseProgress * scaledPulseSpeed));

            light.intensity = lightIntensityCurve.Evaluate(pulseProgress * scaledPulseSpeed);
            emissiveAnimator.SetScalar(pulseProgress * scaledPulseSpeed);
          }
        }
      }
      
    }

  }
}
