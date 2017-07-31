using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using KSP.Localization;

namespace FarFutureTechnologies.UI
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

        System.Random randomizer;
        int windowIdentifier;
        int windowIdentifier2;
        private UIResources resources;
        private static ApplicationLauncherButton stockToolbarButton = null;
        public UIResources GUIResources { get {return resources;}}
        public static AntimatterFactoryUI Instance { get; private set; }

        public void Awake()
        {
            Utils.Log("[AM Factory UI]: Awake");
            Instance = this;
            resources = new UIResources();
            GameEvents.onGUIApplicationLauncherReady.Add(OnGUIAppLauncherReady);
            GameEvents.onGUIApplicationLauncherDestroyed.Add(OnGUIAppLauncherDestroyed);
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
            {}
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
            mainWindowPos.height = 250f;
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
                        miniWindowPos = new Rect(Screen.width-280f, 0f, 240f, 42f);
                    }
                    else
                    {
                        miniWindowPos = new Rect(Screen.width - 280f, Screen.height-92f, 245f, 42f);
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

            if (AntimatterFactory.Instance != null)
            {
                if (showMiniWindow)
                {
                    miniWindowPos = GUI.Window(windowIdentifier, miniWindowPos, DrawMiniWindow, "", GUIResources.GetStyle("window_toolbar"));
                }

                if (showMainWindow)
                {
                    mainWindowPos = GUILayout.Window(windowIdentifier2, mainWindowPos, DrawMainWindow,
                      new GUIContent(),
                      GUIResources.GetStyle("window_main"), GUILayout.MinHeight(20), GUILayout.ExpandHeight(true));
                }
            }
        }

        void DrawMiniWindow(int WindowID)
        {
            float curAM = (float)(AntimatterFactory.Instance.Antimatter);
            float maxAM = (float)(AntimatterFactory.Instance.AntimatterMax);
            float rateAM = (float)(AntimatterFactory.Instance.AntimatterRate);

            Rect barAreaRect = new Rect(39f, 0f, 160f, 32f);

            Vector2 barBackgroundSize = new Vector2(150, 20f);
            Vector2 barForegroundSize = new Vector2(Mathf.Max(barBackgroundSize.x * (curAM / maxAM),8f), 18f);

            Rect barBackgroundRect = new Rect(0f, 10f, barBackgroundSize.x, barBackgroundSize.y);
            Rect barForeroundRect = new Rect(0f, 11f, barForegroundSize.x, barForegroundSize.y);
            Rect storageTextRect = new Rect(5f, 10f, 160f, 20f);
            Rect rateTextRect = new Rect(barBackgroundSize.x - 90f, 10f, 90f, 20f);

            Rect amIconRect = new Rect(5f, 5f, 32f, 32f);
            Rect factoryButtonRect = new Rect (200f, 5f, 32f, 32f);
            Rect loadoutButtonRect = new Rect (200f, 5f, 32f, 32f);

            GUI.BeginGroup(barAreaRect);
            GUI.Box(barBackgroundRect, "", GUIResources.GetStyle("bar_background"));
            GUI.color = GUIResources.GetColor("bar_blue");
            GUI.Box(barForeroundRect, "", GUIResources.GetStyle("bar_foreground"));
            GUI.color = Color.white;

            GUI.Label(storageTextRect, String.Format("<color=#ffffff>{0:F2} / {1:F0}</color>", curAM, maxAM), GUIResources.GetStyle("text_basic"));
            GUI.Label(rateTextRect, Localizer.Format("#LOC_FFT_AntimatterFactoryUI_MiniWindow_Rate", rateAM.ToString("F2")), GUIResources.GetStyle("text_label"));

            GUI.EndGroup();
            GUI.DrawTextureWithTexCoords(amIconRect, GUIResources.GetIcon("antimatter").iconAtlas, GUIResources.GetIcon("antimatter").iconRect);

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                if (GUI.Button(factoryButtonRect, "", GUIResources.GetStyle("button_overlaid")))
                {
                    ShowFactory();
                }
                GUI.DrawTextureWithTexCoords(factoryButtonRect, GUIResources.GetIcon("factory").iconAtlas, GUIResources.GetIcon("factory").iconRect);
            }

            if (HighLogic.LoadedSceneIsFlight && AntimatterLoader.Instance != null && AntimatterLoader.Instance.loadingAllowed)
            {
                if (GUI.Button(loadoutButtonRect, "", GUIResources.GetStyle("button_overlaid")))
                {
                    ShowLoading();
                }
                GUI.DrawTextureWithTexCoords(loadoutButtonRect, GUIResources.GetIcon("pump").iconAtlas, GUIResources.GetIcon("pump").iconRect);
            }
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
            GUILayout.BeginHorizontal();
            GUILayout.Label(String.Format("{0}", Localizer.Format("#LOC_FFT_AntimatterFactoryUI_LoadoutWindow_Title")));
            GUILayout.EndHorizontal();

            AntimatterLoader.Instance.availableAM = AntimatterFactory.Instance.Antimatter;
            AntimatterLoader.Instance.usedAM = 0d;

            // Fuelling helper buttons
            GUILayout.BeginVertical(GUIResources.GetStyle("block_background"));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(Localizer.Format("#LOC_FFT_AntimatterFactoryUI_Loader_FillAllTanks"), GUIResources.GetStyle("button_mini")))
            {
                AntimatterLoader.Instance.FillAllTanks();
            }
            if (GUILayout.Button(Localizer.Format("#LOC_FFT_AntimatterFactoryUI_Loader_EmptyAllTanks"), GUIResources.GetStyle("button_mini")))
            {
                AntimatterLoader.Instance.EmptyAllTanks();
            }
            if (GUILayout.Button(Localizer.Format("#LOC_FFT_AntimatterFactoryUI_Loader_EvenAllTanks"), GUIResources.GetStyle("button_mini")))
            {
                AntimatterLoader.Instance.EvenAllTanks();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            // Tank display
            GUILayout.BeginVertical(GUIResources.GetStyle("block_background"));
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUIResources.GetStyle("scroll_bar"), GUIResources.GetStyle("scroll_bar"), GUILayout.MinWidth(370f), GUILayout.MinHeight(250f));
            for (int i = 0; i < AntimatterLoader.Instance.antimatterTanks.Count; i++)
            {
                DrawAMContainer(AntimatterLoader.Instance.antimatterTanks[i]);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            // Results


            GUILayout.BeginHorizontal(GUIResources.GetStyle("block_background"));
            GUILayout.Label(Localizer.Format("#LOC_FFT_AntimatterFactoryUI_LoadoutWindow_AvailableAM", availableAM.ToString("F2")), GUIResources.GetStyle("text_basic"));
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();

            if (AntimatterLoader.Instance.usedAM < 0d)
            {
                GUILayout.Label(Localizer.Format("#LOC_FFT_AntimatterFactoryUI_LoadoutWindow_LoadingResult_Refund", (-AntimatterLoader.Instance.usedAM).ToString("F2")), GUIResources.GetStyle("text_label"));
            } else if (AntimatterLoader.Instance.usedAM > AntimatterLoader.Instance.availableAM)
            {
                GUILayout.Label(Localizer.Format("#LOC_FFT_AntimatterFactoryUI_LoadoutWindow_LoadingResult_NotEnough", AntimatterLoader.Instance.usedAM.ToString("F2")), GUIResources.GetStyle("text_label"));
            } else
            {
                GUILayout.Label(Localizer.Format("#LOC_FFT_AntimatterFactoryUI_LoadoutWindow_LoadingResult_Ready", AntimatterLoader.Instance.usedAM.ToString("F2")), GUIResources.GetStyle("text_label"));
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            // Actions
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close", GUIResources.GetStyle("button_basic"), GUILayout.Width(60f)))
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
              if (GUILayout.Button(Localizer.Format("#LOC_FFT_AntimatterFactoryUI_LoadoutWindow_LoadButton_Refund"), GUIResources.GetStyle("button_basic"), GUILayout.Width(180f)))
              {
                  AntimatterLoader.Instance.ConsumeAntimatter();
              }
            }
            else if (AntimatterLoader.Instance.usedAM > AntimatterLoader.Instance.availableAM)
            {
                GUI.enabled = false;
                GUILayout.Button(Localizer.Format("#LOC_FFT_AntimatterFactoryUI_LoadoutWindow_LoadButton_NotEnough"), GUIResources.GetStyle("button_basic"), GUILayout.Width(180f));
            }
            else
            {
                GUI.enabled = true;
                if (GUILayout.Button(Localizer.Format("#LOC_FFT_AntimatterFactoryUI_LoadoutWindow_LoadButton_Ready"), GUIResources.GetStyle("button_basic"), GUILayout.Width(180f)))
                {
                    AntimatterLoader.Instance.ConsumeAntimatter();
                }
            }

            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        void DrawFactoryMode()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(String.Format("{0}", Localizer.Format("#LOC_FFT_AntimatterFactoryUI_LoadoutWindow_Title")));
            GUILayout.EndHorizontal();
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal(GUIResources.GetStyle("block_background"));
            GUILayout.Label(Localizer.Format("#LOC_FFT_AntimatterFactoryUI_FactoryWindow_Description"), GUIResources.GetStyle("text_basic"));

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label(Localizer.Format("#LOC_FFT_AntimatterFactoryUI_FactoryWindow_Level", AntimatterFactory.Instance.FactoryLevel + 1), GUIResources.GetStyle("text_basic"));
            GUILayout.Label(Localizer.Format("#LOC_FFT_AntimatterFactoryUI_FactoryWindow_Status", AntimatterFactory.Instance.GetStatusString()), GUIResources.GetStyle("text_basic"));
            GUILayout.EndVertical();
            if (!AntimatterFactory.Instance.IsMaxLevel())
            {
                if (GUILayout.Button(Localizer.Format("#LOC_FFT_AntimatterFactoryUI_FactoryWindow_Upgrade", FormatPrice(AntimatterFactory.Instance.GetNextLevelCost())), GUIResources.GetStyle("button_basic")))
                {
                    TryUpgradeFactory(AntimatterFactory.Instance.GetNextLevelCost());
                }
            }
            else
            {
                GUILayout.Space(100f);
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(GUIResources.GetStyle("block_background"));

            float curAM = (float)(AntimatterFactory.Instance.Antimatter);
            float maxAM = (float)(AntimatterFactory.Instance.AntimatterMax);
            float rateAM = (float)(AntimatterFactory.Instance.AntimatterRate);

            Rect barAreaRect = GUILayoutUtility.GetRect(280, 68f);

            Vector2 barBackgroundSize = new Vector2(260, 20f);
            Vector2 barForegroundSize = new Vector2(Mathf.Max(barBackgroundSize.x * (curAM / maxAM),8f), 18f);

            Rect barBackgroundRect = new Rect(10f, 10f, barBackgroundSize.x, barBackgroundSize.y);
            Rect barForeroundRect = new Rect(10f, 11f, barForegroundSize.x, barForegroundSize.y);

            Rect storageTextRect = new Rect(barBackgroundSize.x - 80f, 33f, 80f, 40f);

            Rect rateTextRect = new Rect(0f, 29f, 80f, 20f);

            GUI.BeginGroup(barAreaRect);
            GUI.Box(barBackgroundRect, "", GUIResources.GetStyle("bar_background"));
            GUI.color = GUIResources.GetColor("bar_blue");
            GUI.Box(barForeroundRect, "", GUIResources.GetStyle("bar_foreground"));
            GUI.color = Color.white;


            GUI.Label(storageTextRect, Localizer.Format("#LOC_FFT_AntimatterFactoryUI_FactoryWindow_Storage", curAM.ToString("F2"), maxAM.ToString("F2")),
              GUIResources.GetStyle("text_label"));
            GUI.Label(rateTextRect, Localizer.Format("#LOC_FFT_AntimatterFactoryUI_FactoryWindow_Rate", rateAM.ToString("F2")), GUIResources.GetStyle("text_label"));


            GUI.EndGroup();

            GUILayout.EndHorizontal();
            if (GUILayout.Button("Close", GUIResources.GetStyle("button_basic"), GUILayout.Width(60f)))
            {
                showMainWindow = false;
            }

            GUILayout.EndVertical();
        }
        void TryUpgradeFactory(float cost)
        {
            if (Funding.Instance.Funds < cost)
            {
                ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_FFT_AntimatterFactoryUI_Messages_NotEoughFunds"), 5f, ScreenMessageStyle.UPPER_CENTER));
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
            GUILayout.Label("<b>" + tank.part.partInfo.title +"</b>", GUIResources.GetStyle("text_basic"), GUILayout.MaxWidth(180f));

            GUILayout.BeginVertical();
            tank.totalAmount = (double)GUILayout.HorizontalSlider((float)tank.totalAmount, 0f, (float)tank.resource.maxAmount, GUIResources.GetStyle("slider"), GUIResources.GetStyle("slider_thumb"), GUILayout.MinWidth(110f), GUILayout.MaxWidth(110f));
            tank.requestedAmount = tank.totalAmount - tank.resource.amount;
            GUILayout.Label(String.Format("{0:F2} / {1:F2}", tank.totalAmount, tank.resource.maxAmount), GUIResources.GetStyle("text_label"));
            GUILayout.EndVertical();
            if (tank.requestedAmount >= 0d)
              GUILayout.Label(String.Format("+{0:F2}", tank.requestedAmount), GUIResources.GetStyle("text_label"));
            else
              GUILayout.Label(String.Format("{0:F2}", tank.requestedAmount), GUIResources.GetStyle("text_label"));
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
