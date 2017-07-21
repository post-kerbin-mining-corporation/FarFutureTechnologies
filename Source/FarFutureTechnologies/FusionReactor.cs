/// FissionReactor
/// ---------------------------------------------------
/// Fission Generator!
///

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI;
using KSP.Localization;

namespace FarFutureTechnologies
{
    public class FusionReactor: ModuleResourceConverter
    {
        public struct ResourceBaseRatio
        {
            public string ResourceName;
            public double ResourceRatio;

            public ResourceBaseRatio(string name, double ratio)
            {
                ResourceName = name;
                ResourceRatio = ratio;
            }
         }

        /// CONFIGURABLE FIELDS
        // ----------------------

        // --- Charge-up -----
        [KSPField(isPersistant = true)]
        public bool Charged = false;

        [KSPField(isPersistant = true)]
        public bool Charging = false;

        [KSPField(isPersistant = true)]
        public float ChargeAmount = 0f;

        [KSPField(isPersistant = false)]
        public float ChargeRate = 500000f;

        [KSPField(isPersistant = false)]
        public float ChargeGoal = 500000f;

        // --- Power Generation -----
        // Heat generation at full power
        [KSPField(isPersistant = false)]
        public float PowerGeneration = 1000f;

        // --- Fuel Use -----
        // Heat generation at full power
        [KSPField(isPersistant = false)]
        public float PassiveFuelUseScale = 0.001f;

        // --- Heat -----
        // Temperature for auto-shutdown
        [KSPField(isPersistant = true, guiActive = true, guiName = "Auto-Shutdown Temp"), UI_FloatRange(minValue = 700f, maxValue = 6000f, stepIncrement = 100f)]
        public float CurrentSafetyOverride = 1000f;

        // Heat generation at full power
        [KSPField(isPersistant = false)]
        public float HeatGeneration;

        // Nominal reactor temperature (where the reactor should live)
        [KSPField(isPersistant = false)]
        public float NominalTemperature = 900f;

        // Critical reactor temperature (core damage after this)
        [KSPField(isPersistant = false)]
        public float CriticalTemperature = 1400f;

        // Critical reactor temperature (kaboom at this)
        [KSPField(isPersistant = false)]
        public float MaximumTemperature = 2000f;

        // name of the overheat animation
        [KSPField(isPersistant = false)]
        public string OverheatAnimation;

        [KSPField(isPersistant = false)]
        public int smoothingInterval = 25;


        // REPAIR VARIABLES
        // integrity of the core
        [KSPField(isPersistant = true)]
        public float CoreIntegrity = 100f;

        // Rate the core is damaged, in % per S per K
        [KSPField(isPersistant = false)]
        public float CoreDamageRate = 0.005f;

        // Engineer level to repair the core
        [KSPField(isPersistant = false)]
        public int EngineerLevelForRepair = 5;

        [KSPField(isPersistant = false)]
        public float MaxRepairPercent = 75;

        [KSPField(isPersistant = false)]
        public float MinRepairPercent = 10;

        [KSPField(isPersistant = false)]
        public float MaxTempForRepair = 325;

        [KSPField(isPersistant = true)]
        public bool FirstLoad = true;

        /// UI ACTIONS
        /// --------------------
        [KSPAction("Enable Startup Charging")]
        public void EnableAction(KSPActionParam param) { EnableCharging(); }

        [KSPAction("Disable Startup Charging")]
        public void DisableAction(KSPActionParam param) { DisableCharging(); }

        [KSPAction("Toggle Startup Charging")]
        public void ToggleAction(KSPActionParam param)
        {
            Charging = !Charging;
        }

        [KSPEvent(guiActive = true, guiActiveEditor= true, guiName = "Enable Startup Charging", active = true)]
        public void EnableCharging()
        {
            Charging = true;
        }
        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Disable Startup Charging", active = false)]
        public void DisableCharging()
        {
            Charging = false;
        }
        // Try to fix the reactor
        [KSPEvent(externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 3.5f, guiName = "Repair Reactor")]
        public void RepairReactor()
        {
            if (TryRepairReactor())
            {
              DoReactorRepair();
            }
        }

        // ACCESSORS
        /// ---------------
        public ModuleCoreHeat Core{ get {return core;}}
        public bool ReactorEnabled { get {return base.ModuleIsActive();}}

