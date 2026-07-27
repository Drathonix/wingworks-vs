namespace Wingworks;

[UsedImplicitly]
public sealed class ModConfig
{
    public static ModConfig Instance { get; set; } = new();
    public float FlapVerticalBoost { get; set; } = 4f;
    public float FlapForwardBoost { get; set; } = 4f/19f;
    public float FlightHunger { get; set; } = 0.1f;
    public float FlapHunger { get; set; } = 0.2f;

    public float FlapCooldown { get; set; } = 1.5f;
}
