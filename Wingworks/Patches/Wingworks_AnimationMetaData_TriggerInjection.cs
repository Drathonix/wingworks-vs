using System;
using System.Collections.Generic;
using System.Text;

namespace Wingworks.Patches;

[HarmonyPatch(typeof(AnimationMetaData), nameof(AnimationMetaData.Init)), HarmonyPriority(401)]

public class WingWorks_AnimationMetaData_TriggerInjection
{
    internal static bool Prefix(AnimationMetaData __instance)
    {
        if (__instance.Code.Equals("ww_flap"))
        {
            __instance.TriggeredBy = new WingFlapTrigger(__instance.TriggeredBy);
        }
        if (__instance.Code.Equals("ww_flap-fp"))
        {
            __instance.TriggeredBy = new WingFlapTrigger(__instance.TriggeredBy);
        }
        if (__instance.Code.Equals("glide"))
        {
            __instance.TriggeredBy = new WingFlapTrigger.NoGlide(__instance.TriggeredBy);
        }
        if (__instance.Code.Equals("glide-fp"))
        {
            __instance.TriggeredBy = new WingFlapTrigger.NoGlide(__instance.TriggeredBy);
        }
        return true;
    }
}
