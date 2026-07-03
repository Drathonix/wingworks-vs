using Wingworks;

namespace Wingworks.HudElements;

internal class GliderSpeedHudElement(ICoreClientAPI capi) : HudElement(capi)
{
    private const string SpeedText = "speedtext";
    
    private long _tickListenerId;
    private EntityPlayer _player;
    private bool _isInitialized;

    public override void OnOwnPlayerDataReceived()
    {
        base.OnOwnPlayerDataReceived();
        _player = capi.World.Player.Entity;
        
        var elementBounds = ElementBounds.Fixed(EnumDialogArea.CenterMiddle, 150, 0, 150, 30);

        SingleComposer = capi
            .Gui
            .CreateCompo("gliderspeed", elementBounds)
            .AddDynamicText(
                "Speed: 0.0 m/s",
                CairoFont
                    .WhiteMediumText()
                    .WithFontSize(14),
                elementBounds.ForkChild(),
                SpeedText)
            .Compose();

        _tickListenerId = capi.World.RegisterGameTickListener(UpdateSpeed, 50);
        _isInitialized = true;
    }

    private void UpdateSpeed(float dt)
    {
        if (_player == null)
        {
            return;
        }

        // Check if the player is gliding
        var entityControls = _player.Controls;
        bool isGliding = entityControls.Gliding;

        // Only update if ShowSpeed is enabled AND player is gliding
        if (!ModConfig.Instance.ShowSpeed || !isGliding)
        {
            // Hide the HUD if ShowSpeed is disabled or player is not gliding
            if (IsOpened())
            {
                TryClose();
            }
            return;
        }

        // Show the HUD if ShowSpeed is enabled and player is gliding
        if (!IsOpened() && _isInitialized)
        {
            TryOpen();
        }

        // Get the current motion to calculate speed
        var motion = _player.Pos.Motion;
        var speedBlocksPerSecond = motion.Length() * 60; // Approximate blocks per second
        
        if (SingleComposer?.GetDynamicText(SpeedText) != null)
        {
            SingleComposer.GetDynamicText(SpeedText).SetNewText($"Speed: {speedBlocksPerSecond:F1} m/s");
        }
    }


    public override bool TryOpen()
    {
        if (base.TryOpen())
        {
            if (!_isInitialized)
            {
                OnOwnPlayerDataReceived();
            }
            return true;
        }
        return false;
    }

    public override bool TryClose()
    {
        if (base.TryClose())
        {
            return true;
        }
        return false;
    }

    public override void Dispose()
    {
        base.Dispose();
        if (_tickListenerId > 0)
        {
            capi.World.UnregisterGameTickListener(_tickListenerId);
        }
    }
}

