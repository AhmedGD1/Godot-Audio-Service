// CustomBusExample.cs
//
// Demonstrates registering your own bus beyond the built-in "SFX"
// (pooled) and "Music" (streamed) buses that AudioHost registers by
// default. Useful for e.g. a dedicated "Ambience" streamed bus, a
// "UI" pooled bus separate from gameplay SFX, or a "VoiceOver" bus.
//
// IMPORTANT: the bus name you pass to RegisterBus must already exist
// in your Godot project's Audio bus layout (Project > Audio Bus Layout),
// otherwise registration is skipped with a console warning and the
// bus will silently do nothing when played.
//
// Attach to any Node and call RegisterCustomBuses() before you try to
// play anything on them — e.g. from an early _Ready(), or your own
// game-init sequence. AudioHost itself is an autoload, so it already
// exists by the time your own _Ready() code runs.

using Godot;
using AudioService.Handlers;

namespace AudioService.Examples;

public partial class CustomBusExample : Node
{
    [Export] public AudioStream UiClickSound;
    [Export] public AudioStream AmbienceLoop;

    public override void _Ready()
    {
        RegisterCustomBuses();
        UseCustomBuses();
    }

    private void RegisterCustomBuses()
    {
        // A pooled bus for UI sounds, kept separate from gameplay SFX
        // so you can duck/mute them independently in the Audio Bus Layout.
        AudioHost.Instance.RegisterBus(new BusConfig("UI", BusBehaviorMode.Pooled));

        // A streamed bus for ambient loops (wind, rain, room tone) that
        // crossfades the same way Music does.
        AudioHost.Instance.RegisterBus(new BusConfig("Ambience", BusBehaviorMode.Streamed));
    }

    private void UseCustomBuses()
    {
        // Pooled custom bus: fetch the handler directly and call Play/Play2D/Play3D.
        PooledBusHandler uiBus = AudioHost.Instance.GetPooledBus("UI");
        if (UiClickSound is not null)
            uiBus?.Play(UiClickSound, new AudioOptions(VolumeDb: -3f));

        // Streamed custom bus: use the generic extension overload that takes a bus name,
        // or fetch the handler directly via GetStreamedBus and call PlayStream/StopStream on it.
        if (AmbienceLoop is not null)
            AudioHost.Instance.PlayStream("Ambience", AmbienceLoop, fadeDuration: 4f);
    }
}
