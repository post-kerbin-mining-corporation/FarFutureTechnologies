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
        private bool uiShown = false;
        private bool initStyles = false;

        private Rect mainWindowPos = new Rect(5, 15, 150, 120);

        private GUIStyle entryStyle;
        private GUIStyle windowStyle;
        private GUIStyle buttonStyle;


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
            entryStyle = new GUIStyle(HighLogic.Skin.textArea);
            entryStyle.active = entryStyle.hover = entryStyle.normal;
            windowStyle = new GUIStyle(HighLogic.Skin.window);
            buttonStyle = new GUIStyle(HighLogic.Skin.button);
            initStyles = true;

        }

        public void Start()
        {
            Utils.Log("UI: Start");

       
            if (ApplicationLauncher.Ready)
                OnGUIAppLauncherReady();

            randomizer = new System.Random(335462);

            windowIdentifier = randomizer.Next();
        }
        private void OnGUI()
        {
            if (Event.current.type == EventType.Repaint || Event.current.isMouse)
            {
            }
            OnUIDraw();
        }

        
        public void OnUIDraw()
        {
            if (!initStyles)
                InitStyles();
            if (uiShown)
            {
                mainWindowPos = GUILayout.Window(windowIdentifier, mainWindowPos, DrawMainWindow, "Antimatter Factory", windowStyle, GUILayout.MinHeight(20), GUILayout.ExpandHeight(true));
            }
        }

        public void DrawMainWindow(int WindowID)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(String.Format("Current Level: {0}", AntimatterFactory.Instance.FactoryLevel));
            GUILayout.Label(String.Format("Current AM: {0}",AntimatterFactory.Instance.Antimatter));
            GUILayout.Label(String.Format("Current AM Rate: {0}", AntimatterFactory.Instance.AntimatterRate));
            GUILayout.Label(String.Format("Current AM Max: {0}", AntimatterFactory.Instance.AntimatterMax));
            GUILayout.EndVertical();
            GUI.DragWindow();
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
