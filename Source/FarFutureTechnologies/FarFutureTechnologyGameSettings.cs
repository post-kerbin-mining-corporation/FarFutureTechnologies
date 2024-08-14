using KSP.Localization;

namespace FarFutureTechnologies
{

  public class FarFutureTechnologiesSettings_Antimatter : GameParameters.CustomParameterNode
  {

    [GameParameters.CustomParameterUI("antimatterCostsScience",
      title = "#LOC_FFT_Settings_AntimatterCostsScience_Title",
      toolTip = "#LOC_FFT_Settings_AntimatterCostsScience_Tooltip",
      autoPersistance = true)]
    public bool antimatterCostsScience = true;


    [GameParameters.CustomFloatParameterUI("antimatterScienceCostPerUnit",
      title = "#LOC_FFT_Settings_AntimatterScienceCost_Title",
      maxValue = 10f, minValue = 0.00f, stepCount = 20,
      toolTip = "#LOC_FFT_Settings_AntimatterScienceCost_Tooltip", autoPersistance = true)]
    public float antimatterScienceCostPerUnit = 1f;


    public override string DisplaySection
    {
      get
      {
        return "#LOC_FFT_Settings_MainSection_Title";
      }
    }

    public override string Section
    {
      get
      {
        return "FFT";
      }
    }

    public override string Title
    {
      get
      {
        return Localizer.Format("#LOC_FFT_Settings_Antimatter_Section_Title");
      }
    }

    public override int SectionOrder
    {
      get
      {
        return 0;
      }
    }

    public override GameParameters.GameMode GameMode
    {
      get
      {
        return GameParameters.GameMode.ANY;
      }
    }

    public override bool HasPresets
    {
      get
      {
        return false;
      }
    }

    public override bool Enabled(System.Reflection.MemberInfo member, GameParameters parameters)
    {
      if (antimatterCostsScience || member.Name == "antimatterCostsScience")
        return true;
      else
        return false;
    }

    public static bool AntimatterCostsScience
    {
      get
      {
        if (HighLogic.LoadedScene == GameScenes.MAINMENU)
          return true;
        FarFutureTechnologiesSettings_Antimatter settings = HighLogic.CurrentGame.Parameters.CustomParams<FarFutureTechnologiesSettings_Antimatter>();
        return settings.antimatterCostsScience;
      }

      set
      {
        FarFutureTechnologiesSettings_Antimatter settings = HighLogic.CurrentGame.Parameters.CustomParams<FarFutureTechnologiesSettings_Antimatter>();
        settings.antimatterCostsScience = value;
      }
    }


    public static float AntimatterScienceCostPerUnit
    {
      get
      {
        FarFutureTechnologiesSettings_Antimatter settings = HighLogic.CurrentGame.Parameters.CustomParams<FarFutureTechnologiesSettings_Antimatter>();
        return settings.antimatterScienceCostPerUnit;
      }

      set
      {
        FarFutureTechnologiesSettings_Antimatter settings = HighLogic.CurrentGame.Parameters.CustomParams<FarFutureTechnologiesSettings_Antimatter>();
        settings.antimatterScienceCostPerUnit = value;
      }
    }
   

  }
}