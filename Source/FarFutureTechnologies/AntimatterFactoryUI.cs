using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;

namespace FarFutureTechnologies
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class AntimatterFactoryUI:MonoBehaviour
    {

        double availableAM = 0d;
        double usedAM = 0d;
        List<Part> antimatterTanks = new List<Part>();
        List<PartResource> antimatterResources = new List<PartResource>();

        private bool showToolbarButton = false;
        private bool uiShown = false;
        private bool launchMode = false;
        private bool initStyles = false;

        private Rect mainWindowPos = new Rect(75, 100, 150, 120);

        private GUIStyle entryStyle;
        private GUIStyle scrollStyle;
        private GUIStyle scrollBarStyle;
        private GUIStyle scrollThumbStyle;
        private GUIStyle guiBodyTextStyle;
        private GUIStyle guiAMLabelTextStyle;

        private GUIStyle windowStyle;
        private GUIStyle buttonStyle;

        GUIStyle progressBarBG;
        GUIStyle progressBarFG;
        GUIStyle slider;
        GUIStyle sliderThumb;

        System.Random randomizer;
        int windowIdentifier;
        private static ApplicationLauncherButton stockToolbarButton = null;

        public void Awake()
        {
            Utils.Log("UI: Awake");
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
        }

        private void InitStyles()
        {
            // areas
            entryStyle = new GUIStyle(HighLogic.Skin.textArea);
            entryStyle.active = entryStyle.hover = entryStyle.normal;

            scrollStyle = new GUIStyle(HighLogic.Skin.scrollView);
            scrollBarStyle = new GUIStyle(HighLogic.Skin.verticalScrollbar);
            scrollThumbStyle = new GUIStyle(HighLogic.Skin.verticalScrollbarThumb);

            // text
            guiBodyTextStyle = new GUIStyle(HighLogic.Skin.label);
            guiBodyTextStyle.alignment = TextAnchor.UpperLeft;
            guiBodyTextStyle.fontSize = 11;
            guiBodyTextStyle.normal.textColor = new Color(192f / 255f, 196f / 255f, 176f / 255f);

            guiAMLabelTextStyle = new GUIStyle(guiBodyTextStyle);
            guiAMLabelTextStyle.alignment = TextAnchor.UpperRight;
            guiAMLabelTextStyle.normal.textColor = new Color(107f / 255f, 201f / 255f, 238f / 255f);
            guiAMLabelTextStyle.padding = new RectOffset(2, 2, 2, 2);

            // bars
            progressBarBG = new GUIStyle(HighLogic.Skin.textField);
            progressBarBG.active = progressBarBG.hover = progressBarBG.normal;

            progressBarFG = new GUIStyle(HighLogic.Skin.button);
            progressBarFG.active = progressBarBG.hover = progressBarBG.normal;
            progressBarFG.border = progressBarBG.border;
            progressBarFG.padding = progressBarBG.padding;

            windowStyle = new GUIStyle(HighLogic.Skin.window);
            windowStyle.alignment = TextAnchor.UpperLeft;

            // button
            buttonStyle = new GUIStyle(HighLogic.Skin.button);

            slider = new GUIStyle(HighLogic.Skin.horizontalSlider);
            sliderThumb = new GUIStyle(HighLogic.Skin.horizontalSliderThumb);
            // slider
            initStyles = true;

        }

        public void Start()
        {
            Utils.Log("UI: Start");

       
            if (ApplicationLauncher.Ready)
                OnGUIAppLauncherReady();

            randomizer = new System.Random(335462);
            windowIdentifier = randomizer.Next();

            if (HighLogic.LoadedSceneIsEditor)
            {
                EditorLogic.fetch.launchBtn.onClick.RemoveListener(new UnityEngine.Events.UnityAction(EditorLogic.fetch.launchVessel));
                EditorLogic.fetch.launchBtn.onClick.AddListener(new UnityEngine.Events.UnityAction((AntimatterLaunchCheck)));
            }
        }


        // Editor Logic
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
                    List<PartResource> prl = p.Resources.ToList();
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
            uiShown = true;
            launchMode = true;
            mainWindowPos.x = Screen.width / 2f - mainWindowPos.width / 2f;
            mainWindowPos.y = Screen.height / 2f - mainWindowPos.height / 2f;
        
            mainWindowPos.width = 400f;
            
            EditorLogic.fetch.Lock(true, true, true, "AMFactoryLock");
        }
        void ClearLaunchAlert()
        {
            uiShown = false;
            launchMode = false;
            mainWindowPos.width = 150f;
            
            EditorLogic.fetch.Unlock("AMFactoryLock");
        }

        // Launch the vessel
        void LaunchVessel()
        {
            ClearLaunchAlert();
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
            usedAM = 0d;
        }
        void ConsumeAntimatter()
        {
            double total = 0d;
            foreach (PartResource res in antimatterResources)
            {
                total = total + res.amount;
            }

            AntimatterFactory.Instance.ScheduleConsumeAntimatter(total);
        }
        void EmptyAllTanks()
        {
            ClearAntimatterFromVessel();
        }
        void FillAllTanks()
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
        void EvenAllTanks()
        {
 
            double each = availableAM / (double)antimatterResources.Count;
            foreach (PartResource res in antimatterResources)
            {
                res.amount = Math.Min(each, res.maxAmount);
                
            }
        }

        void FixedUpdate()
        {
            if (!showToolbarButton)
            {
                if (AntimatterFactory.Instance.Researched)
                {
                    showToolbarButton = true;
                    ResetAppLauncher();
                }
            }
        }

        // UI FUNCTIONS
        void OnGUI()
        {
            if (Event.current.type == EventType.Repaint || Event.current.isMouse)
            {
            }
            OnUIDraw();
        }

        void OnUIDraw()
        {
            if (!initStyles)
                InitStyles();
            if (uiShown)
            {
                mainWindowPos = GUILayout.Window(windowIdentifier, mainWindowPos, DrawMainWindow, "Antimatter Factory", windowStyle, GUILayout.MinHeight(20), GUILayout.ExpandHeight(true));
            }
        }

        void DrawMainWindow(int WindowID)
        {
            if (launchMode)
            {
                DrawLaunchMode();
            }
            else
            {
                DrawFactoryMode();
            }
           
            GUI.DragWindow();
        }
        public Vector2 scrollPosition = Vector2.zero;
        void DrawLaunchMode()
        {
            availableAM = AntimatterFactory.Instance.Antimatter;
            GUILayout.BeginVertical(entryStyle);
            GUI.skin = HighLogic.Skin;
            scrollPosition = GUILayout.BeginScrollView(scrollPosition,scrollBarStyle, scrollBarStyle, GUILayout.MinWidth(370f), GUILayout.MinHeight(250f));

            
            usedAM = 0d;
            for (int i = 0; i < antimatterTanks.Count; i++)
            {
                 DrawAMContainer(antimatterTanks[i], antimatterResources[i]);
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.Label(String.Format("<b><color=#66badb>Required Antimatter: {0:F2}</color></b>", usedAM), guiBodyTextStyle);
            
            
            if (usedAM > availableAM)
            {
                GUILayout.Label(String.Format("<b><color=#f30802>Available Antimatter: {0:F2}</color></b>", availableAM), guiBodyTextStyle);
            } else 
            {
                GUILayout.Label(String.Format("<b><color=#7fa542>Available Antimatter: {0:F2}</color></b>", availableAM), guiBodyTextStyle);
            }
         
            GUILayout.BeginHorizontal(entryStyle);
            if (GUILayout.Button("Fill All Tanks", buttonStyle))
            {
                FillAllTanks();
            }
            if (GUILayout.Button("Empty All Tanks", buttonStyle))
            {
                EmptyAllTanks();
            }
            if (GUILayout.Button("Even All Tanks", buttonStyle))
            {
                EvenAllTanks();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Abort Launch", buttonStyle))
            {
                ClearLaunchAlert();
            }
            if (usedAM > availableAM)
            {
                GUI.enabled = false;
                GUILayout.Button("<color=#f30802>Insufficient Antimatter</color>", buttonStyle);
            }
            else
            {

                GUI.enabled = true;
                if (GUILayout.Button("<color=#7fa542>Launch</color>", buttonStyle))
                {
                    ClearLaunchAlert();
                    LaunchVessel();
                }
            }
            
            GUI.enabled = true;
            
            GUILayout.EndHorizontal();
        }

        void DrawFactoryMode()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal(entryStyle);
            GUILayout.Label("The Antimatter Factory produces a steady stream of Antimatter for the KSC's advanced propulsion needs, up to a maxiumum capacity.", guiBodyTextStyle);

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label(String.Format("<color=#ffa500ff><b>Facility Level {0}</b></color>", AntimatterFactory.Instance.FactoryLevel + 1), guiBodyTextStyle);
            GUILayout.Label(String.Format("<b><color=#ffffff>Status:</color> {0}</b>", AntimatterFactory.Instance.GetStatusString()), guiBodyTextStyle);
            GUILayout.EndVertical();
            if (!AntimatterFactory.Instance.IsMaxLevel())
            {
                if (GUILayout.Button(String.Format("<b>Upgrade\n<color=#ffa500ff>{0}</color></b>", FormatPrice(AntimatterFactory.Instance.GetNextLevelCost())), buttonStyle))
                {
                    TryUpgradeFactory(AntimatterFactory.Instance.GetNextLevelCost());
                }
            }
            else
            {
                GUILayout.Space(100f);
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(entryStyle);


            float curAM = (float)(AntimatterFactory.Instance.Antimatter);
            float maxAM = (float)(AntimatterFactory.Instance.AntimatterMax);
            float rateAM = (float)(AntimatterFactory.Instance.AntimatterRate);

            float tempAreaWidth = 250f;
            float tempBarWidth = 250f;
            Rect tempArea = GUILayoutUtility.GetRect(tempAreaWidth, 60f);
            Rect barArea = new Rect(20f, 20f, tempBarWidth, 40f);

            float tempBarFGSize = tempBarWidth * (curAM / maxAM);

            GUI.BeginGroup(tempArea);
            GUI.Box(new Rect(0f, 10f, tempBarWidth, 10f), "", progressBarBG);
            GUI.color = new Color(107f / 255f, 201f / 255f, 238f / 255f);
            GUI.Box(new Rect(0f, 11f, tempBarFGSize, 7f), "", progressBarFG);
            GUI.color = Color.white;
            GUI.Label(new Rect(tempBarWidth - 80f, 23f, 80f, 20f), String.Format("{0:F2} u", curAM), guiAMLabelTextStyle);
            GUI.Label(new Rect(tempBarWidth - 80f, 38f, 80f, 20f), String.Format("of {0:F2} u", maxAM), guiAMLabelTextStyle);

            GUI.Label(new Rect(0f, 23f, 50f, 20f), String.Format("+ {0:F2} u/day", rateAM), guiAMLabelTextStyle);

            // GUI.Label(new Rect(20f+tempBarWidth, 30f, 40f, 20f), String.Format("{0:F0} K", meltdownTemp), gui_text);
            GUI.EndGroup();

            GUILayout.EndHorizontal();


            GUILayout.EndVertical();
        }
        void TryUpgradeFactory(float cost)
        {
            if (Funding.Instance.Funds < cost)
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage("Not enough Funds to upgrade the facilty", 5f, ScreenMessageStyle.UPPER_CENTER));
            }
            else
            {
                Funding.Instance.AddFunds(-cost, TransactionReasons.Structures);
                AntimatterFactory.Instance.Upgrade();
            }
        }

        void DrawAMContainer(Part p, PartResource res)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>" + p.partInfo.title +"</b>", guiBodyTextStyle);

            res.amount = (double)GUILayout.HorizontalSlider((float)res.amount, 0f, (float)res.maxAmount, slider, sliderThumb, GUILayout.MinWidth(110f), GUILayout.MaxWidth(110f));
            GUILayout.Label(String.Format("{0:F2} / {1:F2}", res.amount, res.maxAmount), guiAMLabelTextStyle);
            GUILayout.EndHorizontal();
            usedAM += res.amount;
        }


        // Misc
        string FormatPrice(float num)
        {
            return String.Format("£{0:n0}", num);
        }

        // Toolbar Handling

        void ResetAppLauncher()
        {
            //FindReactors();
            if (stockToolbarButton == null)
            {
                if (showToolbarButton)
                {
                    stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
                    OnToolbarButtonToggle,
                    OnToolbarButtonToggle,
                    DummyVoid,
                    DummyVoid,
                    DummyVoid,
                    DummyVoid,
                     ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.MAPVIEW,
                    (Texture)GameDatabase.Instance.GetTexture("FarFutureTechnologies/UI/toolbar_off", false));
                }
                else
                {
                }
            }
            else
            {
                if (showToolbarButton)
                {
                }
                else
                {
                   
                    GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
                    ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
                }
            }

        }

        public void OnDestroy()
        {

            // Remove the stock toolbar button
            GameEvents.onGUIApplicationLauncherReady.Remove(OnGUIAppLauncherReady);
            if (stockToolbarButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
            }

        }

        private void OnToolbarButtonToggle()
        {
            uiShown = !uiShown;
            stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(uiShown ? "FarFutureTechnologies/UI/toolbar_on" : "FarFutureTechnologies/UI/toolbar_off", false));

        }


        void OnGUIAppLauncherReady()
        {
            if (stockToolbarButton == null && showToolbarButton)
            {
                stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(
                    OnToolbarButtonToggle,
                    OnToolbarButtonToggle,
                    DummyVoid,
                    DummyVoid,
                    DummyVoid,
                    DummyVoid,
                    ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.SPACECENTER | ApplicationLauncher.AppScenes.MAPVIEW,
                    (Texture)GameDatabase.Instance.GetTexture("FarFutureTechnologies/UI/toolbar_off", false));
            }
        }

        void OnGUIAppLauncherDestroyed()
        {
            if (stockToolbarButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
                stockToolbarButton = null;
            }
        }

        void onAppLaunchToggleOff()
        {
            stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture("FarFutureTechnologies/UI/toolbar_off", false));
        }

        void DummyVoid() { }
    }
}
