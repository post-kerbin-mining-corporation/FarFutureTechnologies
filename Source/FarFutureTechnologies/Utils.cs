using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FarFutureTechnologies
{
    public static class Utils
    {
        public static bool CheckTechPresence(string techName)
        {
            if (ResearchAndDevelopment.Instance != null)
            {

                ProtoTechNode techNode = ResearchAndDevelopment.Instance.GetTechState(techName);
                if (techNode != null)
                {
                    bool available = techNode.state == RDTech.State.Available;
                    if (available)
                    {
                        Utils.Log("Tech is available");
                        return available;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
            {
                return false;
            }
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

        public static void Log(string str)
        {
            Debug.Log("[Far Future Tech]: " + str);
        }
        public static void LogError(string str)
        {
            Debug.LogError("[Far Future Tech]: " + str);
        }
        public static void LogWarning(string str)
        {
            Debug.LogWarning("[Far Future Tech]: " + str);
        }

        // Node loading
        // several variants for data types
        public static string GetValue(ConfigNode node, string nodeID, string defaultValue)
        {
            if (node.HasValue(nodeID))
            {
                return node.GetValue(nodeID);
            }
            return defaultValue;
        }
        public static int GetValue(ConfigNode node, string nodeID, int defaultValue)
        {
            if (node.HasValue(nodeID))
            {
                int val;
                if (int.TryParse(node.GetValue(nodeID), out val))
                    return val;
            }
            return defaultValue;
        }
        public static float GetValue(ConfigNode node, string nodeID, float defaultValue)
        {
            if (node.HasValue(nodeID))
            {
                float val;
                if (float.TryParse(node.GetValue(nodeID), out val))
                    return val;
            }
            return defaultValue;
        }
        public static double GetValue(ConfigNode node, string nodeID, double defaultValue)
        {
            if (node.HasValue(nodeID))
            {
                double val;
                if (double.TryParse(node.GetValue(nodeID), out val))
                    return val;
            }
            return defaultValue;
        }
        public static bool GetValue(ConfigNode node, string nodeID, bool defaultValue)
        {
            if (node.HasValue(nodeID))
            {
                bool val;
                if (bool.TryParse(node.GetValue(nodeID), out val))
                    return val;
            }
            return defaultValue;
        }// Based on some Firespitter code by Snjo
        public static FloatCurve GetValue(ConfigNode node, string nodeID, FloatCurve defaultValue)
        {
            if (node.HasNode(nodeID))
            {
                FloatCurve theCurve = new FloatCurve();
                ConfigNode[] nodes = node.GetNodes(nodeID);
                for (int i = 0; i < nodes.Length; i++)
                {
                    string[] valueArray = nodes[i].GetValues("key");

                    for (int l = 0; l < valueArray.Length; l++)
                    {
                        string[] splitString = valueArray[l].Split(' ');
                        Vector2 v2 = new Vector2(float.Parse(splitString[0]), float.Parse(splitString[1]));
                        theCurve.Add(v2.x, v2.y, 0, 0);
                    }
                }
                Debug.Log(theCurve.Evaluate(0f));
                return theCurve;
            }
            Debug.Log("default");
            return defaultValue;
        }
    }
}
