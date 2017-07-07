using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using KSP.Localization;

namespace FarFutureTechnologies.UI
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
  public class ProfilingUI:MonoBehaviour
  {
      private Vector2 plotTextureSize = new Vector2(1600, 800);

      private Texture2D graphTexture;
      private bool showWindow = false;
      private List<ResourceProfile> drawnProfiles;
      private Rect windowPos = new Rect(75, 100, 400, 220);
      System.Random randomizer;
       private UIResources resources;
       private int windowIdentifier;
        public UIResources GUIResources { get {return resources;}}
        public static ProfilingUI Instance { get; private set; }

        public Color[] profileColors;

      public void ShowProfileWindow(List<ResourceProfile> profiles)
      {
          drawnProfiles = profiles;
          GeneratePlotTexture();
        showWindow = true;



      }
       public void Awake()
        {

            Instance = this;
            resources = new UIResources();
           profileColors = new Color[6];
           profileColors[0] = GUIResources.GetColor("profile_0");
           profileColors[1] = GUIResources.GetColor("profile_1");
           profileColors[2] = GUIResources.GetColor("profile_2");
           profileColors[3] = GUIResources.GetColor("profile_3");
           profileColors[4] = GUIResources.GetColor("profile_4");
           profileColors[5] = GUIResources.GetColor("profile_5");
        }

      public void Start()
      {
          randomizer = new System.Random(3354622);
          windowIdentifier = randomizer.Next();
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
            if (showWindow)
            {
                windowPos = GUI.Window(windowIdentifier, windowPos, DrawPlottingWindow, "", GUIResources.GetStyle("window_toolbar"));
            }

        }

        void DrawPlottingWindow(int WindowID)
        {

            Rect closeRect = new Rect(350f, 10f, 48f, 48f);
            Rect plotGroupRect = new Rect(10f,50f, 330f, 155f);

            GUI.Box(new Rect(10f, 10f, 330f, 32f), "", GUIResources.GetStyle("block_background"));
            GUI.Label(new Rect(10f, 10f, 330f, 32f), "Spectrometer Profile", GUIResources.GetStyle("header_center"));

            DrawPlot(plotGroupRect);


            if (GUI.Button(closeRect, "X", GUIResources.GetStyle("button_basic")))
                showWindow = false;
        }

        void DrawPlot(Rect group)
        {
            Rect plotRect = new Rect(40f, 10f, 275f, 120f);

            GUI.BeginGroup(group, GUIResources.GetStyle("block_background"));
            GUI.DrawTexture(plotRect, graphTexture);

            int colorIndex = 0;
            foreach (ResourceProfile profile in drawnProfiles)
            {
                GUI.color = profileColors[colorIndex];
                GUI.Label(new Rect(236f, 15f + colorIndex * 18f, 80f, 25f), profile.resourceName, GUIResources.GetStyle("text_colored"));
                colorIndex++;
            }
            GUI.color = Color.white;
            GUIUtility.RotateAroundPivot(90f, new Vector2(20f, 65f));
            GUI.Label(new Rect(0f, 65f, 140f, 25f), String.Format("Relative Abundance"), GUIResources.GetStyle("text_label"));

            GUIUtility.RotateAroundPivot(-90f, new Vector2(20f, 65f));

            GUI.Label(new Rect(0f, 140f, 60f, 25f), String.Format("0 km"), GUIResources.GetStyle("text_label"));
            GUI.Label(new Rect(230f, 140f, 80f, 25f), String.Format("{0} km", (drawnProfiles[0].maxDistance/1000f).ToString("F0")) , GUIResources.GetStyle("text_label"));
            GUI.EndGroup();
        }

      void GeneratePlotTexture()
      {
          graphTexture = new Texture2D((int)plotTextureSize.x, (int)plotTextureSize.y, TextureFormat.ARGB32, false);
          FillTexture( graphTexture, new Color(1f,1f,1f,0f));

          GenerateAxes();
          GenerateData();


          graphTexture.Apply();
      }

      void GenerateAxes()
      {

          Dictionary<float, float> xAxis1 = new Dictionary<float, float>();
          Dictionary<float, float> xAxis2 = new Dictionary<float, float>();

          xAxis1.Add(0f, 0f);
          xAxis1.Add(plotTextureSize.x, 0f);
          xAxis2.Add(0f, plotTextureSize.y-1);
          xAxis2.Add(plotTextureSize.x, plotTextureSize.y-1);

          CreateLine(graphTexture, xAxis1, GUIResources.GetColor("profile_axis"), 1f, 1f);
          CreateLine(graphTexture, xAxis2, GUIResources.GetColor("profile_axis"), 1f, 1f);
      }
      void GenerateData()
      {
          // Draw the plots
          int colorIndex = 0;
          foreach (ResourceProfile profile in drawnProfiles)
          {
              if (profile.maxConcentration == 0f)
                  profile.maxConcentration = 1f;
              CreateLine(graphTexture, profile.concentrations, profileColors[colorIndex], plotTextureSize.x / profile.maxDistance, plotTextureSize.y / profile.maxConcentration);
              colorIndex++;
          }
      }



      // Draw a line into a texture2D
      void CreateLine ( Texture2D tex, Dictionary<float, float> vals, Color col, float xScale, float yScale) {

            PlotLine curve = new PlotLine(vals);
            //FloatCurve curve = new FloatCurve();
            //foreach (var item in vals) {
            //    curve.Add(item.Key * xScale, item.Value * yScale);
                //tex.SetPixel((int)(xScale*item.Key), (int)(yScale*item.Value), col);
            //}
            for (int i = 0; i < tex.width-1; i++)
            {
                tex.SetPixel(i, (int)(curve.Evaluate(i*xScale)*yScale), col);
                tex.SetPixel(i, (int)(curve.Evaluate(i*xScale+1)*yScale), col);
                tex.SetPixel(i+1, (int)(curve.Evaluate(i*xScale + 1)*yScale), col);
                tex.SetPixel(i, (int)(curve.Evaluate(i*xScale)*yScale), col);
            }
      }
      void FillTexture(Texture2D tex, Color col)
      {
          Color[] pxs = tex.GetPixels();
          for (int i = 0; i < pxs.Length; i++)
          {
              pxs[i] = col;
          }
          tex.SetPixels(pxs);
          tex.Apply();
      }

      public class PlotLine
      {
        float[] xVals;
        float[] yVals;

        public PlotLine (Dictionary<float, float> vals)
        {
          xVals = vals.Keys.ToArray();
          yVals = vals.Values.ToArray();
        }
        public float Evaluate(float x)
        {
          int lowerIndex = ClosestLower(x);
          int higherIndex = i+1;

          float m = (yVals[higherIndex] - yVals[lowerIndex])/(xVals[higherIndex] - xVals[lowerIndex]);
          y = m * x + yVals[lowerIndex]
        }

        int ClosestLower(float x)
        {
          for (int i = 1; i <xVals.Length; i++)
          {
            if (xVals[i-1] <= x && xVals[i] >= x)
              return i-1;
          }
          return 0;
        }

      }
  }


}
