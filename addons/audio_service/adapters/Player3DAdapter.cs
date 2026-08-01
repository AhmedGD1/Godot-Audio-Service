using Godot;

namespace AudioService.Adapters;

internal readonly struct Player3DAdapter : IAudioPlayerAdapter<AudioStreamPlayer3D>
{
    public void SetStream(AudioStreamPlayer3D node, AudioStream stream) => node.Stream = stream;
    public void SetBus(AudioStreamPlayer3D node, StringName bus) => node.Bus = bus;
    public void SetVolumeDb(AudioStreamPlayer3D node, float volumeDb) => node.VolumeDb = volumeDb;
    public void SetPitchScale(AudioStreamPlayer3D node, float pitch) => node.PitchScale = pitch;
    public void Play(AudioStreamPlayer3D node) => node.Play();
}
