using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace FarFutureTechnologies
{
    public class ModuleMultiRatioEngine : PartModule
    {

        [KSPField(isPersistant = true, guiActive = true, guiName = "Mixing Ratio"), UI_FloatRange(minValue = 0.01f, maxValue = 3.5f, stepIncrement = 0.05f)]
        public float MixingRatio = 2.5f;

        [KSPField(isPersistant = false)]
        public float RatioScale = 32;

        [KSPField(isPersistant = false)]
        public string PropellantName = "LqdHydrogen";

        // Related mixing ratio to heat
        [KSPField(isPersistant = false)]
        public FloatCurve HeatCurve = new FloatCurve();

        // Relates mixing ratio to thrust
        [KSPField(isPersistant = false)]
        public FloatCurve ThrustCurve = new FloatCurve();

        // Relates mixing ratio to Isp
        [KSPField(isPersistant = false)]
        public FloatCurve IspCurve = new FloatCurve();

        // Relates mixing ratio to Isp
        [KSPField(isPersistant = false)]
        public FloatCurve IspCurveScale = new FloatCurve();

        private ModuleEnginesFX engine;
        private Propellant propellant;

        // VAB UI
        public string GetModuleTitle()
        {
            return "AdjustableRatio";
        }
        public override string GetModuleDisplayName()
        {
            return Localizer.Format("#LOC_FFT_ModuleMultiRatioEngine_ModuleName");
        }

        public override string GetInfo()
        {
            string msg = Localizer.Format("#LOC_FFT_ModuleMultiRatioEngine_PartInfo",
              (IspCurve.Evaluate(0f) * IspCurveScale.Evaluate(IspCurve.maxTime * RatioScale)).ToString("F2"),
              (IspCurve.Evaluate(0f) * IspCurveScale.Evaluate(IspCurve.minTime * RatioScale)).ToString("F2"),
              ThrustCurve.Evaluate(IspCurve.maxTime * RatioScale).ToString("F2"),
              ThrustCurve.Evaluate(IspCurve.minTime * RatioScale).ToString("F2"));
            return msg;
        }

        void Start()
        {
            if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
            {
                SetupUI();
                SetupEngine();

            }
            if (HighLogic.LoadedSceneIsFlight)
            {

            }
        }
        void FixedUpdate()
        {
            if ((HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor) && engine != null)
            {
                HandleMixingRatioChange();
            }
        }
        void SetupEngine()
        {
            engine = this.GetComponent<ModuleEnginesFX>();
            if (engine != null)
            {
                foreach (Propellant prop in engine.propellants)
                {
                    if (prop.name == PropellantName)
                        propellant = prop;
                }
            }
        }
        void SetupUI()
        {
            Fields["MixingRatio"].guiName = Localizer.Format("#LOC_FFT_ModuleMultiRatioEngine_Field_MixingRatio_Title");
        }
        void HandleMixingRatioChange()
        {
            float thrust = ThrustCurve.Evaluate(MixingRatio * RatioScale);
            float heat = HeatCurve.Evaluate(MixingRatio * RatioScale);
            float ispScale = IspCurveScale.Evaluate(MixingRatio * RatioScale);
            FloatCurve atmosphereCurve = new FloatCurve();
            atmosphereCurve.Add(0f, IspCurve.Evaluate(0f) * ispScale);
            atmosphereCurve.Add(1f, IspCurve.Evaluate(1f) * ispScale);
            atmosphereCurve.Add(4f, IspCurve.Evaluate(4f) * ispScale);

            propellant.ratio = MixingRatio * RatioScale;
            engine.atmosphereCurve = atmosphereCurve;
            engine.maxThrust = thrust;
            engine.heatProduction = heat;
        }

    }
}