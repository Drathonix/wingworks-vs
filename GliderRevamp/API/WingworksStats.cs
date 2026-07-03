using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common.Entities;

namespace GliderRevamp.API;

public static class WingworksStats
{
    public static bool CanFly(EntityStats stats) {
        return (stats?.GetBlended("ww_can_fly") ?? 1) > 1;
    }
    

    public static void OnDefaultedStat(EntityStats stats, string category, float defaultValue, Action<float> action)
    {
        float f = GetOrDefault(stats, category, defaultValue);
        if(f != -1)
        {
            action.Invoke(f);
        }
    }

    public static bool ShouldUseHunger(EntityPlayer player)
    {
        return player.World.PlayerByUid(player.PlayerUID).WorldData.CurrentGameMode != EnumGameMode.Creative;
    }

    /// <summary>
    /// Returns 0 if the stat is not present.
    /// </summary>
    /// <param name="stats"></param>
    /// <param name="category"></param>
    /// <returns></returns>
    public static float GetZeroableFloat(EntityStats stats, string category)
    {
        return (stats?.GetBlended(category) ?? 1) - 1F;
    }

    public static float GetOrDefault(EntityStats stats, string category, float defaultValue)
    {
        // 0 if not present, -1 if disabled, >1 if present.
        var f = GetZeroableFloat(stats,category);
        if (f == 0)
        {
            return defaultValue;
        }
        return f;

    }
}
