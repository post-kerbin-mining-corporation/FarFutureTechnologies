using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FarFutureTechnologies
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class AntimatterFactory : MonoBehaviour
    {
        private static AntimatterFactory instance;
        public static AntimatterFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AntimatterFactory>();
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        instance = obj.AddComponent<AntimatterFactory>();
                    }
                }
                return instance;
            }
        }

        public int FactoryLevel { get { return factoryLevel; } }
        public bool Researched { get { return researched; } }
        public bool Infinite { get { return infinite; } set {infinite = value;}}
        public double Antimatter {
          get {
            if (Infinite)
              return AntimatterMax;
            return curAntimatter;
            } }
        public double AntimatterRate { get { return curAntimatterRate; } }
        public double AntimatterMax { get { return maxAntimatter; } }
        public double DeferredAntimatterAmount { get { return deferredAntimatterAmount; } }

        private bool productionOn = false;
        private bool researched = false;
        private bool infinite = false;

        private int factoryLevel = 0;

        private double curAntimatter = 0d;
        private double maxAntimatter = 0d;
        private double curAntimatterRate = 0d;
        private double deferredAntimatterAmount = 0d;

        private AntimatterFactoryLevelData curLevelDat;

        private double lastUpdateTime = 0d;

        public bool IsMaxLevel()
        {
            if (factoryLevel >= FarFutureTechnologySettings.factoryLevels.Count-1)
                return true;
            return false;
        }
        public float GetNextLevelCost()
        {
            if (factoryLevel >= FarFutureTechnologySettings.factoryLevels.Count - 1)
                return 0f;
            else
                return (float)FarFutureTechnologySettings.GetAMFactoryLevelData(factoryLevel + 1).purchaseCost;
        }
        public string GetStatusString()
        {
            if (productionOn)
                return "Fully Operational";
            else
                return "Not Functional";
        }
        public void SetProductionStatus(bool status)
        {
            productionOn = status;
        }
        public void ToggleProduction()
        {
            productionOn = !productionOn;
        }
        public void Upgrade()
        {
            curLevelDat = FarFutureTechnologySettings.GetAMFactoryLevelData(factoryLevel+1);
            factoryLevel = factoryLevel + 1;
            maxAntimatter = curLevelDat.maxCapacity;
            curAntimatterRate = curLevelDat.baseRate;
        }
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        private void Start()
        {
            double worldTime = Planetarium.GetUniversalTime();
            GameEvents.OnVesselRollout.Add(new EventData<ShipConstruct>.OnEvent(OnVesselRollout));
            if (worldTime - lastUpdateTime > 0d)
            {
                Utils.Log(String.Format("[AntimatteryFactory]: Delta time of {0} seconds detected, catching up", worldTime-lastUpdateTime));
                // update storage to reflect delta
                //CatchupProduction(worldTime - lastUpdateTime);
                lastUpdateTime = worldTime;
            }

        }

        void OnVesselRollout(ShipConstruct ship)
        {
          int id = PartResourceLibrary.Instance.GetDefinition("Antimatter").id;
          for (int i = 0; i < ship.Parts.Count; i++)
          {
            PartResource res =  ship.Parts[i].Resources.Get(id);
            res.maxAmount = 0d;

          }
        }

        public void Initialize(int loadedLevel, double loadedStorage, double deferredConsumption)
        {
            factoryLevel = loadedLevel;
            curAntimatter = loadedStorage;
            deferredAntimatterAmount = deferredConsumption;

            // If game mode is sandbox, set the level to max immediately and begin production
            if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX)
            {
                Utils.Log("AntimatteryFactory]: Detected Sandbox, setting AM factory to max level and activating");
                researched = true;
                factoryLevel = FarFutureTechnologySettings.factoryLevels.Count - 1;
                SetProductionStatus(true);
            }
            // If science sandbox, check for the needed technology first
            else if (HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
            {
                Utils.Log("AntimatteryFactory]: Detected Science Sandbox, setting AM factory to max level and detecting tech level");
                bool isResearched = Utils.CheckTechPresence(FarFutureTechnologySettings.antimatterFactoryUnlockTech);
                if (isResearched)
                {
                    researched = true;
                    factoryLevel = FarFutureTechnologySettings.factoryLevels.Count - 1;
                    SetProductionStatus(true);
                }
                else
                {
                    researched = false;
                    SetProductionStatus(false);
                }

            }
            else
            {
                Utils.Log("AntimatteryFactory]: Detected Career, setting AM factory to stored level and detecting tech level");
                bool isResearched = Utils.CheckTechPresence(FarFutureTechnologySettings.antimatterFactoryUnlockTech);
                if (isResearched)
                {
                    researched = true;
                    factoryLevel = loadedLevel;
                    SetProductionStatus(true);
                }
                else
                {
                    researched = false;
                    SetProductionStatus(false);
                }
            }

            Utils.Log("AntimatteryFactory]: Completed data load, initializing AM factory for level "+ factoryLevel.ToString());
            curLevelDat =  FarFutureTechnologySettings.GetAMFactoryLevelData(factoryLevel);


            maxAntimatter = curLevelDat.maxCapacity;
            curAntimatterRate = curLevelDat.baseRate;

            if (curAntimatter > maxAntimatter)
            {
                curAntimatter = maxAntimatter;
            }




        }

        public void ScheduleConsumeAntimatter(double amt)
        {
            deferredAntimatterAmount = amt;
        }

        public void ConsumeAntimatter(double amt)
        {
            curAntimatter = curAntimatter - amt;
            if (curAntimatter < 0d)
            {
                curAntimatter = 0d;
            }

        }

        void CatchupProduction(double elapsed)
        {

            curAntimatter = curAntimatter + curAntimatterRate * elapsed;
            if (curAntimatter > maxAntimatter)
            {
                curAntimatter = maxAntimatter;
            }
        }

        void FixedUpdate()
        {
            bool isTechReady = Utils.CheckTechPresence(FarFutureTechnologySettings.antimatterFactoryUnlockTech);

            if (isTechReady && !researched)
            {
                researched = true;
                SetProductionStatus(true);
            }

            if (productionOn)
            {

                curAntimatter = curAntimatter + ConvertRate( curAntimatterRate) * TimeWarp.fixedDeltaTime;

                if (curAntimatter > maxAntimatter)
                {
                    curAntimatter = maxAntimatter;
                }

            }
        }
        void Update()
        {
          if (productionOn)
          {
            if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) &&
              (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)) &&
              Input.GetKeyDown(KeyCode.A) )
              {
                Infinite = !Infinite;
              // CTRL + Z
            }
          }
        }
        double ConvertRate(double rateDays)
        {
            double rateSeconds = 0d;
            if (GameSettings.KERBIN_TIME)
            {
                rateSeconds = rateDays / 21600d;
            }
            else
            {
                rateSeconds = rateDays / 86400d;
            }
            return rateSeconds;
        }

    }
}
