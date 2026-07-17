using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Vintagestory.API.Common.Entities;

namespace Wingworks.Patches;

// Disabled Debug for Animation checking. Leaving this here incase I need it.
//[HarmonyPatch(typeof(PlayerAnimationManager), nameof(PlayerAnimationManager.StartAnimation), [typeof(AnimationMetaData)]), HarmonyPriority(401)]
public class DebugAnimHandle
{
    internal static bool Prefix(PlayerAnimationManager __instance, AnimationMetaData animdata)
    {
        Console.WriteLine("active anims");
        foreach (var item in __instance.ActiveAnimationsByAnimCode)
        {
            Console.WriteLine(item.Value.Code);
        }
        //Console.WriteLine("Running: " + animdata.Code + ", " + animdata.WithFpVariant);
        //System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
        //Console.WriteLine(t);
        return true;
    }
}
