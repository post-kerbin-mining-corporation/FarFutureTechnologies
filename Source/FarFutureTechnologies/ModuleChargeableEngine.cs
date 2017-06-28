using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace FarFutureTechnologies
{
    public class  ModuleChargeableEngine: PartModule
    {
        // Power used to charge the engine
        [KSPField(isPersistant = false)]
        public float ChargeRate = 150f;

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



        // UI FIELDS/ BUTTONS
        // Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Capacitors")]
        public string ChargeStatus = "N/A";

        [KSPField(isPersistant = false, guiActive = true, guiName = "Recharge")]
        public string RechargeStatus = "N/A";

        [KSPEvent(guiActive = true, guiName = "Enable Engine Charging", active = true)]
        public void Enable()
        {
            Charging = true;
        }
        [KSPEvent(guiActive = false, guiName = "Disable Engine Charging", active = false)]
        public void Disable()
        {
            Charging = false;
        }

        // ACTIONS
        [KSPAction("Enable Engine Charging")]
        public void EnableAction(KSPActionParam param) { Enable(); }

        [KSPAction("Disable Engine Charging")]
        public void DisableAction(KSPActionParam param) { Disable(); }

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
          string msg = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_PartInfo", ChargeRate.ToString("F2"), (ChargeGoal/ChargeRate).ToString("F0"));
          return msg;
        }

        private ModuleEnginesFX engine;

        void Start()
        {
            if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
            {
                engine = this.GetComponent<ModuleEnginesFX>();
                Fields["RechargeStatus"].guiName = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_RechargeStatus_Title");
                Fields["ChargeStatus"].guiName = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_ChargeStatus_Title");

                Events["Enable"].guiName = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Event_Enable_Title");
                Events["Disable"].guiName = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Event_Disable_Title");

                Actions["EnableAction"].guiName = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Action_EnableAction_Title");
                Actions["DisableAction"].guiName = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Action_DisableAction_Title");
                Actions["ToggleAction"].guiName = Localizer.Format("#LOC_ModuleChargeableEngine_Action_ToggleAction_Title");
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
            if (engine.EngineIgnited)
            {
                if (engine.requestedThrottle <= 0.01)
                {
                    OnShutdown();
                }
            }

          }
        }

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

                if (Events["Enable"].active == Charging || Events["Disable"].active != Charging)
                {
                    Events["Disable"].active = Charging;
                    Events["Enable"].active = !Charging;
               }
            
            }

        }


        void DoRecharge()
        {
          double req = part.RequestResource("ElectricCharge", ChargeRate * TimeWarp.fixedDeltaTime);
          CurrentCharge = Mathf.MoveTowards(CurrentCharge, ChargeGoal, (float)req);

          if (req > 0.0d)
            RechargeStatus = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_RechargeStatus_Charging", ChargeRate.ToString("F2"));
          else
            RechargeStatus = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_RechargeStatus_NoPower");

          ChargeStatus = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_ChargeStatus_Normal", (CurrentCharge/ChargeGoal * 100.0f).ToString("F1"));
          if (CurrentCharge >= ChargeGoal)
          {
            RechargeStatus = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_RechargeStatus_Ready");
            Charged = true;
            SetEngineUI(true);
          }
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
        }

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
