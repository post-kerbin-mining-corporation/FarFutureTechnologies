using KSP.Localization;
using System;
using System.Linq;
using System.Collections.Generic;
using FarFutureTechnologies.UI;

namespace FarFutureTechnologies
{
  public class ModuleAntimatterTank : PartModule
  {
    // Name of the fuel to boil off
    [KSPField(isPersistant = false)]
    public string FuelName;

    // Cost to contain in u/s
    [KSPField(isPersistant = false)]
    public float ContainmentCost = 0.0f;

    // KJ of energy released per unit of AM detonated
    // canoncially 1 ug of AM is 36000 kJ, 1 ug = 1 unit
    [KSPField(isPersistant = false)]
    public float DetonationKJPerUnit = 360000f;

    // Rate of AM detonation in u/s
    [KSPField(isPersistant = false)]
    public float DetonationRate = 5f;

    // Whether tank containment is enabled
    [KSPField(isPersistant = true)]
    public bool ContainmentEnabled = true;

    // Whether detonation is occurring
    [KSPField(isPersistant = true)]
    public bool DetonationOccuring = false;

    [KSPField(isPersistant = false)]
    public float AlertRate = 5f;

    [KSPField(isPersistant = true)]
    public float ContainmentCostCurrent = 0f;

    [KSPField(isPersistant = false)]
    public string OnLightAnimatorName = "";

    [KSPField(isPersistant = false)]
    public string OffLightAnimatorName = "";

    [KSPField(isPersistant = false)]
    public string AlertLightAnimatorName = "";

    [KSPField(isPersistant = false)]
    public int DetonationFrameTimer = 0;

    [KSPField(isPersistant = false)]
    public int DetonationFrameThreshold = 10;

    // PRIVATE
    private double fuelAmount = 0.0;
    private double maxFuelAmount = 0.0;
    private ModuleColorAnimator onAnimator;
    private ModuleColorAnimator offAnimator;
    private ModuleColorAnimator alertAnimator;
    private float alertDirection = 1f;

    // UI FIELDS/ BUTTONS
    // Status string
    [KSPField(isPersistant = false, guiActive = true, guiName = "#LOC_FFT_ModuleAntimatterTank_Field_DetonationStatus_Title",
      groupName = "antimatterTank", groupDisplayName = "#LOC_FFT_ModuleAntimatterTank_UIGroup_Title", groupStartCollapsed = false)]
    public string DetonationStatus = "N/A";

    [KSPField(isPersistant = false, guiActive = true, guiName = "#LOC_FFT_ModuleAntimatterTank_Field_ContainmentStatus_Title",
      groupName = "antimatterTank", groupDisplayName = "#LOC_FFT_ModuleAntimatterTank_UIGroup_Title", groupStartCollapsed = false)]
    public string ContainmentStatus = "N/A";

    [KSPEvent(guiActive = true, guiName = "#LOC_FFT_ModuleAntimatterTank_Event_Enable_Title", active = true,
      groupName = "antimatterTank", groupDisplayName = "#LOC_FFT_ModuleAntimatterTank_UIGroup_Title", groupStartCollapsed = false)]
    public void Enable()
    {
      ContainmentEnabled = true;
    }
    [KSPEvent(guiActive = true, guiName = "#LOC_FFT_ModuleAntimatterTank_Event_Disable_Title", active = false,
      groupName = "antimatterTank", groupDisplayName = "#LOC_FFT_ModuleAntimatterTank_UIGroup_Title", groupStartCollapsed = false)]
    public void Disable()
    {
      ContainmentEnabled = false;
    }

    // ACTIONS
    [KSPAction(guiName = "#LOC_FFT_ModuleAntimatterTank_Action_EnableAction_Title")]
    public void EnableAction(KSPActionParam param) { Enable(); }

    [KSPAction(guiName = "#LOC_FFT_ModuleAntimatterTank_Action_DisableAction_Title")]
    public void DisableAction(KSPActionParam param) { Disable(); }

    [KSPAction(guiName = "#LOC_FFT_ModuleAntimatterTank_Action_ToggleAction_Title")]
    public void ToggleAction(KSPActionParam param)
    {
      ContainmentEnabled = !ContainmentEnabled;
    }

    // VAB UI
    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_FFT_ModuleAntimatterTank_ModuleName");
    }

    public override string GetInfo()
    {
      return Localizer.Format("#LOC_FFT_ModuleAntimatterTank_PartInfo", ContainmentCost.ToString("F1"), (DetonationKJPerUnit / 1000f).ToString("F2"));
    }


