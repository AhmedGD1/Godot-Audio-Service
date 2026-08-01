using Godot;

namespace AudioService;

public static class AudioServiceExtensions
{
    private static readonly StringName SfxBusName = "SFX";
    private static readonly StringName MusicBusName = "Music";
    
    public static AudioStreamPlayer PlaySfx(this AudioHost host,
        AudioStream stream, AudioOptions options = default)
    {
        return host.GetPooledBus(SfxBusName)?.Play(stream, options);
    }

    public static AudioStreamPlayer2D PlaySfx2D(this AudioHost host, AudioStream stream,
        Vector2 position, AudioOptions options = default)
    {
        return host.GetPooledBus(SfxBusName)?.Play2D(stream, position, options);
    }

    public static AudioStreamPlayer3D PlaySfx3D(this AudioHost host, AudioStream stream,
        Vector3 position, AudioOptions options = default)
    {
        return host.GetPooledBus(SfxBusName)?.Play3D(stream, position, options);
    }

    public static void PlayStream(this AudioHost host, StringName busName, AudioStream stream, float fadeDuration = 1f)
    {
        host.GetStreamedBus(busName)?.PlayStream(stream, fadeDuration);
    }
    
    public static void StopStream(this AudioHost host, StringName busName, float fadeDuration = 1f)
    {
        host.GetStreamedBus(busName)?.StopStream(fadeDuration);
    }

    public static void PlayMusic(this AudioHost host, AudioStream stream, float fadeDuration = 1f)
    {
        host.PlayStream(MusicBusName, stream, fadeDuration);
    }
    
    public static void StopMusic(this AudioHost host, float fadeDuration = 1f)
    {
        host.StopStream(MusicBusName, fadeDuration);
    }
}
