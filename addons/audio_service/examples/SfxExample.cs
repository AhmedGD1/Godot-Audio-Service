// SfxExample.cs
//
// Demonstrates the simplest use case: firing one-shot sound effects
// through the "SFX" pooled bus. No node reference bookkeeping required —
// pooled players return themselves to the pool automatically once their
// stream finishes.
//
// Attach this script to any Node in a test scene, assign `ClickSound`
// in the inspector, and press Space / click to hear it play.

using Godot;
using AudioService;

public partial class SfxExample : Node
{
    [Export] public AudioStream ClickSound;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Keycode: Key.Space }
            || @event is InputEventMouseButton { Pressed: true })
        {
            PlayClick();
        }
    }

    private void PlayClick()
    {
        if (ClickSound is null)
        {
            GD.PushWarning("[SfxExample] Assign a ClickSound in the inspector first.");
            return;
        }

        // Plain playback at default volume/pitch.
        AudioHost.Instance.PlaySfx(ClickSound);

        // Or with per-call customization: quieter, slightly higher pitch,
        // and a bit of pitch randomization so repeated clicks don't sound identical.
        AudioHost.Instance.PlaySfx(ClickSound, new AudioOptions(
            VolumeDb: -4f,
            PitchScale: 1.1f,
            PitchVariance: 0.05f
        ));
    }
}
