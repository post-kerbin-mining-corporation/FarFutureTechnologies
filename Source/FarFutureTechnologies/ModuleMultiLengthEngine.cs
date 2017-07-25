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
        [KSPField(isPersistant = true)]
      public int SelectedConfigIndex = 0;
        [KSPField(isPersistant = false)]
      public string NozzleTransform;

      private Transform nozzle;
      private PartModule b9PartModule;
      private ModuleEnginesFX engine;
      private List<LengthConfiguration> lengthConfigs;

      // Represents a length configuration
      [System.Serializable]
      public class LengthConfiguration
      {
        public string subtypeName;
        public FloatCurve atmosphereCurve;
        public float minThrust ;
        public float maxThrust ;
        public float heatProduction;
        public Vector3 localPosition;

        public List<Propellant> propellants;

        public LengthConfiguration(ConfigNode node)
        {
            node.TryGetValue("subtypeName", ref subtypeName);
            node.TryGetValue("minThrust", ref minThrust);
            node.TryGetValue("maxThrust", ref maxThrust);
            node.TryGetValue("heatProduction", ref heatProduction );

            string str = "";
            node.TryGetValue("NozzlePosition", ref str);
            string[] strSplit = str.Split(","[0]);
            localPosition = new Vector3(float.Parse(strSplit[0]),
                float.Parse(strSplit[1]),
                float.Parse(strSplit[2]));

            atmosphereCurve = Utils.GetValue(node, "atmosphereCurve", new FloatCurve());
          ConfigNode[] varNodes = node.GetNodes("PROPELLANT");
          propellants = new List<Propellant>();
          for (int i=0; i < varNodes.Length; i++)
          {
            Propellant p = new Propellant();
            p.Load(varNodes[i]);
            propellants.Add(p);
          }
        }

      }

      public string GetModuleTitle()
      {
          return "Reaction Chamber";
      }
      public override string GetModuleDisplayName()
      {
          return Localizer.Format("#LOC_FFT_ModuleMultiLengthEngine_ModuleName");
      }

      public override string GetInfo()
      {

        string msg = Localizer.Format("#LOC_FFT_ModuleMultiLengthEngine_PartInfo"); ;
        return msg;
      }

      public void  Start()
      {

        if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
        {
            if (lengthConfigs == null || lengthConfigs.Count == 0)
            {
                ConfigNode node = GameDatabase.Instance.GetConfigs("PART").
                    Single(c => part.partInfo.name == c.name).config.
                    GetNodes("MODULE").Single(n => n.GetValue("name") == moduleName);
                Utils.Log(node.ToString());
                OnLoad(node);
            }

          SetupTransform();
          SetupB9();
          SetupEngines();

          if (b9PartModule != null && engine != null)
          {
            SelectLengthConfig(SelectedConfigIndex);
          }
        }
      }

      private void FixedUpdate()
      {
        if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
        {
          if (b9PartModule != null && engine != null)
          {
              // Monitor for a change in subtype
              int result = (int)b9PartModule.Fields.GetValue("currentSubtypeIndex");
              if (result != SelectedConfigIndex)
              {
                  SelectLengthConfig(result);
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

      private void SetupTransform()
      {
          Utils.LogWarning(String.Format("{0}", NozzleTransform));
          nozzle = part.FindModelTransform(NozzleTransform);
          if (nozzle == null)
          {
              Utils.LogWarning(String.Format("[ModuleMultiLengthEngine]: Could not find nozzle Transform"));
          }
      }
      private void SetupEngines()
      {
        engine = this.GetComponent<ModuleEnginesFX>();
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
            if (pm.moduleName == "ModuleB9PartSwitch")
                b9PartModule = pm;
        }
        if (b9PartModule == null)
        {
          Utils.LogWarning(String.Format("[ModuleMultiLengthEngine]: Could not find B9PartSwitch Module"));
        }
      }

      private void SelectLengthConfig(int configIndex)
      {
          Utils.LogWarning(String.Format("[ModuleMultiLengthEngine]: Selected config {0}", lengthConfigs[configIndex].subtypeName));
            AssignParameters(lengthConfigs[configIndex]);
            SelectedConfigIndex = configIndex;
      }

      private void AssignParameters(LengthConfiguration config)
      {
        engine.atmosphereCurve = config.atmosphereCurve;
        engine.maxThrust = config.maxThrust;
        engine.minThrust = config.minThrust;
        engine.heatProduction = config.heatProduction;

        nozzle.localPosition = config.localPosition;
        for (int i =0; i< engine.propellants.Count; i++)
        {
          for (int j = 0; j < config.propellants.Count; j++)
          {
            if (engine.propellants[i].name == config.propellants[j].name)
            {
              engine.propellants[i].ratio = config.propellants[j].ratio;
            }
          }
        }
      }

    }
}
