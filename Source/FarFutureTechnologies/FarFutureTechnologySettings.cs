using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FarFutureTechnologies
{
    public static class FarFutureTechnologySettings
    {

        public static string antimatterFactoryUnlockTech = "antimatterPower";
        public static string antimatterFactoryUpgradeTech = "antimatterTechniques";

        public static string amFactoryConfigNodeName = "FFTAMFactory";

        public static List<AntimatterFactoryLevelData> factoryLevels;

        public static void Load()
        {
            ConfigNode settingsNode;

            Utils.Log("Settings: Started loading");
            if (GameDatabase.Instance.ExistsConfigNode("FarFutureTechnologies/FFTSETTINGS"))
            {
                Utils.Log("Settings: Located settings file");
                settingsNode = GameDatabase.Instance.GetConfigNode("FarFutureTechnologies/FFTSETTINGS");

                ConfigNode amSettingsNode = settingsNode.GetNode("AntimatterFactory");

                antimatterFactoryUnlockTech = Utils.GetValue(amSettingsNode, "TechnologyToUnlock", "antimatterPower");
                antimatterFactoryUpgradeTech = Utils.GetValue(amSettingsNode, "TechnologyToUpgrade", "antimatterTechniques");

                ConfigNode[] levelNodes = amSettingsNode.GetNodes("FactoryLevel");
                factoryLevels = new List<AntimatterFactoryLevelData>();

                foreach (ConfigNode k in levelNodes)
                {
                    int lvl = Utils.GetValue(k, "Level", 0);
                    double cost = Utils.GetValue(k, "PurchaseCost", 0d);
                    double rate = Utils.GetValue(k, "ProductionRate", 0d);
                    double max = Utils.GetValue(k, "ProductionCapacity", 0d);

                    factoryLevels.Add(new AntimatterFactoryLevelData(lvl, max, rate, cost));
                    Utils.Log(String.Format("AM factory level {0} found, cost {1}, rate {2}, capacity {3}", lvl, cost, rate, max));
                }

            }
            else
            {
                Utils.LogWarning("Settings: Couldn't find settings file, using defaults");
            }
            Utils.Log("Settings: Finished loading");
        }

        public static AntimatterFactoryLevelData GetAMFactoryLevelData(int lvl)
        {
            if (factoryLevels != null && factoryLevels.Count > 0)
                return factoryLevels[lvl];
            else
                return null;
        }
    }

    public class AntimatterFactoryLevelData
    {
        public int level = 0;
        public double maxCapacity = 0d;
        public double baseRate = 0d;
        public double purchaseCost = 0d;

        public AntimatterFactoryLevelData(int lvl, double cap, double rate, double cost)
        {
            level = lvl;
            maxCapacity = cap;
            baseRate = rate;
            purchaseCost = cost;
        }
    }
}
