using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace FarFutureTechnologies
{
    public class ModuleEngineHeatDisplay: PartModule
    {

        [KSPField(isPersistant = false, guiActive = true, guiName = "Engine Heat Production")]
        public string HeatProductionStatus = "N/A";

        private List<ModuleEnginesFX> engines = new List<ModuleEnginesFX>();

        public override string GetInfo()
        {
          string msg = "";
          engines = part.GetComponents<ModuleEnginesFX>();
          for (ModuleEnginesFX engine in engines)
          {
            msg += String.Format("<b>{0}</b>\n", engine.engineID);
            msg+= String.Format("Heat production (full throttle): {0:F2} kW\n\n", engine.heatProduction*800.0*0.025*0.4975);
          }
          return msg;
        }

        public void Start()
        {
          if (HighLogic.LoadedSceneIsFlight)
          {
              engines = part.GetComponents<ModuleEnginesFX>();
          }
        }

        protected void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
              HeatProductionStatus = "No engines active";
              for (ModuleEnginesFX engine in engines)
              {
                if (engine.EngineIgnited)
                  HeatProductionStatus = String.Format("Heat production: {0:F2} kW", engine.heatProduction*800.0*0.025*0.4975 * engine.GetCurrentThrust()/ engine.GetMaxThrust() );
              }
            }
        }

    }
}
