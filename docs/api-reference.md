# API Reference

## `AudioHost`

The autoload singleton (`AudioHost.Instance`) that owns every registered bus.

### Registration

```csharp
void RegisterPooledBus(StringName busName, int capacity = 8)
void RegisterPooledBus<T>(StringName busName, T handler) where T : PooledBusHandler

void RegisterStreamedBus(StringName busName)
void RegisterStreamedBus<T>(StringName busName, T handler) where T : StreamedBusHandler
```

- The non-generic overloads construct the default handler for you.
- The generic overloads accept a handler instance you've already built — use these to register a [subclassed handler](extending.md).
- `busName` must already exist in the project's **Audio Bus Layout**, or registration is skipped with a `PushWarning`.
- Re-registering an existing bus name overwrites the previous handler. Any players still parented under the old handler are not automatically cleaned up — avoid registering the same bus name twice unless you mean to replace it.

### Retrieval

```csharp
PooledBusHandler GetPooledBus(StringName busName)
T GetPooledBus<T>(StringName busName) where T : PooledBusHandler

StreamedBusHandler GetStreamedBus(StringName busName)
T GetStreamedBus<T>(StringName busName) where T : StreamedBusHandler
```

The generic overloads return the handler pre-cast to `T` — use this to get back a subclassed handler without casting yourself. If the bus isn't registered, or is registered as a different type than `T`, both overloads log a `PushError` and return `null`.

### Bus volume / mute

```csharp
void SetBusVolume(StringName busName, float linearVolume) // 0..1
float GetBusVolume(StringName busName)                    // 0..1
void SetBusMute(StringName busName, bool isMuted)
```

Thin wrappers over Godot's `AudioServer`, working in linear volume (0–1) instead of decibels. For anything not covered here, use `AudioServer` directly — `AudioHost` doesn't try to wrap the entire `AudioServer` API.

---

## `AudioServiceExtensions`

Extension methods on `AudioHost`. These are the everyday API — plain C# extension methods, not special-cased members, so you can add your own alongside them.

```csharp
AudioStreamPlayer PlaySfx(this AudioHost host, AudioStream stream, AudioOptions options = default)
AudioStreamPlayer2D PlaySfx2D(this AudioHost host, AudioStream stream, Vector2 position, AudioOptions options = default)
AudioStreamPlayer3D PlaySfx3D(this AudioHost host, AudioStream stream, Vector3 position, AudioOptions options = default)

void PlayStream(this AudioHost host, StringName busName, AudioStream stream, float fadeDuration = 1f)
void StopStream(this AudioHost host, StringName busName, float fadeDuration = 1f)

void PlayMusic(this AudioHost host, AudioStream stream, float fadeDuration = 1f)
void StopMusic(this AudioHost host, float fadeDuration = 1f)
```

`PlaySfx*` are shortcuts for `GetPooledBus("SFX")?.Play*(...)`. `PlayMusic`/`StopMusic` are shortcuts for `PlayStream`/`StopStream` against the `"Music"` bus. `PlayStream`/`StopStream` work against any streamed bus by name, built-in or custom.

---

## `AudioOptions`

```csharp
public readonly record struct AudioOptions(
    float VolumeDb = 0f,
    float PitchScale = 1f,
    float PitchVariance = 0f
);
```

Passed to any `Play`/`PlaySfx*` call. `PitchVariance` adds a random offset (`± PitchVariance`) to `PitchScale` on every play, so repeated sounds (footsteps, clicks) don't sound identical. The randomness source is the owning `PooledBusHandler`'s RNG (see below) — seed it for deterministic output.

---

## `PooledBusHandler`

Backs a pooled bus (default: `"SFX"`). Unsealed — every method below is `virtual` or `protected virtual`, so it's a supported base class for subclassing. See [Extending](extending.md) for worked examples.

```csharp
public PooledBusHandler(Node owner, StringName busName, int capacity, RandomNumberGenerator rng = null)

public virtual AudioStreamPlayer   Play(AudioStream stream, AudioOptions options)
public virtual AudioStreamPlayer2D Play2D(AudioStream stream, Vector2 position, AudioOptions options)
public virtual AudioStreamPlayer3D Play3D(AudioStream stream, Vector3 position, AudioOptions options)

public void SeedRng(ulong seed)

protected virtual TNode Acquire<TNode>(Stack<TNode> pool) where TNode : Node, new()
protected virtual void  Release<TNode>(Stack<TNode> pool, TNode player) where TNode : Node

public TNode Commit<TNode, TAdapter>(Stack<TNode> pool, AudioStream stream, AudioOptions options, TAdapter adapter)
    where TNode : Node, new()
    where TAdapter : struct, IAudioPlayerAdapter<TNode>
```

| Member | Purpose |
|---|---|
| `Play` / `Play2D` / `Play3D` | Entry points. Override to intercept playback entirely (logging, debouncing, concurrency limits). |
| `Acquire` | Pulls a player from the pool, or creates one if empty/invalid. Override to change reuse logic. |
| `Release` | Called when a player finishes. Default: return to pool, or `QueueFree()` if at capacity. Override to change eviction policy. |
| `Commit` | The shared pipeline all three `Play*` methods funnel through — acquires a player, applies `AudioOptions`, plays it. Generic over node type and adapter, so it also supports node types beyond the built-in three (see [`IAudioPlayerAdapter`](#iaudioplayeradaptertnode)). |
| `SeedRng` | Reseeds the pitch-variance RNG for deterministic output. |

---

## `StreamedBusHandler`

Backs a streamed bus (default: `"Music"`). Unsealed, `virtual` entry points.

```csharp
public StreamedBusHandler(Node owner, StringName busName)

public virtual void PlayStream(AudioStream stream, float fadeDuration = 1f)
public virtual void StopStream(float fadeDuration = 1f)
```

Internally alternates between two `AudioStreamPlayer` channels: starting a new stream fades the new channel in from `-80dB` while fading the previous active channel out to `-80dB`, then stops it. Only one `Tween` is active at a time — starting a new crossfade kills any in-flight one first.

---

## `IAudioPlayerAdapter<TNode>`

```csharp
public interface IAudioPlayerAdapter<TNode> where TNode : Node
{
    void SetStream(TNode node, AudioStream stream);
    void SetBus(TNode node, StringName bus);
    void SetVolumeDb(TNode node, float volumeDb);
    void SetPitchScale(TNode node, float pitch);
    void Play(TNode node);
}
```

The seam that lets `PooledBusHandler.Commit` pool any node type, not just the three built-in Godot player types. Implemented as `readonly struct`s (not classes) so calls are devirtualized/inlined by the JIT rather than going through interface dispatch.

Shipped implementations: `Player1DAdapter` (`AudioStreamPlayer`), `Player2DAdapter` (`AudioStreamPlayer2D`), `Player3DAdapter` (`AudioStreamPlayer3D`). See [Extending](extending.md#custom-node-types) for how to add your own.

---
