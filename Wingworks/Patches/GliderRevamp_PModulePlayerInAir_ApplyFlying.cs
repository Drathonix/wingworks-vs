using Wingworks.API;
using System;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.Server;

namespace Wingworks.Patches;

[HarmonyPatch(typeof(PModulePlayerInAir), "ApplyFlying"), UsedImplicitly]
public class GliderRevamp_PModulePlayerInAir_ApplyFlying
{
    private static Vec3d RotateTowards(Vec3d fromDir, Vec3d toDir, double maxRadians)
    {
        fromDir = fromDir.Normalize();
        toDir = toDir.Normalize();

        var dot = GameMath.Clamp(fromDir.Dot(toDir), -1, 1);
        var angle = Math.Acos(dot);
        if (angle < 1e-6) return toDir;

        var t = Math.Min(1.0, maxRadians / angle);

        // Slerp on the unit sphere
        var sinAngle = Math.Sin(angle);
        var a = Math.Sin((1 - t) * angle) / sinAngle;
        var b = Math.Sin(t * angle) / sinAngle;

        var blended = fromDir * a + toDir * b;
        return blended.Normalize();
    }
    
    [UsedImplicitly]
    internal static bool Prefix(PModulePlayerInAir __instance, float dt, Entity entity, EntityPos pos, EntityControls controls)
    {
        if (!controls.Gliding)
        {
            GliderRevamp_PModuleInAir_ApplyFlying.ApplyFlying(__instance, dt, entity, pos, controls);
            return false;
        }

        var config = ModConfig.Instance;

        var v = pos.Motion;
        var speed = v.Length();

        var stall = WingworksStats.GetOrDefault(entity.Stats, "ww_stall_speed", config.StallSpeedMs);


        if (speed < stall / 60f || config.DisableGlider)
        {
            controls.Gliding = false;
            controls.GlideSpeed = 0;
            return false;
        }

        if (controls.GlideSpeed <= 0)
        {
            controls.GlideSpeed = speed;
        }

        var vDir = v.Normalize();
        //vDir.Y = 0;
        var viewDir = pos.GetViewVector().ToVec3d().Normalize();
        //viewDir.Y = 0;

        WingworksStats.OnDefaultedStat(entity.Stats, "ww_turn_rate", config.TurnRate, (deg) =>
        {
            var turnRateRadPerSec = deg * (float)Math.PI / 180f;
            var maxTurn = turnRateRadPerSec * dt;

            var newDir = RotateTowards(vDir, viewDir, maxTurn);
            //newDir.Y = v.Normalize().Y;
            var energy = controls.GlideSpeed;
            WingworksStats.OnDefaultedStat(entity.Stats, "ww_climb_coefficient", config.ClimbCoefficiency, (climbCoeff) =>
            {
                WingworksStats.OnDefaultedStat(entity.Stats, "ww_drag_coefficient", config.DragCoefficiency, (dragCoeff) =>
                {
                    // Apply lift.
                    energy -= climbCoeff * v.Y * dt;

                    // Apply drag.
                    energy -= dragCoeff * Math.Max(speed * speed, 0.15f) * dt;

                    WingworksStats.OnDefaultedStat(entity.Stats, "ww_top_speed", config.TerminalVelocityMs, (maxVelocity) =>
                    {
                        // Limit new speed to terminal velocity.
                        energy = GameMath.Clamp(energy,0F, maxVelocity / 60f);

                        controls.GlideSpeed = energy;

                        pos.Motion = newDir * energy;
                    });
                });
            });
        });
        

        

        return false;
    }
}
