using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FarFutureTechnologies;
using KSP.UI.Screens;

namespace FarFutureTechnologies.UI
{
  public class UIResources
  {

    private Dictionary<string, AtlasIcon> iconList;
    private Dictionary<string, GUIStyle> styleList;
    private Dictionary<string, Color> colorList;

    private Texture generalIcons;


    // Get any color, given its name
    public Color GetColor(string name)
    {
        return colorList[name];
    }

    // Get any icon, given its name
    public AtlasIcon GetIcon(string name)
    {
      return iconList[name];
    }


    // Get a style, given its name
    public GUIStyle GetStyle(string name)
    {
      return styleList[name];
    }

    // Constructor
    public UIResources()
    {
      CreateIconList();
      CreateStyleList();
      CreateColorList();
    }

    // Iniitializes the icon database
    private void CreateIconList()
    {
      generalIcons = (Texture)GameDatabase.Instance.GetTexture("FarFutureTechnologies/UI/icon_general", false);

      iconList = new Dictionary<string, AtlasIcon>();

      // Add the general icons
      iconList.Add("antimatter", new AtlasIcon(generalIcons, 0.00f, 0.75f, 0.25f, 0.25f));
      iconList.Add("factory", new AtlasIcon(generalIcons, 0.25f, 0.75f, 0.25f, 0.25f));
      iconList.Add("pump", new AtlasIcon(generalIcons, 0.50f, 0.75f, 0.25f, 0.25f));
      iconList.Add("timer", new AtlasIcon(generalIcons, 0.75f, 0.75f, 0.25f, 0.25f));
    }

