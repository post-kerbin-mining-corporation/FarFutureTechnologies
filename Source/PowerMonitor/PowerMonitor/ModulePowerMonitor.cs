using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PowerMonitor
{
  public class ModulePowerMonitor : VesselModule
  {

        float timeWarpLimit = 1000f;
        //public List<ModuleCryoTank> cryoTanks = new List<ModuleCryoTank>();

        public List<IPowerConsumingPart> managedParts = new List<IPowerConsumingPart>();

        public List<PowerConsumer> powerConsumers = new List<PowerConsumer>();
        public List<PowerProducer> powerProducers = new List<PowerProducer>();

        bool vesselLoaded = false;
        bool analyticMode = false;
        bool dataReady = false;
        int partCount = -1;

        public bool AnalyticMode {get {return analyticMode;}}

        protected override void  OnStart()
        {
 	          base.OnStart();
            GetVesselElectricalData();
        }


        void FixedUpdate()
        {
          if (HighLogic.LoadedSceneIsFlight && dataReady)
          {
             // Debug.Log(String.Format("CryoTanks: Vessel {0}, loaded state is {1}",  vessel.name, vessel.loaded.ToString()));
              if (!vesselLoaded && FlightGlobals.ActiveVessel == vessel)
              {
                  Debug.Log("Vessel changed state from unfocused to focused");
                  GetVesselElectricalData();
                  vesselLoaded = true;
              }
              if (vesselLoaded && FlightGlobals.ActiveVessel != vessel)
              {
                  vesselLoaded = false;
              }

            if (TimeWarp.CurrentRate < timeWarpLimit)
            {
              analyticMode = false;
                DoLowWarpSimulation();

            } else
            {
              analyticMode = true;
              DoHighWarpSimulation();
            }

          }
        }
        protected void DoLowWarpSimulation()
        {
          for (int i = 0; i< managedParts.Count;i++)
          {
              managedParts[i].ProcessLowWarp();
          }
        }
        protected void DoHighWarpSimulation()
        {
          double production = DetermineShipPowerProduction();
          double consumption = DetermineShipPowerConsumption();

          double managedConsumption = DetermineManagedConsumption();

          AllocatePower(production-consumption, managedConsumption);
        }

        protected void AllocatePower(double availablePower, double managedConsumption)
        {

          float powerDeficit = Mathf.Clamp((float)(availablePower - managedConsumption),-9999999f, 0f);

         // Debug.Log(String.Format("Power Deficit: {0}", powerDeficit));
          double usedPower = 0d;

          for (int i = 0; i< managedParts.Count;i++)
          {
              if (usedPower >= availablePower)
              {
                managedParts[i].ProcessHighWarp();
                //usedPower += cryoTanks[i].SetBoiloffState(true);
              } else
              {
                managedParts[i].SetPoweredState(false);
                  usedPower =managedParts[i].CalculatePowerUsage();
              }
          }
        }

        public double DetermineManagedConsumption()
        {
          double totalConsumption = 0d;
          for (int i = 0; i < managedParts.Count;i++)
          {
            totalConsumption += managedParts[i].CalculatePowerUsage();
          }
          //Debug.Log(String.Format("CryoTanks: total ship boiloff consumption: {0} Ec/s", totalConsumption));
          return totalConsumption;
        }

        // TODO: implement me!
        public double DetermineShipPowerConsumption()
        {
          double currentPowerRate = 0d;
          foreach (PowerConsumer p in powerConsumers)
          {
            currentPowerRate += p.GetPowerConsumption();
          }
          //Debug.Log(String.Format("CryoTanks: total ship power consumption: {0} Ec/s", currentPowerRate));
          return currentPowerRate;
        }

        public double DetermineShipPowerProduction()
        {
          double currentPowerRate = 0d;
          foreach (PowerProducer p in powerProducers)
          {
            currentPowerRate += p.GetPowerProduction();
          }
          //Debug.Log(String.Format("CryoTanks: total ship power production: {0} Ec/s", currentPowerRate));
          return currentPowerRate;
        }

        protected void GetVesselElectricalData()
        {
          //cryoTanks.Clear();
          powerProducers.Clear();
          partCount = vessel.parts.Count;
          for (int i = partCount - 1; i >= 0; --i)
          {
              Part part = vessel.Parts[i];

              List<IPowerConsumingPart> managedConsumers = part.FindModulesImplementing<IPowerConsumingPart>().ToList();

              for (int j = 0; j < managedConsumers.Count; j++)
              {
                managedParts.Add(managedConsumers[i]);
              }

              for (int j = part.Modules.Count - 1; j >= 0; --j)
              {
                  PartModule m = part.Modules[j];
                  // Try to create accessor modules
                  bool success = TrySetupProducer(m);
                  if (!success)
                    TrySetupConsumer(m);
              }
            }
          managedParts = managedParts.OrderBy(o => o.GetPriority()).ToList();
          Debug.Log(String.Format("Power Monitor: Summary: \n vessel {0} (loaded state {1})\n" +
            "- {2} managed consumers \n" +
            "- {3} stock power producers \n" +
            "- {4} stock power consumers", vessel.name,vessel.loaded.ToString(), managedParts.Count, powerProducers.Count, powerConsumers.Count));

          dataReady = true;
        }
        protected bool TrySetupProducer(PartModule pm)
        {
          PowerProducerType prodType;
          if (TryParse<PowerProducerType>(pm.moduleName, out prodType))
          {
            // Verify
            bool isProducer = VerifyInputs(pm, true);
            if (isProducer)
            {
              PowerProducer prod =  new PowerProducer(prodType, pm);
              powerProducers.Add(prod);
              return true;
            }
          }
          return false;


        }
        protected bool TrySetupConsumer(PartModule pm)
        {
          PowerConsumerType prodType;
          if (TryParse<PowerConsumerType>(pm.moduleName, out prodType))
          {
            // Verify
            bool isConsumer = VerifyInputs(pm, false);
            if (isConsumer)
            {
              PowerConsumer con =  new PowerConsumer(prodType, pm);
              powerConsumers.Add(con);
              return true;
            }
          }
          return false;


        }
        /// Checks to see whether a ModuleGenerator/ModuleResourceConverter/ModuleResourceHarvester is a producer or consumer
        protected bool VerifyInputs(PartModule pm, bool isProducer)
        {
          if (pm.moduleName == "ModuleResourceConverter" || pm.moduleName == "ModuleResourceHarvester")
          {
            BaseConverter conv = (BaseConverter)pm;
            if (isProducer)
            {
              for (int i = 0;i < conv.outputList.Count;i++)
                if (conv.inputList[i].ResourceName == "ElectricCharge")
                    return true;
              return false;
            } else
            {
                for (int i = 0; i < conv.inputList.Count; i++)
                    if (conv.inputList[i].ResourceName == "ElectricCharge")
                        return true;
              return false;
            }
          }
          if (pm.moduleName == "ModuleGenerator")
          {
            ModuleGenerator gen = (ModuleGenerator)pm;
            if (isProducer)
            {
              for (int i = 0; i < gen.resHandler.outputResources.Count; i++)
                  if (gen.resHandler.outputResources[i].name == "ElectricCharge")
                  {
                      return true;
                  }
              return false;
            } else
            {
              for (int i = 0; i < gen.resHandler.inputResources.Count; i++)
                  if (gen.resHandler.inputResources[i].name == "ElectricCharge")
                  {
                      return true;
                  }
              return false;
            }
          }
          return true;
        }


        public static bool TryParse<TEnum>(string value, out TEnum result)
      where TEnum : struct, IConvertible
        {
            var retValue = value == null ?
                        false :
                        Enum.IsDefined(typeof(TEnum), value);
            result = retValue ?
                        (TEnum)Enum.Parse(typeof(TEnum), value) :
                        default(TEnum);
            return retValue;
        }

  }
}
