# AudioService Examples

This folder contains standalone example scripts showing how to use the
AudioService plugin. Each file is self-contained and commented — copy
whichever ones are relevant into your own project, or use them as a
reference while wiring up your own audio calls.

## Setup

1. Copy `addons/audio_service/` into your Godot project (see the main
   repo README for install instructions).
2. Enable the plugin under **Project > Project Settings > Plugins**.
   This registers `AudioHost` as an autoload automatically.
3. Make sure your project's **Audio Bus Layout** (Project > Project
   Settings > Audio > Buses, or the Audio panel) has buses named
   `SFX` and `Music` — these are the two buses `AudioHost` registers
   by default. If a bus with that name doesn't exist, registration is
   skipped and you'll see a console warning; playback calls on that
   bus will then silently no-op.
4. Attach any of the example scripts below to a node in a test scene
   and assign the exported `AudioStream` fields in the inspector.

## Examples

| File | Demonstrates |
|---|---|
| [`SfxExample.cs`](scripts/SfxExample.cs) | Basic one-shot SFX playback via `PlaySfx`, including per-call volume/pitch/pitch-variance options. |
| [`PositionalSfxExample.cs`](scripts/PositionalSfxExample.cs) | Spatial SFX via `PlaySfx2D` / `PlaySfx3D` for sounds that should pan/attenuate with distance. |
| [`MusicExample.cs`](scripts/MusicExample.cs) | Crossfading between music tracks with `PlayMusic` / `StopMusic`, and the general streamed-bus API underneath. |
| [`CustomBusExample.cs`](scripts/CustomBusExample.cs) | Registering your own pooled or streamed bus beyond the built-in `SFX`/`Music` buses. |

## Core concepts

**Pooled buses** (like `SFX`) are for short, fire-and-forget sounds.
Calling `PlaySfx` (or `PlaySfx2D`/`PlaySfx3D`) hands you an
`AudioStreamPlayer`/`2D`/`3D` node, but you don't need to hold onto
it or clean it up — the node returns itself to the pool automatically
once its stream finishes playing.

> **Note:** pooled players only reclaim themselves via natural
> playback completion. There is currently no supported API to stop a
> pooled sound early — if you call `.Stop()` directly on a node
> returned from `PlaySfx`, it will **not** return to the pool and will
> stay alive for the life of the bus. If your project needs
> interruptible one-shots (e.g. a footstep cut short by a jump), keep
> a manual reference and manage its lifecycle yourself outside the
> pool, rather than stopping a pooled node directly.

**Streamed buses** (like `Music`) are for long-running audio where you
want automatic crossfading between tracks — e.g. background music or
ambience. There are always exactly two alternating channels per
streamed bus; starting a new track crossfades out of whatever was
previously playing on that bus.

**Custom buses**: `AudioHost` only registers `SFX` and `Music` by
default. Call `AudioHost.Instance.RegisterBus(new BusConfig(...))` for
any additional bus you want (see `CustomBusExample.cs`), as long as a
matching bus name already exists in your project's Audio Bus Layout.

## Extending AudioService

`AudioServiceExtensions.cs` in the plugin is a plain C# extension
class on `AudioHost` — the built-in `PlaySfx`/`PlayMusic`/etc. helpers
are just convenience wrappers around `GetPooledBus`/`GetStreamedBus`.
Your own game can add its own extension methods the same way, e.g.:

```csharp
public static class MyGameAudioExtensions
{
    private static readonly StringName FootstepsBus = "SFX";

    public static void PlayFootstep(this AudioHost host, AudioStream stream, Vector3 position)
    {
        host.PlaySfx3D(stream, position, new AudioOptions(VolumeDb: -6f, PitchVariance: 0.1f));
    }
}
```

This keeps game-specific audio logic (which stream to use for which
gameplay event, custom volume curves, etc.) out of the plugin itself.
