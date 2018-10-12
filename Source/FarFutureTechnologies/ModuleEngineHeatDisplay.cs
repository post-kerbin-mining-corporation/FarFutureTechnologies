using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace FarFutureTechnologies
{
    public class ModuleEngineHeatDisplay : PartModule
    {

        [KSPField(isPersistant = false, guiActive = true, guiName = "Heat Production")]
        public string HeatProductionStatus = "N/A";

        private ModuleEnginesFX[] engines;


        public string GetModuleTitle()
        {
            return "HeatProduction";
        }
        public override string GetModuleDisplayName()
        {
            return Localizer.Format("#LOC_FFT_ModuleEngineHeatDisplay_ModuleName");
        }

        public override string GetInfo()
        {
            string msg = "";
            engines = part.GetComponents<ModuleEnginesFX>();
            foreach (ModuleEnginesFX engine in engines)
            {
                msg += Localizer.Format("#LOC_FFT_ModuleEngineHeatDisplay_PartInfo", engine.engineID,
                  (engine.heatProduction * 800.0 * 0.025 * 0.4975 * part.mass / 1000.0 * 2.0).ToString("F1"));
            }
            return msg;
        }

        public void Start()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                Fields["HeatProductionStatus"].guiName = Localizer.Format("#LOC_FFT_ModuleEngineHeatDisplay_Field_HeatProductionStatus_Title");
                engines = part.GetComponents<ModuleEnginesFX>();
            }
        }

        protected void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                HeatProductionStatus = Localizer.Format("#LOC_FFT_ModuleEngineHeatDisplay_Field_HeatProductionStatus_None");

                foreach (ModuleEnginesFX engine in engines)
                {
                    if (engine.EngineIgnited)
                    {
                        HeatProductionStatus = Localizer.Format("#LOC_FFT_ModuleEngineHeatDisplay_Field_HeatProductionStatus_Normal",
                          (engine.heatProduction * 800.0 * 0.025 * 0.4975 * part.mass / 1000.0 * engine.GetCurrentThrust() / engine.GetMaxThrust() * 2.0).ToString("F1"));
                    }
                }
            }
        }

    }
}
