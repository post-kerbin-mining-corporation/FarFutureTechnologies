using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace PowerMonitor
{
    public interface IPowerConsumingPart
    {
        // Sets the powered/unpowered state
        void SetPoweredState(bool state);

        // Gets the canonical power usage
        double GetPowerUsage();

        // Gets the current power usage
        double CalculatePowerUsage();

        // Does processing at "low" warp
        void ProcessLowWarp();

        // Does processing at "high" warp
        void ProcessHighWarp();

        // Get the power use priority of this part. 0 is highest
        int GetPriority();
    }
}
