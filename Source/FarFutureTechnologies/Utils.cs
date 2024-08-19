
using System;
using System.Collections.Generic;
using UnityEngine;


namespace FarFutureTechnologies
{
  public static class Utils
  {
    public static T FindNamedComponent<T>(Part part, string moduleID)
    {
      T module = default(T);
      T[] candidates = part.GetComponents<T>();
      if (candidates == null || candidates.Length == 0)
      {
        Utils.LogError(String.Format("[]: No module of type {0} could be found on {1}, everything will now explode", typeof(T).ToString(), part.partInfo.name));
        return module;
      }
      if (string.IsNullOrEmpty(moduleID))
      {
        Utils.LogWarning(String.Format("No moduleID was specified on {0}, using first instance of {1}", part.partInfo.name, typeof(T).ToString()));
        module = candidates[0];
      }
      else
      {

        foreach (var sys in candidates)
        {
          if ((string)sys.GetType().GetProperty("moduleID").GetValue(sys, null) == moduleID)
            module = sys;
        }
      }
      if (module == null)
        Utils.LogError(String.Format("No valid module of type {0} could be found on {1}, everything will now explode", typeof(T).ToString(), part.partInfo.name));

      return module;
    }

    // This function loads up some animationstates
    public static AnimationState[] SetUpAnimation(string animationName, Part part)
    {
      var states = new List<AnimationState>();
      foreach (var animation in part.FindModelAnimators(animationName))
      {
        var animationState = animation[animationName];
        animationState.speed = 0;
        animationState.enabled = true;
        // Clamp this or else weird things happen
        animationState.wrapMode = WrapMode.ClampForever;
        animation.Blend(animationName);
        states.Add(animationState);
      }
      // Convert
      return states.ToArray();
    }
    public static AnimationState[] SetUpAnimation(string animationName, Part part, int layer)
    {
      var states = new List<AnimationState>();
      foreach (var animation in part.FindModelAnimators(animationName))
      {
        var animationState = animation[animationName];
        animationState.speed = 0;
        animationState.layer = layer;
        animationState.enabled = true;
        // Clamp this or else weird things happen
        animationState.wrapMode = WrapMode.ClampForever;
        animation.Blend(animationName);
        states.Add(animationState);
      }
      // Convert
      return states.ToArray();
    }

    public static void Log(string str)
    {
      Debug.Log("[FFT]" + str);
    }
    public static void LogError(string str)
    {
      Debug.LogError("[FFT]" + str);
    }
    public static void LogWarning(string str)
    {
      Debug.LogWarning("[FFT]" + str);
    }
  }

  public static class TransformDeepChildExtension
  {
    //Breadth-first search
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
      Queue<Transform> queue = new Queue<Transform>();
      queue.Enqueue(aParent);
      while (queue.Count > 0)
      {
        var c = queue.Dequeue();
        if (c.name == aName)
          return c;
        foreach (Transform t in c)
          queue.Enqueue(t);
      }
      return null;
    }


  }
}
