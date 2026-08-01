// CustomPooledHandlerExample.cs
//
// Demonstrates extending PooledBusHandler by overriding Play(). Every
// method on PooledBusHandler is virtual, so a subclass can add behavior
// around playback without touching the original class.
//
// This example just logs every SFX play to the console — swap the
// GD.Print for anything you need (analytics, muting rules, etc.).

using AudioService.Handlers;
using Godot;

namespace AudioService.Examples;

// The extension: a pooled bus handler that logs before playing.
public partial class LoggingPooledBusHandler : PooledBusHandler
{
    public LoggingPooledBusHandler(Node owner, StringName busName, int capacity)
        : base(owner, busName, capacity) { }

    public override AudioStreamPlayer Play(AudioStream stream, AudioOptions options)
    {
        GD.Print($"[Audio] Playing {stream?.ResourcePath}");
        return base.Play(stream, options);
    }
}

public partial class CustomPooledHandlerExample : Node
{
    [Export] public AudioStream ClickSound;

    public override void _Ready()
    {
        var loggingBus = new LoggingPooledBusHandler(AudioHost.Instance, "SFX", capacity: 16);
        AudioHost.Instance.RegisterPooledBus("SFX", loggingBus);
    }

    public void PlayClick()
    {
        AudioHost.Instance.PlaySfx(ClickSound); // now logs on every play
    }
}
