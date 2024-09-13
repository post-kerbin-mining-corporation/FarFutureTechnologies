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

  public class FarFutureTechnologiesSettings_AntimatterContainment : GameParameters.CustomParameterNode
  {

    [GameParameters.CustomParameterUI("antimatterContainment",
    title = "#LOC_FFT_Settings_AntimatterContainmentEnabled_Title",
    toolTip = "#LOC_FFT_Settings_AntimatteContainmentEnabled_Tooltip", autoPersistance = true)]
    public bool containmentEnabled = true;

    [GameParameters.CustomFloatParameterUI("antimatterDetonationRate",
      title = "#LOC_FFT_Settings_AntimatterDetonationRate_Title",
      maxValue = 5.0f, minValue = 0.01f, asPercentage = true, stepCount = 100,
      toolTip = "#LOC_FFT_Settings_AntimatterDetonationRate_Tooltip", autoPersistance = true)]
    public float detonationRate = 1.0f;

    [GameParameters.CustomFloatParameterUI("antimatterVolatility",
      title = "#LOC_FFT_Settings_AntimatterVolatility_Title",
      maxValue = 5.0f, minValue = 0.01f, asPercentage = true, stepCount = 100,
      toolTip = "#LOC_FFT_Settings_AntimatterVolatility_Tooltip", autoPersistance = true)]
    public float volatility = 1.0f;

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
        return Localizer.Format("#LOC_FFT_Settings_AntimatterContainment_Section_Title");
      }
    }

    public override int SectionOrder
    {
      get
      {
        return 1;
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
      if (containmentEnabled || member.Name == "antimatterContainment")
        return true;
      else
        return false;
    }

    public static bool ContainmentEnabled
    {
      get
      {
        if (HighLogic.LoadedScene == GameScenes.MAINMENU)
          return true;
        FarFutureTechnologiesSettings_AntimatterContainment settings = HighLogic.CurrentGame.Parameters.CustomParams<FarFutureTechnologiesSettings_AntimatterContainment>();
        return settings.containmentEnabled;
      }

      set
      {
        FarFutureTechnologiesSettings_AntimatterContainment settings = HighLogic.CurrentGame.Parameters.CustomParams<FarFutureTechnologiesSettings_AntimatterContainment>();
        settings.containmentEnabled = value;
      }
    }


    public static float DetonationRate
    {
      get
      {
        FarFutureTechnologiesSettings_AntimatterContainment settings = HighLogic.CurrentGame.Parameters.CustomParams<FarFutureTechnologiesSettings_AntimatterContainment>();
        return settings.detonationRate;
      }

      set
      {
        FarFutureTechnologiesSettings_AntimatterContainment settings = HighLogic.CurrentGame.Parameters.CustomParams<FarFutureTechnologiesSettings_AntimatterContainment>();
        settings.detonationRate = value;
      }
    }
    public static float Volatility
    {
      get
      {
        FarFutureTechnologiesSettings_AntimatterContainment settings = HighLogic.CurrentGame.Parameters.CustomParams<FarFutureTechnologiesSettings_AntimatterContainment>();
        return settings.volatility;
      }

      set
      {
        FarFutureTechnologiesSettings_AntimatterContainment settings = HighLogic.CurrentGame.Parameters.CustomParams<FarFutureTechnologiesSettings_AntimatterContainment>();
        settings.volatility = value;
      }
    }
  }

  public class FarFutureTechnologiesSettings_EngineDamage : GameParameters.CustomParameterNode
  {

    [GameParameters.CustomParameterUI("damageFacilities",
    title = "#LOC_FFT_Settings_EngineDamageFacilities_Title",
    toolTip = "#LOC_FFT_Settings_EngineDamageFacilities_Tooltip", autoPersistance = true)]
    public bool damageFacilities = true;

    [GameParameters.CustomFloatParameterUI("facilityDamageScale",
      title = "#LOC_FFT_Settings_EngineDamageFacilityScale_Title",
      maxValue = 5.0f, minValue = 0.01f, asPercentage = true, stepCount = 100,
      toolTip = "#LOC_FFT_Settings_EngineDamageFacilityScale_Tooltip", autoPersistance = true)]
    public float facilityDamageScale = 1.0f;


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
        return Localizer.Format("#LOC_FFT_Settings_EngineDamage_Section_Title");
      }
    }

    public override int SectionOrder
    {
      get
      {
        return 1;
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
      if (damageFacilities || member.Name == "damageFacilities")
        return true;
      else
        return false;
    }

    public static bool DamageFacilities
    {
      get
      {
        if (HighLogic.LoadedScene == GameScenes.MAINMENU)
          return true;
        FarFutureTechnologiesSettings_EngineDamage settings = HighLogic.CurrentGame.Parameters.CustomParams<FarFutureTechnologiesSettings_EngineDamage>();
        return settings.damageFacilities;
      }

      set
      {
        FarFutureTechnologiesSettings_EngineDamage settings = HighLogic.CurrentGame.Parameters.CustomParams<FarFutureTechnologiesSettings_EngineDamage>();
        settings.damageFacilities = value;
      }
    }


    public static float FacilityDamageScale
    {
      get
      {
        FarFutureTechnologiesSettings_EngineDamage settings = HighLogic.CurrentGame.Parameters.CustomParams<FarFutureTechnologiesSettings_EngineDamage>();
        return settings.facilityDamageScale;
      }

      set
      {
        FarFutureTechnologiesSettings_EngineDamage settings = HighLogic.CurrentGame.Parameters.CustomParams<FarFutureTechnologiesSettings_EngineDamage>();
        settings.facilityDamageScale = value;
      }
    }
  }
}