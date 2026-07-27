using GliderRevamp;
using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Wingworks.API;

namespace Wingworks;

public class WingworksEventHandler
{
    public static void Init()
    {
        GliderEvents.RegisterCalculateActivationSpeed(CalculateActivationSpeed, int.MaxValue);
        GliderEvents.RegisterCalculateClimbCoefficient(CalculateClimb, int.MaxValue);
        GliderEvents.RegisterCalculateDragCoefficient(CalculateDrag, int.MaxValue);
        GliderEvents.RegisterCalculateStallSpeed(CalculateStall, int.MaxValue);
        GliderEvents.RegisterCalculateTurnRate(CalculateTurnRate, int.MaxValue);
        GliderEvents.RegisterCalculateTerminalVelocity(CalculateTerminalVelocity, int.MaxValue);
        GliderEvents.RegisterBeforeGliderPhysicsCalculations(HandleFlapping, int.MaxValue);
    }

    private static float CalculateTerminalVelocity(Entity entity, EntityPos pos, float velocity)
    {
        return WingworksStats.GetOrDefault(entity.Stats, "ww_top_speed", velocity);
    }

    private static float CalculateTurnRate(Entity entity, EntityPos pos, float turnrate)
    {
        return WingworksStats.GetOrDefault(entity.Stats, "ww_turn_rate", turnrate);
    }

    public static float CalculateStall(Entity entity, EntityPos pos, float stall)
    {
        return WingworksStats.GetOrDefault(entity.Stats, "ww_stall_speed", stall);
    }

    public static float CalculateDrag(Entity entity, EntityPos pos, float drag)
    {
        //                                                                         TODO VVV RM THIS, I have added this temporarily to nerf the severity of drag's effect which is just too strong and prevents achieving terminal velocity.
        return WingworksStats.GetOrDefault(entity.Stats, "ww_drag_coefficient", drag) * 0.85F;
    }

    public static float CalculateClimb(Entity entity, EntityPos pos, float climb)
    {
        return WingworksStats.GetOrDefault(entity.Stats, "ww_climb_coefficient", climb);
    }

    public static float CalculateActivationSpeed(Entity entity, EntityPos pos, float activation)
    {
        return WingworksStats.GetOrDefault(entity.Stats, "ww_start_speed", activation);
    }

    private static bool HandleFlapping(PModulePlayerInAir pModule, float dt, Entity entity, EntityPos pos, EntityControls controls)
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
                WingworksStats.OnDefaultedStat(entity.Stats, "ww_brake_decceleration", 0.08F, (gainTick) =>
                {
                    controls.GlideSpeed -= gainTick * dt;
                });
            }

            if (wings.GetFloat("flap") > 9f / 24f)
            {
                // Bonus velocity when looking up at the cost of greater hunger drain.
                var pitchVerticalCoefficient = 1 - Math.Min(0f, WingworksStats.GetPitchFrac(pos));
                WingworksStats.OnDefaultedStat(entity.Stats, "ww_flap_vertical_acceleration", ModConfig.Instance.FlapVerticalBoost, (gainTickY) =>
                {
                    //TODO Either permanently remove this or redo it.
                    WingworksStats.OnDefaultedStat(entity.Stats, "ww_pitch_vertical_multiplier", 0.4F, (val) =>
                    {
                        pos.Motion.Y += gainTickY / 15F * dt * val;// * val;// * pitchVerticalCoefficient * val;
                    });
                });
                WingworksStats.OnDefaultedStat(entity.Stats, "ww_flap_forward_acceleration", ModConfig.Instance.FlapForwardBoost, (gainTickF) =>
                {
                    WingworksStats.OnDefaultedStat(entity.Stats, "ww_flap_min_speed", 0.3F, (speedMin) => {
                        //WingworksStats.OnDefaultedStat(entity.Stats, "ww_pitch_forward_multiplier", 1, (val) =>
                        // {
                        // Bonus velocity when below specific speed.
                        var lowSpeedMultiplier = float.Clamp(speedMin / (float)controls.GlideSpeed, 1F, 3F);
                        //TODO: Make this make sense? Idk I'm just trying to make vertical flight easier but not busted.
                        var pitchForwardMultiplier = Math.Clamp(float.Pow(pitchVerticalCoefficient, lowSpeedMultiplier * 2F) * (pitchVerticalCoefficient > 1 ? 1F : 1.5F), 1F, 5.5F);
                        controls.GlideSpeed += (gainTickF / 15F) * dt * pitchForwardMultiplier * lowSpeedMultiplier;
                        //});
                    });
                });
            }
        }
        return true; //return WingworksPModuleFlight.ApplyFlying(dt,entity,pos,controls);
    }
}
