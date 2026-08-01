using Godot;

namespace AudioService.Adapters;

internal readonly struct Player2DAdapter : IAudioPlayerAdapter<AudioStreamPlayer2D>
{
    public void SetStream(AudioStreamPlayer2D node, AudioStream stream) => node.Stream = stream;
    public void SetBus(AudioStreamPlayer2D node, StringName bus) => node.Bus = bus;
    public void SetVolumeDb(AudioStreamPlayer2D node, float volumeDb) => node.VolumeDb = volumeDb;
    public void SetPitchScale(AudioStreamPlayer2D node, float pitch) => node.PitchScale = pitch;
    public void Play(AudioStreamPlayer2D node) => node.Play();
}