    // INTERFACE METHODS
    // Sets the powered/unpowered state
    public void SetPoweredState(bool state)
    {
      if (ContainmentEnabled && ContainmentCost > 0f)
      {
        if (state)
        {
          DetonationOccuring = false;
          DetonationStatus = Localizer.Format("#LOC_FFT_ModuleAntimatterTank_Field_DetonationStatus_Contained");
          ContainmentStatus = Localizer.Format("#LOC_FFT_ModuleAntimatterTank_Field_ContainmentStatus_Contained", ContainmentCost.ToString("F2"));
          ContainmentCostCurrent = -1f * ContainmentCost;
        }
        else
        {
          DetonationOccuring = true;
          DetonationStatus = Localizer.Format("#LOC_FFT_ModuleAntimatterTank_Field_DetonationStatus_Uncontained", DetonationRate.ToString("F2"));
          ContainmentStatus = Localizer.Format("#LOC_FFT_ModuleAntimatterTank_Field_ContainmentStatus_Uncontained");
          ContainmentCostCurrent = -1f * ContainmentCost;
        }
      }
      else
      {
        ContainmentCostCurrent = 0f;
      }
    }

    public void Start()
    {

      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {

        fuelAmount = GetResourceAmount(FuelName);
        maxFuelAmount = GetMaxResourceAmount(FuelName);

        // Catchup

      }
      if (HighLogic.LoadedSceneIsFlight)
      {
        GameEvents.OnVesselRollout.Add(new EventData<ShipConstruct>.OnEvent(OnVesselRollout));
        DoCatchup();

        if (OnLightAnimatorName != "")
          onAnimator = this.GetComponents<ModuleColorAnimator>().ToList().Find(x => x.moduleID == OnLightAnimatorName);
        if (OffLightAnimatorName != "")
          offAnimator = this.GetComponents<ModuleColorAnimator>().ToList().Find(x => x.moduleID == OffLightAnimatorName);
        if (AlertLightAnimatorName != "")
          alertAnimator = this.GetComponents<ModuleColorAnimator>().ToList().Find(x => x.moduleID == AlertLightAnimatorName);
      }
    }

    void OnDestroy()
    {
      // Clean up events when the item is destroyed
      GameEvents.OnVesselRollout.Remove(OnVesselRollout);
    }

    /// <summary>
    /// 
    /// </summary>
    protected void OnVesselRollout(ShipConstruct node)
    {
      if (HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX || HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
      {
        if (FarFutureTechnologySettings.DebugModules)
          Utils.Log(String.Format("[ModuleAntimatterTank]: Doing rollout actions"));
        double shipAM = 0d;
        double shipMaxAM = 0d;
        // Determine need for power
        part.GetConnectedResourceTotals(PartResourceLibrary.Instance.GetDefinition(FuelName).id, ResourceFlowMode.NO_FLOW, out shipAM, out shipMaxAM, true);

        double amToAdd = shipAM;
        // Clean out the AM
        part.RequestResource(FuelName, amToAdd, ResourceFlowMode.NO_FLOW);
        // Add the AM
        if (FarFutureTechnologySettings.DebugModules)
          Utils.Log($"[ModuleAntimatterTank]: deleted {amToAdd} units of {FuelName}");

        AntimatterManager.Instance.AddTank(this, (float)amToAdd);
        //if (ResearchAndDevelopment.Instance.Science >= amCost)
        //{

        //}
        //ResearchAndDevelopment.Instance.AddScience(-amCost, TransactionReasons.RnDPartPurchase);
      }
    }

    public void AddAntimatter(double amt)
    {
      part.RequestResource(FuelName, amt, ResourceFlowMode.NO_FLOW);
    }

    public void DoCatchup()
    {
      if (part.vessel.missionTime > 0.0)
      {
        if (part.RequestResource("ElectricCharge", (double)ContainmentCost * TimeWarp.fixedDeltaTime) < ContainmentCost * TimeWarp.fixedDeltaTime)
        {
        }
      }
    }

    public void Update()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {

        // Show the containment status field if there is a cooling cost
        if (ContainmentCost > 0f)
        {

          Fields["ContainmentStatus"].guiActive = true;

          if (Events["Enable"].active == ContainmentEnabled || Events["Disable"].active != ContainmentEnabled)
          {
            Events["Disable"].active = ContainmentEnabled;
            Events["Enable"].active = !ContainmentEnabled;
          }
        }
      }
      if (HighLogic.LoadedSceneIsEditor)
      {

        Fields["ContainmentStatus"].guiActive = true;
        ContainmentCostCurrent = -1f * ContainmentCost;
        double max = GetMaxResourceAmount(FuelName);
        ContainmentStatus = Localizer.Format("#LOC_FFT_ModuleAntimatterTank_Field_ContainmentStatus_Editor", (ContainmentCost * (float)(max)).ToString("F2"));
      }
    }



