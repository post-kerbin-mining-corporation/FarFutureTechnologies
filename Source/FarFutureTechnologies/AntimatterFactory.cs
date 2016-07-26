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
        public static AntimatterFactory Instance { get; private set; }

        public int FactoryLevel { get { return factoryLevel; } }
        public double Antimatter { get { return curAntimatter; } }
        public double AntimatterRate { get { return curAntimatterRate; } }
        public double AntimatterMax { get { return maxAntimatter; } }

        private bool productionOn = false;
        
        private int factoryLevel = 0;

        private double curAntimatter = 0d;
        private double maxAntimatter = 0d;
        private double curAntimatterRate = 0d;

        private AntimatterFactoryLevelData curLevelDat;

        private double lastUpdateTime = 0d;


        public void SetProductionStatus(bool status)
        {
            productionOn = status;
        }
        public void ToggleProduction()
        {
            productionOn = !productionOn;
        }

        void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            double worldTime = Planetarium.GetUniversalTime();

            if (worldTime - lastUpdateTime > 0d)
            {
                Utils.Log(String.Format("Delta time of {0} detected, catching up", worldTime-lastUpdateTime));
                // update storage to reflect delta
                //CatchupProduction(worldTime - lastUpdateTime);
                lastUpdateTime = worldTime;
            }
        }

        public void Initialize(int loadedLevel, double loadedStorage)
        {
            factoryLevel = loadedLevel;
            curAntimatter = loadedStorage;

            // If game mode is sandbox, set the level to max immediately and begin production
            if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX)
            {
                Utils.Log("Detected Sandbox, setting AM factory to max and activating");
                factoryLevel = FarFutureTechnologySettings.factoryLevels.Count - 1;
                SetProductionStatus(true);
            }
            // If science sandbox, check for the needed technology first
            else if (HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX)
            {
                Utils.Log("Detected Science Sandbox, setting AM factory to max, detecting tech");
                bool isResearched = Utils.CheckTechPresence(FarFutureTechnologySettings.antimatterFactoryUnlockTech);
                if (isResearched)
                {
                    factoryLevel = FarFutureTechnologySettings.factoryLevels.Count - 1;
                    SetProductionStatus(true);
                }
                else
                {
                    SetProductionStatus(false);
                }

            }
            else
            {
                Utils.Log("Detected Career, setting AM factory to stored level, detecting tech");
                bool isResearched = Utils.CheckTechPresence(FarFutureTechnologySettings.antimatterFactoryUnlockTech);
                if (isResearched)
                {
                    factoryLevel = loadedLevel;
                    SetProductionStatus(true);
                }
                else
                {
                    SetProductionStatus(false);
                }
            }

            Utils.Log("Completed data load, setting up factory for level "+ factoryLevel.ToString());
            curLevelDat =  FarFutureTechnologySettings.GetAMFactoryLevelData(factoryLevel);

            maxAntimatter = curLevelDat.maxCapacity;
            curAntimatterRate = curLevelDat.baseRate;

               if (curAntimatter > maxAntimatter)
               {
                   curAntimatter = maxAntimatter;
               }
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
            if (productionOn)
            {
                curAntimatter = curAntimatter + curAntimatterRate * TimeWarp.fixedDeltaTime;
                if (curAntimatter > maxAntimatter)
                {
                    curAntimatter = maxAntimatter;
                }
            }
        }



    }
}
