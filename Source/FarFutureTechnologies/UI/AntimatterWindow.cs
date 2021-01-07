using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using KSP.UI;
using KSP.Localization;

namespace FarFutureTechnologies.UI
{
  public class AntimatterWindow : MonoBehaviour
  {
    public bool active = true;
    public bool panelOpen = false;
    public RectTransform rect;

    public Button okbutton;
    public Button cancelButton;
    public Text titleText;
    public Text scienceText;
    public Text antimatterText;
    public Text descriptionText;

    protected float totalScienceCost= 0f;
    protected float totalAntimatterLoad = 0f;

    public void Awake()
    {
      // Find all the components
      rect = this.GetComponent<RectTransform>();
      scienceText = transform.FindDeepChild("ScienceCostText").GetComponent<Text>();
      antimatterText = transform.FindDeepChild("AntimatterCostText").GetComponent<Text>();
      descriptionText = transform.FindDeepChild("DescriptiveText").GetComponent<Text>();
      titleText = transform.FindDeepChild("TitleText").GetComponent<Text>();
      okbutton = transform.FindDeepChild("OKButton").GetComponent<Button>();
      cancelButton = transform.FindDeepChild("CancelButton").GetComponent<Button>();

      cancelButton.onClick.AddListener(delegate { SetVisible(false); });

      okbutton.onClick.AddListener(delegate { LoadAntimatter(); });

      titleText.text = Localizer.Format("#LOC_FFT_AntimatterManager_PanelTitle");
      scienceText.text = Localizer.Format("#LOC_FFT_AntimatterManager_ScienceText", totalScienceCost.ToString("F1"));
      antimatterText.text = Localizer.Format("#LOC_FFT_AntimatterManager_AntimatterText", totalAntimatterLoad.ToString("F2"));
      descriptionText.text = Localizer.Format("#LOC_FFT_AntimatterManager_DescriptionText", totalScienceCost.ToString("F1"));
    }

    public void SetVisible(bool state)
    {
      active = state;
      rect.gameObject.SetActive(state);

    }

    public void LoadAntimatter()
    {
      SetVisible(false);
      AntimatterManager.Instance.FillTanks();

    }

    public void AddTank( float amount)
    {
      
      totalAntimatterLoad += amount;
      totalScienceCost = FarFutureTechnologySettings.antimatterScienceCostPerUnit * totalAntimatterLoad;

      scienceText.text = Localizer.Format("#LOC_FFT_AntimatterManager_ScienceText", totalScienceCost.ToString("+F0;-F0;0"));
      antimatterText.text = Localizer.Format("#LOC_FFT_AntimatterManager_AntimatterText", totalAntimatterLoad.ToString("+F0;-F0;0"));
      descriptionText.text = Localizer.Format("#LOC_FFT_AntimatterManager_DescriptionText", totalScienceCost.ToString());
    }
  }
}
