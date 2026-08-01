using Godot;

namespace AudioService.Adapters;

public interface IAudioPlayerAdapter<TNode> where TNode : Node
{
    void SetStream(TNode node, AudioStream stream);
    void SetBus(TNode node, StringName bus);
    void SetVolumeDb(TNode node, float volumeDb);
    void SetPitchScale(TNode node, float pitch);
    void Play(TNode node);
}
