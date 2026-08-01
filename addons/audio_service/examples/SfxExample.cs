// SfxExample.cs
//
// Simplest use case: play a one-shot sound effect through the built-in
// pooled "SFX" bus. No node bookkeeping needed — the player returns
// itself to the pool automatically once the sound finishes.
//
// Attach to any Node, assign ClickSound in the inspector, press Space.

using Godot;

namespace AudioService.Examples;

public partial class SfxExample : Node
{
    [Export] public AudioStream ClickSound;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Keycode: Key.Space })
            AudioHost.Instance.PlaySfx(ClickSound);
    }
}
