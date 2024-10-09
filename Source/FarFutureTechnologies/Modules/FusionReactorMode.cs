using System.Collections.Generic;
using UnityEngine;

namespace FarFutureTechnologies
{

  /// <summary>
  /// A class that holds information about a fusion reactor mode
  /// </summary>
  public class FusionReactorMode
  {
    public string modeName;
    public string modeID;

    public float powerGeneration;
    public Color modeColor;
    public List<ResourceRatio> inputs;
    public List<ResourceRatio> outputs;

    public FusionReactorMode()
    {
    }
    /// <summary>
    /// Construct from confignode
    /// </summary>
    /// <param name="node"></param>
    ///
    public FusionReactorMode(ConfigNode node)
    {
      OnLoad(node);
    }

    public void OnLoad(ConfigNode node)
    {
      // Process nodes
      node.TryGetValue("DisplayName", ref modeName);
      node.TryGetValue("ModeID", ref modeID);
      node.TryGetValue("ModeColor", ref modeColor);
      node.TryGetValue("PowerGeneration", ref powerGeneration);

      ConfigNode[] inNodes = node.GetNodes("INPUT_RESOURCE");
      ConfigNode[] outNodes = node.GetNodes("OUTPUT_RESOURCE");

      inputs = new List<ResourceRatio>();
      for (int i = 0; i < inNodes.Length; i++)
      {
        ResourceRatio p = new ResourceRatio();
        p.Load(inNodes[i]);
        inputs.Add(p);
      }
      outputs = new List<ResourceRatio>();
      for (int i = 0; i < outNodes.Length; i++)
      {
        ResourceRatio p = new ResourceRatio();
        p.Load(outNodes[i]);
        outputs.Add(p);
      }
    }


  }
}
