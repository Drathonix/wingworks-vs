using Wingworks.API;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Datastructures;

namespace Wingworks.Patches;

[HarmonyPatch(typeof(EntityPlayer),nameof(EntityPlayer.OnFallToGround))]
public class Wingworks_EntityPlayer_TouchGround
{
    public static void Prefix(EntityPlayer __instance, double motionY)
    {
        if (WingworksStats.CanFly(__instance.Stats))
        {
            ITreeAttribute wings = __instance.WatchedAttributes.GetOrAddTreeAttribute("wingworks");
            wings.SetFloat("flap", -1);
            wings.SetFloat("time", 0);
            WingPositionHelper.SetPosition(wings,WingPosition.EXPANDED);
        }
    }
}
