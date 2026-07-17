using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;

namespace Wingworks;

public enum WingPosition
{
    EXPANDED,
    DIVING,
    BRAKING
}

public class WingPositionHelper
{
    public static float GetGravityCoefficient(WingPosition position)
    {
        if(position == WingPosition.DIVING)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }

    public static float GetDragCoefficient(WingPosition position)
    {
        if (position == WingPosition.EXPANDED)
        {
            return 1;
        }
        else if(position == WingPosition.BRAKING)
        {
            return 3;
        }
        else
        {
            return 0.5F;
        }
    }

    public static WingPosition GetPosition(ITreeAttribute wingAttris)
    {
        return (WingPosition)wingAttris.GetInt("position");
    }

    public static void SetPosition(ITreeAttribute wingAttris, WingPosition position)
    {
        wingAttris.SetInt("position", (int)position);
    }
}
