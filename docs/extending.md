# Extending

Audio Service is built as a small set of overridable pieces rather than a closed API. This page covers every extension seam, from smallest to largest change.

## Overview

| Seam | Override | Use for |
|---|---|---|
| Randomness | `SeedRng(ulong)` | Deterministic pitch variance (tests, replays) |
| Eviction | `protected virtual Release<TNode>` | Change what happens to a finished player |
| Reuse | `protected virtual Acquire<TNode>` | Change how a player is pulled from the pool |
| Playback | `virtual Play` / `Play2D` / `Play3D` / `PlayStream` / `StopStream` | Intercept or wrap playback entirely |
| Node type | `IAudioPlayerAdapter<TNode>` | Pool a node type Audio Service doesn't ship support for |
| Registration | `RegisterPooledBus<T>` / `RegisterStreamedBus<T>` | Wire your subclass into `AudioHost` |

None of these require modifying the library itself — every one is a subclass, an interface implementation, or a registration call from your own code.

## Your own high-level API

`PlaySfx`, `PlaySfx2D`, `PlaySfx3D`, `PlayMusic`, and `StopMusic` are plain C# extension methods on `AudioHost`, not special-cased members. Add your own the same way instead of modifying `AudioHost` itself:

```csharp
public static class MyAudioExtensions
{
    public static void PlayVoiceLine(this AudioHost host, AudioStream stream)
        => host.GetPooledBus("VoiceOver")?.Play(stream, new AudioOptions(VolumeDb: -2f));
}
```

## Deterministic randomness

The smallest override: no subclassing at all.

```csharp
AudioHost.Instance.GetPooledBus("SFX")?.SeedRng(seed: 12345);
```

Every `PooledBusHandler` owns its own `RandomNumberGenerator`, used for `AudioOptions.PitchVariance`. Reseeding it makes pitch-variance rolls reproducible — useful for automated tests or deterministic replays where audio randomness shouldn't drift the outcome.

## Changing eviction policy

Override `Release` alone to change what happens when a player finishes, without touching acquisition or playback.

```csharp
public partial class NoReusePooledBusHandler : PooledBusHandler
{
    public NoReusePooledBusHandler(Node owner, StringName busName, int capacity)
        : base(owner, busName, capacity) { }

    protected override void Release<TNode>(Stack<TNode> pool, TNode player)
    {
        player.QueueFree(); // never return to the pool
    }
}
```

Register it in place of the default handler:

```csharp
var handler = new NoReusePooledBusHandler(AudioHost.Instance, "RareEvents", capacity: 4);
AudioHost.Instance.RegisterPooledBus("RareEvents", handler);
```

Other things you might do here: priority-based culling instead of a hard capacity cutoff, or logging when a player is evicted.

## Changing reuse logic

Override `Acquire` to change how a player is pulled from the pool — e.g. skip players below some internal readiness check, or prefer the most-recently-used player instead of a stack's natural LIFO order.

```csharp
protected override TNode Acquire<TNode>(Stack<TNode> pool)
{
    // custom selection logic, then fall back to the default:
    return base.Acquire(pool);
}
```

`Acquire` and `Release` are independent — override one without the other.

## Intercepting playback

Override `Play`/`Play2D`/`Play3D` (pooled) or `PlayStream`/`StopStream` (streamed) to wrap or gate playback entirely. This is the seam to reach for when the decision has to happen *before* a player is even acquired, or when you need to run logic around the whole call.

**Example — log every SFX play:**

```csharp
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
```

Calling `base.Play(...)` keeps the original pooling/pitch-variance behavior — you're wrapping it, not replacing it. You can also skip calling `base.Play` altogether to veto playback (e.g. a debounce that returns `null` if the same stream just played).

## Custom node types

The deepest seam: teach the pooling system about a node type it doesn't ship support for, by implementing `IAudioPlayerAdapter<TNode>` — the same interface `Player1DAdapter`/`Player2DAdapter`/`Player3DAdapter` implement for the built-in types.

```csharp
public readonly struct MyCustomPlayerAdapter : IAudioPlayerAdapter<MyCustomPlayer>
{
    public void SetStream(MyCustomPlayer node, AudioStream stream) => node.Stream = stream;
    public void SetBus(MyCustomPlayer node, StringName bus) => node.Bus = bus;
    public void SetVolumeDb(MyCustomPlayer node, float volumeDb) => node.VolumeDb = volumeDb;
    public void SetPitchScale(MyCustomPlayer node, float pitch) => node.PitchScale = pitch;
    public void Play(MyCustomPlayer node) => node.Play();
}
```

From a subclassed `PooledBusHandler`, add your own `Stack<MyCustomPlayer>` pool field and a `PlayCustom()` method that calls `Commit(myPool, stream, options, new MyCustomPlayerAdapter())` — this reuses all the existing pooling, capacity, and pitch-variance logic without duplicating any of it.

Implement adapters as `readonly struct`, not `class` — this lets the JIT devirtualize the interface calls in `Commit`'s generic method instead of going through interface dispatch.

## Registering a subclassed handler

However you've extended `PooledBusHandler` or `StreamedBusHandler`, wire it into `AudioHost` with the generic registration overloads:

```csharp
var handler = new LoggingPooledBusHandler(AudioHost.Instance, "SFX", capacity: 16);
AudioHost.Instance.RegisterPooledBus("SFX", handler);

// later, fetch it back fully typed — no casting required:
var typed = AudioHost.Instance.GetPooledBus<LoggingPooledBusHandler>("SFX");
```

This works for both replacing a built-in bus (`"SFX"`, `"Music"`) and for a bus name you've defined yourself.
