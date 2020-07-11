using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FarFutureTechnologies
{
  public class ModuleColorAnimator: PartModule, IScalarModule
  {
    [KSPField]
    public float colorScale = 1f;

    [KSPField]
    public string moduleID;

    [KSPField]
    public float animRate;

    [KSPField]
    public string shaderProperty;

    [KSPField]
    public FloatCurve redCurve = new FloatCurve();

    [KSPField]
    public FloatCurve greenCurve = new FloatCurve();

    [KSPField]
    public FloatCurve blueCurve = new FloatCurve();

    [KSPField]
    public FloatCurve alphaCurve = new FloatCurve();

    [KSPField]
    public string includedTransformList;


    public string ScalarModuleID
    {
      get { return moduleID; }
    }
    public bool CanMove
    {
      get { return true; }
    }
    public float GetScalar
    {
      get { return animationFraction; }
    }

    public EventData<float,float> OnMoving
    {
      get { return new EventData<float,float>("OnMoving"); }
    }

    public EventData<float> OnStop
    {
      get { return new EventData<float>("OnStop"); }
    }

    
    public void SetScalar(float t )
    {
      animationGoal = t;
    }

    public bool IsMoving()
    {
      return true;
    }

    
   
    public void SetUIWrite(bool value)
    { }
    public void SetUIRead(bool value)
    { }

    protected float animationFraction = 0f;
    protected float animationGoal = 0f;
    protected List<Renderer> targetRenderers;

    protected void Start()
    {

      targetRenderers = new List<Renderer>();
      if (includedTransformList == "")
      {
        foreach (Transform x in part.GetComponentsInChildren<Transform>())
        {
          Renderer r = x.GetComponent<Renderer>();

          if (r != null && r.material.HasProperty(shaderProperty)) targetRenderers.Add(r);
        }
      }
      else
      {
        string[] allXformNames = includedTransformList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string xformName in allXformNames)
        {
          Transform[] xforms = part.FindModelTransforms(xformName);
          foreach (Transform x in xforms)
          {
            Renderer r = x.GetComponent<Renderer>();
            
            if (r != null && r.material.HasProperty(shaderProperty))
              targetRenderers.Add(r);
          }
        }
      }
      if (HighLogic.LoadedSceneIsEditor && targetRenderers != null)
      {
        animationFraction = 0f;
        Color c = new Color(redCurve.Evaluate(animationFraction) * colorScale, greenCurve.Evaluate(animationFraction)*colorScale, blueCurve.Evaluate(animationFraction) * colorScale, alphaCurve.Evaluate(animationFraction) * colorScale);
        
        foreach (Renderer r in targetRenderers)
        {
          r.material.SetColor(shaderProperty, c);
        }
      }
      Utils.Log($"[ModuleColorAnimator] {moduleID} Set up {targetRenderers.Count} renderers");
    }

    protected void Update()
    {
      if (HighLogic.LoadedSceneIsFlight && targetRenderers != null)
      {
         animationFraction = Mathf.MoveTowards(animationFraction, animationGoal, TimeWarp.deltaTime * animRate);
        
        Color c = new Color(redCurve.Evaluate(animationFraction) * colorScale, greenCurve.Evaluate(animationFraction) * colorScale, blueCurve.Evaluate(animationFraction) * colorScale, alphaCurve.Evaluate(animationFraction) * colorScale);
     
        foreach (Renderer r in targetRenderers)
        {
          r.material.SetColor(shaderProperty, c);
        }
      }
    }

  }
}
