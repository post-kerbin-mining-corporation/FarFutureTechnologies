using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KSP.UI;
using KSP.UI.Screens;
using KSP.Localization;

namespace FarFutureTechnologies.UI
{

  [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
  public class AntimatterManager : MonoBehaviour
  {
    public static AntimatterManager Instance { get; private set; }

    protected AntimatterWindow amWindow;

    protected float totalScienceCost = 0f;
    protected float totalAntimatterLoad = 0f;
    protected List<ModuleAntimatterTank> tanks;
    protected void Awake()
    {

      Instance = this;
      tanks = new List<ModuleAntimatterTank>();
    }

    protected void Start()
    {
     
    }

    protected void CreateWindow()
    {
      GameObject newUIPanel = (GameObject)Instantiate(UILoader.AntimatterWindowPrefab, Vector3.zero, Quaternion.identity);
      newUIPanel.transform.SetParent(UIMasterController.Instance.dialogCanvas.transform);
      newUIPanel.transform.localPosition = Vector3.zero;
      amWindow = newUIPanel.AddComponent<AntimatterWindow>();
      amWindow.SetVisible(false);
    }

    protected void DestroyWindow()
    {

    }

    public void AddTank(ModuleAntimatterTank tank, float amount)
    {
      if (amWindow == null)
        CreateWindow();

      if (!tanks.Contains(tank))
      {
        tanks.Add(tank);
        totalAntimatterLoad += amount;
        amWindow.SetVisible(true);
        amWindow.AddTank(amount);
      }
    }

    public void FillTanks()
    {
      float availableScience = ResearchAndDevelopment.Instance.Science;
      

      foreach (ModuleAntimatterTank tank in tanks)
      {
        double partAM = 0d;
        double partMaxAM = 0d;
        // Determine need for power
        tank.part.GetConnectedResourceTotals(PartResourceLibrary.Instance.GetDefinition(tank.FuelName).id, 
          ResourceFlowMode.NO_FLOW, out partAM, out partMaxAM, true);

        float cost = (float)partMaxAM * FarFutureTechnologySettings.antimatterScienceCostPerUnit;
        double toAdd = 0d;
        if (availableScience - cost > 0f)
        {
          ResearchAndDevelopment.Instance.AddScience(-cost, TransactionReasons.RnDPartPurchase);
          availableScience = availableScience - cost;
          toAdd = partMaxAM;
        } else
        {
          toAdd = availableScience / FarFutureTechnologySettings.antimatterScienceCostPerUnit;
          ResearchAndDevelopment.Instance.AddScience(-availableScience, TransactionReasons.RnDPartPurchase);
          availableScience = 0f;
        }
        
        tank.part.RequestResource(tank.FuelName, -toAdd, ResourceFlowMode.NO_FLOW);
      }
    }
  }
}
