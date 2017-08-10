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
            Utils.Log("[FFT Persistence]: Initialized");
            Instance = this;
            base.OnAwake();


        }

        public override void OnLoad(ConfigNode node)
        {
            Utils.Log("[FFT Persistence]: Started Loading");
            base.OnLoad(node);
            FarFutureTechnologySettings.Load();
            DoLoad(node);
            Utils.Log("[FFT Persistence]: Done Loading");
        }

        public override void OnSave(ConfigNode node)
        {
            Utils.Log("[FFT Persistence]: Started Saving");
            base.OnSave(node);
            DoSave(node);
            Utils.Log("[FFT Persistence]: Finished Saving");
        }

        void DoLoad(ConfigNode node)
        {
            int factoryLevel = 0;
            double currentStorage = 0d;
            double deferredConsumption = 0d;
            bool firstLoad = true;

            if (node != null)
            {
                ConfigNode[] kNodes = node.GetNodes(FarFutureTechnologySettings.amFactoryConfigNodeName);
                if (kNodes.Length > 0)
                {
                    foreach (ConfigNode k in kNodes)
                    {
                        factoryLevel = Utils.GetValue(k, "AMFactoryLevel", 0);
                        currentStorage = Utils.GetValue(k, "AMAmount", 0d);
                        deferredConsumption = Utils.GetValue(k, "AMDeferred", 0d);
                        firstLoad = Utils.GetValue(k, "FirstLoad", true);
                    }
                    Utils.Log(String.Format("[FFT Persistence]: AM Factory loaded at Level {0}, Stored Fuel {1}, First Load {2}",factoryLevel,currentStorage, firstLoad));
                }
            }

            AntimatterFactory.Instance.Initialize(factoryLevel, currentStorage, deferredConsumption, firstLoad);

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
            dbNode.SetValue("FirstLoad", AntimatterFactory.Instance.FirstLoad.ToString(), true);
            Utils.Log(String.Format("[FFT Persistence]: AM Factory saved with Level {0}, Stored Fuel {1}", AntimatterFactory.Instance.FactoryLevel, AntimatterFactory.Instance.Antimatter));
        }
    }
}
