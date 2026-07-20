using ConfigLib;
using Wingworks.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;

namespace Wingworks.Patches;

[HarmonyPatch(typeof(EntityAgent), nameof(EntityAgent.OnGameTick)), HarmonyPriority(401)]
public class Wingworks_EntityAgent_Animations
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        /*var found = false;
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(f_someField))
            {
                yield return new CodeInstruction(OpCodes.Call, m_MyExtraMethod);
                found = true;
            }
            yield return instruction;
        }
        if (found is false)
            ReportError("Cannot find <Stdfld someField> in OriginalType.OriginalMethod");
        */
        return Transpilers.MethodReplacer(instructions,
            typeof(AnimationMetaData).GetMethod("Matches"), typeof(Wingworks_EntityAgent_Animations).GetMethod("SpecMatch"));
    }

    public static bool SpecMatch(AnimationMetaData __instance, int controls)
    {
        if(__instance.TriggeredBy is ISpecialAnimationTrigger trigger)
        {
            if (trigger.ShouldDoDefaultChecksAdditionally() && !__instance.Matches(controls))
            {
                return false;
            }
            return trigger.Matches(cachedInst,controls);
        }
        return __instance.Matches(controls);
    }

    private static EntityAgent cachedInst;

    internal static bool Prefix(EntityAgent __instance, float dt)
    {
        cachedInst = __instance;
        if (WingworksStats.CanFly(__instance.Stats)) {
            ITreeAttribute wings = __instance.WatchedAttributes.GetOrAddTreeAttribute("wingworks");
            if (__instance.Controls.Gliding)
            {
                var t = wings.GetFloat("flap");
                var ft = wings.GetFloat("time");
                if (t > -1)
                {
                    t += dt;
                    wings.SetFloat("flap", t);
                }
                ft += dt;
                bool blockFlap = false;
                if (__instance.Controls.Backward) {
                    WingPositionHelper.SetPosition(wings,WingPosition.BRAKING);
                    blockFlap = true;
                }
                else if (WingworksStats.IsDiving(__instance.Pos))
                {
                    WingPositionHelper.SetPosition(wings,WingPosition.DIVING);
                }
                else
                {
                    WingPositionHelper.SetPosition(wings, WingPosition.EXPANDED);
                }
                if (ft >= 1.25f)
                {
                    var pitchVerticalCoefficient = 1 - Math.Min(0f, WingworksStats.GetPitchFrac(__instance.Pos));
                    wings.SetFloat("time", 0);
                    WingworksStats.OnDefaultedStat(__instance.Stats, "ww_flight_hunger", ModConfig.Instance.FlightHunger, (hungerDrain) =>
                    {
                        EntityBehaviorHunger hunger = __instance.GetBehavior<EntityBehaviorHunger>();
                        if ((__instance is EntityPlayer player && WingworksStats.ShouldUseHunger(player)) && hunger != null)
                        {
                            hunger.ConsumeSaturation(hungerDrain * pitchVerticalCoefficient);
                        }
                    });
                }

                else
                {
                    wings.SetFloat("time", ft);
                }
                if (t > ModConfig.Instance.FlapCooldown)
                {
                    wings.SetFloat("flap", -1);
                }
                if (t < 0 && __instance.Controls.Jump && !blockFlap)
                {
                    wings.SetFloat("flap", 0);
                    WingworksStats.OnDefaultedStat(__instance.Stats, "ww_flap_hunger", ModConfig.Instance.FlapHunger, (hungerDrain) =>
                    {
                        EntityBehaviorHunger hunger = __instance.GetBehavior<EntityBehaviorHunger>();
                        if ((__instance is EntityPlayer player && WingworksStats.ShouldUseHunger(player)) && hunger != null)
                        {
                            hunger.ConsumeSaturation(hungerDrain);
                        }
                    });
                }
            } else
            {
                wings.SetFloat("flap", -1);
            }
        }
        if (__instance.Api is ICoreClientAPI)
        {
            EntityControls servercontrols = Traverse.Create(__instance).Field("servercontrols").GetValue() as EntityControls;
            bool alwaysRunIdle = (bool)Traverse.Create(__instance).Field("alwaysRunIdle").GetValue();

            __instance.CurrentControls = !__instance.Alive ? EnumEntityActivity.Dead : (EnumEntityActivity)((servercontrols.TriesToMove || (servercontrols.Jump || servercontrols.Sneak) && servercontrols.IsClimbing ? 2 : 1) | (!__instance.Swimming || servercontrols.FloorSitting ? 0 : 32) | (servercontrols.FloorSitting ? 512  : 0) | (!servercontrols.Sneak || servercontrols.IsClimbing || servercontrols.FloorSitting || __instance.Swimming ? 0 : 8) | (!servercontrols.TriesToMove || !servercontrols.Sprint || __instance.Swimming || servercontrols.Sneak ? 0 : 4) | (servercontrols.IsFlying ? (servercontrols.Gliding ? 8192  : 16) : 0) | (servercontrols.IsClimbing ? 256 : 0) | (!servercontrols.Jump || !__instance.OnGround ? 0 : 64 ) | (__instance.OnGround || __instance.Swimming || __instance.FeetInLiquid || servercontrols.IsClimbing || servercontrols.IsFlying || __instance.Pos.Motion.Y >= -0.05 ? 0 : 128 ) | (__instance.MountedOn != null ? 16384 : 0));
            __instance.CurrentControls = __instance.CurrentControls == EnumEntityActivity.None ? EnumEntityActivity.Idle : __instance.CurrentControls;
            if (__instance.MountedOn != null && __instance.MountedOn.SkipIdleAnimation)
                __instance.CurrentControls &= ~EnumEntityActivity.Idle;
            ReversePatch.HandleHandAnimations(__instance,dt);
            AnimationMetaData animdata = null;
            bool flag1 = false;
            bool flag2 = false;
            AnimationMetaData[] animations = __instance.Properties.Client.Animations;
            for (int index = 0; animations != null && index < animations.Length; ++index)
            {
                AnimationMetaData animationMetaData = animations[index];
                bool isTarget = animationMetaData.Code.Equals("glide");
                bool wasActive = __instance.AnimManager.IsAnimationActive(animationMetaData.Animation);
                bool flag3 = animationMetaData != null && (animationMetaData.TriggeredBy?.DefaultAnim ?? false);
                bool matches = SpecMatch(animationMetaData, (int)__instance.CurrentControls);
                bool oldMatches = servercontrols.IsFlying;
                bool nowActive = matches || flag3 && __instance.CurrentControls == EnumEntityActivity.Idle;

                flag1 = ((flag1 ? 1 : 0) | (!(!flag3 & wasActive) ? 0 : (animationMetaData.BlendMode == EnumAnimationBlendMode.Average ? 1 : 0))) != 0;
                flag2 = ((flag2 ? 1 : 0) | (nowActive || wasActive && !animationMetaData.WasStartedFromTrigger ? (animationMetaData.SupressDefaultAnimation ? 1 : 0) : 0)) != 0;

                if (isTarget) Console.WriteLine("[" + wasActive + " : " + flag3 + " : " + matches + " : " + oldMatches + " : " + nowActive + "] : [" + flag1 + " : " + flag2 + "] : [" + __instance.CurrentControls + " : " + (__instance.CurrentControls == EnumEntityActivity.Idle) + "]");
                if (flag3)
                    animdata = animationMetaData;
                if (!ReversePatch.onAnimControls(__instance,animationMetaData, wasActive, nowActive))
                {
                    if (!wasActive & nowActive)
                    {
                        animationMetaData.WasStartedFromTrigger = true;
                        __instance.AnimManager.StartAnimation(animationMetaData);
                    }
                    if (!flag3 & wasActive && !nowActive && animationMetaData.WasStartedFromTrigger)
                    {
                        animationMetaData.WasStartedFromTrigger = false;
                        __instance.AnimManager.StopAnimation(animationMetaData.Animation);
                    }
                }
            }
            if (animdata != null && __instance.Alive && !flag2)
            {
                if (flag1 || __instance.MountedOn != null)
                {
                    if (!alwaysRunIdle)
                    {
                        if (__instance.AnimManager.IsAnimationActive(animdata.Animation))
                            __instance.AnimManager.StopAnimation(animdata.Animation);
                    }
                }
                else
                {
                    animdata.WasStartedFromTrigger = true;
                    if (!__instance.AnimManager.IsAnimationActive(animdata.Animation))
                        __instance.AnimManager.StartAnimation(animdata);
                }
            }
            if (!__instance.Alive | flag2 && animdata != null)
                __instance.AnimManager.StopAnimation(animdata.Code);
        }
        return true;
    }
}

[HarmonyPatch]
public class ReversePatch
{
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(EntityAgent), "HandleHandAnimations")]
    public static void HandleHandAnimations(object instance, float dt) =>
        // its a stub so it has no initial content
        throw new NotImplementedException("It's a stub");

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(EntityAgent), "onAnimControls")]
    public static bool onAnimControls(object instance, AnimationMetaData animationMetaData, bool wasActive, bool nowActive) =>
        // its a stub so it has no initial content
        throw new NotImplementedException("It's a stub");
}
