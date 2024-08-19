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
