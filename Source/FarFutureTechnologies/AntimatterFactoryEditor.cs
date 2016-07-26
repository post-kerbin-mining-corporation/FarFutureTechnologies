using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FarFutureTechnologies
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class AntimatterFactoryEditor: MonoBehaviour
    {
        double availableAM = 0d;
        double usedAM = 0d;
        List<Part> antimatterTanks = new List<Part>();
        List<PartResource> antimatterResources = new List<PartResource>();

        void Awake()
        {
            randomizer = new System.Random(335462);
            windowIdentifier = randomizer.Next();
        }

        public void Start()
        {
            EditorLogic.fetch.launchBtn.onClick.RemoveListener(new UnityEngine.Events.UnityAction(EditorLogic.fetch.launchVessel));
            EditorLogic.fetch.launchBtn.onClick.AddListener(new UnityEngine.Events.UnityAction((AntimatterLaunchCheck)));
        }

        void AntimatterLaunchCheck()
        {
            antimatterTanks = new List<Part>();
            antimatterResources = new List<PartResource>();
            availableAM = AntimatterFactory.Instance.Antimatter;

            if (EditorLogic.fetch.ship != null)
            {
                List<Part> parts = EditorLogic.fetch.ship.parts;
                foreach (Part p in parts)
                {
                    List<PartResource> prl = p.Resources.list;
                    foreach (PartResource res in prl)
                    {
                        if (res.resourceName == "Antimatter")
                        {
                            antimatterTanks.Add(p);
                            antimatterResources.Add(res);
                        }
                    }
                }
            }
            if (antimatterResources.Count > 0)
            {
                ShowLaunchAlert();
            }
            else
            {
                EditorLogic.fetch.launchVessel();
            }
        }

        void ShowLaunchAlert()
        {
            showUI = true;
            mainWindowPos.x = Screen.width / 2f - mainWindowPos.width / 2f;
            mainWindowPos.y = Screen.height / 2f - mainWindowPos.height / 2f;
            EditorLogic.fetch.Lock(true, true, true, "AMFactoryLock");
        }
        // Launch the vessel
        void LaunchVessel()
        {
            //ClearAntimatterFromVessel();
            ConsumeAntimatter();
            EditorLogic.fetch.launchVessel();
        }
        // Removes the antimatter from the current tanks
        void ClearAntimatterFromVessel()
        {
            foreach (PartResource a in antimatterResources)
            {
                a.amount = 0d;
            }
        }
        void ConsumeAntimatter()
        {
            double total = 0d;
            foreach (PartResource res in antimatterResources)
            {
                total = total + res.amount;
            }

            AntimatterFactory.Instance.ConsumeAntimatter(total);
        }

        bool showUI = false;
        private bool initStyles = false;

        private Rect mainWindowPos = new Rect(5, 15, 350, 120);

        private GUIStyle entryStyle;
        private GUIStyle windowStyle;
        private GUIStyle buttonStyle;
        int windowIdentifier;
        System.Random randomizer;

        private void InitStyles()
        {
            entryStyle = new GUIStyle(HighLogic.Skin.textArea);
            entryStyle.active = entryStyle.hover = entryStyle.normal;
            windowStyle = new GUIStyle(HighLogic.Skin.window);
            buttonStyle = new GUIStyle(HighLogic.Skin.button);
            initStyles = true;

        }

        void OnGUI()
        {
            if (!initStyles)
                InitStyles();
            if (showUI)
            {
                mainWindowPos = GUILayout.Window(windowIdentifier, mainWindowPos, DrawWindow, "Antimatter Factory", windowStyle, GUILayout.MinHeight(20), GUILayout.ExpandHeight(true));
            }
        }

        void DrawWindow(int WindowID)
        {
            GUILayout.BeginVertical();
            usedAM = 0d;
            for (int i = 0; i< antimatterTanks.Count; i++)
            {
                DrawAntimatterTank(antimatterTanks[i], antimatterResources[i]);
            }
            GUILayout.EndVertical();
            GUILayout.Label(String.Format("Required Antimatter: {0:F2}", usedAM));
            GUILayout.Label(String.Format("Available Antimatter: {0:F2}", availableAM));
            GUILayout.BeginHorizontal();

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Launch"))
            {
                showUI = false;
                EditorLogic.fetch.Unlock("AMFactoryLock");
                LaunchVessel();
                
            }
            if (GUILayout.Button("Abort Launch"))
            {
                showUI = false;
                EditorLogic.fetch.Unlock("AMFactoryLock");
            }
            GUILayout.EndHorizontal();
        }

        void DrawAntimatterTank(Part p, PartResource res)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(p.partInfo.title);

            res.amount = (double)GUILayout.HorizontalSlider((float)res.amount, 0f, (float)res.maxAmount);
            GUILayout.Label(String.Format("{0:F3}/{1:F3}",res.amount,res.maxAmount));
            GUILayout.EndHorizontal();
            usedAM += res.amount;
        }

    }
}
