using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using KSP.Localization;

namespace FarFutureTechnologies.UI
{
  public class ProfilingUI:MonoBehaviour
  {
      private Vector2 plotTextureSize = new Vector2(800, 200);

      private Texture2D graphTexture;
      private bool showWindow = false;
      private List<ResourceProfile> drawnProfiles;

      public void ShowProfileWindow(List<ResourceProfile> profiles)
      {
        showWindow = true;
        drawnProfiles = profiles;
        GeneratePlotTexture();

      }


            void Draw()
            {

            }

      void GeneratePlotTexture()
      {
        graphTexture = new Texture2D(plotTextureSize.x, plotTextureSize.y, TextureFormat.ARGB32, false);
        foreach (ResourceProfile profile in profiles) {

          CreateLine(graphTexture, profile.concentrations, Color.White, plotTextureSize.x/profile.maxDistance, plotTextureSize.y/profile.maxConcentration);
        }
        graphTexture.Apply();
      }



      // Draw a line into a texture2D
      void CreateLine ( Texture2D tex, Dictionary<float, float> vals, Color col, float xScale, float yScale) {
        foreach (var item in vals) {
            tex.SetPixel((int)(xScale*item.Key), (int)(yScale*item.Value), col);
        }
      }
  }


}
