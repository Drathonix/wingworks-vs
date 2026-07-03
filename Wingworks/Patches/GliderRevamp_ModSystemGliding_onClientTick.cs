using System;
using Vintagestory.API.MathTools;

namespace Wingworks.Patches;

[HarmonyPatch(typeof(ModSystemGliding), "onClientTick", typeof(float)), UsedImplicitly]
public static class GliderRevamp_ModSystemGliding_onClientTick
{
    private static readonly AccessTools.FieldRef<ModSystemGliding, ICoreClientAPI> CapiRef =
        AccessTools.FieldRefAccess<ModSystemGliding, ICoreClientAPI>("capi");

    private static readonly AccessTools.FieldRef<ModSystemGliding, ILoadedSound> GlideSoundRef =
        AccessTools.FieldRefAccess<ModSystemGliding, ILoadedSound>("glideSound");

    internal static void Postfix(ModSystemGliding __instance, float dt)
    {
        SoundModulation(__instance);
        Tunnelvision(__instance, dt);
    }

    private static void SoundModulation(ModSystemGliding __instance)
    {
        var cApi = CapiRef(__instance);
        if (cApi == null) return;

        var sound = GlideSoundRef(__instance);
        if (sound is not { IsPlaying: true }) return;

        var ent = cApi.World?.Player?.Entity;
        if (ent == null) return;

        if (!ent.Controls.Gliding)
        {
            // optional: ensure it’s quiet if something left it playing
            sound.SetVolume(0f);
            return;
        }

        // ---- speed ----
        // If your motion is blocks/tick in this context, convert to blocks/sec:
        var speed = ent.Pos.Motion.Length() * 60.0;

        // ---- mapping ----
        var stall = 6f;
        var max = 40f;

        var t = (float)GameMath.Clamp((speed - stall) / (max - stall), 0, 1);

        // smoothstep
        t = t * t * (3f - 2f * t);

        var volume = 0.05f + 0.95f * t;   // 0.05..1.0
        var pitch  = 0.85f + 0.45f * t;   // 0.85..1.30

        sound.SetVolume(volume);

        // Different VS versions expose pitch differently.
        // Replace this line with the correct API call/property in your version.
        sound.SetPitch(pitch);
    }
    
    private static void Tunnelvision(ModSystemGliding __instance, float dt)
    {
        var cApi = CapiRef(__instance);

        var ent = cApi?.World?.Player?.Entity;
        if (ent == null) return;
        
        if (ent.Controls.Gliding)
        {
            var speedMs = (float)(ent.Pos.Motion.Length() * 60.0);

            var targetScale = SpeedToFovScale(
                speedMs,
                minSpeed: 6f,
                maxSpeed: 40f,
                minScale: 0.9f,
                maxScale: 1.1f
            );

            Patch_Set3DProjection.FovScale = SmoothTo(
                Patch_Set3DProjection.FovScale,
                targetScale,
                responsiveness: 8f,   // 6–12 feels good
                dt: dt
            );
        }
        else
        {
            // Smooth back to normal when not gliding
            Patch_Set3DProjection.FovScale = SmoothTo(
                Patch_Set3DProjection.FovScale,
                1.0f,
                responsiveness: 8f,
                dt: dt
            );
        }
    }

    private static float Smoothstep(float t) => t * t * (3f - 2f * t);

    private static float SpeedToFovScale(float speedMs, float minSpeed, float maxSpeed, float minScale, float maxScale)
    {
        var t = GameMath.Clamp((speedMs - minSpeed) / (maxSpeed - minSpeed), 0, 1);
        t = Smoothstep(t);
        return GameMath.Lerp(minScale, maxScale, t);
    }

    private static float SmoothTo(float current, float target, float responsiveness, float dt)
    {
        var a = 1f - (float)Math.Exp(-responsiveness * dt);
        return current + (target - current) * a;
    }
}