    // Initializes all the styles
    private void CreateStyleList()
    {
        styleList = new Dictionary<string, GUIStyle>();

        GUIStyle draftStyle;

        // Window
        draftStyle = new GUIStyle(HighLogic.Skin.window);
        draftStyle.padding = new RectOffset(draftStyle.padding.left, draftStyle.padding.right, draftStyle.padding.bottom, draftStyle.padding.bottom);
        draftStyle.alignment = TextAnchor.UpperLeft;
        styleList.Add("window_main", new GUIStyle(draftStyle));
        // Toolbar window
        draftStyle = new GUIStyle(draftStyle);
        styleList.Add("window_toolbar", new GUIStyle(draftStyle));
        // Basic text
        draftStyle = new GUIStyle(HighLogic.Skin.label);
        draftStyle.fontSize = 11;
        draftStyle.alignment = TextAnchor.UpperLeft;
        draftStyle.normal.textColor = new Color(192f / 255f, 196f / 255f, 176f / 255f);
        styleList.Add("text_basic", new GUIStyle(draftStyle));
        // Enhanced text_basic
        draftStyle = new GUIStyle(draftStyle);
        draftStyle.alignment = TextAnchor.UpperRight;
        draftStyle.normal.textColor = new Color(107f / 255f, 201f / 255f, 238f / 255f);
        draftStyle.padding = new RectOffset(2, 2, 2, 2);
        styleList.Add("text_label", new GUIStyle(draftStyle));
        // Basic of all texts
        draftStyle = new GUIStyle(draftStyle);
        draftStyle.alignment = TextAnchor.UpperRight;
        draftStyle.normal.textColor = new Color(255f / 255f, 255f / 255f, 255f / 255f);
        draftStyle.padding = new RectOffset(2, 2, 2, 2);
        styleList.Add("text_colored", new GUIStyle(draftStyle));
        // Progress bar
        // background
        draftStyle = new GUIStyle(HighLogic.Skin.textField);
        draftStyle.active = draftStyle.hover = draftStyle.normal;
        styleList.Add("bar_background", new GUIStyle(draftStyle));
        // foreground
        draftStyle = new GUIStyle(HighLogic.Skin.button);
        draftStyle.active = draftStyle.hover = draftStyle.normal;
        draftStyle.border = GetStyle("bar_background").border;
        draftStyle.padding = GetStyle("bar_background").padding;
        styleList.Add("bar_foreground", new GUIStyle(draftStyle));
        // Overlaid button
        draftStyle = new GUIStyle(HighLogic.Skin.button);
        draftStyle.normal.textColor = draftStyle.normal.textColor;
        styleList.Add("button_overlaid", new GUIStyle(draftStyle));
        // Area Background
        draftStyle = new GUIStyle(HighLogic.Skin.textArea);
        draftStyle.active = draftStyle.hover = draftStyle.normal;
        draftStyle.padding = new RectOffset(0,0,0,0);
        styleList.Add("block_background", new GUIStyle(draftStyle));
        // Baisic button
        draftStyle = new GUIStyle(HighLogic.Skin.button);
        draftStyle.normal.textColor = draftStyle.normal.textColor;
        styleList.Add("button_basic", new GUIStyle(draftStyle));
        // Small text button
        draftStyle = new GUIStyle(HighLogic.Skin.button);
        draftStyle.normal.textColor = draftStyle.normal.textColor;
        draftStyle.fontSize = 12;
        styleList.Add("button_mini", new GUIStyle(draftStyle));

        draftStyle = new GUIStyle(HighLogic.Skin.scrollView);
        styleList.Add("scroll_area", new GUIStyle(draftStyle));
        draftStyle = new GUIStyle(HighLogic.Skin.verticalScrollbarThumb);
        styleList.Add("scroll_thumb", new GUIStyle(draftStyle));
        draftStyle = new GUIStyle(HighLogic.Skin.verticalScrollbar);
        styleList.Add("scroll_bar", new GUIStyle(draftStyle));

        draftStyle = new GUIStyle(HighLogic.Skin.horizontalSlider);
        styleList.Add("slider", new GUIStyle(draftStyle));

        draftStyle = new GUIStyle(HighLogic.Skin.horizontalSliderThumb);
        styleList.Add("slider_thumb", new GUIStyle(draftStyle));

        // Box
        draftStyle = new GUIStyle(HighLogic.Skin.textArea);
        draftStyle.normal.background = null;
        styleList.Add("item_box", new GUIStyle(draftStyle));
        // Header1
        draftStyle = new GUIStyle(HighLogic.Skin.label);
        draftStyle.fontStyle = FontStyle.Bold;
        draftStyle.alignment = TextAnchor.UpperLeft;
        draftStyle.fontSize = 12;
        draftStyle.stretchWidth = true;
        styleList.Add("header_basic", new GUIStyle(draftStyle));
        // Header 2
        draftStyle.alignment = TextAnchor.MiddleCenter;
        styleList.Add("header_center", new GUIStyle(draftStyle));

        // Text area
        draftStyle = new GUIStyle(HighLogic.Skin.textArea);
        draftStyle.active = draftStyle.hover = draftStyle.normal;
        draftStyle.fontSize = 11;
        styleList.Add("text_area", new GUIStyle(draftStyle));

        // Toggle
        draftStyle = new GUIStyle(HighLogic.Skin.toggle);
        draftStyle.normal.textColor = draftStyle.normal.textColor;
        styleList.Add("button_toggle", new GUIStyle(draftStyle));

        // Accept button
        draftStyle = new GUIStyle(HighLogic.Skin.button);
        draftStyle.normal.textColor = draftStyle.normal.textColor;
        styleList.Add("button_accept", new GUIStyle(draftStyle));
        // Cancel button
        draftStyle = new GUIStyle(HighLogic.Skin.button);
        draftStyle.normal.textColor = draftStyle.normal.textColor;
        styleList.Add("button_cancel", new GUIStyle(draftStyle));

    }
    void CreateColorList()
    {
      colorList = new Dictionary<string, Color>();

      colorList.Add("cancel_color", new Color(208f / 255f, 131f / 255f, 86f / 255f));
      colorList.Add("accept_color", new Color(209f / 255f, 250f / 255f, 146f / 255f));
      colorList.Add("bar_blue", new Color(107f / 255f, 201f / 255f, 238f / 255f));

      colorList.Add("profile_axis", new Color(200f / 255f, 200f / 255f, 200f / 255f));
      colorList.Add("profile_0", new Color(0f / 255f, 255f / 255f, 0f / 255f));
      colorList.Add("profile_1", new Color(255f / 255f, 0f / 255f, 0f / 255f));
      colorList.Add("profile_2", new Color(0f / 255f, 0f / 255f, 255f / 255f));
      colorList.Add("profile_3", new Color(238f / 255f, 66f / 255f, 244f / 255f));
      colorList.Add("profile_4", new Color(66f / 255f, 255f / 255f, 232f / 255f));
      colorList.Add("profile_5", new Color(2550f / 255f, 232f / 255f, 66f / 255f));

      //colorList.Add("readout_green", new Color(203f / 255f, 238f / 255f, 115f / 255f));
    }
  }

  // Represents an atlased icon via a source texture and rectangle
  public class AtlasIcon
  {
    public Texture iconAtlas;
    public Rect iconRect;

    public AtlasIcon(Texture theAtlas, float bl_x, float bl_y, float x_size, float y_size)
    {
      iconAtlas = theAtlas;
      iconRect = new Rect(bl_x, bl_y, x_size, y_size);
    }
  }

}