        /// PRIVATE VARIABLES
        /// ----------------------
        private ModuleCoreHeat core;
        private AnimationState[] overheatStates;


        // base paramters
        private List<ResourceBaseRatio> inputs;
        private List<ResourceBaseRatio> outputs;

        /// UI FIELDS
        /// --------------------
        // Reactor Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Reactor Power")]
        public string ReactorOutput;

        // Reactor Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Available Power")]
        public string ThermalTransfer;

        // integrity of the core
        [KSPField(isPersistant = false, guiActive = true, guiName = "Core Temperature")]
        public string CoreTemp;

        // integrity of the core
        [KSPField(isPersistant = false, guiActive = true, guiName = "Core Health")]
        public string CoreStatus;

        public override string GetInfo()
        {
            double baseRate = 0d;
            for (int i = 0 ;i < inputList.Count;i++)
            {
                if (inputList[i].ResourceName == FuelName)
                    baseRate = inputList[i].Ratio;
            }
            return
                Localizer.Format("#LOC_FFT_ModuleFusionReactor_PartInfo")
                + base.GetInfo();
        }
        public string GetModuleTitle()
        {
            return "FusionReactor";
        }
        public override string GetModuleDisplayName()
        {
            return Localizer.Format("#LOC_FFT_ModuleFusionReactor_ModuleName");
        }

        private void SetupResourceRatios()
        {

            inputs = new List<ResourceBaseRatio>();
            outputs = new List<ResourceBaseRatio>();

            for (int i = 0 ;i < inputList.Count;i++)
            {
                inputs.Add(new ResourceBaseRatio(inputList[i].ResourceName, inputList[i].Ratio));
            }
            for (int i = 0 ;i < outputList.Count;i++)
            {
                outputs.Add(new ResourceBaseRatio(outputList[i].ResourceName, outputList[i].Ratio));
            }
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            SetupLocalization();
            SetupSafetyOverride();


          if (HighLogic.LoadedScene != GameScenes.EDITOR)
          {
              core = this.GetComponent<ModuleCoreHeat>();
              if (core == null)
                  Utils.LogError("[FusionReactor]: Could not find core heat module!");

              SetupResourceRatios();

              if (OverheatAnimation != "")
                  overheatStates = Utils.SetUpAnimation(OverheatAnimation, this.part);
          } else
          {
              //this.CurrentSafetyOverride = this.NominalTemperature;
          }
        }

        // Sets up the localization
        private void SetupLocalization()
        {
          //Events["RepairReactor"].guiName = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Event_RepairReactor");
          //Fields["CurrentSafetyOverride"].guiName = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_CurrentSafetyOverride");
          //Fields["CurrentPowerPercent"].guiName = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_CurrentPowerPercent");
          //Fields["ReactorOutput"].guiName = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_ReactorOutput");
          //Fields["ThermalTransfer"].guiName = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_ThermalTransfer");
          //Fields["CoreTemp"].guiName = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_CoreTemp");
          //Fields["CoreStatus"].guiName = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_CoreStatus");
          //Fields["FuelStatus"].guiName = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_FuelStatus");
        }
        // Sets up the safery overrides
        private void SetupSafetyOverride()
        {
          var range = (UI_FloatRange)this.Fields["CurrentSafetyOverride"].uiControlEditor;
          range.minValue = 0f;
          range.maxValue = MaximumTemperature;

          range = (UI_FloatRange)this.Fields["CurrentSafetyOverride"].uiControlFlight;
          range.minValue = 0f;
          range.maxValue = MaximumTemperature;


          if (FirstLoad)
          {
            this.CurrentSafetyOverride = this.CriticalTemperature;
            FirstLoad = false;
          }
        }

