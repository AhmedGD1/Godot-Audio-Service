# Audio Service

A lightweight, extensible audio management addon for Godot (C#). It gives you a single autoload — `AudioHost` — that manages **buses** for you, so you stop wiring up `AudioStreamPlayer` nodes by hand and start calling `PlaySfx()` / `PlayMusic()` instead.

```csharp
AudioHost.Instance.PlaySfx(ClickSound);
AudioHost.Instance.PlaySfx3D(ExplosionSound, worldPosition);
AudioHost.Instance.PlayMusic(GameplayTheme, fadeDuration: 1.5f);
```

## Why

Godot gives you `AudioStreamPlayer`, `AudioStreamPlayer2D`, and `AudioStreamPlayer3D` as raw building blocks. Audio Service adds the layer most games end up hand-rolling on top of them:

- **Pooling** — one-shot sounds (SFX) reuse a pool of players per bus instead of spawning/freeing nodes constantly.
- **Crossfading** — streamed audio (Music) alternates between two channels and tweens volume automatically.
- **Bus-driven config** — behavior (pooled vs. streamed) is attached to a Godot audio bus, not scattered across scenes.

## Installation

1. Copy `addons/audio_service` into your project's `addons/` folder.
2. Enable **Audio Service** under **Project → Project Settings → Plugins**.
3. Make sure the bus names you intend to use (`SFX`, `Music`, or any custom ones) exist in **Project → Audio Bus Layout**.

The plugin registers an `AudioHost` autoload for you — no manual autoload setup needed.

## Quick start

```csharp
// One-shot SFX
AudioHost.Instance.PlaySfx(ClickSound);

// Positional SFX (2D / 3D)
AudioHost.Instance.PlaySfx2D(ImpactSound, worldPosition2D);
AudioHost.Instance.PlaySfx3D(ImpactSound, worldPosition3D);

// Per-call tuning: quieter, slightly pitched up, with variance so repeats don't sound identical
AudioHost.Instance.PlaySfx(ClickSound,
    volumeDb: -4f,
    pitchScale: 1.1f,
    pitchVariance: 0.05f
));

// Music, crossfaded automatically between two channels
AudioHost.Instance.PlayMusic(GameplayTheme, fadeDuration: 1.5f);
AudioHost.Instance.StopMusic(fadeDuration: 3f);
```

By default, two buses are registered for you:

| Bus     | Mode     | Meaning |
|---------|----------|---------|
| `SFX`   | Pooled   | Fire-and-forget one-shots, players are recycled |
| `Music` | Streamed | Long-running tracks, crossfaded via two alternating channels |

## Custom buses

You aren't limited to `SFX` and `Music`. Register any bus your project's audio layout defines:

```csharp
AudioHost.Instance.RegisterPooledBus("UI", capacity: 8);
AudioHost.Instance.RegisterStreamedBus("Ambience");

// Pooled: fetch the handler and play directly
var uiBus = AudioHost.Instance.GetPooledBus("UI");
uiBus?.Play(UiClickSound, new AudioOptions(VolumeDb: -3f));

// Streamed: use the generic PlayStream/StopStream overloads
AudioHost.Instance.PlayStream("Ambience", AmbienceLoop, fadeDuration: 4f);
```

The bus name must already exist in your project's Audio Bus Layout — registration is skipped with a console warning otherwise.

## Designed to be extended

Audio Service is deliberately built as a small set of composable, overridable pieces rather than a closed API. Reach past the convenience methods whenever you need to:

- **Subclass the bus handlers.** `PooledBusHandler` and `StreamedBusHandler` are unsealed with `virtual` entry points (`Play` / `Play2D` / `Play3D` / `PlayStream` / `StopStream`), so you can override playback behavior wholesale — e.g. add a concurrency cap, layer in analytics, or change how a bus responds to `Play`.
- **Override just the pooling strategy.** `PooledBusHandler` exposes `protected virtual Acquire<TNode>` and `Release<TNode>` methods, separate from `Play`. Override `Release` alone to change eviction policy (e.g. priority-based culling instead of a hard capacity cutoff) without touching anything else.
- **Register your own subclass.** `AudioHost.RegisterPooledBus<T>(busName, handler)` and `RegisterStreamedBus<T>(busName, handler)` accept a handler instance you construct yourself, and the matching generic `GetPooledBus<T>` / `GetStreamedBus<T>` return it back fully typed — no casting required.
- **Add new player types.** Pooling logic is written once, generically, against `IAudioPlayerAdapter<TNode>` — implement that interface for a new node type and the same `Commit<TNode, TAdapter>` pipeline can pool it. `Player1DAdapter`, `Player2DAdapter`, and `Player3DAdapter` are just the three adapters shipped out of the box.
- **Add your own high-level API.** `PlaySfx`, `PlaySfx2D`, `PlaySfx3D`, `PlayMusic`, and `StopMusic` are plain C# extension methods on `AudioHost` in `AudioServiceExtensions.cs` — not special-cased members. Add your own extension methods the same way (e.g. `PlayVoiceLine`, `DuckMusic`) instead of modifying `AudioHost` itself.
- **Control randomness.** Each `PooledBusHandler` takes an optional `RandomNumberGenerator` at construction (or reseed later via `SeedRng(ulong)`), so pitch-variance rolls can be made deterministic — useful for replays or tests.
- **Bus volume/mute helpers.** `SetBusVolume`, `GetBusVolume`, and `SetBusMute` on `AudioHost` work off linear volume (0–1) and wrap Godot's `AudioServer` for you, but you can always drop to `AudioServer` directly for anything more advanced.

```csharp
// Example: a pooled bus that never exceeds N concurrent players of the *same* stream
public class LimitedPooledBusHandler : PooledBusHandler
{
    public LimitedPooledBusHandler(Node owner, StringName busName, int capacity, RandomNumberGenerator rng = null)
        : base(owner, busName, capacity, rng) { }

    // override Play / Acquire / Release here to add the limiting behavior
}

AudioHost.Instance.RegisterPooledBus("SFX", new LimitedPooledBusHandler(AudioHost.Instance, "SFX", capacity: 16));
var handler = AudioHost.Instance.GetPooledBus<LimitedPooledBusHandler>("SFX"); // typed, no cast
```
