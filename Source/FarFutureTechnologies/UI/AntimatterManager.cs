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
    protected double totalAntimatterLoad = 0f;
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
        if (totalAntimatterLoad > 0d)
        {
          double goalAntimatterToLoad = 0d;

          double partAntimatter = 0d;
          double partAntimatterCapacity = 0d;

          double maxPurchaseableAM = ResearchAndDevelopment.Instance.Science * FarFutureTechnologiesSettings_Antimatter.AntimatterScienceCostPerUnit;
          // Find tank capacities
          tank.part.GetConnectedResourceTotals(PartResourceLibrary.Instance.GetDefinition(tank.FuelName).id,
            ResourceFlowMode.NO_FLOW, out partAntimatter, out partAntimatterCapacity, true);

          if (totalAntimatterLoad < partAntimatterCapacity)
            goalAntimatterToLoad = partAntimatterCapacity - totalAntimatterLoad;
          else
            goalAntimatterToLoad = partAntimatterCapacity;


          double antimatterToLoad = Math.Min(maxPurchaseableAM, goalAntimatterToLoad);
          if (FarFutureTechnologiesSettings_Antimatter.AntimatterScienceCostPerUnit > 0)
            ResearchAndDevelopment.Instance.AddScience((float)-antimatterToLoad / FarFutureTechnologiesSettings_Antimatter.AntimatterScienceCostPerUnit, TransactionReasons.RnDPartPurchase);

          totalAntimatterLoad -= antimatterToLoad;
         
          tank.part.RequestResource(tank.FuelName, -antimatterToLoad, ResourceFlowMode.NO_FLOW);
        }
      }
    }
  }
}
