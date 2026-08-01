# AudioService

A lightweight audio bus manager for Godot 4 (C#). Play pooled sound
effects and crossfaded music through named audio buses, with a simple
API you can extend for your own game.

## Features

- **Pooled SFX playback** — one-shot sounds (2D, 3D, and non-positional) are handed out from a per-bus pool and reclaimed automatically when they finish, no manual cleanup required.
- **Streamed music/ambience** — automatic crossfading between tracks on a bus, with fade-in and fade-out support.
- **Bus-driven configuration** — buses map directly to Godot's Audio Bus Layout, so mixing, effects, and routing stay in the Godot editor where they belong.
- **Extensible by design** — the built-in `PlaySfx` / `PlayMusic` helpers are just extension methods; add your own for game-specific audio calls.
- **Adapter-based internals** — a single pooling implementation drives `AudioStreamPlayer`, `AudioStreamPlayer2D`, and `AudioStreamPlayer3D` without duplicated code.

## Installation

1. Copy the `addons/audio_service/` folder into your Godot project's `addons/` directory.
2. In Godot, go to **Project > Project Settings > Plugins** and enable **AudioService**.
3. This registers `AudioHost` as an autoload singleton automatically — no manual setup needed.

## Setup

AudioService registers two buses by default: `SFX` (pooled) and `Music`
(streamed). Both must exist in your project's **Audio Bus Layout**
(Project Settings > Audio > Buses) with matching names, or registration
is skipped with a console warning and playback calls on that bus will
silently no-op.

You can add your own buses at runtime:

```csharp
AudioHost.Instance.RegisterBus(new BusConfig("Ambience", BusBehaviorMode.Streamed));
AudioHost.Instance.RegisterBus(new BusConfig("UI", BusBehaviorMode.Pooled));
```

## Usage

### Sound effects

```csharp
// Simple one-shot
AudioHost.Instance.PlaySfx(clickSound);

// With volume, pitch, and pitch randomization
AudioHost.Instance.PlaySfx(clickSound, new AudioOptions(
    VolumeDb: -4f,
    PitchScale: 1.1f,
    PitchVariance: 0.05f
));

// Positional
AudioHost.Instance.PlaySfx2D(impactSound, position2D);
AudioHost.Instance.PlaySfx3D(impactSound, position3D);
```

Pooled players return themselves to the pool automatically once their
stream finishes — you don't need to hold a reference or clean anything
up. Note that pooled sounds currently reclaim only via natural
completion; there's no built-in way to stop one early (see
[Limitations](#limitations)).

### Music

```csharp
AudioHost.Instance.PlayMusic(themeTrack, fadeDuration: 2f);
AudioHost.Instance.StopMusic(fadeDuration: 3f);
```

Starting a new track crossfades out of whatever was previously playing
on that bus. The same pattern works on any streamed bus via
`PlayStream(busName, stream, fadeDuration)` and `StopStream(...)`.

### Bus control

```csharp
AudioHost.Instance.SetBusVolume("Music", 0.5f); // linear 0–1
AudioHost.Instance.SetBusMute("SFX", true);
```

## Extending

The built-in helpers (`PlaySfx`, `PlayMusic`, etc.) are plain C#
extension methods on `AudioHost`. Add your own the same way to keep
game-specific logic out of the plugin:

```csharp
public static class MyGameAudioExtensions
{
    public static void PlayFootstep(this AudioHost host, AudioStream stream, Vector3 position)
    {
        host.PlaySfx3D(stream, position, new AudioOptions(VolumeDb: -6f, PitchVariance: 0.1f));
    }
    
    public static void PlayVoice(this AudioHost host, AudioStream stream, Vector3 position)
    {
        host.GetPooledBus("Voice").Play(stream, new AudioOptions(VolumeDb: 8f, PitchVariance: 0.1f));
    }
}
```

See [`examples/`](examples/) for complete, runnable scripts covering
SFX, positional audio, music crossfading, and custom bus registration.

## Limitations

- Pooled players (`SFX` bus, or any custom `Pooled` bus) are only
  reclaimed when their stream finishes naturally. Calling `.Stop()`
  directly on a node returned from `PlaySfx`/`PlaySfx2D`/`PlaySfx3D`
  will stop the sound but the node will **not** return to the pool —
  it stays alive for the life of the bus. If you need to interrupt a
  sound early, manage that instance's lifecycle yourself outside the
  pool.
- Bus names must match an existing bus in the project's Audio Bus
  Layout; AudioService does not create buses for you.