    protected void FixedUpdate()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        fuelAmount = GetResourceAmount(FuelName);

        // If we have no fuel, no need to do any calculations
        if (fuelAmount == 0.0)
        {
          if (offAnimator)
            offAnimator.SetScalar(1f);
          if (onAnimator)
            onAnimator.SetScalar(0f);
          if (alertAnimator)
            alertAnimator.SetScalar(0f);
          ContainmentStatus = Localizer.Format("#LOC_FFT_ModuleAntimatterTank_Field_ContainmentStatus_None");
          return;
        }

        // If the cooling cost is zero, we must boil off
        if (ContainmentCost == 0f)
        {
          if (offAnimator)
            offAnimator.SetScalar(1f);
          if (onAnimator)
            onAnimator.SetScalar(0f);


          DetonationOccuring = true;
          DetonationStatus = Localizer.Format("#LOC_FFT_ModuleAntimatterTank_Field_DetonationStatus_Uncontained", DetonationRate.ToString("F2"));
        }
        // else check for available power
        else
        {
          if (!ContainmentEnabled)
          {
            if (offAnimator)
              offAnimator.SetScalar(1f);
            if (onAnimator)
              onAnimator.SetScalar(0f);
            DetonationOccuring = true;
            DetonationStatus = Localizer.Format("#LOC_FFT_ModuleAntimatterTank_Field_DetonationStatus_Uncontained", DetonationRate.ToString("F2"));
            ContainmentStatus = Localizer.Format("#LOC_FFT_ModuleAntimatterTank_Field_ContainmentStatus_Disabled");
          }
          else
          {
            if (offAnimator)
              offAnimator.SetScalar(0f);
            if (onAnimator)
              onAnimator.SetScalar(1f);
          }
        }

        ConsumeCharge();

        if (DetonationOccuring)
        {
          if (alertAnimator)
          {
            if (alertAnimator.GetScalar > 1f)
            {
              alertDirection = -1f;
            }
            if (alertAnimator.GetScalar < 0f)
              alertDirection = 1f;

            alertAnimator.SetScalar(alertAnimator.GetScalar + TimeWarp.fixedDeltaTime * AlertRate * alertDirection);
          }
          DetonationFrameTimer++;
          DoDetonation();
        }
        else
        {
          DetonationFrameTimer = 0;
          if (alertAnimator)
            alertAnimator.SetScalar(0f);
        }
        if (part.vessel.missionTime > 0.0)
        {
          //LastUpdateTime = part.vessel.missionTime;
        }
      }
    }
    protected void ConsumeCharge()
    {

      if (ContainmentEnabled && ContainmentCost > 0f)
      {
        double chargeRequest = ContainmentCost * TimeWarp.fixedDeltaTime;

        double req = part.RequestResource("ElectricCharge", chargeRequest, ResourceFlowMode.ALL_VESSEL);
        // Fully cooled
        double tolerance = 0.0001;
        if (req >= chargeRequest - tolerance)
        {
          SetPoweredState(true);
        }
        else
        {
          SetPoweredState(false);
        }
      }
      else
      {

      }

    }
    protected void DoDetonation()
    {
      if (DetonationFrameTimer >= DetonationFrameThreshold)
      {
        double detonatedAmount = part.RequestResource(FuelName, TimeWarp.fixedDeltaTime * DetonationRate);
        part.AddThermalFlux(detonatedAmount * DetonationKJPerUnit);
      }
    }



    public bool isResourcePresent(string nm)
    {
      int id = PartResourceLibrary.Instance.GetDefinition(nm).id;
      PartResource res = this.part.Resources.Get(id);
      if (res == null)
        return false;
      return true;
    }
    protected double GetResourceAmount(string nm)
    {

      PartResource res = this.part.Resources.Get(PartResourceLibrary.Instance.GetDefinition(nm).id);
      if (res != null)
        return res.amount;
      else
        return 0f;
    }
    protected double GetMaxResourceAmount(string nm)
    {

      int id = PartResourceLibrary.Instance.GetDefinition(nm).id;

      PartResource res = this.part.Resources.Get(id);

      if (res != null)
        return res.maxAmount;
      else
        return 0f;
    }


  }
}
