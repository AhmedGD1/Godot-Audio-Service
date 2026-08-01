// SfxOptionsExample.cs
//
// Demonstrates AudioOptions: per-call volume, pitch, and pitch variance
// so repeated sounds (footsteps, clicks) don't sound identical every time.
//
// Attach to any Node, assign FootstepSound, press Space repeatedly.

using Godot;

namespace AudioService.Examples;

public partial class SfxOptionsExample : Node
{
    [Export] public AudioStream FootstepSound;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Keycode: Key.Space })
        {
            AudioHost.Instance.PlaySfx(FootstepSound, new AudioOptions(
                VolumeDb: -4f,
                PitchScale: 1f,
                PitchVariance: 0.08f // +/- random pitch each play
            ));
        }
    }
}
