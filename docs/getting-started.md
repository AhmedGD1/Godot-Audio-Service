# Getting Started

## Installation

1. Copy `addons/audio_service` into your project's `addons/` folder.
2. Enable **Audio Service** under **Project → Project Settings → Plugins**.
3. Make sure the bus names you intend to use (`SFX`, `Music`, or any custom ones) exist in **Project → Audio Bus Layout**.

The plugin registers an `AudioHost` autoload for you — no manual autoload setup needed. Access it anywhere via `AudioHost.Instance`.

## Your first sound

```csharp
AudioHost.Instance.PlaySfx(ClickSound);
```

That's it. `ClickSound` is any `AudioStream` (a `.wav`, `.ogg`, or `.mp3` resource). Behind the scenes, `AudioHost` pulls a player out of a pool, plays your sound, and returns the player to the pool automatically when it finishes — no node bookkeeping required on your part.

## Positional sound

```csharp
AudioHost.Instance.PlaySfx2D(ImpactSound, worldPosition2D);
AudioHost.Instance.PlaySfx3D(ImpactSound, worldPosition3D);
```

Same pooling behavior, but the returned player is positioned in world space so it pans/attenuates naturally.

## Music

```csharp
AudioHost.Instance.PlayMusic(GameplayTheme, fadeDuration: 1.5f);
AudioHost.Instance.StopMusic(fadeDuration: 3f);
```

Calling `PlayMusic` again while something is already playing crossfades automatically — the old track fades out while the new one fades in. You never have to track what's currently playing yourself.

## What's registered by default

| Bus     | Mode     | Meaning |
|---------|----------|---------|
| `SFX`   | Pooled   | Fire-and-forget one-shots, players are recycled |
| `Music` | Streamed | Long-running tracks, crossfaded via two alternating channels |

Both buses must exist in your project's **Audio Bus Layout** or registration silently fails with a console warning — Audio Service never creates buses in the layout for you.

## Next steps

- [API Reference](api-reference.md) — every public method on `AudioHost`, the handlers, and `AudioOptions`.
- [Custom Buses](custom-buses.md) — registering your own buses beyond `SFX`/`Music`.
- [Extending](extending.md) — subclassing handlers, custom eviction, custom node types.
