using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemHeat;
using UnityEngine;

namespace FarFutureTechnologies
{

  public enum ChargeState
  {
    Ready, Charging, Running
  }
  public class FusionReactor : PartModule
  {
    /// CONFIGURABLE FIELDS
    // ----------------------

    // --- General -----
    [KSPField(isPersistant = true)]
    public bool Enabled = false;

    // Current fuel mode name
    [KSPField(isPersistant = true)]
    public string CurrentModeID = null;


    [KSPField(isPersistant = false)]
    public float MinimumReactorPower = 0.1f;


    [KSPField(isPersistant = false)]
    public float CurrentPowerProduced = 0f;

    // --- Charge-up -----
    [KSPField(isPersistant = true)]
    public bool Charged = false;

    [KSPField(isPersistant = true)]
    public bool Charging = false;

    [KSPField(isPersistant = true)]
    public float CurrentCharge = 0f;

    [KSPField(isPersistant = true, guiActive = true, guiName = "Charge Rate", groupName = "fusionreactor", groupDisplayName = "Fusion Reactor", groupStartCollapsed = false), UI_FloatRange(minValue = 10f, maxValue = 1000f, stepIncrement = 10f)]
    public float ChargeRate = 50f;

    [KSPField(isPersistant = false)]
    public float ChargeGoal = 500000f;

    // --- Heat ---
    // Heat generation at full power
    [KSPField(isPersistant = false)]
    public float SystemPower;

    [KSPField(isPersistant = false)]
    public float ShutdownTemperature = 2000f;

    [KSPField(isPersistant = false)]
    public float SystemOutletTemperature;

    [KSPField(isPersistant = false)]
    public string ModuleID = "";

    [KSPField(isPersistant = false)]
    public string HeatModuleID = "";

    // --- Model Lights ---
    [KSPField(isPersistant = false)]
    public string ChargingLightRootTransformName;

    [KSPField(isPersistant = false)]
    public string OnLightTransformName = "Lights_On";

    [KSPField(isPersistant = false)]
    public string OffLightTransformName = "Lights_Off";

    [KSPField(isPersistant = false)]
    public string ModeLightTransformName = "Lights_Mode";

    // name of the overheat color changer
    [KSPField(isPersistant = false)]
    public string OverheatColorChangerName;



    /// UI
    /// ---------------------
    /// // Current fuel mode
    [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "#LOC_FFT_ModuleFusionReactor_Field_CurrentModeIndex_Title", groupName = "fusionreactor", groupDisplayName = "#LOC_FFT_ModuleFusionReactor_UIGroup_Title", groupStartCollapsed = false)]
    [UI_ChooseOption(affectSymCounterparts = UI_Scene.None, scene = UI_Scene.All, suppressEditorShipModified = true)]
    public int currentModeIndex = 0;

    // Heat Status string
    [KSPField(isPersistant = false, guiActive = true, guiName = "#LOC_FFT_ModuleFusionReactor_Field_HeatOutput_Title", groupName = "fusionreactor", groupDisplayName = "#LOC_FFT_ModuleFusionReactor_UIGroup_Title", groupStartCollapsed = true)]
    public string HeatOutput;

    // Reactor Status string
    [KSPField(isPersistant = false, guiActive = true, guiName = "#LOC_FFT_ModuleFusionReactor_Field_ReactorOutput_Title", groupName = "fusionreactor", groupDisplayName = "#LOC_FFT_ModuleFusionReactor_UIGroup_Title", groupStartCollapsed = false)]
    public string ReactorOutput;

    // Fuel Status string
    [KSPField(isPersistant = false, guiActive = true, guiName = "#LOC_FFT_ModuleFusionReactor_Field_FuelInput_Title", groupName = "fusionreactor", groupDisplayName = "#LOC_FFT_ModuleFusionReactor_UIGroup_Title", groupStartCollapsed = false)]
    public string FuelInput;
    // Vessel Temperature
    [KSPField(isPersistant = false, guiActive = true, guiName = "#LOC_FFT_ModuleFusionReactor_Field_CoreTemp_Title", groupName = "fusionreactor", groupDisplayName = "#LOC_FFT_ModuleFusionReactor_UIGroup_Title", groupStartCollapsed = false)]
    public string CoreTemp;

    [KSPField(isPersistant = false, guiActive = true, guiName = "#LOC_FFT_ModuleFusionReactor_Field_ChargeStatus_Title", groupName = "fusionreactor", groupDisplayName = "#LOC_FFT_ModuleFusionReactor_UIGroup_Title", groupStartCollapsed = false)]
    public string ChargeStatus = "N/A";

