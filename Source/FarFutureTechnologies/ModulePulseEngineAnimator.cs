using UnityEngine;

namespace FarFutureTechnologies
{
  public class ModulePulseEngineAnimator : PartModule
  {
    // Animation that plays
    [KSPField(isPersistant = false)]
    public string PulseAnimation;

    [KSPField(isPersistant = false)]
    public string InitiateAnimation;

    [KSPField(isPersistant = false)]
    public float PulseInterval = 0.4f;

    [KSPField(isPersistant = false)]
    public float PulseDuration = 0.1f;

    [KSPField(isPersistant = false)]
    public float MaxPulseInterval = 3.0f;

    [KSPField(isPersistant = false)]
    public float PulseAnimationDecayRate = 3.0f;

    [KSPField(isPersistant = false)]
    public float InitiateAnimationGrowRate = 2.0f;
    [KSPField(isPersistant = false)]
    public float InitiateAnimationDecayRate = 3.0f;

    [KSPField(isPersistant = false)]
    public float InitiateDuration = 3.0f;

    [KSPField(isPersistant = false)]
    public float PulseAnimationGrowRate = 2.0f;
    [KSPField(isPersistant = false)]
    public string PulseTransformName = "";

    [KSPField(isPersistant = false)]
    public string PulseEffectName1 = "";

    [KSPField(isPersistant = false)]
    public string PulseEffectName2 = "";

    // Private variables

    // ModuleEngines to use
    private ModuleEnginesFX engineModule;

    // Pulse progression
    private float pulseProgress = 0f;

    private Transform fxCenter;

    // Animations

    private AnimationState[] pulseStates;
    private AnimationState[] initStates;

    public override void OnStart(PartModule.StartState state)
    {

      fxCenter = part.FindModelTransform(PulseTransformName);

      engineModule = part.GetComponent<ModuleEnginesFX>();

      if (engineModule == null)
      {
        Utils.LogError("[ModulePulseEngineAnimator]: No ModuleEnginesFX found on part!");
      }
      // Set up animations
      if (PulseAnimation != "")
      {
        pulseStates = Utils.SetUpAnimation(PulseAnimation, part);

        foreach (AnimationState pulseState in pulseStates)
        {
          pulseState.layer = 1;
        }
      }
      if (InitiateAnimation != "")
      {
        initStates = Utils.SetUpAnimation(InitiateAnimation, part);

        foreach (AnimationState initState in initStates)
        {
          initState.layer = 2;
        }

      }

    }

    private float currentPulseInterval = 0f;
    KSPParticleEmitter[] fxes;

    public void FixedUpdate()
    {

      if (HighLogic.LoadedScene == GameScenes.FLIGHT)
      {
        if (!engineModule)
          return;

        if (engineModule.EngineIgnited)
        {
          if (engineModule.requestedThrottle > 0f && !engineModule.flameout)
          {

            currentPulseInterval = Mathf.Clamp(PulseInterval * (1f / engineModule.normalizedThrustOutput), 0f, MaxPulseInterval);

            // At start of pulse
            if (pulseProgress == 0f)
            {
              // Fire effect

              part.Effect(PulseEffectName1, 1f);
              part.Effect(PulseEffectName2, 1f);

              if (PulseAnimation != "")
                foreach (AnimationState pulseState in pulseStates)
                {
                  pulseState.normalizedTime = 0f;
                  pulseState.speed = PulseAnimationGrowRate;
                }
              if (InitiateAnimation != "")

                foreach (AnimationState initState in initStates)
                {
                  initState.normalizedTime = 0f;
                  initState.speed = InitiateAnimationGrowRate;
                }

              pulseProgress = pulseProgress + TimeWarp.fixedDeltaTime;
            }
            // During Pulse
            else if (pulseProgress <= PulseDuration)
            {
              part.Effect(PulseEffectName1, Mathf.Clamp01(1f - (pulseProgress / PulseDuration)));
              part.Effect(PulseEffectName2, Mathf.Clamp01(1f - (pulseProgress / PulseDuration)));


              if (InitiateAnimation != "" && pulseProgress >= InitiateDuration)
                foreach (AnimationState initState in initStates)
                {
                  initState.normalizedTime = Mathf.MoveTowards(initState.normalizedTime, 0f, TimeWarp.fixedDeltaTime * InitiateAnimationDecayRate);
                  initState.speed = 0f;
                }
              pulseProgress = pulseProgress + TimeWarp.fixedDeltaTime;
            }
            else if (pulseProgress >= currentPulseInterval)
            {
              foreach (AnimationState pulseState in pulseStates)
              {
                pulseState.normalizedTime = 0f;
                pulseState.speed = 0f;
              }
              if (InitiateAnimation != "")

                foreach (AnimationState initState in initStates)
                {
                  initState.normalizedTime = 0f;
                  initState.speed = 0f;
                }
              pulseProgress = 0f;

            }
            // After pulse but before next
            else
            {
              part.Effect(PulseEffectName1, 0f);
              part.Effect(PulseEffectName2, 0f);
              pulseProgress = pulseProgress + TimeWarp.fixedDeltaTime;

              foreach (AnimationState pulseState in pulseStates)
              {
                pulseState.speed = 0f;
                pulseState.normalizedTime = Mathf.MoveTowards(pulseState.normalizedTime, 0f, TimeWarp.fixedDeltaTime * PulseAnimationDecayRate);
              }
              if (InitiateAnimation != "")
                foreach (AnimationState initState in initStates)
                {
                  initState.normalizedTime = Mathf.MoveTowards(initState.normalizedTime, 0f, TimeWarp.fixedDeltaTime * InitiateAnimationDecayRate);
                  initState.speed = 0f;
                }

            }
          }
          else

          {

            foreach (AnimationState pulseState in pulseStates)
            {
              pulseState.normalizedTime = Mathf.MoveTowards(pulseState.normalizedTime, 0f, TimeWarp.fixedDeltaTime * PulseAnimationDecayRate);
              pulseState.speed = 0f;
            }
            if (InitiateAnimation != "")
              foreach (AnimationState initState in initStates)
              {
                initState.normalizedTime = Mathf.MoveTowards(initState.normalizedTime, 0f, TimeWarp.fixedDeltaTime * InitiateAnimationDecayRate); ;
                initState.speed = 0f;
              }
            if (PulseEffectName1 != "")
            {
              part.Effect(PulseEffectName1, 0f);
              part.Effect(PulseEffectName2, 0f);
            }
          }

        }
        else
        {

          pulseProgress = 0f;
          if (PulseEffectName1 != "")
          {
            part.Effect(PulseEffectName1, 0f);
            part.Effect(PulseEffectName2, 0f);
          }
          foreach (AnimationState pulseState in pulseStates)
          {
            pulseState.time = Mathf.MoveTowards(pulseState.time, 0f, TimeWarp.fixedDeltaTime * PulseAnimationDecayRate);
            pulseState.speed = 0f;
          }
          if (InitiateAnimation != "")
            foreach (AnimationState initState in initStates)
            {
              initState.time = Mathf.MoveTowards(initState.time, 0f, TimeWarp.fixedDeltaTime * InitiateAnimationDecayRate); ;
              initState.speed = 0f;
            }
        }


      }
    }

  }
}
