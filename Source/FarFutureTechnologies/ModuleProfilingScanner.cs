using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace FarFutureTechnologies
{
    public class ModuleProfilingScanner: PartModule
    {

        // Range of this scanner
        [KSPField(isPersistant = false)]
        public float ScanRange = 250000f;

        // Resources to scan for
        [KSPField(isPersistant = false)]
        public string ScannedResources = "";

        // Events
        [KSPEvent(guiActive = true, guiName = "Analyze Profile", active = true)]
        public void TakeProfile()
        {

        }
        // actions
        [KSPAction("Take Profile")]
        public void ScanAction(KSPActionParam param)
        {
          TakeProfile();
        }

        // Private
        private List<string> scannedResources;

        public string GetModuleTitle()
        {
            return "Resource Scanner";
        }
        public override string GetModuleDisplayName()
        {
            return Localizer.Format("#LOC_FFT_ModuleProfilingScanner_ModuleName");
        }

        public override string GetInfo()
        {
          scannedResources = SplitString(ScannedResources);
          string msg = "";

            msg = Localizer.Format("#LOC_FFT_ModuleProfilingScanner_PartInfo", engine.engineID);

          return msg;
        }

        public void Start()
        {
          if (HighLogic.LoadedSceneIsFlight)
          {

          }
        }
        protected void TakeProfile()
        {

        }

        protected void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {



            }
        }
        private List<string> SplitString(string toSplit)
        {
            return toSplit.Split(',').ToList();
        }
    }
    public class ResourceProfile
    {
      public string resourceName;
      public Dictionary<float,float> concentrations;
      public float maxConcentration= 0f;
      public float minConcentration = 100f;
      public float maxDistance= 0f;
      public float minDistance= 999999999f;

      public ResourceProfile(string resName, Dictionary<float,float> samples)
      {
          resourceName = resName;
          concentrations = new Dictionary<float,float>(samples);
          GetBoundaries();
      }
      float GetBoundaries()
      {
        foreach(var sample in concentrations)
        {
          if (sample.Key > maxDistance) maxDistance = sample.Key;
          if (sample.Key < minDistance) minDistance = sample.Key;
          if (sample.Value < minConcentration) minConcentration = sample.Value;
          if (sample.Value > maxConcentration) maxConcentration = sample.Value;
        }
      }

    }
}
