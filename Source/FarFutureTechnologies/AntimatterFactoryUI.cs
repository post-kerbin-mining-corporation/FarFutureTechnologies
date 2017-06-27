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
        private bool showMiniWindow = false;

        private bool showMainWindow = false;
        private bool showLaunchMode = false;

        private bool initStyles = false;

        private Rect miniWindowPos = new Rect(75, 100, 250, 50);
        private Rect mainWindowPos = new Rect(75, 100, 250, 50);

        private GUIStyle entryStyle;
        private GUIStyle scrollStyle;
        private GUIStyle scrollBarStyle;
        private GUIStyle scrollThumbStyle;
        private GUIStyle guiBodyTextStyle;
        private GUIStyle guiAMLabelTextStyle;

        private GUIStyle windowStyle;
        private GUIStyle miniWindowStyle;
        private GUIStyle buttonStyle;
        private GUIStyle miniButtonStyle;

        GUIStyle progressBarBG;
        GUIStyle progressBarFG;
        GUIStyle slider;
        GUIStyle sliderThumb;

        System.Random randomizer;
        int windowIdentifier;
        int windowIdentifier2;
        private static ApplicationLauncherButton stockToolbarButton = null;
        public static AntimatterFactoryUI Instance { get; private set; }

        public void Awake()
        {
            Utils.Log("[AM Factory UI]: Awake");
            Instance = this;
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

            miniWindowStyle = new GUIStyle(windowStyle);
            miniWindowStyle.border = new RectOffset(0, 0,0,0);
            miniWindowStyle.padding = new RectOffset(0, 0, 0, 0);

            // button
            buttonStyle = new GUIStyle(HighLogic.Skin.button);
            miniButtonStyle = new GUIStyle(buttonStyle);
            miniButtonStyle.fontSize = 12;

            slider = new GUIStyle(HighLogic.Skin.horizontalSlider);
            sliderThumb = new GUIStyle(HighLogic.Skin.horizontalSliderThumb);
            // slider
            initStyles = true;

        }

        public void Start()
        {
            Utils.Log("[AM Factory UI]: Start");


            if (ApplicationLauncher.Ready)
                OnGUIAppLauncherReady();

            randomizer = new System.Random(335462);
            windowIdentifier = randomizer.Next();
            windowIdentifier2 = randomizer.Next();

            if (HighLogic.LoadedSceneIsFlight)
            {

            }
        }



        void ShowLoading()
        {
            showMainWindow = true;
            showLaunchMode = true;
            mainWindowPos.width = 400f;
            mainWindowPos.height = 400f;
            mainWindowPos.x = Screen.width / 2f - mainWindowPos.width / 2f;
            mainWindowPos.y = Screen.height / 2f - mainWindowPos.height / 2f;
        }
        void ShowFactory()
        {
            showMainWindow = true;
            showLaunchMode = false;
            mainWindowPos.width = 300f;
            mainWindowPos.height = 300f;
            mainWindowPos.x = Screen.width / 2f - mainWindowPos.width / 2f;
            mainWindowPos.y = Screen.height / 2f - mainWindowPos.height / 2f;
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
            else
            {
                if (showMiniWindow)
                {
                    Vector3 pos = stockToolbarButton.GetAnchor();



                    if (ApplicationLauncher.Instance.IsPositionedAtTop)
                    {
                        miniWindowPos = new Rect(Screen.width-280f, 0f, 250f, 60f);
                    }
                    else {
                        miniWindowPos = new Rect(Screen.width - 280f, Screen.height-150f, 250f, 60f);
                    }

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

            if (AntimatterFactory.Instance != null)
            {
                if (showMiniWindow)
                {
                    miniWindowPos = GUI.Window(windowIdentifier, miniWindowPos, DrawMiniWindow, "", windowStyle);
                }

                if (showMainWindow)
                {
                    mainWindowPos = GUILayout.Window(windowIdentifier2, mainWindowPos, DrawMainWindow, "Antimatter Factory", windowStyle, GUILayout.MinHeight(20), GUILayout.ExpandHeight(true));
                }
            }
        }

        void DrawMiniWindow(int WindowID)
        {

            float curAM = (float)(AntimatterFactory.Instance.Antimatter);
            float maxAM = (float)(AntimatterFactory.Instance.AntimatterMax);
            float rateAM = (float)(AntimatterFactory.Instance.AntimatterRate);

            float tempAreaWidth = 200f;
            float tempBarWidth = 200f;
            Rect tempArea = new Rect(10f, 0f, tempAreaWidth, 40f);

            float tempBarFGSize = Mathf.Clamp((tempBarWidth-6f) * (curAM / maxAM), 5f, tempBarWidth);

            GUI.BeginGroup(tempArea);
            GUI.Box(new Rect(0f, 10f, tempBarWidth, 20f), "", progressBarBG);
            GUI.color = new Color(107f / 255f, 201f / 255f, 238f / 255f);
            GUI.Box(new Rect(3f, 11f, tempBarFGSize, 18f), "", progressBarFG);
            GUI.color = Color.white;
            GUI.Label(new Rect(20f, 10f, 160f, 20f), String.Format("<color=#ffffff>{0:F2} / {1:F0}</color>", curAM, maxAM), guiBodyTextStyle);

            GUI.Label(new Rect(tempBarWidth - 90f, 10f, 90f, 20f), String.Format("<color=#ffffff>({0:F2} u/day)</color>", rateAM), guiAMLabelTextStyle);
            // GUI.Label(new Rect(20f+tempBarWidth, 30f, 40f, 20f), String.Format("{0:F0} K", meltdownTemp), gui_text);
            GUI.EndGroup();
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                if (GUI.Button(new Rect(50f, 35f, 150, 20), "Enter Facility", buttonStyle))
                {
                    ShowFactory();
                }
            }

            if (HighLogic.LoadedSceneIsFlight && AntimatterLoader.Instance != null && AntimatterLoader.Instance.loadingAllowed)
                if (GUI.Button(new Rect(50f, 35f, 150, 20), "Load Antimatter", buttonStyle))
                {
                    ShowLoading();
                }

            //GUILayout.Label(String.Format("<color=#ffa500ff><b>Level {0}</b></color>", AntimatterFactory.Instance.FactoryLevel + 1), guiBodyTextStyle);
        }

        void DrawMainWindow(int WindowID)
        {
            if (showLaunchMode)
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
            GUI.skin = HighLogic.Skin;

            AntimatterLoader.Instance.availableAM = AntimatterFactory.Instance.Antimatter;
            AntimatterLoader.Instance.usedAM = 0d;

            // Fuelling helper buttons
            GUILayout.BeginVertical(entryStyle);
            //GUILayout.Label("Fuelling Helpers", guiAMLabelTextStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Fill All Tanks", miniButtonStyle))
            {
                AntimatterLoader.Instance.FillAllTanks();
            }
            if (GUILayout.Button("Empty All Tanks", miniButtonStyle))
            {
                AntimatterLoader.Instance.EmptyAllTanks();
            }
            if (GUILayout.Button("Even All Tanks", miniButtonStyle))
            {
                AntimatterLoader.Instance.EvenAllTanks();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            // Tank display
            GUILayout.BeginVertical(entryStyle);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition,scrollBarStyle, scrollBarStyle, GUILayout.MinWidth(370f), GUILayout.MinHeight(250f));
            for (int i = 0; i < AntimatterLoader.Instance.antimatterTanks.Count; i++)
            {
                DrawAMContainer(AntimatterLoader.Instance.antimatterTanks[i]);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            // Results


            GUILayout.BeginHorizontal(entryStyle);
            GUILayout.Label(String.Format("<b>Available Antimatter: {0:F2}</b>", availableAM), guiBodyTextStyle);
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();

            if (AntimatterLoader.Instance.usedAM < 0d)
            {
                GUILayout.Label(String.Format("<b><color=#66badb>This operation will return some antimatter\nRefunded Antimatter: {0:F2}</color></b>", -AntimatterLoader.Instance.usedAM), guiAMLabelTextStyle);
            } else if (AntimatterLoader.Instance.usedAM > AntimatterLoader.Instance.availableAM)
            {
                GUILayout.Label(String.Format("<b><color=#f30802>There is not enough Antimatter in storage\nRequired Antimatter: {0:F2}</color></b>", AntimatterLoader.Instance.usedAM), guiAMLabelTextStyle);
            } else
            {
                GUILayout.Label(String.Format("<b><color=#7fa542>Ready for fuelling\nRequired Antimatter: {0:F2}</color></b>", AntimatterLoader.Instance.usedAM), guiAMLabelTextStyle);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            // Actions
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close", buttonStyle, GUILayout.Width(60f)))
            {
                showMainWindow = false;
            }
            GUILayout.FlexibleSpace();
            // If antimatter used is negative, we will be unloading
            // If positive but greater than capacity, we cannot do anything
            // else just consume
            if (AntimatterLoader.Instance.usedAM < 0d)
            {
              GUI.enabled = true;
              if (GUILayout.Button("<color=#7fa542>Unload Antimatter</color>", buttonStyle, GUILayout.Width(180f)))
              {
                  AntimatterLoader.Instance.ConsumeAntimatter();
              }
            }
            else if (AntimatterLoader.Instance.usedAM > AntimatterLoader.Instance.availableAM)
            {
                GUI.enabled = false;
                GUILayout.Button("<color=#f30802>Insufficient Antimatter</color>", buttonStyle, GUILayout.Width(180f));
            }
            else
            {
                GUI.enabled = true;
                if (GUILayout.Button("<color=#7fa542>Load Antimatter</color>", buttonStyle, GUILayout.Width(180f)))
                {
                    AntimatterLoader.Instance.ConsumeAntimatter();
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
            if (GUILayout.Button("Close", buttonStyle, GUILayout.Width(60f)))
            {
                showMainWindow = false;
            }

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

        void DrawAMContainer(AntimatterContainer tank)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("<b>" + tank.part.partInfo.title +"</b>", guiBodyTextStyle, GUILayout.MaxWidth(180f));

            GUILayout.BeginVertical();
            tank.totalAmount = (double)GUILayout.HorizontalSlider((float)tank.totalAmount, 0f, (float)tank.resource.maxAmount, slider, sliderThumb, GUILayout.MinWidth(110f), GUILayout.MaxWidth(110f));
            tank.requestedAmount = tank.totalAmount - tank.resource.amount;
            GUILayout.Label(String.Format("{0:F2} / {1:F2}", tank.totalAmount, tank.resource.maxAmount), guiAMLabelTextStyle);
            GUILayout.EndVertical();
            if (tank.requestedAmount >= 0d)
              GUILayout.Label(String.Format("+{0:F2}", tank.requestedAmount), guiAMLabelTextStyle);
            else
              GUILayout.Label(String.Format("{0:F2}", tank.requestedAmount), guiAMLabelTextStyle);
            GUILayout.EndHorizontal();

            AntimatterLoader.Instance.usedAM += tank.requestedAmount;
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
            showMiniWindow = !showMiniWindow;
            stockToolbarButton.SetTexture((Texture)GameDatabase.Instance.GetTexture(showMiniWindow ? "FarFutureTechnologies/UI/toolbar_on" : "FarFutureTechnologies/UI/toolbar_off", false));
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
