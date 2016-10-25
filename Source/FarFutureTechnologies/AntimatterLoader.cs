using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FarFutureTechnologies
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class AntimatterLoader: MonoBehaviour
    {
        public bool loadingAllowed = false;

        public double availableAM = 0d;
        public double usedAM = 0d;
        public List<Part> antimatterTanks = new List<Part>();
        public List<PartResource> antimatterResources = new List<PartResource>();

        public static AntimatterLoader Instance { get; private set; }

        void Awake()
        {
            Instance = this;
        }
         void Start()
         {
             if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ActiveVessel != null)
             {
                 RefreshAntimatterData(FlightGlobals.ActiveVessel);
             }
         }

         void RefreshAntimatterData(Vessel vessel)
         {
             antimatterTanks = new List<Part>();
             antimatterResources = new List<PartResource>();
             availableAM = AntimatterFactory.Instance.Antimatter;

             usedAM = 0d;
            List<Part> parts = vessel.parts;
            foreach (Part p in parts)
            {
                List<PartResource> prl = p.Resources.ToList();
                foreach (PartResource res in prl)
                {
                    if (res.resourceName == "Antimatter")
                    {
                        antimatterTanks.Add(p);
                        antimatterResources.Add(res);
                        usedAM += res.amount;
                    }
                }
            }
            if (antimatterResources.Count > 0)
            {
                if (vessel.LandedInKSC)
                {
                    loadingAllowed = true;
                }
                else
                {
                    loadingAllowed = false;
                }
            }
            else
            {
                loadingAllowed = false;
            }
         }

         // Removes the antimatter from the current tanks
         public void ClearAntimatterFromVessel()
         {
             foreach (PartResource a in antimatterResources)
             {
                 a.amount = 0d;
             }
             usedAM = 0d;
         }
         public void ConsumeAntimatter()
         {
             double total = 0d;
             foreach (PartResource res in antimatterResources)
             {
                 total = total + res.amount;
             }

             AntimatterFactory.Instance.ConsumeAntimatter(total);
         }
         public void EmptyAllTanks()
         {
             ClearAntimatterFromVessel();
         }
         public void FillAllTanks()
         {
             double toUse = availableAM;
             foreach (PartResource res in antimatterResources)
             {
                 if (toUse >= res.maxAmount)
                 {
                     res.amount = res.maxAmount;
                     toUse -= res.maxAmount;
                 }
                 else
                 {
                     res.amount = toUse;
                     toUse = 0d;
                 }
             }
         }
         public void EvenAllTanks()
         {

             double each = availableAM / (double)antimatterResources.Count;
             foreach (PartResource res in antimatterResources)
             {
                 res.amount = Math.Min(each, res.maxAmount);

             }
         }
    }
}
