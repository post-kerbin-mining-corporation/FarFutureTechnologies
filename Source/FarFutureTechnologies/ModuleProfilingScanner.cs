using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;
using FarFutureTechnologies.UI;

namespace FarFutureTechnologies
{
    public class ModuleProfilingScanner: PartModule
    {

        // Range of this scanner
        [KSPField(isPersistant = false)]
        public float ScanRange = 250000f;

        // How frequently to sample a profile
        [KSPField(isPersistant = false)]
        public float ScanInterval = 10000f;

        // Resources to scan for
        [KSPField(isPersistant = false)]
        public string ScannedResources = "";

        // Events
        [KSPEvent(guiActive = true, guiName = "Analyze Profile", active = true)]
        public void Scan()
        {
            TakeProfile();
        }
        // actions
        [KSPAction("Take Profile")]
        public void ScanAction(KSPActionParam param)
        {
          TakeProfile();
        }

        // Private
        private List<ResourceProfile> profiledResources;
        private List<string> scannedResources;

        public string GetModuleTitle()
        {
            return "Resource Profiler";
        }
        public override string GetModuleDisplayName()
        {
            return Localizer.Format("#LOC_FFT_ModuleProfilingScanner_ModuleName");
        }

        public override string GetInfo()
        {
          scannedResources = SplitString(ScannedResources);
          string msg = Localizer.Format("#LOC_FFT_ModuleProfilingScanner_PartInfo"); ;
          foreach (string sub in scannedResources)
          {
              msg += Localizer.Format("#LOC_FFT_ModuleProfilingScanner_PartInfo2", sub);
          }

          return msg;
        }

        public void Start()
        {
          if (HighLogic.LoadedSceneIsFlight)
          {
              scannedResources = SplitString(ScannedResources);
          }
        }
        // Profiles all resources
        protected void TakeProfile()
        {
            profiledResources = new List<ResourceProfile>();
            for (int i = 0; i < scannedResources.Count; i++)
            {
                Utils.Log(String.Format("[ModuleProfilingScanner]: Taking profile for {0}", scannedResources[i]));
                profiledResources.Add(TakeResourceProfile(scannedResources[i]));
            }
            ProfilingUI.Instance.ShowProfileWindow(profiledResources); 
        }
        // Profiles a specific resource
        protected ResourceProfile TakeResourceProfile(string resourceName)
        {
            float distance = 0f;
            
            Dictionary<float,float> samples = new Dictionary<float,float>();
            while (distance <= ScanRange)
            {
                 
                Vector3 pos = part.partTransform.position + part.partTransform.up.normalized * distance;
                samples.Add(distance, Sample(resourceName, pos));

                distance += ScanInterval;
            }
            return new ResourceProfile(resourceName, samples);
        }
        protected float Sample(string resourceName, Vector3 worldPos)
        {
            float abundance = 0f;
            AbundanceRequest req = new AbundanceRequest();
            double alt;
            double lat;
            double lon;

            part.vessel.mainBody.GetLatLonAlt(new Vector3d(worldPos.x, worldPos.y, worldPos.z), out lat, out lon, out alt);

            
            req.BodyId = FlightGlobals.GetBodyIndex(part.vessel.mainBody);
            req.ResourceType = HarvestTypes.Atmospheric;
            req.ResourceName = resourceName;
            req.Latitude = lat;
            req.Altitude = alt;
            req.Longitude = lon;
            abundance += ResourceMap.Instance.GetAbundance(req);
            abundance *= (float)part.vessel.mainBody.GetPressure(alt);
            // Sample exo
            req.ResourceType = HarvestTypes.Exospheric;
            abundance += ResourceMap.Instance.GetAbundance(req);
            Utils.Log(String.Format("[ModuleProfilingScanner]: Sampling position {0}, geocentric alt {1}, lat {2} lon {3}\n Result: {4}", worldPos.ToString(), alt, lat, lon, abundance));
            return abundance;
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
    [Serializable]
    public class ResourceProfile
    {
      public string resourceName;
      public Dictionary<float,float> concentrations;
      public float maxConcentration= 0f;
      public float minConcentration = 100f;
      public float maxDistance = 0f;
      public float minDistance= 999999999f;

      public ResourceProfile(string resName, Dictionary<float,float> samples)
      {
          resourceName = resName;
          concentrations = new Dictionary<float,float>(samples);
          GetBoundaries();
      }
      void GetBoundaries()
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
