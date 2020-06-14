using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace FarFutureTechnologies
{
    public class ModuleChargeableEngine : PartModule
    {
        // Power used to charge the engine
        [KSPField(isPersistant = true, guiActive = true, guiName = "Recharge Rate"), UI_FloatRange(minValue = 10f, maxValue = 1000f, stepIncrement = 10f)]
        public float ChargeRate = 50f;

        // Amount of charge needed to start the engine
        [KSPField(isPersistant = false)]
        public float ChargeGoal = 150000f;

        // Current charge level
        [KSPField(isPersistant = true)]
        public float CurrentCharge = 0f;

        // Is the thing charging
        [KSPField(isPersistant = true)]
        public bool Charging = false;

        // Is the engine charged and ready
        [KSPField(isPersistant = false)]
        public bool Charged = false;

        // Is the engine running
        [KSPField(isPersistant = false)]
        public bool EngineOn = false;

        // Amout of power generated
        [KSPField(isPersistant = true)]
        public bool ShutdownEngineOnLowThrottle = true;

        // Current charge level
        [KSPField(isPersistant = true)]
        public float MinimumThrottleFraction = 0.05f;

        // Can power be generated if the engine is offline?
        [KSPField(isPersistant = true)]
        public bool PowerGeneratedOffline = false;

        // UI FIELDS/ BUTTONS
        // Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Capacitors")]
        public string ChargeStatus = "N/A";

        [KSPField(isPersistant = false, guiActive = true, guiName = "Recharge")]
        public string RechargeStatus = "N/A";

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Enable Engine Charging", active = true)]
        public void EnableCharging()
        {
            Charging = true;
        }
        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Disable Engine Charging", active = false)]
        public void DisableCharging()
        {
            Charging = false;
        }

        [KSPField(isPersistant = true, guiActive = true, guiName = "Power Setting"), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f)]
        public float GeneratorRate = 50f;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Power Generated")]
        public string GeneratorStatus = "N/A";

        // ACTIONS
        [KSPAction("Enable Engine Charging")]
        public void EnableAction(KSPActionParam param) { EnableCharging(); }

        [KSPAction("Disable Engine Charging")]
        public void DisableAction(KSPActionParam param) { DisableCharging(); }

        [KSPAction("Toggle Engine Charging")]
        public void ToggleAction(KSPActionParam param)
        {
            Charging = !Charging;
        }

        // VAB UI
        public string GetModuleTitle()
        {
            return "Chargeable Engine";
        }
        public override string GetModuleDisplayName()
        {
            return Localizer.Format("#LOC_FFT_ModuleChargeableEngine_ModuleName");
        }

        public override string GetInfo()
        {
            string msg = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_PartInfo", ChargeGoal.ToString("F0"), (ChargeGoal / 1000f).ToString("F0"));
            if (PowerGeneratedOffline)
            {
                msg += Localizer.Format("#LOC_FFT_ModuleChargeableEngine_PartInfo_Generator", GetPowerOutput().ToString("F0"));
                foreach (ResourceRatio input in powerInputs)
                {
                    msg += Localizer.Format("#LOC_FFT_ModuleChargeableEngine_PartInfo_GeneratorFuel",
                      input.ResourceName, input.Ratio.ToString("F5"));
                }
            }

            return msg;
        }

        private ModuleEnginesFX engine;
        public List<ResourceRatio> powerInputs;
        public List<ResourceRatio> powerOutputs;

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            if (PowerGeneratedOffline)
            {
                // Process nodes
                ConfigNode[] inNodes = node.GetNodes("INPUT_RESOURCE");
                ConfigNode[] outNodes = node.GetNodes("OUTPUT_RESOURCE");

                powerInputs = new List<ResourceRatio>();
                powerOutputs = new List<ResourceRatio>();
                for (int i = 0; i < inNodes.Length; i++)
                {
                    ResourceRatio p = new ResourceRatio();
                    p.Load(inNodes[i]);
                    powerInputs.Add(p);
                }
                for (int i = 0; i < outNodes.Length; i++)
                {
                    ResourceRatio p = new ResourceRatio();
                    p.Load(outNodes[i]);
                    powerOutputs.Add(p);
                }
            }
        }

        void Start()
        {
            if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
            {
                engine = this.GetComponent<ModuleEnginesFX>();
                SetupUI();
                if (PowerGeneratedOffline)
                {
                    if (powerInputs == null || powerOutputs.Count == 0)
                    {
                        ConfigNode node = GameDatabase.Instance.GetConfigs("PART").
                            Single(c => part.partInfo.name == c.name).config.
                            GetNodes("MODULE").Single(n => n.GetValue("name") == moduleName);
                        OnLoad(node);
                    }
                }


            }
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (CurrentCharge >= ChargeGoal)
                {
                    SetEngineUI(true);
                }
                else
                {
                    SetEngineUI(false);
                }
            }
        }
        void SetupUI()
        {
            if (PowerGeneratedOffline)
            {
                Fields["GeneratorRate"].guiActive = true;
                Fields["GeneratorRate"].guiActiveEditor = true;
                Fields["GeneratorStatus"].guiActive = true;
                Fields["GeneratorStatus"].guiActiveEditor = true;
            }
            else
            {
                Fields["GeneratorRate"].guiActive = false;
                Fields["GeneratorRate"].guiActiveEditor = false;
                Fields["GeneratorStatus"].guiActive = false;
                Fields["GeneratorStatus"].guiActiveEditor = false;
            }
            Fields["RechargeStatus"].guiName = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_RechargeStatus_Title");
            Fields["ChargeStatus"].guiName = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_ChargeStatus_Title");
            Fields["GeneratorRate"].guiName = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_GeneratorRate_Title");
            Fields["GeneratorStatus"].guiName = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_GeneratorStatus_Title");

            Events["EnableCharging"].guiName = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Event_Enable_Title");
            Events["DisableCharging"].guiName = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Event_Disable_Title");

            Actions["EnableAction"].guiName = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Action_EnableAction_Title");
            Actions["DisableAction"].guiName = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Action_DisableAction_Title");
            Actions["ToggleAction"].guiName = Localizer.Format("#LOC_ModuleChargeableEngine_Action_ToggleAction_Title");
        }

        void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight && engine != null)
            {
                if (Charging && !Charged)
                {
                    DoRecharge();
                }
                // iF the states don't match up, see if we need to change things
                if (engine.EngineIgnited != EngineOn)
                {
                    EvaluateEngineStateChange();
                }
                // If engine is on but dropped below a throttle setting, kill the engine
                if (engine.EngineIgnited && ShutdownEngineOnLowThrottle)
                {
                    if (engine.requestedThrottle <= MinimumThrottleFraction)
                    {
                        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Message_ThrottleLow",
                                                                                            part.partInfo.title,
                                                                                            (100f * MinimumThrottleFraction).ToString()),
                                                                           3.0f,
                                                                           ScreenMessageStyle.UPPER_CENTER));
                        OnShutdown();
                    }
                }
                if (EngineOn && PowerGeneratedOffline)
                {
                    DoPowerGeneration();
                }

            }
        }
        /// <summary>
        /// Checks to see when an engine changed state and applies appropriate effects
        /// </summary>
        void EvaluateEngineStateChange()
        {
            // Engine was turned on
            if (engine.EngineIgnited && !EngineOn)
            {
                OnActive();
            }
            // Engine was turned off
            else if (!engine.EngineIgnited && EngineOn)
            {
                OnShutdown();
            }

        }

        void Update()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (EngineOn)
                {
                    RechargeStatus = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_RechargeStatus_Running");
                    ChargeStatus = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_ChargeStatus_Running");
                }
                else
                {
                    if (!Charged && !Charging)
                        RechargeStatus = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_RechargeStatus_Disabled");
                }



            }
            if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
            {
                if (Events["EnableCharging"].active == Charging || Events["DisableCharging"].active != Charging)
                {
                    Events["DisableCharging"].active = Charging;
                    Events["EnableCharging"].active = !Charging;
                }
            }
        }

        /// <summary>
        /// Method to recharge the engine's startup capacitors
        /// </summary>
        void DoRecharge()
        {
            double req = part.RequestResource("ElectricCharge",(double)ChargeRate * TimeWarp.fixedDeltaTime);
            CurrentCharge = Mathf.MoveTowards(CurrentCharge, ChargeGoal, (float)req);

            if (req > 0.0d)
                RechargeStatus = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_RechargeStatus_Charging", ChargeRate.ToString("F2"));
            else
                RechargeStatus = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_RechargeStatus_NoPower");

            ChargeStatus = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_ChargeStatus_Normal", (CurrentCharge / ChargeGoal * 100.0f).ToString("F1"));
            if (CurrentCharge >= ChargeGoal)
            {
                RechargeStatus = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_RechargeStatus_Ready");
                Charged = true;
                SetEngineUI(true);
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Message_Ready",
                                                                                    part.partInfo.title),
                                                                           5.0f,
                                                                           ScreenMessageStyle.UPPER_CENTER));
            }
        }

        /// <summary>
        /// Method to do power generation if the engine is on
        /// </summary>
        void DoPowerGeneration()
        {
            double rate = GeneratorRate / 100d;
            double powerToGenerate = GetPowerOutput() * rate * TimeWarp.fixedDeltaTime;
            bool resourceCheck = true;

            for (int i = 0; i < powerInputs.Count(); i++)
            {
                double req = part.RequestResource(powerInputs[i].ResourceName, rate * powerInputs[i].Ratio * TimeWarp.fixedDeltaTime);
                if (engine.requestedThrottle < 0.01)
                {
                    if (req < 0.0000000001)
                    {
                        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Message_StaticFuelOut",
                                                                                            part.partInfo.title, powerInputs[i].ResourceName),
                                                                               5.0f,
                                                                               ScreenMessageStyle.UPPER_CENTER));
                        GeneratorStatus = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_GeneratorStatus_NoFuel", powerInputs[i].ResourceName);
                        OnShutdown();
                        resourceCheck = false;
                    }
                }

            }

            if (resourceCheck)
            {
                GeneratorStatus = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_GeneratorStatus_Running", (GetPowerOutput() * rate).ToString("F2"));
                part.RequestResource("ElectricCharge", -powerToGenerate);
            }
            else
            {

            }

        }

        public double GetPowerOutput()
        {
            for (int i = 0; i < powerOutputs.Count; i++)
            {
                if (powerOutputs[i].ResourceName == "ElectricCharge")
                    return powerOutputs[i].Ratio;
            }
            return 0d;
        }

        public override void OnActive()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (!Charged && engine.EngineIgnited)
                {
                    KillEngine();
                }
                if (Charged && engine.EngineIgnited)
                {
                    EngineOn = true;
                }
            }
        }
        public void OnShutdown()
        {
            EngineOn = false;
            Charged = false;
            CurrentCharge = 0f;
            SetEngineUI(false);
            GeneratorStatus = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_GeneratorStatus_Offline");
        }

        /// <summary>
        /// Sets the engine UI to be on or off
        /// </summary>
        /// <param name="state">If set to <c>on</c> state.</param>
        void SetEngineUI(bool state)
        {
            engine.Events["Activate"].guiActive = state;
            engine.Events["Shutdown"].guiActive = state;
            engine.Events["Activate"].guiActiveEditor = state;
            engine.Events["Shutdown"].guiActiveEditor = state;
        }

        void KillEngine()
        {
            if (engine != null)
            {
                EngineOn = false;
                engine.Events["Shutdown"].Invoke();
                engine.currentThrottle = 0;
                engine.requestedThrottle = 0;
            }
        }
    }
}
