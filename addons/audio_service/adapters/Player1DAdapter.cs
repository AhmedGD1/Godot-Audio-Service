using Godot;

namespace AudioService.Adapters;

internal readonly struct Player1DAdapter : IAudioPlayerAdapter<AudioStreamPlayer>
{
    public void SetStream(AudioStreamPlayer node, AudioStream stream) => node.Stream = stream;
    public void SetBus(AudioStreamPlayer node, StringName bus) => node.Bus = bus;
    public void SetVolumeDb(AudioStreamPlayer node, float volumeDb) => node.VolumeDb = volumeDb;
    public void SetPitchScale(AudioStreamPlayer node, float pitch) => node.PitchScale = pitch;
    public void Play(AudioStreamPlayer node) => node.Play();
}
