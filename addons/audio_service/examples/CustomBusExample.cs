// CustomBusExample.cs
//
// Demonstrates registering your own bus beyond the built-in "SFX" and
// "Music" buses — e.g. a separate "UI" pooled bus or an "Ambience"
// streamed bus. The bus name must already exist in your project's
// Audio Bus Layout (Project > Audio Bus Layout).
//
// Attach to any Node and call _Ready() before playing anything on them.

using Godot;

namespace AudioService.Examples;

public partial class CustomBusExample : Node
{
    [Export] public AudioStream UiClickSound;
    [Export] public AudioStream AmbienceLoop;

    public override void _Ready()
    {
        AudioHost.Instance.RegisterPooledBus("UI", capacity: 8);
        AudioHost.Instance.RegisterStreamedBus("Ambience");

        // Pooled custom bus: fetch the handler and call Play directly.
        var uiBus = AudioHost.Instance.GetPooledBus("UI");
        uiBus?.Play(UiClickSound, new AudioOptions(VolumeDb: -3f));

        // Streamed custom bus: use the generic PlayStream overload.
        AudioHost.Instance.PlayStream("Ambience", AmbienceLoop, fadeDuration: 4f);
    }
}
