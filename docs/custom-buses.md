# Custom Buses

You aren't limited to the built-in `SFX` and `Music` buses. Register any bus your project's Audio Bus Layout defines.

## Prerequisite

The bus name must already exist in **Project → Audio Bus Layout** before you register it. If it doesn't, registration is skipped and a warning is logged to the console — Audio Service never creates buses in the layout for you, it only attaches behavior to buses that already exist.

## Pooled custom bus

Use `Pooled` mode for anything fire-and-forget: UI sounds, footsteps, impacts.

```csharp
AudioHost.Instance.RegisterPooledBus("UI", capacity: 8);

var uiBus = AudioHost.Instance.GetPooledBus("UI");
uiBus?.Play(UiClickSound, new AudioOptions(VolumeDb: -3f));
```

`capacity` caps how many idle players are kept around per node type (1D/2D/3D each have their own pool). Once a bus's pool is full, additional finished players are freed instead of recycled.

## Streamed custom bus

Use `Streamed` mode for anything long-running and singular: ambience, voice-over, a second music layer.

```csharp
AudioHost.Instance.RegisterStreamedBus("Ambience");

AudioHost.Instance.PlayStream("Ambience", AmbienceLoop, fadeDuration: 4f);
AudioHost.Instance.StopStream("Ambience", fadeDuration: 2f);
```

`PlayMusic`/`StopMusic` are just `PlayStream`/`StopStream` pinned to `"Music"` — the underlying mechanism (two alternating channels, automatic crossfade) is identical for any streamed bus.

## Choosing pooled vs. streamed

| | Pooled | Streamed |
|---|---|---|
| Use for | Short one-shots, potentially many at once | Long-running, one active track at a time |
| Overlap behavior | Multiple instances play simultaneously | New track crossfades out the previous one |
| Backing nodes | A pool of `AudioStreamPlayer`(2D/3D), sized by `capacity` | Exactly two `AudioStreamPlayer` channels, alternated |

## Re-registering a bus

Calling `RegisterPooledBus`/`RegisterStreamedBus` again with a name that's already registered overwrites the existing handler. This isn't guarded against — avoid doing it accidentally (e.g. from a script that re-runs `_Ready()` more than once), since players parented under the previous handler aren't cleaned up automatically.

## Next steps

To register a bus backed by your own handler subclass instead of the default behavior, see [Extending](extending.md).
