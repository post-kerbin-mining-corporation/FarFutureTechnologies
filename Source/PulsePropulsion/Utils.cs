using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI;

namespace PulsePropulsion
{
    public static class Utils
    {

       

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

        // LOGGING
        // -------
        public static void Log(string message)
        {
            Debug.Log("PulsePropulsion: " + message);
        }

        public static void LogWarn(string message)
        {
            Debug.LogWarning("PulsePropulsion: " + message);
        }

        public static void LogError(string message)
        {
            Debug.LogError("PulsePropulsion: " + message);
        }
    }
}
