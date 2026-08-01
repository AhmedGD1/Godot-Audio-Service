// CustomEvictionExample.cs
//
// Demonstrates overriding just Release() on PooledBusHandler to change
// what happens to a player once it finishes — everything else (Play,
// Acquire, pitch variance) is inherited unchanged.
//
// Here, finished players always free instead of returning to the pool
// -- e.g. useful for a rarely-used bus where you'd rather not hold
// idle nodes in memory between plays.

using System.Collections.Generic;
using AudioService.Handlers;
using Godot;

namespace AudioService.Examples;

// The extension: never reuse players, always free them when done.
public partial class NoReusePooledBusHandler : PooledBusHandler
{
    public NoReusePooledBusHandler(Node owner, StringName busName, int capacity)
        : base(owner, busName, capacity) { }

    protected override void Release<TNode>(Stack<TNode> pool, TNode player)
    {
        player.QueueFree(); // ignore the pool entirely
    }
}

public partial class CustomEvictionExample : Node
{
    [Export] public AudioStream RareEventSound;

    public override void _Ready()
    {
        var noReuseBus = new NoReusePooledBusHandler(AudioHost.Instance, "RareEvents", capacity: 4);
        AudioHost.Instance.RegisterPooledBus("RareEvents", noReuseBus);
    }

    public void PlayRareEvent()
    {
        var bus = AudioHost.Instance.GetPooledBus("RareEvents");
        bus?.Play(RareEventSound, default);
    }
}
