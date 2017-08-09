using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI;
using KSP.Localization;

namespace FarFutureTechnologies
{
    public class ModuleFusionReactor: ModuleResourceConverter
    {
        /// CONFIGURABLE FIELDS
        // ----------------------

        // --- Charge-up -----
        [KSPField(isPersistant = true)]
        public bool Charged = false;

        [KSPField(isPersistant = true)]
        public bool Charging = false;

        [KSPField(isPersistant = true)]
        public float CurrentCharge = 0f;

        [KSPField(isPersistant = true, guiActive = true, guiName = "Charge Rate"), UI_FloatRange(minValue = 10f, maxValue = 1000f, stepIncrement = 10f)]
        public float ChargeRate = 50f;

        [KSPField(isPersistant = false)]
        public float ChargeGoal = 500000f;

        // --- Heat ---
        // Heat generation at full power
        [KSPField(isPersistant = false)]
        public float HeatGeneration;


        // --- Animation ---
        // name of the overheat animation
        [KSPField(isPersistant = false)]
        public string ActiveAnimationName;
        // name of the overheat animation
        [KSPField(isPersistant = false)]
        public string OverheatAnimationName;


        // ---- Fuels ----
        // Current fuel mode name
        public string CurrentModeID = null;
        [KSPField(isPersistant = false)]
        public float MinimumReactorPower = 0.1f;

        /// UI
        /// ---------------------
        /// // Current fuel mode
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Fuel Mode")]
        [UI_ChooseOption(affectSymCounterparts = UI_Scene.None, scene = UI_Scene.All, suppressEditorShipModified = true)]
        public int currentModeIndex = 0;

        // Heat Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Heat Generated")]
        public string HeatOutput;

        // Reactor Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Power Generated")]
        public string ReactorOutput;

        // Fuel Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Fuel Usage")]
        public string FuelInput;

        // Vessel Temperature
        [KSPField(isPersistant = false, guiActive = true, guiName = "System Temperature")]
        public string CoreTemp;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Capacitors")]
        public string ChargeStatus = "N/A";

        [KSPField(isPersistant = false, guiActive = true, guiName = "Recharge")]
        public string RechargeStatus = "N/A";

        /// KSPACTIONS
        /// ----------------------
        [KSPAction("Enable Startup Charging")]
        public void EnableAction(KSPActionParam param) { EnableCharging(); }

        [KSPAction("Disable Startup Charging")]
        public void DisableAction(KSPActionParam param) { DisableCharging(); }

        [KSPAction("Toggle Startup Charging")]
        public void ToggleAction(KSPActionParam param)
        {
            Charging = !Charging;
        }

        /// KSPEVENTS
        /// ----------------------

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Enable Startup Charging", active = true)]
        public void EnableCharging()
        {
            Charging = true;
        }
        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Disable Startup Charging", active = true)]
        public void DisableCharging()
        {
            Charging = false;
        }

        private float maintenenceConsumption = 0f;
        private bool activeFlag;
        private int heatTicker = 0;
        private List<FusionReactorMode> modes;

        private AnimationState activeAnimation;
        private AnimationState overheatAnimation;

        private ModuleFusionCore core;

        public string GetModuleTitle()
        {
            return "FusionReactor";
        }
        public override string GetModuleDisplayName()
        {
            return Localizer.Format("#LOC_FFT_ModuleFusionReactor_ModuleName");
        }

        public override string GetInfo()
        {
            string msg = Localizer.Format("#LOC_FFT_ModuleFusionReactor_PartInfo");
            foreach(FusionReactorMode mode in modes)
            {
              msg += Localizer.Format("#LOC_FFT_ModuleFusionReactor_PartInfo_Mode",
                  mode.modeName,
                  mode.GetOutput().ToString("F0"));
              foreach (ResourceRatio input in mode.inputs)
              {
                msg += Localizer.Format("#LOC_FFT_ModuleFusionReactor_PartInfo_Fuel",
                  input.ResourceName, input.Ratio);
              }
            }
            return msg;
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);
            ConfigNode[] varNodes = node.GetNodes("FUSIONMODE");
            modes = new List<FusionReactorMode>();
            for (int i = 0; i < varNodes.Length; i++)
            {
                modes.Add(new FusionReactorMode(varNodes[i]));
            }
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            /// Reload nodes as needed
            if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
            {
                if (modes == null || modes.Count == 0)
                {
                    ConfigNode node = GameDatabase.Instance.GetConfigs("PART").
                        Single(c => part.partInfo.name == c.name).config.
                        GetNodes("MODULE").Single(n => n.GetValue("name") == moduleName);
                    OnLoad(node);
                }
            }
            SetupUI();
            SetupAnimations();
            SetupHeat();
            SetupRecharge();

