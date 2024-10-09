using KSP.Localization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FarFutureTechnologies
{
  public class ModuleChargeableEngine : PartModule
  {

    // --- Charge-up -----
    // Charged flag
    [KSPField(isPersistant = true)]
    public bool Charged = false;

    // Charging state
    [KSPField(isPersistant = true)]
    public bool Charging = false;

    // Current charge level
    [KSPField(isPersistant = true)]
    public float CurrentCharge = 0f;

    // Slider for charge rate
    [KSPField(isPersistant = true, guiActive = true, guiName = "Charge Rate",
      groupName = "chargeengine", groupDisplayName = "#LOC_FFT_ModuleChargeableEngine_Group_DisplayName", groupStartCollapsed = false), UI_FloatRange(minValue = 10f, maxValue = 1000f, stepIncrement = 10f)]
    public float ChargeRate = 50f;

    // Target charge level
    [KSPField(isPersistant = false)]
    public float ChargeGoal = 500000f;

    // Is the engine running
    [KSPField(isPersistant = false)]
    public bool EngineOn = false;

    // Should the engine be shut down when the throttle is low?
    [KSPField(isPersistant = true)]
    public bool ShutdownEngineOnLowThrottle = true;

    // Fraction at which to kill the engine
    [KSPField(isPersistant = true)]
    public float MinimumThrottleFraction = 0.05f;

    // Persistent charge field
    [KSPField(isPersistant = true)]
    public float CurrentPowerConsumption = 0f;

    // --- Model Lights ---
    [KSPField(isPersistant = false)]
    public string ChargingLightRootTransformName;

    // UI FIELDS/ BUTTONS
    // Status string
    [KSPField(isPersistant = false, guiActive = true, guiName = "Capacitors",
      groupName = "chargeengine", groupDisplayName = "#LOC_FFT_ModuleChargeableEngine_Group_DisplayName", groupStartCollapsed = false)]
    public string ChargeStatus = "N/A";

    /// KSPEVENTS
    /// ----------------------

    [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "#LOC_FFT_ModuleChargeableEngine_Event_Enable_Title", active = true,
      groupName = "chargeengine", groupDisplayName = "#LOC_FFT_ModuleChargeableEngine_Group_DisplayName", groupStartCollapsed = false)]
    public void EnableCharging()
    {
      Charging = true;
    }
    [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "#LOC_FFT_ModuleChargeableEngine_Event_Disable_Title",
      active = false, groupName = "chargeengine", groupDisplayName = "#LOC_FFT_ModuleChargeableEngine_Group_DisplayName", groupStartCollapsed = false)]
    public void DisableCharging()
    {
      Charging = false;
    }
    /// KSPACTIONS
    /// ----------------------
    [KSPAction("Enable Startup Charging", guiName = "#LOC_FFT_ModuleChargeableEngine_Action_EnableAction_Title")]
    public void EnableChargingAction(KSPActionParam param) { EnableCharging(); }

    [KSPAction("Disable Startup Charging", guiName = "#LOC_FFT_ModuleChargeableEngine_Action_DisableAction_Title")]
    public void DisableChargingAction(KSPActionParam param) { DisableCharging(); }

    [KSPAction("Toggle Startup Charging", guiName = "#LOC_FFT_ModuleChargeableEngine_Action_ToggleAction_Title")]
    public void ToggleChargingAction(KSPActionParam param)
    {
      Charging = !Charging;
    }

    // VAB UI
    // -----------
    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_FFT_ModuleChargeableEngine_ModuleName");
    }

    public override string GetInfo()
    {
      string msg = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_PartInfo", ChargeGoal.ToString("F0"), (ChargeGoal / 1000f).ToString("F0"));
      return msg;
    }


    protected List<Renderer> chargeLights;

    protected ChargeState chargeState;
    protected Color chargeLightColor;
    protected Color chargeLightColorOff;
    private Dictionary<ModuleEnginesFX, bool> engineStates;
    private MultiModeEngine multiEngine;

    void Start()
    {
      if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
      {

        engineStates = new Dictionary<ModuleEnginesFX, bool>();

        ModuleEnginesFX[] engines = this.GetComponents<ModuleEnginesFX>();
        foreach (ModuleEnginesFX fx in engines)
          engineStates.Add(fx, fx.EngineIgnited);
        multiEngine = this.GetComponent<MultiModeEngine>();
      }
      SetupAnimations();
      SetupRecharge();

    }

    void SetupRecharge()
    {

      // Set up what the initial UI should look like
      if (HighLogic.LoadedSceneIsFlight)
      {
        ChargeState currentState;
        if (CurrentCharge >= ChargeGoal)
        {
          Charged = true;
          currentState = ChargeState.Ready;
        }
        else
        {
          currentState = ChargeState.Charging;
        }
        if (multiEngine)
        {
          if (multiEngine.runningPrimary)
            SetUIState(currentState, multiEngine.PrimaryEngine, multiEngine.SecondaryEngine);
          else
            SetUIState(currentState, multiEngine.SecondaryEngine, multiEngine.PrimaryEngine);

          multiEngineRunningPrimary = multiEngine.runningPrimary;
        }

        else
        {
          SetUIState(currentState, engineStates.First().Key, null);
        }

      }

    }
    void SetUIState(ChargeState newState, ModuleEnginesFX activeEngine, ModuleEnginesFX otherEngine)
    {
      if (FarFutureTechnologySettings.DebugModules)
        Utils.Log($"[ModuleChargeableEngine] Setting UI state to {newState.ToString()}");
      chargeState = newState;

      if (newState == ChargeState.Ready)
      {
        Fields["ChargeStatus"].guiActive = true;
        activeEngine.Events["Activate"].guiActive = true;
        activeEngine.Events["Shutdown"].guiActive = false;
        if (otherEngine)
        {
          otherEngine.Events["Activate"].guiActive = false;
          otherEngine.Events["Shutdown"].guiActive = false;
        }
      }
      if (newState == ChargeState.Running)
      {
        Fields["ChargeStatus"].guiActive = false;
        activeEngine.Events["Activate"].guiActive = false;
        activeEngine.Events["Shutdown"].guiActive = true;
        if (otherEngine)
        {
          otherEngine.Events["Activate"].guiActive = false;
          otherEngine.Events["Shutdown"].guiActive = false;
        }
      }
      if (newState == ChargeState.Charging)
      {
        Fields["ChargeStatus"].guiActive = true;
        activeEngine.Events["Activate"].guiActive = false;
        activeEngine.Events["Shutdown"].guiActive = false;
        if (otherEngine)
        {
          otherEngine.Events["Activate"].guiActive = false;
          otherEngine.Events["Shutdown"].guiActive = false;
        }
      }
    }

    void SetUIState(ChargeState currentState)
    {
      if (multiEngine)
      {
        if (multiEngine.runningPrimary)
          SetUIState(currentState, multiEngine.PrimaryEngine, multiEngine.SecondaryEngine);
        else
          SetUIState(currentState, multiEngine.SecondaryEngine, multiEngine.PrimaryEngine);
      }

      else
      {
        SetUIState(currentState, engineStates.First().Key, null);
      }
    }

    void SetupAnimations()
    {
      if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
      {

        chargeLightColor = new Color(0f / 255f, 76f / 255f, 131f / 255f, 0.5f);
        chargeLightColorOff = new Color(0f / 255f, 76f / 255f, 131f / 255f, 0f);
        chargeLights = new List<Renderer>();
        Transform chargeLightParent = part.FindModelTransform(ChargingLightRootTransformName);
        if (!chargeLightParent)
          if (FarFutureTechnologySettings.DebugModules)
            Utils.Log($"[ModuleChargeableEngine] Couldn't find ChargingLightRootTransformName {ChargingLightRootTransformName}");
          else
            foreach (Transform child in chargeLightParent)
            {
              chargeLights.Add(child.GetComponent<Renderer>());
            }
      }
    }

    bool multiEngineRunningPrimary;

    void FixedUpdate()
    {
      if (HighLogic.LoadedSceneIsFlight && engineStates != null)
      {

        DoRecharge();


        if (multiEngine)
        {
          if (multiEngineRunningPrimary != multiEngine.runningPrimary)
          {
            EvaluateEngineModeChange();
          }
        }

        List<ModuleEnginesFX> keys = new List<ModuleEnginesFX>(engineStates.Keys);
        foreach (ModuleEnginesFX key in keys)
        {
          if (key.EngineIgnited != engineStates[key])
          {
            EvaluateEngineStateChange(key, engineStates[key]);
          }

          if (key.EngineIgnited && ShutdownEngineOnLowThrottle)
          {
            if (key.requestedThrottle <= MinimumThrottleFraction)
            {
              ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Message_ThrottleLow",
                                                                                  part.partInfo.title,
                                                                                  (100f * MinimumThrottleFraction).ToString()),
                                                                 5.0f,
                                                                 ScreenMessageStyle.UPPER_CENTER));
              HandleShutdownEngine(key);
            }
          }
        }

      }
      else
      {
        CurrentPowerConsumption = 0f;
      }
    }

    void EvaluateEngineModeChange()
    {
      if (FarFutureTechnologySettings.DebugModules)
        Utils.Log($"[ModuleChargeableEngine] Handling multi engine switch activation, primary is {multiEngine.primaryEngineID}");

      SetUIState(chargeState);
      multiEngineRunningPrimary = multiEngine.runningPrimary;
    }

    /// <summary>
    /// Checks to see when an engine changed state and applies appropriate effects
    /// </summary>
    void EvaluateEngineStateChange(ModuleEnginesFX engine, bool state)
    {
      // Engine was turned on
      if (engine.EngineIgnited && !state)
      {
        HandleActivateEngine(engine);
      }
      // Engine was turned off
      else if (!engine.EngineIgnited && state)
      {
        HandleShutdownEngine(engine);
      }

    }


    void HandleActivateEngine(ModuleEnginesFX engine)
    {
      if (FarFutureTechnologySettings.DebugModules)
        Utils.Log($"[ModuleChargeableEngine] Handling engine activation");
      // If system is charged, enable 
      if (Charged && engine.EngineIgnited)
      {
        engine.Events["Activate"].guiActive = false;
        engine.Events["Shutdown"].guiActive = true;
        engine.Events["Activate"].guiActiveEditor = false;
        engine.Events["Shutdown"].guiActiveEditor = true;
        Charged = false;
        engineStates[engine] = true;
        CurrentCharge = 0f;
      }
      // Else disable the engine
      else
      {
        KillEngine(engine);
        engineStates[engine] = false;
      }

    }
    void HandleShutdownEngine(ModuleEnginesFX engine)
    {
      if (FarFutureTechnologySettings.DebugModules)
        Utils.Log($"[ModuleChargeableEngine] Handling engine shutdown");
      engineStates[engine] = false;
      Charged = false;
      CurrentCharge = 0f;
      SetUIState(ChargeState.Charging);
    }

    void Update()
    {

      if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
      {
        if (Events["EnableCharging"].active == Charging || Events["DisableCharging"].active != Charging)
        {
          Events["DisableCharging"].active = Charging;
          Events["EnableCharging"].active = !Charging;
        }
      }
    }

    /// <summary>
    /// Method to recharge the engine's startup capacitors
    /// </summary>
    void DoRecharge()
    {
      if (IsEngineRunning())
      {
        CurrentPowerConsumption = 0f;
        if (chargeState != ChargeState.Running)
        {
          SetUIState(ChargeState.Running);
        }
        ChargeStatus = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_ChargeStatus_Running");
      }
      else
      {
        if (Charging)
        {

          if (CurrentCharge >= ChargeGoal)
          {
            CurrentPowerConsumption = 0f;
            if (chargeState != ChargeState.Ready)
            {
              SetUIState(ChargeState.Ready);
              Charged = true;

              ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Message_Ready",
                                                                                  part.partInfo.title),
                                                                         5.0f,
                                                                         ScreenMessageStyle.UPPER_CENTER));
            }
          }
          else
          {
            double req = part.RequestResource("ElectricCharge", (double)ChargeRate * TimeWarp.fixedDeltaTime);
            CurrentCharge = Mathf.MoveTowards(CurrentCharge, ChargeGoal, (float)req);
            CurrentPowerConsumption = -ChargeRate;

            if (req > 0.0d)

              ChargeStatus = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_RechargeStatus_Charging", ChargeRate.ToString("F2"));
            else
              ChargeStatus = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_RechargeStatus_NoPower");
            if (chargeState != ChargeState.Charging)
            {
              SetUIState(ChargeState.Charging);
            }
          }


          ChargeStatus = Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Field_ChargeStatus_Normal", (CurrentCharge / ChargeGoal * 100.0f).ToString("F1"));

        }
        else
        {
          CurrentPowerConsumption = 0f;
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
    public bool IsEngineRunning()
    {
      foreach (var kvp in engineStates)
      {
        if (kvp.Key.EngineIgnited)
          return true;
      }
      return false;
    }
    public override void OnActive()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        if (multiEngine)
        {
          if (multiEngine.runningPrimary)
            HandleActivateEngine(multiEngine.PrimaryEngine);
          else
            HandleActivateEngine(multiEngine.SecondaryEngine);
        }
        else
        {
          HandleActivateEngine(engineStates.First().Key);
        }
      }
    }
    /// <summary>
    /// Sets the engine UI to be on or off
    /// </summary>
    /// <param name="state">If set to <c>on</c> state.</param>

    void KillEngine(ModuleEnginesFX engine)
    {
      if (engine != null)
      {
        engineStates[engine] = false;

        engine.Events["Shutdown"].Invoke();
        engine.currentThrottle = 0;
        engine.requestedThrottle = 0;
      }
    }
  }
}
