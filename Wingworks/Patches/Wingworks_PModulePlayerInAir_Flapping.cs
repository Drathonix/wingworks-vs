using ConfigLib;
using Wingworks.API;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Vintagestory;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.Server;
using System.Diagnostics;

namespace Wingworks.Patches;

[HarmonyPatch(typeof(PModulePlayerInAir), nameof(PModulePlayerInAir.ApplyFlying)), HarmonyPriority(401)]
public class Wingworks_PModulePlayerInAir_Flapping
{
    internal static bool Prefix(PModulePlayerInAir __instance, float dt, Entity entity, EntityPos pos, EntityControls controls)
    {
        if (entity is EntityPlayer player)
        {
            if (!WingworksStats.CanFly(entity.Stats) || !controls.Gliding)
            {
                return true;
            }
            ITreeAttribute wings = entity.WatchedAttributes.GetOrAddTreeAttribute("wingworks");
            var config = ModConfig.Instance;
            WingPosition position = WingPositionHelper.GetPosition(wings);
            if (position == WingPosition.BRAKING)
            {
                WingworksStats.OnDefaultedStat(entity.Stats, "ww_brake_decceleration", 0.03F, (gainTick) =>
                {
                    controls.GlideSpeed -= gainTick * dt;
                });
            }
            if (wings.GetFloat("flap") > 9f / 30f)
            {
                // Bonus velocity when looking up at the cost of greater hunger drain.
                var pitchVerticalCoefficient = 1-Math.Min(0f,WingworksStats.GetPitchFrac(pos));
                WingworksStats.OnDefaultedStat(entity.Stats, "ww_flap_vertical_acceleration", ModConfig.Instance.FlapVerticalBoost, (gainTickY) =>
                {
                    WingworksStats.OnDefaultedStat(entity.Stats, "ww_pitch_vertical_multiplier", 0.5F, (val) =>
                    {
                        pos.Motion.Y += gainTickY / 21F * dt * pitchVerticalCoefficient * val;
                    });
                });
                WingworksStats.OnDefaultedStat(entity.Stats, "ww_flap_forward_acceleration", ModConfig.Instance.FlapForwardBoost, (gainTickY) =>
                {
                    WingworksStats.OnDefaultedStat(entity.Stats, "ww_pitch_forward_multiplier", 1, (val) =>
                    {
                        controls.GlideSpeed += (gainTickY / 21F * val) * dt * pitchVerticalCoefficient;
                    });
                });
            }
            // Modifying this on the client as well to reduce desync.
            /*var t = wings.GetFloat("flap");
            if (t > -1)
            {
                Console.WriteLine("T: " + t + " : " + dt);
                t += dt;
                wings.SetFloat("flap", t);
            }*/
        }
        return true; //return WingworksPModuleFlight.ApplyFlying(dt,entity,pos,controls);
    }
}

//grounded: no flap anim, no cooldown.
//glide: no flap anim, no cooldown.