            ChangeMode(currentModeIndex);

            part.force_activate();
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            RechargeCapacitors();
            GenerateHeat();
            MaintainMinimumConsumption();
            HandleAnimation();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (base.ModuleIsActive())
            {
                ReactorOutput = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_ReactorOutput_Running", (base.lastTimeFactor * modes[currentModeIndex].GetOutput()).ToString("F1"));

                float fuelUse = maintenenceConsumption;
                for (int i = 0; i < inputList.Count ; i++)
                {
                    fuelUse += (float)base.lastTimeFactor * (float)inputList[i].Ratio;
                }
                FuelInput = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_FuelInput_Running", fuelUse.ToString("F4"));
            }
            else
            {
                ReactorOutput = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_ReactorOutput_Offline");
                FuelInput = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_FuelInput_Offline");
            }

            CoreTemp = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_CoreTemp_Running", core.CoreTemperature.ToString("F0"), core.CoreTempGoal.ToString("F0"));

            if (Events["EnableCharging"].active == Charging || Events["DisableCharging"].active != Charging)
            {
                Events["DisableCharging"].active = Charging;
                Events["EnableCharging"].active = !Charging;
            }
        }

        /// <summary>
        /// Sets up the UI
        /// </summary>
        void SetupUI()
        {
            var chooseField = Fields["currentModeIndex"];
            var chooseOption = (UI_ChooseOption)chooseField.uiControlEditor;
            chooseOption.options = modes.Select(s => s.modeName).ToArray();
            chooseOption.onFieldChanged = UpdateModesFromControl;
            chooseOption = (UI_ChooseOption)chooseField.uiControlFlight;
            chooseOption.options = modes.Select(s => s.modeName).ToArray();
            chooseOption.onFieldChanged = UpdateModesFromControl;

            // Kill converter field
            Fields["status"].guiActive = false;

            Fields["currentModeIndex"].guiName = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_CurrentModeIndex_Title");
            Fields["HeatOutput"].guiName =  Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_HeatOutput_Title");
            Fields["ReactorOutput"].guiName =  Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_ReactorOutput_Title");
            Fields["FuelInput"].guiName =  Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_FuelInput_Title");
            Fields["CoreTemp"].guiName = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_CoreTemp_Title");
            Fields["ChargeStatus"].guiName =  Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_ChargeStatus_Title");
            Fields["RechargeStatus"].guiName =  Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_RechargeStatus_Title");

            Events["EnableCharging"].guiName = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Event_Enable_Title ");
            Events["DisableCharging"].guiName = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Event_Disable_Title ");

            Actions["EnableAction"].guiName = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Action_EnableAction_Title ");
            Actions["DisableAction"].guiName = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Action_DisableAction_Title ");
            Actions["ToggleAction"].guiName = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Action_ToggleAction_Title ");
        }

        /// <summary>
        /// Sets up Animations
        /// </summary>
        void SetupAnimations()
        {
            if (ActiveAnimationName != "")
                activeAnimation = Utils.SetUpAnimation(ActiveAnimationName, part, 2)[0];

            for (int i = 0; i < modes.Count; i++)
            {
                modes[i].InitializeAnimations(part);
            }
        }

        /// <summary>
        /// Sets up heat related parameters
        /// </summary>
        void SetupHeat()
        {
            if (base.ModuleIsActive())
                activeFlag = true;
            else
                activeFlag = false;

            if (HighLogic.LoadedScene != GameScenes.EDITOR)
            {
                core = this.GetComponent<ModuleFusionCore>();
            }
        }

        /// <summary>
        /// Sets up the capacitor recharge/discharge parameters
        /// </summary>
        void SetupRecharge()
        {
            if (CurrentCharge >= ChargeGoal)
            {
                SetRechargeUI(true);
            }
            else
            {
                SetRechargeUI(false);
            }
        }

        /// <summary>
        /// Delegate method called when the fuel mode button is pressed
        /// </summary>
        /// <param name="field"></param>
        /// <param name="oldFieldValueObj"></param>
        void UpdateModesFromControl(BaseField field, object oldFieldValueObj)
        {
            ChangeMode(currentModeIndex);
        }

