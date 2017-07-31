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
      private Vector2 plotTextureSize = new Vector2(800, 400);

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

            Rect closeRect = new Rect(348f, 10f, 46f, 46f);
            Rect plotGroupRect = new Rect(10f,50f, 330f, 155f);

            GUI.Box(new Rect(10f, 10f, 330f, 32f), "", GUIResources.GetStyle("block_background"));
            GUI.Label(new Rect(10f, 10f, 330f, 32f), "Spectrometer Profile", GUIResources.GetStyle("header_center"));

            DrawPlot(plotGroupRect);


            if (GUI.Button(closeRect, "X", GUIResources.GetStyle("button_basic")))
                showWindow = false;

            GUI.DragWindow();
        }

        void DrawPlot(Rect group)
        {
            Rect plotRect = new Rect(30f, 10f, 275f, 120f);

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
            GUIUtility.RotateAroundPivot(90f, new Vector2(10f, 10f));
            
            GUI.Label(new Rect(0f, 0f, 120f, 25f), String.Format("Relative Abundance"), GUIResources.GetStyle("text_label"));

            GUIUtility.RotateAroundPivot(-90f, new Vector2(10f, 10f));

            GUI.Label(new Rect(5f, 130f, 60f, 25f), String.Format("0 km"), GUIResources.GetStyle("text_label"));
            GUI.Label(new Rect(235f, 130f, 80f, 25f), String.Format("{0} km", (drawnProfiles[0].maxDistance/1000f).ToString("F0")) , GUIResources.GetStyle("text_label"));
            GUI.EndGroup();
        }

      void GeneratePlotTexture()
      {
          graphTexture = new Texture2D((int)plotTextureSize.x, (int)plotTextureSize.y, TextureFormat.ARGB32, false);
          Utils.Log(String.Format("[ProfilingUI]: Generating texture of size {0}", plotTextureSize.ToString()));

          FillTexture( graphTexture, new Color(1f,1f,1f,0f));
          GenerateAxes();

          graphTexture.Apply();
          GenerateData();

          graphTexture.Apply();
      }

      void GenerateAxes()
      {

          Dictionary<float, float> xAxis1 = new Dictionary<float, float>();
          Dictionary<float, float> xAxis2 = new Dictionary<float, float>();

          xAxis1.Add(0f, 1f);
          xAxis1.Add(plotTextureSize.x, 0f);

          xAxis2.Add(0f, plotTextureSize.y-2);
          xAxis2.Add(plotTextureSize.x, plotTextureSize.y-2);

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

              Utils.Log(String.Format("[ProfilingUI]: max Distance: {0} \nmax Concentration: {1}", profile.maxDistance.ToString(), profile.maxConcentration.ToString()));
              CreateLine(graphTexture, profile.concentrations, profileColors[colorIndex], plotTextureSize.x / profile.maxDistance, plotTextureSize.y / profile.maxReadout);
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
            Utils.Log(String.Format("[ProfilingUI]: xScale: {0} \nyScale: {1}", xScale.ToString(), yScale.ToString()));

            

            for (int i = 1; i < tex.width-1; i++)
            {
                int y1 = Mathf.Clamp((int)(curve.Evaluate(i / xScale) * yScale),0,tex.height-1);

                tex.SetPixel((int)(i), y1 , col);
                tex.SetPixel((int)(i + 1), y1, col);
                tex.SetPixel((int)(i - 1), y1, col);

                tex.SetPixel((int)(i), y1 + 1, col);
                tex.SetPixel((int)(i + 1), y1 + 1, col);
                tex.SetPixel((int)(i - 1), y1 + 1, col);

                tex.SetPixel((int)(i), y1 - 1, col);
                tex.SetPixel((int)(i + 1), y1 - 1, col);
                tex.SetPixel((int)(i - 1), y1 - 1, col);
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
          //Utils.Log(String.Format("[ProfilingUI]: xCount: {0} \nyCount: {1}",xVals.Length.ToString(), yVals.Length.ToString()));
        }
        public float Evaluate(float x)
        {
            
          int lowerIndex = ClosestLower(x);
          int higherIndex = lowerIndex+1;

          float m = (yVals[higherIndex] - yVals[lowerIndex])/(xVals[higherIndex] - xVals[lowerIndex]);
          float b = yVals[higherIndex] - m * xVals[higherIndex];
          //Utils.Log(String.Format("[ProfilingUI]: x {0:F4}\ny: {1:F4}", x, m * x + b));

          return m * x + b;
        }

        int ClosestLower(float x)
        {
          for (int i = 1; i < xVals.Length; i++)
          {
            if (x >= xVals[i-1] && x < xVals[i] )
              return i-1;
          }
          return 0;
        }

      }
  }


}
