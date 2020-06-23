using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KSP.Localization;

namespace FarFutureTechnologies
{
  public class ModuleFusionEngine: FusionReactor
  {
    
    private List<bool> engineOnStates;
    private List<ModuleEnginesFX> engines;
    private MultiModeEngine multiEngine;

    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_FFT_ModuleFusionEngine_ModuleName");
    }

    public override void Start()
    {
      if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
      {
        engineOnStates = new List<bool>();
        engines = this.GetComponents<ModuleEnginesFX>().ToList();
        foreach (ModuleEnginesFX fx in engines)
          engineOnStates.Add(false);
        multiEngine = this.GetComponent<MultiModeEngine>();
      }

      base.Start();
      
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if (HighLogic.LoadedSceneIsFlight && engines != null && engines.Count > 0)
      {
        for (int i= 0; i < engines.Count; i++)
        {
          if (engines[i].EngineIgnited != engineOnStates[i])
          {
            EvaluateEngineStateChange(i);
          }
        }
      }
    }
    /// <summary>
    /// Checks to see when an engine changed state and applies appropriate effects
    /// </summary>
    void EvaluateEngineStateChange(int engineIndex)
    {
      // Engine was turned on
      if (engines[engineIndex].EngineIgnited && !engineOnStates[engineIndex])
      {
        HandleActivateEngine(engineIndex);
      }
      // Engine was turned off
      else if (!engines[engineIndex].EngineIgnited && engineOnStates[engineIndex])
      {
        HandleShutdownEngine(engineIndex);
      }

    }
    protected void HandleActivateEngine(int engineIndex)
    {
      // If reactor is not enabled
      if (!Enabled && engines[engineIndex].EngineIgnited)
      {
        // If system is charged, enable the reactor
        if (Charged)
        {
          EnableReactor();
          engines[engineIndex].Events["Activate"].guiActive = false;
          engines[engineIndex].Events["Shutdown"].guiActive = true;
          engines[engineIndex].Events["Activate"].guiActiveEditor = false;
          engines[engineIndex].Events["Shutdown"].guiActiveEditor = true;
        }
        // Else disable the engine
        else
        {
          KillEngine(engineIndex);
        }
      }
      // if reactor is enabled
      if (Enabled)
      {
        engineOnStates[engineIndex] = true;
        engines[engineIndex].Events["Activate"].guiActive = false;
        engines[engineIndex].Events["Shutdown"].guiActive = true;
        engines[engineIndex].Events["Activate"].guiActiveEditor = false;
        engines[engineIndex].Events["Shutdown"].guiActiveEditor = true;
      }
    }
    protected void HandleShutdownEngine(int engineIndex)
    {
      // If reactor is not enabled
      if (!Enabled && engines[engineIndex].EngineIgnited)
      {
        engineOnStates[engineIndex] = false;
      }
      // if reactor is enabled
      if (Enabled)
      {
        engineOnStates[engineIndex] = false;
      }
      engines[engineIndex].Events["Activate"].guiActive = true;
      engines[engineIndex].Events["Shutdown"].guiActive = false;
      engines[engineIndex].Events["Activate"].guiActiveEditor = true;
      engines[engineIndex].Events["Shutdown"].guiActiveEditor = false;
    }
    public override void OnActive()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        for (int engineIndex = 0; engineIndex < engines.Count; engineIndex++)
        {
          HandleActivateEngine(engineIndex);
        }
      }
    }

    public override void ReactorActivated()
    {
      base.ReactorActivated();
    }

    public override void ReactorDeactivated()
    {
      base.ReactorDeactivated();
      for (int engineIndex = 0; engineIndex < engines.Count; engineIndex++)
      {
        KillEngine(engineIndex);
      }
    }
    protected override void SetChargeStateUI(ChargeState newState)
    {
      base.SetChargeStateUI(newState);
      for (int i = 0; i < engines.Count; i++)
      {
        if (Enabled && !engineOnStates[i])
        {
          engines[i].Events["Activate"].guiActive = true;
          engines[i].Events["Shutdown"].guiActive = false;
          engines[i].Events["Activate"].guiActiveEditor = true;
          engines[i].Events["Shutdown"].guiActiveEditor = false;
        }
        else if (Enabled && engineOnStates[i])
        {
          engines[i].Events["Activate"].guiActive = false;
          engines[i].Events["Shutdown"].guiActive = true;
          engines[i].Events["Activate"].guiActiveEditor = false;
          engines[i].Events["Shutdown"].guiActiveEditor = true;
        }
        if (!Enabled)
        {
          if (newState == ChargeState.Ready)
          {
            engines[i].Events["Activate"].guiActive = true;
            engines[i].Events["Shutdown"].guiActive = false;
            engines[i].Events["Activate"].guiActiveEditor = true;
            engines[i].Events["Shutdown"].guiActiveEditor = false;
          }
          if (newState == ChargeState.Running)
          {
            engines[i].Events["Activate"].guiActive = true;
            engines[i].Events["Shutdown"].guiActive = false;
            engines[i].Events["Activate"].guiActiveEditor = true;
            engines[i].Events["Shutdown"].guiActiveEditor = false;
          }
          if (newState == ChargeState.Charging)
          {
            engines[i].Events["Activate"].guiActive = false;
            engines[i].Events["Shutdown"].guiActive = false;
            engines[i].Events["Activate"].guiActiveEditor = false;
            engines[i].Events["Shutdown"].guiActiveEditor = false;
          }
        }
      }
    }
    void KillEngine(int engineIndex)
    {
      if (engines[engineIndex] != null)
      {
        engineOnStates[engineIndex] = false;
        engines[engineIndex].Events["Shutdown"].Invoke();
        engines[engineIndex].currentThrottle = 0;
        engines[engineIndex].requestedThrottle = 0;
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_FFT_ModuleFusionEngine_Message_ReactorNotReady",
                                                                            part.partInfo.title),
                                                                   5.0f,
                                                                   ScreenMessageStyle.UPPER_CENTER));
      }
    }
  }
}