        /// <summary>
        /// Handles updating animations
        /// </summary>
        void HandleAnimation()
        {
            if (ActiveAnimationName != "")
            {
                if (base.ModuleIsActive())
                {
                    activeAnimation.speed = 1f;
                    activeAnimation.normalizedTime = Mathf.Clamp01(activeAnimation.normalizedTime);
                }
                else
                {
                    activeAnimation.speed = -1f;
                    activeAnimation.normalizedTime = Mathf.Clamp01(activeAnimation.normalizedTime);
                }
            }
        }

        /// <summary>
        ///
        /// Handles heat generation
        /// </summary>
        void GenerateHeat()
        {
            if (base.ModuleIsActive())
            {
                if (base.ModuleIsActive() != activeFlag)
                {
                    base.lastUpdateTime = Planetarium.GetUniversalTime();
                    heatTicker = 60;

                    ReactorActivated();

                }
                HeatOutput = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_HeatOutput_Running", (HeatGeneration/50f).ToString("F0"));
                SetHeatGeneration(HeatGeneration);
            }
            else
            {
                if (base.ModuleIsActive() != activeFlag)
                {

                    ZeroThermal();
                    ReactorDeactivated();
                }
                HeatOutput = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_HeatOutput_Offline");
                SetHeatGeneration(0f);
            }
        }

        void ReactorActivated()
        {
            Debug.Log("[ModuleFusionReactor]: Reactor Startup");
            if (!Charged)
            {
                base.StopResourceConverter();
            }
            else
            {

                activeFlag = true;
            }
        }

        void ReactorDeactivated()
        {
            Debug.Log("[ModuleFusionReactor]: Reactor Shutdown");
            Charged = false;
            CurrentCharge = 0f;
            activeFlag = false;
        }

        /// <summary>
        /// Handles the lower level heat functionality
        /// </summary>
        /// <param name="heat"></param>
        private void SetHeatGeneration(float heat)
        {
            if (Time.timeSinceLevelLoad > 1f)
                GeneratesHeat = true;
            else
                GeneratesHeat = false;

            if (heatTicker <= 0)
            {
                core.AddEnergyToCore(heat / 50f);
                //TemperatureModifier = new FloatCurve();
                //TemperatureModifier.Add(0f, heat);
            }
            else
            {
                ZeroThermal();
                heatTicker = heatTicker - 1;
            }
            core.MaxCoolant = heat;
        }

        /// <summary>
        /// Zeros all thermal parameters
        /// </summary>
        private void ZeroThermal()
        {
            base.lastHeatFlux = 0d;
            core.ZeroThermal();
            base.GeneratesHeat = false;
            TemperatureModifier = new FloatCurve();
            TemperatureModifier.Add(0f, 0f);
        }

        /// <summary>
        /// Consumes fuel to maintaina minimum consumption
        /// </summary>
        void MaintainMinimumConsumption()
        {
            if (base.ModuleIsActive())
            {

                float diff = (float)base.lastTimeFactor - MinimumReactorPower;

                if (diff < 0)
                {
                    diff = Mathf.Abs(diff);
                    maintenenceConsumption = 0f;
                    for (int i = 0; i < inputList.Count; i++)
                    {
                        maintenenceConsumption += diff * (float)inputList[i].Ratio;
                        double req = part.RequestResource(inputList[i].ResourceName, diff * inputList[i].Ratio * TimeWarp.fixedDeltaTime);
                        if (req < 0.000001)
                        {
                            ToggleResourceConverterAction(new KSPActionParam(0, KSPActionType.Activate));
                        }
                    }
                }
                else
                {
                    maintenenceConsumption = 0f;
                }
            }
            else
            {
                maintenenceConsumption = 0f;
            }
        }

        /// <summary>
        /// Handles capacitor recharge
        /// </summary>
        void RechargeCapacitors()
        {
            if (Charging && !Charged)
            {
                double req = part.RequestResource("ElectricCharge", ChargeRate * TimeWarp.fixedDeltaTime);
                CurrentCharge = Mathf.MoveTowards(CurrentCharge, ChargeGoal, (float)req);

                if (req > 0.0d)
                    RechargeStatus = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_RechargeStatus_Charging", ChargeRate.ToString("F2"));
                else
                    RechargeStatus = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_RechargeStatus_NoPower");

                ChargeStatus = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_ChargeStatus_Normal", (CurrentCharge / ChargeGoal * 100.0f).ToString("F1"));
                if (CurrentCharge >= ChargeGoal)
                {
                    RechargeStatus = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_RechargeStatus_Ready");
                    Charged = true;
                   SetRechargeUI(false);
                }
            }
        }

