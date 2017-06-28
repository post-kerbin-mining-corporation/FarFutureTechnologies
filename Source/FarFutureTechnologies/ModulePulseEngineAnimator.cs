using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FarFutureTechnologies
{
    public class ModulePulseEngineAnimator: PartModule
    {
        // Animation that plays
        [KSPField(isPersistant = false)]
        public string PulseAnimation;

        // Heat
        [KSPField(isPersistant = false)]
        public string HeatAnimation;

        [KSPField(isPersistant = false)]
        public string ThrottleAnimation;

        [KSPField(isPersistant = false)]
        public float PulseInterval = 0.4f;

        [KSPField(isPersistant = false)]
        public float PulseDuration = 0.1f;

        [KSPField(isPersistant = false)]
        public float MaxPulseInterval = 3.0f;

        [KSPField(isPersistant = false)]
        public float PulseAnimationDecayRate = 3.0f;

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
        private AnimationState[] throttleStates;

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
                pulseStates    = Utils.SetUpAnimation(PulseAnimation, part);

                foreach (AnimationState pulseState in pulseStates)
                {
                    pulseState.layer =1;
                }

            }
            if (ThrottleAnimation != "")
            {
               throttleStates = Utils.SetUpAnimation(ThrottleAnimation, part);

                foreach (AnimationState throttleState in throttleStates)
                {
                    throttleState.layer = 2;
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

                        currentPulseInterval = Mathf.Clamp(PulseInterval * (1f/engineModule.normalizedThrustOutput), 0f, MaxPulseInterval);

                        // At start of pulse
                        if (pulseProgress == 0f)
                        {
                            // Fire effect

                            part.Effect(PulseEffectName1, 1f);
                            part.Effect(PulseEffectName2, 1f);

                            foreach (AnimationState pulseState in pulseStates)
                            {
                                pulseState.time = 0f;
                                pulseState.speed = PulseAnimationGrowRate;
                            }
                            foreach (AnimationState throttleState in throttleStates)
                            {
                                throttleState.normalizedTime = 1.0f;
                            }
                            pulseProgress = pulseProgress + TimeWarp.fixedDeltaTime;
                        }
                        // During Pulse
                        else if (pulseProgress <= PulseDuration)
                        {
                            part.Effect(PulseEffectName1, Mathf.Clamp01(1f - (pulseProgress / PulseDuration)));
                            part.Effect(PulseEffectName2, Mathf.Clamp01(1f - (pulseProgress / PulseDuration)));
                            pulseProgress = pulseProgress + TimeWarp.fixedDeltaTime;
                        } else if (pulseProgress >= currentPulseInterval)
                        {
                            foreach (AnimationState pulseState in pulseStates)
                            {
                                pulseState.time = 0f;
                                pulseState.speed = 0f;
                            }
                            pulseProgress = 0f;
                            foreach (AnimationState throttleState in throttleStates)
                            {
                                throttleState.normalizedTime = 0f;
                            }
                        }
                        // After pulse but before next
                        else
                        {
                            part.Effect(PulseEffectName1, 0f);
                            part.Effect(PulseEffectName2, 0f);
                            pulseProgress = pulseProgress + TimeWarp.fixedDeltaTime;
                            foreach (AnimationState throttleState in throttleStates)
                            {
                                throttleState.normalizedTime = Mathf.MoveTowards(throttleState.normalizedTime, 0f, TimeWarp.fixedDeltaTime * PulseAnimationDecayRate);
                            }
                            foreach (AnimationState pulseState in pulseStates)
                            {
                                pulseState.normalizedTime = Mathf.MoveTowards(pulseState.normalizedTime, 0f, TimeWarp.fixedDeltaTime * PulseAnimationDecayRate);
                            }
                        }
                    }
                    else

                    {
                        foreach (AnimationState throttleState in throttleStates)
                        {
                            throttleState.normalizedTime = Mathf.Lerp(throttleState.normalizedTime, 0f, TimeWarp.fixedDeltaTime*PulseAnimationDecayRate);
                        }
                        foreach (AnimationState pulseState in pulseStates)
                        {
                            pulseState.time = Mathf.MoveTowards(pulseState.time, 0f, TimeWarp.fixedDeltaTime*PulseAnimationDecayRate);
                            pulseState.speed = 0f;
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
                    foreach (AnimationState throttleState in throttleStates)
                    {
                        throttleState.normalizedTime = Mathf.Lerp(throttleState.normalizedTime, 0f, TimeWarp.fixedDeltaTime);
                    }
                    pulseProgress = 0f;
                    if (PulseEffectName1 != "")
                    {
                        part.Effect(PulseEffectName1, 0f);
                        part.Effect(PulseEffectName2, 0f);
                    }
                }


            }
        }

    }
}
