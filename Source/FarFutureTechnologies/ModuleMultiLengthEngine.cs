using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace FarFutureTechnologies
{
    public class ModuleMultiLengthEngine: PartModule
    {
      public string SelectedConfig = "Size";

      private PartModule b9PartModule;
      private ModuleEnginesFX engine;
      private List<LengthConfiguration> lengthConfigs;

      public class LengthConfiguration
      {
        public string subtypeName;
        public FloatCurve atmosphereCurve;
        public float minThrust ;
        public float maxThrust ;
        public float heatProduction;

        public List<Propellant> propellants;

        public LengthConfiguration(ConfigNode node)
        {
          ConfigNode[] varNodes = node.GetNodes("PROPELLANT");
          propellants = new List<Propellant>();
          for (int i=0; i < varNodes.Length; i++)
          {
            Propellant p = new Propellant();
            p.OnLoad(varNodes[i]);
            propellants.Add(p);
          }
        }

      }
      private void Start()
      {
        if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
        {
          SetupB9();
          SetupEngines();
          if (b9PartModule != null && engine != null)
          {
            AssignParameters(SelectedConfig);
          }
        }
      }
      private void FixedUpdate()
      {
        if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
        {
          if (b9PartModule != null && engine != null)
          {
            if (b9PartModule.Fields.GetValue("currentSubtypeName").ToString() != currentSubtypeName)
            {
                SelectLengthConfig(b9PartModule.Fields.GetValue("currentSubtypeName").ToString());
            }
          }
        }
      }
      public override void OnLoad(ConfigNode node)
      {
          base.OnLoad(node);

          ConfigNode[] varNodes = node.GetNodes("LENGTHCONFIGURATION");
          lengthConfigs = new List<LengthConfiguration>();
          for (int i=0; i < varNodes.Length; i++)
          {
            lengthConfigs.Add(new LengthConfiguration(varNodes[i]));
          }
      }

      private void SetupEngines()
      {
        engine = this.GetComponent<ModuleEnginesFX();
        if (engine == null)
        {
          Utils.LogWarning(String.Format("[ModuleMultiLengthEngine]: Could not find engine Module"));
        }
      }

      private void SetupB9()
      {
        for (int j = part.Modules.Count - 1; j >= 0; --j)
        {
            PartModule pm = part.Modules[j];
            if (pm.moduleName = "ModuleB9PartSwitch")
                b9PartModule = pm;
        }
        if (b9PartModule == null)
        {
          Utils.LogWarning(String.Format("[ModuleMultiLengthEngine]: Could not find B9PartSwitch Module"));
        }

      }

      private void SelectLengthConfig(string configName)
      {
        for (int i = 0 ; i < lengthConfigs.Count;i++)
        {
          if (configName == lengthConfigs[i].subtypeName)
          {
            AssignParameters(lengthConfigs[i]);
            SelectedConfig = configName;
          }
        }
      }
      private void AssignParameters(LengthConfiguration config)
      {
        engine.atmosphereCurve = config.atmosphereCurve;
        engine.maxThrust = config.maxThrust;
        engine.minThrust = config.minThrust;
        engine.heatProduction = config.heatProduction;

        for (int i =0; i< engine.propellants.Count; i++)
        {
          for (int j = 0; j < config.propellants.Count; j++)
          {
            if (engine.propellants[i].name == config.propellants[j])
            {
              engine.propellants[i].ratio = config.propellants[j].ratio;
            }
          }
        }
      }

    }
}
