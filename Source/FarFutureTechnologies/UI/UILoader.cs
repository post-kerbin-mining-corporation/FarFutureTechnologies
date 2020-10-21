using System.IO;
using UnityEngine;

namespace FarFutureTechnologies.UI
{


  [KSPAddon(KSPAddon.Startup.Instantly, true)]
  public class UILoader : MonoBehaviour
  {
    private static GameObject antimatterWindowPrefab;
    
    public static GameObject AntimatterWindowPrefab
    {
      get { return antimatterWindowPrefab; }
    }

    private void Awake()
    {
      Utils.Log("[FFTUILoader]: Loading UI Prefabs");
      AssetBundle prefabs = AssetBundle.LoadFromFile(Path.Combine(KSPUtil.ApplicationRootPath, "GameData/FarFutureTechnologies/UI/fftui.dat"));
      antimatterWindowPrefab = prefabs.LoadAsset("AMWindow") as GameObject;
      Utils.Log("[FFTUILoader]: Loaded UI Prefabs");
    }
  }
}

