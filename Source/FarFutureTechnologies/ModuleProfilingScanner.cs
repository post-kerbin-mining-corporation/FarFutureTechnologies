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

        [System.Serializable]
        public class ProfileableResource
        {
          public string resourceName = "";
          public float noiseMax = 1.0f;
          public float noiseMin = 0.0f;
          public bool useAtmo = true;

          public ProfileableResource(ConfigNode node)
          {
              node.TryGetValue("resourceName", ref resourceName);
              node.TryGetValue("noiseMax", ref noiseMax);
              node.TryGetValue("noiseMin", ref noiseMin);
              node.TryGetValue("useAtmo", ref useAtmo);
          }

        }

        // Current range setting of this scanner
        [KSPField(isPersistant = true, guiActive = true, guiName = "Profiler Range"), UI_FloatRange(minValue = 50000f, maxValue = 250000f, stepIncrement = 10000f)]
        public float ScanRange = 250000f;

        // Minimum Range that can be set
        [KSPField(isPersistant = false)]
        public float MinimumRange = 50000f;

        // Maximum range to be set
        [KSPField(isPersistant = false)]
        public float MaximumRange = 100000f;

        // How frequently to sample a profile
        [KSPField(isPersistant = false)]
        public float ScanCount = 100f;

        // Scan reults
        [KSPField(isPersistant = false, guiActive = true, guiName = "Local")]
        public string LocalResults;

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
        private List<ProfileableResource> scannableResources;

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

          string msg = Localizer.Format("#LOC_FFT_ModuleProfilingScanner_PartInfo"); ;
          foreach (ProfileableResource sub in scannableResources)
          {
              msg += Localizer.Format("#LOC_FFT_ModuleProfilingScanner_PartInfo2", sub.resourceName);
          }

          return msg;
        }

        public override void  OnStart(PartModule.StartState state)
        {
            Fields["LocalResults"].guiName = Localizer.Format("#LOC_FFT_ModuleProfilingScanner_Field_LocalResults_Title");
            Fields["ScanRange"].guiName = Localizer.Format("#LOC_FFT_ModuleProfilingScanner_Field_ScanRange_Title");

            Events["Scan"].guiName = Localizer.Format("#LOC_FFT_ModuleProfilingScanner_Event_Scan_Title");
            Actions["ScanAction"].guiName = Localizer.Format("#LOC_FFT_ModuleProfilingScanner_Action_Scan_Title");
            if (scannableResources == null || scannableResources.Count == 0)
            {
                  
                ConfigNode node = GameDatabase.Instance.GetConfigs("PART").
                    Single(c => part.partInfo.name == c.name).config.
                    GetNodes("MODULE").Single(n => n.GetValue("name") == moduleName);
                Utils.Log("[ModuleProfilingScanner]: Loaded node " + node.ToString());
                OnLoad(node);
            }
            if (HighLogic.LoadedSceneIsFlight)
            {
             
                var range = (UI_FloatRange)this.Fields["ScanRange"].uiControlFlight;
                range.minValue = MinimumRange;
                range.maxValue = MaximumRange;

             
            }
        }
        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            ConfigNode[] varNodes = node.GetNodes("PROFILEABLERESOURCE");
            scannableResources = new List<ProfileableResource>();
            for (int i=0; i < varNodes.Length; i++)
            {
              scannableResources.Add(new ProfileableResource(varNodes[i]));
              Utils.Log("[ModuleProfilingScanner]: Added a new resource");
            }
        }
        // Profiles all resources
        protected void TakeProfile()
        {
            profiledResources = new List<ResourceProfile>();
            
            for (int i = 0; i < scannableResources.Count; i++)
            {
                Utils.Log(String.Format("[ModuleProfilingScanner]: Taking profile for {0}", scannableResources[i].resourceName));
                profiledResources.Add(TakeResourceProfile(scannableResources[i]));
            }
            ProfilingUI.Instance.ShowProfileWindow(profiledResources);
        }
        // Profiles a specific resource
        protected ResourceProfile TakeResourceProfile(ProfileableResource resource)
        {
            float distance = 0f;

            Dictionary<float,float> samples = new Dictionary<float,float>();
            float scanInterval = (ScanRange - MinimumRange)/ScanCount;

            Utils.Log(String.Format("[ModuleProfilingScanner]: Scan Interval {0}", scanInterval));

            while (distance <= ScanRange)
            {
                float fracDist = Mathf.Clamp01((distance - MinimumRange)/(MaximumRange- MinimumRange));
                float noise = (resource.noiseMax-resource.noiseMin)/(MaximumRange-MinimumRange) * fracDist + resource.noiseMin;
                Vector3 pos = part.partTransform.position + part.partTransform.up.normalized * distance;
                samples.Add(distance, Sample(resource, pos, noise));

                distance = distance + scanInterval;
            }
            return new ResourceProfile(resource.resourceName, samples);
        }
        protected float Sample(ProfileableResource resource, Vector3 worldPos, float noiseScalar)
        {
            float abundance = 0f;
            AbundanceRequest req = new AbundanceRequest();
            double alt;
            double lat;
            double lon;

            part.vessel.mainBody.GetLatLonAlt(new Vector3d(worldPos.x, worldPos.y, worldPos.z), out lat, out lon, out alt);

            req.BodyId = FlightGlobals.GetBodyIndex(part.vessel.mainBody);
            req.ResourceType = HarvestTypes.Atmospheric;
            req.ResourceName = resource.resourceName;
            req.Latitude = lat;
            req.Altitude = alt;
            req.Longitude = lon;
            abundance += ResourceMap.Instance.GetAbundance(req);
            if (resource.useAtmo)
                abundance = abundance * (float)part.vessel.mainBody.GetPressure(alt);

            // Sample exo
            req.ResourceType = HarvestTypes.Exospheric;
            abundance = abundance + ResourceMap.Instance.GetAbundance(req);
            abundance = abundance + noiseScalar * UnityEngine.Random.Range(-1.0f, 1.0f);
            Utils.Log(String.Format("[ModuleProfilingScanner]: Sampling position {0}, geocentric alt {1}, lat {2} lon {3}\n Noise: {4} Result: {5}", worldPos.ToString(), alt, lat, lon, noiseScalar, abundance));
            return abundance;
        }

        protected void FixedUpdate()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
              LocalResults = "";
                for (int i = 0; i < scannableResources.Count; i++)
                {
                  //float abundance = Sample(scannableResources[i], part.partTransform.position, 0.0f);
                  //LocalResults += Localizer.Format("#LOC_FFT_ModuleProfilingScanner_Field_LocalResults_Scanning", scannableResources[i].resourceName, abundance.ToString("F3"));
                  if (i < scannableResources.Count - 1)
                    LocalResults += "\n";
                }
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
