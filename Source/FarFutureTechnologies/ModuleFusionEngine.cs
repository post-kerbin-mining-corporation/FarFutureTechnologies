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

    // Is the engine running
    [KSPField(isPersistant = false)]
    public bool EngineOn = false;


    private ModuleEnginesFX engine;


    public override string GetModuleDisplayName()
    {
      return Localizer.Format("#LOC_FFT_ModuleChargeableEngine_ModuleName");
    }

    public override void Start()
    {
      if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
      {
        engine = this.GetComponent<ModuleEnginesFX>();
      }
      base.Start();
      
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if (HighLogic.LoadedSceneIsFlight && engine != null)
      {
       
        // iF the states don't match up, see if we need to change things
        if (engine.EngineIgnited != EngineOn)
        {
          EvaluateEngineStateChange();
        }
        // If engine is on but dropped below a throttle setting, kill the engine
        if (engine.EngineIgnited)
        {
          //if (engine.requestedThrottle <= MinimumThrottleFraction)
          //{
          //  ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_FFT_ModuleChargeableEngine_Message_ThrottleLow",
          //                                                                      part.partInfo.title,
          //                                                                      (100f * MinimumThrottleFraction).ToString()),
          //                                                     3.0f,
          //                                                     ScreenMessageStyle.UPPER_CENTER));
          //  OnShutdown();
          //}
        }

      }
    }
    /// <summary>
    /// Checks to see when an engine changed state and applies appropriate effects
    /// </summary>
    void EvaluateEngineStateChange()
    {
      // Engine was turned on
      if (engine.EngineIgnited && !EngineOn)
      {
        OnActive();
      }
      // Engine was turned off
      else if (!engine.EngineIgnited && EngineOn)
      {
        //OnShutdown();
      }

    }

    public override void OnActive()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        if (!Enabled && engine.EngineIgnited)
        {
          if (Charged)
          {
            EnableReactor();
          }
          else
          {
            KillEngine();
          }
        }
        if (Enabled && engine.EngineIgnited)
        {
          EngineOn = true;
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
    }
    protected override void SetChargeStateUI(ChargeState newState)
    {
      base.SetChargeStateUI(newState);
      if (Enabled && !EngineOn)
      {
        engine.Events["Activate"].guiActive = true;
        engine.Events["Shutdown"].guiActive = false;
        engine.Events["Activate"].guiActiveEditor = true;
        engine.Events["Shutdown"].guiActiveEditor = false;
      }
      else if (Enabled && EngineOn)
      {
        engine.Events["Activate"].guiActive = false;
        engine.Events["Shutdown"].guiActive = true;
        engine.Events["Activate"].guiActiveEditor = false;
        engine.Events["Shutdown"].guiActiveEditor = true;
      }
      if (!Enabled)
      {
        if (newState == ChargeState.Ready)
        {
          engine.Events["Activate"].guiActive = true;
          engine.Events["Shutdown"].guiActive = false;
          engine.Events["Activate"].guiActiveEditor = true;
          engine.Events["Shutdown"].guiActiveEditor = false;
        }
        if (newState == ChargeState.Running)
        {
          engine.Events["Activate"].guiActive = true;
          engine.Events["Shutdown"].guiActive = false;
          engine.Events["Activate"].guiActiveEditor = true;
          engine.Events["Shutdown"].guiActiveEditor = false;
        }
        if (newState == ChargeState.Charging)
        {
          engine.Events["Activate"].guiActive = false;
          engine.Events["Shutdown"].guiActive = false;
          engine.Events["Activate"].guiActiveEditor = false;
          engine.Events["Shutdown"].guiActiveEditor = false;
        }
      }
    }
    public void OnShutdown()
    {
      EngineOn = false;
    }
    void SetEngineUI(bool state)
    {
      engine.Events["Activate"].guiActive = state;
      engine.Events["Shutdown"].guiActive = state;
      engine.Events["Activate"].guiActiveEditor = state;
      engine.Events["Shutdown"].guiActiveEditor = state;
    }

    void KillEngine()
    {
      if (engine != null)
      {
        EngineOn = false;
        engine.Events["Shutdown"].Invoke();
        engine.currentThrottle = 0;
        engine.requestedThrottle = 0;
        ScreenMessages.PostScreenMessage(new ScreenMessage(Localizer.Format("#LOC_FFT_ModuleFusionEngine_Message_ReactorNotReady",
                                                                            part.partInfo.title),
                                                                   5.0f,
                                                                   ScreenMessageStyle.UPPER_CENTER));
      }
    }
  }
}
