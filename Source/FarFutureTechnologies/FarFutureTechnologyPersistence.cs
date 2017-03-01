using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FarFutureTechnologies
{

    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.FLIGHT, GameScenes.TRACKSTATION, GameScenes.EDITOR)]
    public class FarFutureTechnologyPersistence : ScenarioModule
    {
        public static FarFutureTechnologyPersistence Instance { get; private set; }

        public override void OnAwake()
        {
            Utils.Log("Persistence: Init");
            Instance = this;
            base.OnAwake();

            
        }

        public override void OnLoad(ConfigNode node)
        {
            Utils.Log("Persistence: Started Loading");
            base.OnLoad(node);
            FarFutureTechnologySettings.Load();
            DoLoad(node);
            Utils.Log("Persistence: Done Loading");
        }

        public override void OnSave(ConfigNode node)
        {
            Utils.Log("Persistence: Started Saving");
            base.OnSave(node);
            DoSave(node);
            Utils.Log("Persistence: Finished Saving");
        }

        void DoLoad(ConfigNode node)
        {
            int factoryLevel = 0;
            double currentStorage = 0d;
            double deferredConsumption = 0d;
            Utils.Log("x");
            if (node != null)
            {
                ConfigNode[] kNodes = node.GetNodes(FarFutureTechnologySettings.amFactoryConfigNodeName);
                if (kNodes.Length > 0)
                {
                    Utils.Log("y");
                    foreach (ConfigNode k in kNodes)
                    {
                        factoryLevel = Utils.GetValue(k, "AMFactoryLevel", 0);
                        currentStorage = Utils.GetValue(k, "AMAmount", 0d);
                        deferredConsumption = Utils.GetValue(k, "AMDeferred", 0d);
                        Utils.Log("1");
                    }
                    Utils.Log(String.Format("Loaded with Level {0}, Stored {1}, Deferred {2}",factoryLevel,currentStorage,deferredConsumption));
                }
            }
            
            AntimatterFactory.Instance.Initialize(factoryLevel, currentStorage, deferredConsumption);

        }

        void DoSave(ConfigNode node)
        {
            ConfigNode dbNode;
            bool init = node.HasNode(FarFutureTechnologySettings.amFactoryConfigNodeName);
            if (init)
                dbNode = node.GetNode(FarFutureTechnologySettings.amFactoryConfigNodeName);
            else
                dbNode = node.AddNode(FarFutureTechnologySettings.amFactoryConfigNodeName);
            
            Utils.Log( AntimatterFactory.Instance.Antimatter.ToString());

            dbNode.SetValue("AMFactoryLevel",AntimatterFactory.Instance.FactoryLevel.ToString(), true);
            dbNode.SetValue("AMAmount", AntimatterFactory.Instance.Antimatter.ToString(), true);
            dbNode.SetValue("AMDeferred", AntimatterFactory.Instance.DeferredAntimatterAmount.ToString(), true);
            Utils.Log(String.Format("Saved with Level {0}, Stored {1}, Deferred {2}", AntimatterFactory.Instance.FactoryLevel, AntimatterFactory.Instance.Antimatter, AntimatterFactory.Instance.DeferredAntimatterAmount));
        }
    }
}
