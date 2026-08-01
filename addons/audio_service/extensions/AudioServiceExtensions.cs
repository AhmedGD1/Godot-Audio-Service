using Godot;

namespace AudioService;

public static class AudioServiceExtensions
{
    private static readonly StringName SfxBusName = "SFX";
    private static readonly StringName MusicBusName = "Music";
    
    public static AudioStreamPlayer PlaySfx(this AudioHost host,
        AudioStream stream, float volumeDb = 0f, float pitchScale = 1f, float pitchVariance = 0f)
    {
        return host.GetPooledBus(SfxBusName)?.Play(stream, new(volumeDb, pitchScale, pitchVariance));
    }

    public static AudioStreamPlayer2D PlaySfx2D(this AudioHost host, AudioStream stream,
        Vector2 position, float volumeDb = 0f, float pitchScale = 1f, float pitchVariance = 0f)
    {
        return host.GetPooledBus(SfxBusName)?.Play2D(stream, position, new(volumeDb, pitchScale, pitchVariance));
    }

    public static AudioStreamPlayer3D PlaySfx3D(this AudioHost host, AudioStream stream,
        Vector3 position, float volumeDb = 0f, float pitchScale = 1f, float pitchVariance = 0f)
    {
        return host.GetPooledBus(SfxBusName)?.Play3D(stream, position, new(volumeDb, pitchScale, pitchVariance));
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
