using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FarFutureTechnologies
{

    public class AntimatterContainer
    {
      public Part part;
      public PartResource resource;
      public double requestedAmount;
      public double totalAmount;

      public AntimatterContainer(Part tankPart, PartResource amResource)
      {
        part = tankPart;
        resource = amResource;
      }
      public ClearRequest()
      {
        if (resource.amount > 0.0d)
          requestedAmount = -resource.Amount
        requestedAmount = 0d;
      }
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class AntimatterLoader: MonoBehaviour
    {
        public bool loadingAllowed = false;

        public double availableAM = 0d;
        public double usedAM = 0d;
        public List<AntimatterContainer> antimatterTanks = new List<AntimatterContainer>();

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

            List<Part> parts = vessel.parts;
            foreach (Part p in parts)
            {
                List<PartResource> prl = p.Resources.ToList();
                foreach (PartResource res in prl)
                {
                    if (res.resourceName == "Antimatter")
                    {
                        antimatterTanks.Add(new AntimatterContainer(p, res));
                    }
                }
            }
            if (antimatterTanks.Count > 0)
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
         // Loads and refunds antimatter
         public void LoadAntimatter()
         {
           ConsumeAntimatter();
         }
         // Removes the antimatter from the current tanks
         public void ClearAntimatterFromVessel()
         {
             foreach (AntimatterContainer tank in antimatterContainers)
             {

                 tank.ClearRequest();
             }
             usedAM = 0d;
         }
         public void ConsumeAntimatter()
         {
             double total = 0d;
             foreach (AntimatterContainer tank in antimatterContainers)
             {
                 total = total + tank.requestedAmount;
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
             foreach (AntimatterContainer tank in antimatterContainers)
             {
                 if (toUse >= tank.resource.maxAmount)
                 {
                     tank.requestedAmount = tank.resource.maxAmount - tank.resource.amount
                     toUse -= tank.requestedAmount;
                 }
                 else
                 {
                     tank.requestedAmount = toUse;
                     toUse = 0d;
                 }
             }
         }
         public void EvenAllTanks()
         {

             double each = availableAM / (double)antimatterTanks.Count;
             foreach (AntimatterContainer tank in antimatterContainers)
             {
                 tank.requestedAmount = Math.Min(each, tank.resource.maxAmount);

             }
         }
    }
}
