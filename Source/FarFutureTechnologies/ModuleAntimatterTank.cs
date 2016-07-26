using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace FarFutureTechnologies
{
    public class ModuleAntimatterTank: PartModule
    {
        // Name of the fuel to boil off
        [KSPField(isPersistant = false)]
        public string FuelName;

        // Cost to contain in u/s
        [KSPField(isPersistant = false)]
        public float ContainmentCost = 0.0f;

        // KJ of energy released per unit of AM detonated
        // canoncially 1 ug of AM is 36000 kJ, 1 ug = 1 unit
        [KSPField(isPersistant = false)]
        public float DetonationKJPerUnit = 36000f;

        // Rate of AM detonation in u/s
        [KSPField(isPersistant = false)]
        public float DetonationRate = 5f;

        // Whether tank containment is enabled
        [KSPField(isPersistant = true)]
        public bool ContainmentEnabled = true;

        // PRIVATE
        private double fuelAmount = 0.0;
        private double containmentCost = 0.0;
        private double temperatureFo = 0.0;

        // UI FIELDS/ BUTTONS
        // Status string
        [KSPField(isPersistant = false, guiActive = true, guiName = "Containment")]
        public string ContainmentStatus = "N/A";


        [KSPEvent(guiActive = false, guiName = "Enable Containment", active = true)]
        public void Enable()
        {
            ContainmentEnabled= true;
        }
        [KSPEvent(guiActive = false, guiName = "Disable Containment", active = false)]
        public void Disable()
        {
            ContainmentEnabled= false;
        }

        // ACTIONS
        [KSPAction("Enable Containment")]
        public void EnableAction(KSPActionParam param) { Enable(); }

        [KSPAction("Disable Containment")]
        public void DisableAction(KSPActionParam param) { Disable(); }

        [KSPAction("Toggle Containment")]
        public void ToggleAction(KSPActionParam param)
        {
            ContainmentEnabled = !ContainmentEnabled;
        }

        public override string GetInfo()
        {
          string msg = String.Format("Containment Cost: {0:F2} Ec/s", ContainmentCost);
          return msg;
        }

        public override void OnStart(PartModule.StartState state)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
              fuelAmount = GetResourceAmount(FuelName);

              // Catchup
              DoCatchup();
            }
        }

        public void DoCatchup()
        {
          if (part.vessel.missionTime > 0.0)
          {
              if (part.RequestResource("ElectricCharge", coolingCost * TimeWarp.fixedDeltaTime) < coolingCost * TimeWarp.fixedDeltaTime)
              {
                  double elapsedTime = part.vessel.missionTime - LastUpdateTime;

                  double toBoil = Math.Pow(1.0 - boiloffRateSeconds, elapsedTime);
                  part.RequestResource(FuelName, (1.0 - toBoil) * fuelAmount,ResourceFlowMode.NO_FLOW);
              }
          }
        }

        public void Update()
        {
          if (HighLogic.LoadedSceneIsFlight)
          {

            // Show the insulation status field if there is a cooling cost
            if (CoolingCost > 0f)
            {
              foreach (BaseField fld in base.Fields)
                {
                    if (fld.guiName == "Insulation")
                        fld.guiActive = true;
                }

              if (Events["Enable"].active == ContainmentEnabled || Events["Disable"].active != ContainmentEnabled)
                {
                    Events["Disable"].active = ContainmentEnabled;
                    Events["Enable"].active = !ContainmentEnabled;
               }
            }
          }
        }
        protected void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                fuelAmount = GetResourceAmount(FuelName);
                // If we have no fuel, no need to do any calculations
                if (fuelAmount == 0.0)
                {
                  ContainmentStatus = "No Antimatter";
                  return;
                }


                if (CoolingEnabled)
                {
                  double req = part.RequestResource("ElectricCharge", ContainmentCost * TimeWarp.fixedDeltaTime);
                  if (req > ContainmentCost * TimeWarp.fixedDeltaTime)
                  {
                      ContainmentStatus = String.Format("Running");
                  } else
                  {
                      DoDetonation();
                      ContainmentStatus = String.Format("Leaking {0} u/s", TimeWarp.fixedDeltaTime* DetonationRate);

                  }
                }
                else
                {
                    DoDetonation();
                    ContainmentStatus = String.Format("Leaking {0} u/s", TimeWarp.fixedDeltaTime* DetonationRate);

                }

                if (part.vessel.missionTime > 0.0)
                {
                    LastUpdateTime = part.vessel.missionTime;
                }
            }
        }
        protected void DoDetonation()
        {
          double detonatedAmount = part.RequestResource(FuelName, TimeWarp.fixedDeltaTime* DetonationRate);
          part.AddHeatFlux(detonatedAmount*DetonationKJPerUnit);
        }



        protected double GetResourceAmount(string nm)
       {
           PartResource res = this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id);
           if (res)
               return res.amount;
           else
               return 0d;
       }

    }
}
