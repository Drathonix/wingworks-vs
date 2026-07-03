
namespace GliderRevamp;

[UsedImplicitly]
public sealed class ModConfig
{
    public static ModConfig Instance { get; set; } = new();

    // ============================================================
    // Core toggles
    // ============================================================

    public bool DisableGlider { get; set; } = false;
    public bool ShowSpeed { get; set; } = false;

    // ============================================================
    // Flight Physics
    // ============================================================

    public float ClimbCoefficiency { get; set; } = 0.2f;

    public float TurnRate { get; set; } = 90f;  // degrees per second

    public float DragCoefficiency { get; set; } = 0.1f;

    public float StallSpeedMs { get; set; } = 6f;
    
    public float ActivationSpeedMs { get; set; } = 8f;

    public float TerminalVelocityMs { get; set; } = 40f;  // m/s

    public float FlapVerticalBoost { get; set; } = 4;
    public float FlapForwardBoost { get; set; } = 4/19;
    public float FlightHunger { get; set; } = 0.1f;
    public float FlapHunger { get; set; } = 0.2f;

    public float FlapCooldown { get; set; } = 1.5f;
}
