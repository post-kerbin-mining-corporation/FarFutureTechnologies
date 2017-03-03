using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace PowerMonitor
{
    public enum PowerProducerType {
      ModuleDeployableSolarPanel,
      ModuleGenerator,
      ModuleResourceConverter,
      ModuleCurvedSolarPanel,
      FissionGenerator,
      ModuleRadioisotopeGenerator
    }


    public class PowerProducer
    {

      PowerProducerType producerType;
      // Generic reference to PartModule
      PartModule pm;

      // Hard references to stock modules
      ModuleDeployableSolarPanel panel;
      ModuleGenerator gen;
      ModuleResourceConverter converter;

      double converterEcRate;

      public string ProducerType {get {return producerType.ToString();}}

      public PowerProducer(PowerProducerType tp, PartModule mod)
      {
        producerType = tp;
        pm = mod;
        switch (producerType)
        {
          case PowerProducerType.ModuleDeployableSolarPanel:
            panel =  (ModuleDeployableSolarPanel)pm;
            break;
          case PowerProducerType.ModuleGenerator:
            gen = (ModuleGenerator)pm;
            break;
          case PowerProducerType.ModuleResourceConverter:
            converter = (ModuleResourceConverter)pm;
            for (int i = 0; i < converter.resHandler.outputResources.Count; i++)
                if (converter.resHandler.outputResources[i].name == "ElectricCharge")
                    converterEcRate = converter.resHandler.outputResources[i].rate;
            break;
        }
      }

      public double GetPowerProduction()
      {
        switch (producerType)
        {
          case PowerProducerType.ModuleDeployableSolarPanel:
                return GetModuleDeployableSolarPanelProduction();
          case PowerProducerType.ModuleGenerator:
            return GetModuleGeneratorProduction();
          case PowerProducerType.ModuleResourceConverter:
            return GetModuleResourceConverterProduction();
          case PowerProducerType.ModuleCurvedSolarPanel:
            return GetModuleCurvedSolarPanelProduction();

            case PowerProducerType.FissionGenerator:
            return GetFissionGeneratorProduction();

          case PowerProducerType.ModuleRadioisotopeGenerator:
            return GetModuleRadioisotopeGeneratorProduction();
        }
          return 0d;
      }

      double GetModuleDeployableSolarPanelProduction()
      {
        if (panel != null)
          return (double)panel.flowRate;
        return 0d;
      }

      double GetModuleGeneratorProduction()
      {
        if (gen == null || !gen.generatorIsActive)
          return 0d;
        for (int i = 0; i < gen.resHandler.outputResources.Count; i++)
            if (gen.resHandler.outputResources[i].name == "ElectricCharge")
                return (double)gen.efficiency * gen.resHandler.outputResources[i].rate;
        return 0d;
      }

      double GetModuleResourceConverterProduction()
      {
         if (converter == null || !converter.IsActivated)
           return 0d;
         return converterEcRate * converter.lastTimeFactor;
      }

      // NFT
      double GetFissionGeneratorProduction()
      {
          double results = 0d;
        double.TryParse( pm.Fields.GetValue("CurrentGeneration").ToString(), out results);
        return results;
      }
      double GetModuleRadioisotopeGeneratorProduction()
      {
          double results = 0d;
          double.TryParse(pm.Fields.GetValue("ActualPower").ToString(), out results);
          return results;
      }
      double GetModuleCurvedSolarPanelProduction()
      {
          double results = 0d;
          double.TryParse(pm.Fields.GetValue("energyFlow").ToString(), out results);
          return results;
      }

    }
}
