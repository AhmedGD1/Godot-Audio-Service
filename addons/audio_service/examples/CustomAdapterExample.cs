// CustomAdapterExample.cs
//
// Demonstrates the last extension point: teaching the pooling system
// about a node type it doesn't know about yet, by implementing
// IAudioPlayerAdapter<TNode>. This is the same interface Player1DAdapter/
// Player2DAdapter/Player3DAdapter already implement for the built-in
// AudioStreamPlayer types.
//
// Below, a minimal adapter for a hypothetical custom player node that
// only differs by having its own SetStream logic -- in practice you'd
// use this for a node type Audio Service doesn't ship support for.

using AudioService.Adapters;
using Godot;

namespace AudioService.Examples;

// A stand-in "custom" node -- in a real project this would be your own
// Node subclass with whatever playback logic it needs.
public partial class MyCustomPlayer : AudioStreamPlayer
{
}

// The adapter: tells PooledBusHandler.Commit<TNode, TAdapter> how to
// drive MyCustomPlayer the same way it drives the built-in player types.
public readonly struct MyCustomPlayerAdapter : IAudioPlayerAdapter<MyCustomPlayer>
{
    public void SetStream(MyCustomPlayer node, AudioStream stream) => node.Stream = stream;
    public void SetBus(MyCustomPlayer node, StringName bus) => node.Bus = bus;
    public void SetVolumeDb(MyCustomPlayer node, float volumeDb) => node.VolumeDb = volumeDb;
    public void SetPitchScale(MyCustomPlayer node, float pitch) => node.PitchScale = pitch;
    public void Play(MyCustomPlayer node) => node.Play();
}

// From here, a subclassed PooledBusHandler could add its own
// Stack<MyCustomPlayer> pool and a PlayCustom() method that calls
// Commit(myPool, stream, options, new MyCustomPlayerAdapter()) --
// reusing all the existing pooling/pitch-variance logic for free.