    /// KSPACTIONS
    /// ----------------------
    [KSPAction("Enable Startup Charging")]
    public void EnableChargingAction(KSPActionParam param) { EnableCharging(); }

    [KSPAction("Disable Startup Charging")]
    public void DisableChargingAction(KSPActionParam param) { DisableCharging(); }

    [KSPAction("Toggle Startup Charging")]
    public void ToggleChargingAction(KSPActionParam param)
    {
      Charging = !Charging;
    }

    [KSPAction("Enable Reactor")]
    public void EnableAction(KSPActionParam param) { EnableReactor(); }

    [KSPAction("Disable Reactor")]
    public void DisableAction(KSPActionParam param) { DisableReactor(); }

    [KSPAction("Toggle Reactor")]
    public void ToggleAction(KSPActionParam param)
    {
      if (!Enabled) EnableReactor();
      else DisableReactor();
    }

    /// KSPEVENTS
    /// ----------------------

    [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "#LOC_FFT_ModuleFusionReactor_Event_EnableCharging_Title", active = true,
      groupName = "fusionreactor", groupDisplayName = "#LOC_FFT_ModuleFusionReactor_UIGroup_Title", groupStartCollapsed = false)]
    public void EnableCharging()
    {
      Charging = true;
    }
    [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "#LOC_FFT_ModuleFusionReactor_Event_DisableCharging_Title", active = false,
      groupName = "fusionreactor", groupDisplayName = "#LOC_FFT_ModuleFusionReactor_UIGroup_Title", groupStartCollapsed = false)]
    public void DisableCharging()
    {
      Charging = false;
    }
    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "#LOC_FFT_ModuleFusionReactor_Event_Enable_Title", active = true,
      groupName = "fusionreactor", groupDisplayName = "#LOC_FFT_ModuleFusionReactor_UIGroup_Title", groupStartCollapsed = false)]
    public void EnableReactor()
    {
      ReactorActivated();
    }
    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "#LOC_FFT_ModuleFusionReactor_Event_Disable_Title", active = false,
      groupName = "fusionreactor", groupDisplayName = "#LOC_FFT_ModuleFusionReactor_UIGroup_Title", groupStartCollapsed = false)]
    public void DisableReactor()
    {
      ReactorDeactivated();
    }


    protected Color enabledColor = Color.yellow;
    protected Color disabledColor = Color.red;
    protected Color chargeLightColor;
    protected Color chargeLightColorOff;
    protected List<Renderer> chargeLights;
    protected Renderer onLight;
    protected Renderer offLight;
    protected Renderer modeLight;

    protected ChargeState chargeState;
    protected float reactorThrottle;
    protected double fuelConsumption;
    protected float powerGenerated;

    protected List<FusionReactorMode> modes;
    protected ModuleSystemHeat heatModule;

    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_FFT_ModuleFusionReactor_ModuleName");
    }

    public override string GetInfo()
    {
      string msg = Localizer.Format("#LOC_FFT_ModuleFusionReactor_PartInfo", (SystemPower).ToString());
      foreach (FusionReactorMode mode in modes)
      {
        msg += Localizer.Format("#LOC_FFT_ModuleFusionReactor_PartInfo_Mode",
            mode.modeName,
            mode.powerGeneration.ToString("F0"));
        foreach (ResourceRatio input in mode.inputs)
        {
          msg += Localizer.Format("#LOC_FFT_ModuleFusionReactor_PartInfo_Fuel",
            input.ResourceName, input.Ratio.ToString("F5"));
        }
      }
      return msg;
    }

    public override void OnLoad(ConfigNode node)
    {
      base.OnLoad(node);
      ConfigNode[] varNodes = node.GetNodes("FUSIONMODE");
      modes = new List<FusionReactorMode>();
      for (int i = 0; i < varNodes.Length; i++)
      {
        modes.Add(new FusionReactorMode(varNodes[i]));
      }
    }

    public virtual void Start()
    {
      /// Reload nodes as needed
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        if (modes == null || modes.Count == 0)
        {
          ConfigNode node = GameDatabase.Instance.GetConfigs("PART").
              Single(c => part.partInfo.name == c.name).config.
              GetNodes("MODULE").Single(n => n.GetValue("name") == moduleName);
          OnLoad(node);
        }
      }
      SetupUI();
      SetupAnimations();
      SetupHeat();
      SetupRecharge();

      ChangeMode(currentModeIndex);

    }

    public virtual void FixedUpdate()
    {
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {

        RechargeCapacitors();
        GenerateHeat();

        HandleAnimation();

        if (HighLogic.LoadedSceneIsFlight)
        {
          GeneratePower();
        }
        else
        {
          GeneratePowerEditor();
        }

      }
    }
    public void Update()
    {
      if (Enabled)
      {
        ReactorOutput = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_ReactorOutput_Running", (modes[currentModeIndex].powerGeneration * reactorThrottle).ToString("F1"));
        FuelInput = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_FuelInput_Running", fuelConsumption.ToString("F4"));
      }
      else
      {
        ReactorOutput = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_ReactorOutput_Offline");
        FuelInput = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_FuelInput_Offline");
      }

      CoreTemp = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_CoreTemp_Running", heatModule.LoopTemperature.ToString("F0"), SystemOutletTemperature.ToString("F0"));

      if (Events["EnableCharging"].active == Charging || Events["DisableCharging"].active != Charging)
      {
        Events["DisableCharging"].active = Charging;
        Events["EnableCharging"].active = !Charging;
      }
      if (Events["EnableReactor"].active == Enabled || Events["DisableReactor"].active != Enabled)
      {
        Events["DisableReactor"].active = Enabled;
        Events["EnableReactor"].active = !Enabled;
      }
    }

    /// <summary>
    /// Sets up the UI
    /// </summary>
    void SetupUI()
    {
      if (modes.Count > 1)
      {
        var chooseField = Fields["currentModeIndex"];
        var chooseOption = (UI_ChooseOption)chooseField.uiControlEditor;
        chooseOption.options = modes.Select(s => s.modeName).ToArray();
        chooseOption.onFieldChanged = UpdateModesFromControl;
        chooseOption = (UI_ChooseOption)chooseField.uiControlFlight;
        chooseOption.options = modes.Select(s => s.modeName).ToArray();
        chooseOption.onFieldChanged = UpdateModesFromControl;
      }
      else
      {
        Fields["currentModeIndex"].guiActive = false;
        Fields["currentModeIndex"].guiActiveEditor = false;
      }


    }

    /// <summary>
    /// Sets up Animations
    /// </summary>
    void SetupAnimations()
    {
      if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
      {

        chargeLightColor = new Color(0f / 255f, 76f / 255f, 131f / 255f, 0.5f);
        chargeLightColorOff = new Color(0f / 255f, 76f / 255f, 131f / 255f, 0f);
        chargeLights = new List<Renderer>();
        Transform chargeLightParent = part.FindModelTransform(ChargingLightRootTransformName);
        if (!chargeLightParent)
          Utils.Log($"[FusionReactor] Couldn't find ChargingLightRootTransformName {ChargingLightRootTransformName}");
        else
          foreach (Transform child in chargeLightParent)
          {
            chargeLights.Add(child.GetComponent<Renderer>());
          }

        Transform onLightParent = part.FindModelTransform(OnLightTransformName);
        if (!onLightParent)
          Utils.Log($"[FusionReactor] Couldn't find OnLightTransformName {OnLightTransformName}");
        else
          onLight = onLightParent.GetComponent<Renderer>();
        Transform offLightParent = part.FindModelTransform(OffLightTransformName);
        if (!offLightParent)
          Utils.Log($"[FusionReactor] Couldn't find OffLightTransformName {OffLightTransformName}");
        else
          offLight = offLightParent.GetComponent<Renderer>();
        Transform modeLightParent = part.FindModelTransform(ModeLightTransformName);
        if (!modeLightParent)
          Utils.Log($"[FusionReactor] Couldn't find ModeLightTransformName {ModeLightTransformName}");
        else
          modeLight = modeLightParent.GetComponent<Renderer>();


      }
    }

    /// <summary>
    /// Sets up heat related parameters
    /// </summary>
    void SetupHeat()
    {
      if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
      {
        heatModule = part.GetComponents<ModuleSystemHeat>().ToList().Find(x => x.moduleID == HeatModuleID);
      }
    }

    /// <summary>
    /// Sets up the reactor recharge/discharge parameters
    /// </summary>
    void SetupRecharge()
    {
      if (Enabled)
      {
        SetChargeStateUI(ChargeState.Running);
      }
      else
      {
        if (CurrentCharge >= ChargeGoal) SetChargeStateUI(ChargeState.Ready);
        else SetChargeStateUI(ChargeState.Charging);
      }
    }

    /// <summary>
    /// Delegate method called when the fuel mode button is pressed
    /// </summary>
    /// <param name="field"></param>
    /// <param name="oldFieldValueObj"></param>
    void UpdateModesFromControl(BaseField field, object oldFieldValueObj)
    {
      ChangeMode(currentModeIndex);
    }

    /// <summary>
    /// Handles updating animations
    /// </summary>
    void HandleAnimation()
    {
      if (onLight && offLight)
      {
        if (Enabled)

        {
          onLight.material.SetColor("_TintColor", new Color(0.0f, 1.0f, 0.0f, 0.4f));
          offLight.material.SetColor("_TintColor", Color.black);
        }
        else
        {
          offLight.material.SetColor("_TintColor", new Color(1.0f, 0.0f, 0.0f, 0.4f));
          onLight.material.SetColor("_TintColor", Color.black);
        }
      }
    }

    /// <summary>
    ///
    /// Handles heat generation
    /// </summary>
    void GenerateHeat()
    {
      if (Enabled)
      {

        HeatOutput = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_HeatOutput_Running", (SystemPower).ToString("F0"));
        heatModule.AddFlux(ModuleID, SystemOutletTemperature, SystemPower);
      }
      else
      {
        heatModule.AddFlux(ModuleID, 0f, 0f);
        HeatOutput = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_HeatOutput_Offline");
      }
      if (HighLogic.LoadedSceneIsFlight)
        if (heatModule.LoopTemperature >= ShutdownTemperature)
        {
          ReactorDeactivated();
          ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_FFT_ModuleFusionReactor_Message_Overheat",
                                                                              part.partInfo.title),
                                                                     5.0f,
                                                                     ScreenMessageStyle.UPPER_CENTER));
        }
    }


    public virtual void ReactorDeactivated()
    {
      Debug.Log("[ModuleFusionReactor]: Reactor Shutdown");
      Charged = false;
      CurrentCharge = 0f;
      Enabled = false;
    }

    public virtual void ReactorActivated()
    {
      Debug.Log("[ModuleFusionReactor]: Reactor Enabled");

      if (!Charged)
      {
        if (FarFutureTechnologySettings.DebugModules)
          Utils.Log(String.Format("[FusionReactor]: Disabling due to insufficient charge"));
        ReactorDeactivated();
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_FFT_ModuleFusionReactor_Message_OutOfCharge",
                                                                            part.partInfo.title),
                                                                   5.0f,
                                                                   ScreenMessageStyle.UPPER_CENTER));
        return;
      }
      Charged = false;
      CurrentCharge = 0f;
      Enabled = true;
    }

    /// <summary>
    /// Consumes fuel to maintaina minimum consumption
    /// </summary>
    void GeneratePower()
    {
      if (Enabled)
      {
        double shipEC = 0d;
        double shipMaxEC = 0d;
        // Determine need for power
        part.GetConnectedResourceTotals(PartResourceLibrary.ElectricityHashcode, out shipEC, out shipMaxEC, true);

        // Power should be the higher of the minimum consumption and the required power.
        float minGeneration = MinimumReactorPower * modes[currentModeIndex].powerGeneration * TimeWarp.fixedDeltaTime;
        float idealGeneration = Mathf.Min(modes[currentModeIndex].powerGeneration * TimeWarp.fixedDeltaTime, (float)(shipMaxEC - shipEC));
        float powerToGenerate = Mathf.Max(minGeneration, idealGeneration);

        reactorThrottle = powerToGenerate / (modes[currentModeIndex].powerGeneration * TimeWarp.fixedDeltaTime);
        powerGenerated = modes[currentModeIndex].powerGeneration * reactorThrottle;
        fuelConsumption = 0d;


        bool fuelCheckPassed = true;
        for (int i = 0; i < modes[currentModeIndex].inputs.Count; i++)
        {
          double request = reactorThrottle * modes[currentModeIndex].inputs[i].Ratio * TimeWarp.fixedDeltaTime;
          double amount = part.RequestResource(PartResourceLibrary.Instance.GetDefinition(modes[currentModeIndex].inputs[i].ResourceName).id, request, modes[currentModeIndex].inputs[i].FlowMode);
          if (amount < 0.0000000000001)
          {
            ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_FFT_ModuleFusionReactor_Message_OutOfFuel",
                                                                           part.partInfo.title),
                                                                  10.0f,
                                                                  ScreenMessageStyle.UPPER_CENTER));
            ReactorDeactivated();
            fuelCheckPassed = false;
          }
          else
          {
            fuelConsumption += amount / TimeWarp.fixedDeltaTime;
          }
        }

        if (fuelCheckPassed)
        {
          CurrentPowerProduced = powerGenerated;
          part.RequestResource(PartResourceLibrary.ElectricityHashcode, -powerToGenerate, ResourceFlowMode.ALL_VESSEL);
        }
        else
        {
          CurrentPowerProduced = 0f;
        }
      }
      else
      {
        CurrentPowerProduced = 0f;
      }

    }

    /// <summary>
    /// Consumes fuel to maintaina minimum consumption
    /// </summary>
    void GeneratePowerEditor()
    {
      
      CurrentPowerProduced = modes[currentModeIndex].powerGeneration;
    }

    /// <summary>
    /// Handles capacitor recharge
    /// </summary>
    void RechargeCapacitors()
    {
      if (Enabled)
      {
        if (chargeState != ChargeState.Running)
          SetChargeStateUI(ChargeState.Running);
      }
      else
      {
        if (Charging && !Charged)
        {
          double req = part.RequestResource(PartResourceLibrary.ElectricityHashcode, (double)(ChargeRate * TimeWarp.fixedDeltaTime), ResourceFlowMode.ALL_VESSEL);
          CurrentCharge = Mathf.MoveTowards(CurrentCharge, ChargeGoal, (float)req);

          if (req > 0.0d)
          {
            ChargeStatus = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_ChargeStatus_Normal", (CurrentCharge / ChargeGoal * 100.0f).ToString("F1"));
          }
          else
          {
            ChargeStatus = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_ChargeStatus_NoPower");
          }

          if (CurrentCharge >= ChargeGoal)
          {
            ChargeStatus = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_ChargeStatus_Ready");
            Charged = true;
            if (chargeState != ChargeState.Ready)
              SetChargeStateUI(ChargeState.Ready);
          }
          else
          {

            if (chargeState != ChargeState.Charging)
              SetChargeStateUI(ChargeState.Charging);
          }
        }
        if (!Charging && CurrentCharge == 0f)
        {
          ChargeStatus = Localizer.Format("#LOC_FFT_ModuleFusionReactor_Field_ChargeStatus_NotCharging");
        }
      }

      if (chargeLights.Count > 0)
      {
        float fractionalCharge = CurrentCharge / ChargeGoal;
        int numLights = Mathf.RoundToInt(fractionalCharge * chargeLights.Count);
        for (int i = 0; i < chargeLights.Count; i++)
        {

          chargeLights[i].material.SetColor("_TintColor", Color.Lerp(chargeLightColorOff, chargeLightColor, Mathf.Clamp01(fractionalCharge * numLights - i)));
        }
      }
    }

    /// <summary>
    /// Toggles the UI between a "ready" mode and a "recharging" mode
    /// </summary>
    /// <param name="isCharging"></param>
    protected virtual void SetChargeStateUI(ChargeState newState)
    {
      chargeState = newState;
      if (newState == ChargeState.Ready)
      {
        Fields["ChargeStatus"].guiActive = true;
        Fields["CoreTemp"].guiActive = false;
        Fields["HeatOutput"].guiActive = false;
        Fields["ReactorOutput"].guiActive = false;
        Fields["FuelInput"].guiActive = false;
        Events["EnableReactor"].guiActive = true;
      }
      if (newState == ChargeState.Running)
      {
        Fields["ChargeStatus"].guiActive = false;
        Fields["CoreTemp"].guiActive = true;
        Fields["HeatOutput"].guiActive = true;
        Fields["ReactorOutput"].guiActive = true;
        Fields["FuelInput"].guiActive = true;
        Events["EnableReactor"].guiActive = false;
      }
      if (newState == ChargeState.Charging)
      {
        Fields["ChargeStatus"].guiActive = true;
        Fields["CoreTemp"].guiActive = false;
        Fields["HeatOutput"].guiActive = false;
        Fields["ReactorOutput"].guiActive = false;
        Fields["FuelInput"].guiActive = false;
        Events["EnableReactor"].guiActive = false;
      }
    }

    /// <summary>
    /// Switches the reactor's mode to a new mode
    /// </summary>
    /// <param name="newMode"></param>
    ///
    void ChangeMode(int modeIndex)
    {
      // Turn off the reactor when switching fuels
      if (Enabled)
      {
        if (FarFutureTechnologySettings.DebugModules)
          Utils.Log(String.Format("[FusionReactor]: Disabling due to fuel change"));
        ReactorDeactivated();
      }

      modeLight.material.SetColor("_TintColor", modes[modeIndex].modeColor);
      if (FarFutureTechnologySettings.DebugModules)
        Utils.Log(String.Format("[FusionReactor]: Fuel Mode was changed to {0}", modes[modeIndex].modeID));

    }

  }

}
