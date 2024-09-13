using UnityEngine;

namespace FarFutureTechnologies
{

  [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
  public class FarFutureTechnologies : MonoBehaviour
  {
    
    public static FarFutureTechnologies Instance { get; private set; }

    protected void Awake()
    {
      Instance = this;
    }
    protected void Start()
    {
      FarFutureTechnologySettings.Load();
    }
  }
  public static class FarFutureTechnologySettings
  {
    /// Emit debug messages from modules
    public static bool DebugModules = true;
    /// <summary>
    /// Below this number damage to buildings will be ignored
    /// </summary>
    public static float MinimumExhaustBuildingDamagePerTick = 0.05f;
    /// <summary>
    /// Below this number damage to buildings will be ignored (single pulses)
    /// </summary>
    public static float MinimumPulseBuildingDamage = 0.05f;
    /// Below this number heat to parts will be ignored (single pulses)
    public static float MinimumPulsePartsHeat = 0.05f;
    /// Below this number force to parts will be ignored (single pulses)
    public static float MinimumPulsePartsForce = 0.05f;

    public static void Load()
    {
      ConfigNode settingsNode;

      Utils.Log("[FFT Settings]: Started loading");
      if (GameDatabase.Instance.ExistsConfigNode("FarFutureTechnologies/FFTSETTINGS"))
      {
        Utils.Log("[FFT Settings]: Located settings file");
        settingsNode = GameDatabase.Instance.GetConfigNode("FarFutureTechnologies/FFTSETTINGS");
        settingsNode.TryGetValue("debugModules", ref FarFutureTechnologySettings.DebugModules);
      }
      else
      {
        Utils.LogWarning("[FFT Settings]: Couldn't find settings file, using defaults");
      }
      Utils.Log("[FFT Settings]: Finished loading");
    }
  }
}