        /// <summary>
        /// Toggles the UI between a "ready" mode and a "recharging" mode
        /// </summary>
        /// <param name="isCharging"></param>
        void SetRechargeUI(bool isCharging)
        {
            //Fields["HeatOuput"].guiActive = !isCharging;
            //Fields["ReactorOutput"].guiActive = !isCharging;
            //Fields["FuelInput"].guiActive = !isCharging;
            //Fields["FuelStatus"].guiActive = !isCharging;

            //Events["startEvt"].guiActive = !isCharging;

            //Fields["RechargeStatus"].guiActive = isCharging;
            //Fields["ChargeStatus"].guiActive = isCharging;
        }

        /// <summary>
        /// Switches the reactor's mode to a new mode
        /// </summary>
        /// <param name="newMode"></param>
        ///
        void ChangeMode(int modeIndex)
        {
            // Turn off the reactor when switching fuels
            if (base.ModuleIsActive())
            {
                ToggleResourceConverterAction(new KSPActionParam(0, KSPActionType.Activate));
            }

            for (int i = 0; i < modes.Count; i++)
            {
                modes[i].Deactivate();
            }
            Utils.Log(String.Format("[FissionReactor]: Fuel Mode was changed to {0}", modes[modeIndex].modeID));
            modes[modeIndex].Activate();
            inputList = modes[modeIndex].inputs;
            outputList = modes[modeIndex].outputs;
            base._recipe = LoadRecipe();
        }

    }

    /// <summary>
    /// A class that holds information about a fusion reactor mode
    /// </summary>
    public class FusionReactorMode
    {
        public string modeName;
        public string modeID;
        public string animationName;
        public int animationLayer = 0;
        public AnimationState modeAnimation;
        public List<ResourceRatio> inputs;
        public List<ResourceRatio> outputs;

        public FusionReactorMode()
        {
        }
        /// <summary>
        /// Construct from confignode
        /// </summary>
        /// <param name="node"></param>
        ///
        public FusionReactorMode(ConfigNode node)
        {
            OnLoad(node);
        }

        public void OnLoad(ConfigNode node)
        {
            // Process nodes
            node.TryGetValue("DisplayName", ref modeName);
            node.TryGetValue("ModeID", ref modeID);
            node.TryGetValue("AnimationName", ref animationName);
            node.TryGetValue("AnimationLayer", ref animationLayer);

            ConfigNode[] inNodes = node.GetNodes("INPUT_RESOURCE");
            ConfigNode[] outNodes = node.GetNodes("OUTPUT_RESOURCE");

            inputs = new List<ResourceRatio>();
            outputs = new List<ResourceRatio>();
            for (int i = 0; i < inNodes.Length; i++)
            {
                ResourceRatio p = new ResourceRatio();
                p.Load(inNodes[i]);
                inputs.Add(p);
            }
            for (int i = 0; i < outNodes.Length; i++)
            {
                ResourceRatio p = new ResourceRatio();
                p.Load(outNodes[i]);
                outputs.Add(p);
            }
        }
        /// <summary>
        /// Gets the current reactor output
        /// </summary>
        /// <returns></returns>
        public double GetOutput()
        {
            for (int i = 0; i < outputs.Count; i++)
            {
                if (outputs[i].ResourceName == "ElectricCharge")
                    return outputs[i].Ratio;
            }
            return 0d;
        }


        public void InitializeAnimations(Part host)
        {
            if (animationName != "")
                modeAnimation = Utils.SetUpAnimation(animationName, host, animationLayer)[0];

            // TODO: Localize
            modeName = Localizer.Format(modeName);
        }
        public void Activate()
        {
            if (modeAnimation)
                if (modeAnimation.normalizedTime < 0f) modeAnimation.normalizedTime = 0f;
                modeAnimation.speed = 1f;
        }
        public void Deactivate()
        {
            if (modeAnimation)
            {
                if (modeAnimation.normalizedTime > 1f) modeAnimation.normalizedTime = 1f;
                modeAnimation.speed = -1f;
            }
        }
    }
}
