using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FarFutureTechnologies
{
  public class ModuleAnimationGroupLimiter: PartModule
  {


    // Current reactor power setting (0-100, tweakable)
    [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Power Setting"), UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f)]
    public float CurrentDeployLimit = 100f;

    ModuleAnimationGroup grp;

    public void Start()
    {
      grp = part.FindModulesImplementing<ModuleAnimationGroup>().First();
    }

    public void Update()
    {
      if (grp != null)
      {
        Limit();
      }
    }
    public void Limit()
    {
      string clip = grp.DeployAnimation.clip.name;
      Utils.Log($"1: {grp.DeployAnimation.clip.name}, {grp.DeployAnimation[clip].time}");
      //grp.DeployAnimation.Play()
      //grp.DeployAnimation[clip].time = Mathf.Min(grp.DeployAnimation[clip].time, CurrentDeployLimit / 100f);

      //Utils.Log($"2: {CurrentDeployLimit / 100f}, {grp.DeployAnimation[clip].time}");
    }

  }
}