        public virtual void OnUpdate()
        {
          if (HighLogic.LoadedScene == GameScenes.FLIGHT)
          {
              Fields["status"].guiActive = false;




              // Adjust the safety override
              if (core != null)
              {
                  core.CoreShutdownTemp = (double)CurrentSafetyOverride+10d;
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
        public virtual void OnFixedUpdate()
        {
          if (HighLogic.LoadedScene == GameScenes.FLIGHT)
          {
              // Update reactor core integrity readout
              if (CoreIntegrity > 0)
                  CoreStatus = String.Format("{0:F2} %", CoreIntegrity);
              else
                  CoreStatus = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_CoreStatus_Meltdown");

              // Handle core damage tracking and effects
              HandleCoreDamage();
              // Heat consumption occurs if reactor is on or off
              DoHeatConsumption();

              // IF REACTOR ON
              // =============
              if (ReactorEnabled)
              {
                DoFuelConsumption();
                DoHeatGeneration();
              }
              // IF REACTOR OFF
              // =============
              else
              {
                  // Update UI
                  if (CoreIntegrity <= 0f)
                  {
                      FuelStatus = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_FuelStatus_Meltdown");
                      ReactorOutput = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_ReactorOutput_Meltdown");
                  }
                  else
                  {
                      FuelStatus = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_FuelStatus_Offline");
                      ReactorOutput = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_ReactorOutput_Offline");

                  }
              }
          }
        }

        // Gets the current reactor throttle
        private float GetReactorThrottle()
        {
          // lastTimeFactor is a factor that, multiplied by the config ratio parameter, gives the current (well, last frame) production
          if (ReactorEnabled)
            return base.lastTimeFactor;
            else
          return 0d;
        }

        private void DoFuelConsumption()
        {
          if (ReactorEnabled && GetReactorThrottle() <= 0.01)
          {
            for (int i = 0; i < inputs.Count;i++)
            {
                this.part.RequestResource(inputs[i].ResourceName, TimeWarp.fixedDeltaTime*PassiveFuelUseScale);
            }
          }
        }

        // Creates heat from the reaction
        private void DoHeatGeneration()
        {
            // Generate heat from the reaction and apply it
            SetHeatGeneration((GetReactorThrottle() * HeatGeneration)* CoreIntegrity/100f);

            if (CoreIntegrity <= 0f)
            {
                FuelStatus = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_FuelStatus_Meltdown");
                ReactorOutput = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_ReactorOutput_Meltdown");
            }
            else
            {
                ReactorOutput = String.Format("{0:F1} kW", GetReactorThrottle() * HeatGeneration / 50f * CoreIntegrity / 100f);
            }
        }

        // Does the transformation of heat into power
        private void DoHeatConsumption()
        {

            // save some divisions later
            float coreIntegrity = CoreIntegrity / 100f;
            float reactorThrottle = GetReactorThrottle();

            float productionScalingFactor = reactorThrottle * coreIntegrity;

            RecalculateRatios(productionScalingFactor);

            // GUI
            ThermalTransfer = String.Format("{0:F1} kW", AvailablePower);
            CoreTemp = String.Format("{0:F1}/{1:F1} K", (float)core.CoreTemperature, NominalTemperature);
        }

        // Set the reactor's heat generation properties
        private void SetHeatGeneration(float heat)
        {
            if (Time.timeSinceLevelLoad > 5f)
                GeneratesHeat = true;
            else
                GeneratesHeat = false;

            TemperatureModifier = new FloatCurve();
            TemperatureModifier.Add(0f, heat;

            core.MaxCoolant = heat;
        }

        // track and set core damage
        private void HandleCoreDamage()
        {
          // Update reactor damage
          float critExceedance = (float)core.CoreTemperature - CriticalTemperature;

          // If overheated too much, damage the core
          if (critExceedance > 0f && TimeWarp.CurrentRate < 100f)
          {
              // core is damaged by Rate * temp exceedance * time
              CoreIntegrity = Mathf.MoveTowards(CoreIntegrity, 0f, CoreDamageRate * critExceedance * TimeWarp.fixedDeltaTime);
          }

          // Calculate percent exceedance of nominal temp
          float tempNetScale = 1f - Mathf.Clamp01((float)((core.CoreTemperature - NominalTemperature) / (MaximumTemperature - NominalTemperature)));

          if (OverheatAnimation != "")
          {
            for (int i = 0;i<overheatStates.Length;i++)
              {
                  overheatStates[i].normalizedTime = 1f - tempNetScale;
              }
          }
        }

        // Set ModuleResourceConverter ratios based on an input scale
        private void RecalculateRatios(float fuelInputScale)
        {

            for (int i = 0; i < _recipe.Inputs.Count; i++)
            {
                for (int j = 0; j < inputs.Count; j++)
                {
                    if (inputs[j].ResourceName == inputList[i].ResourceName)
                    {
                        _recipe.Inputs[i] = new ResourceRatio(inputList[i].ResourceName, inputs[j].ResourceRatio * fuelInputScale, inputList[i].DumpExcess);
                    }
                }
            }
            for (int i = 0; i < _recipe.Outputs.Count; i++)
            {
                for (int j = 0; j < outputs.Count; j++)
                {
                    if (outputs[j].ResourceName == outputList[i].ResourceName)
                    {
                        //Debug.Log("OUT: edited " + outputList[i].ResourceName + " ratio to " + (outputs[j].ResourceRatio * fuelInputScale).ToString());
                        _recipe.Outputs[i] = new ResourceRatio(outputList[i].ResourceName, inputs[j].ResourceRatio * fuelInputScale, outputList[i].DumpExcess);
                    }
                }
            }
            for (int i = 0; i < inputList.Count; i++)
            {
                //Debug.Log("IN: edited " + inputList[i].ResourceName + " ratio to " + (inputList[i].Ratio).ToString());
            }
        }



        // ####################################
        // Repairing
        // ####################################

        public bool TryRepairReactor()
        {
          if (CoreIntegrity <= MinRepairPercent)
          {
              ScreenMessages.PostScreenMessage(new ScreenMessage(
                Localizer.Format("#LOC_FFT_ModuleFusionReactor_Message_Repair_CoreTooDamaged"),
                5.0f, ScreenMessageStyle.UPPER_CENTER));
              return false;
          }
          if (!CheckEVAEngineerLevel(EngineerLevelForRepair))
          {
              ScreenMessages.PostScreenMessage(new ScreenMessage(
              Localizer.Format("#LOC_FFT_ModuleFusionReactor_Message_Repair_CoreTooDamaged",EngineerLevelForRepair.ToString("F0")),
                  5.0f, ScreenMessageStyle.UPPER_CENTER));
              return false;
          }
          if (base.ModuleIsActive())
          {
              ScreenMessages.PostScreenMessage(new ScreenMessage(
                  Localizer.Format("#LOC_FFT_ModuleFusionReactor_Message_Repair_NotWhileRunning"),
                  5.0f, ScreenMessageStyle.UPPER_CENTER));
              return false;
          }
          if (core.CoreTemperature > MaxTempForRepair)
          {
              ScreenMessages.PostScreenMessage(new ScreenMessage(
                Localizer.Format("#LOC_FFT_ModuleFusionReactor_Message_Repair_CoreTooHot", MaxTempForRepair.ToString("F0")),
                5.0f, ScreenMessageStyle.UPPER_CENTER));
              return false;
          }
          if (CoreIntegrity >= MaxRepairPercent)
          {
              ScreenMessages.PostScreenMessage(new ScreenMessage(
                Localizer.Format("#LOC_FFT_ModuleFusionReactor_Message_Repair_CoreAlreadyRepaired", MaxRepairPercent.ToString("F0")),
                  5.0f, ScreenMessageStyle.UPPER_CENTER));
              return false;
          }
          return true;
        }

        // Repair the reactor to max Repair percent
        public void DoReactorRepair()
        {
            this.CoreIntegrity = MaxRepairPercent;
            ScreenMessages.PostScreenMessage(new ScreenMessage(
              Localizer.Format("#LOC_FFT_ModuleFusionReactor_Message_Repair_RepairSuccess", MaxRepairPercent.ToString("F0")),
              5.0f, ScreenMessageStyle.UPPER_CENTER));
        }

        // Check the current EVA engineer's level
        private bool CheckEVAEngineerLevel(int level)
        {
            ProtoCrewMember kerbal = FlightGlobals.ActiveVessel.GetVesselCrew()[0];
            if (kerbal.experienceTrait.Title == "Engineer" && kerbal.experienceLevel >= level)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public float GetCoreTemperature()
        {
          return (float)core.CoreTemperature;
        }

        // ####################################
        // Refuelling
        // ####################################

        // Finds time remaining at specified fuel burn rates
        public string FindTimeRemaining(double amount, double rate)
        {
            if (rate < 0.0000001)
            {
                return Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_FuelStatus_VeryLong");
            }
            double remaining = amount / rate;
            //TimeSpan t = TimeSpan.FromSeconds(remaining);

            if (remaining >= 0)
            {
                return Utils.FormatTimeString(remaining);
            }
            {
                return Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_FuelStatus_Exhausted");
            }
        }
    }
}
