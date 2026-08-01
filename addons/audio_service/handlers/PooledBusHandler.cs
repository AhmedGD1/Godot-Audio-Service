using System.Collections.Generic;
using AudioService.Adapters;
using Godot;

namespace AudioService.Handlers;

public sealed class PooledBusHandler
{
    public int Capacity { get; set; } = 16;
    
    private readonly StringName busName;
    private readonly Node container;

    private readonly Stack<AudioStreamPlayer> pool1D = new();
    private readonly Stack<AudioStreamPlayer2D> pool2D = new();
    private readonly Stack<AudioStreamPlayer3D> pool3D = new();

    public PooledBusHandler(Node owner, StringName busName)
    {
        this.busName = busName;
        container = new Node { Name = $"{busName}_Pool" };
        owner.AddChild(container);
    }

    #region Play

    public AudioStreamPlayer Play(AudioStream stream, AudioOptions options)
    {
        return PlayInternal(pool1D, stream, options, new Player1DAdapter());
    }

    public AudioStreamPlayer2D Play2D(AudioStream stream, Vector2 position, AudioOptions options)
    {
        var player = PlayInternal(pool2D, stream, options, new Player2DAdapter());
        player.GlobalPosition = position;
        return player;
    }

    public AudioStreamPlayer3D Play3D(AudioStream stream, Vector3 position, AudioOptions options)
    {
        var player = PlayInternal(pool3D, stream, options, new Player3DAdapter());
        player.GlobalPosition = position;
        return player;
    }

    #endregion

    private TNode PlayInternal<TNode, TAdapter>(Stack<TNode> pool, AudioStream stream, AudioOptions options, TAdapter adapter)
        where TNode : Node, new()
        where TAdapter : struct, IAudioPlayerAdapter<TNode>
    {
        if (stream == null)
        {
            GD.PushError($"[Audio Service] Can't play a null pooled stream of bus: {busName}.");
            return null;
        }
        
        var player = pool.TryPop(out var p) && GodotObject.IsInstanceValid(p) ? p : CreateNode(pool);
        var pitch = options.PitchScale;

        if (options.PitchVariance > 0f)
            pitch += (float)GD.RandRange(-options.PitchVariance, options.PitchVariance);

        adapter.SetStream(player, stream);
        adapter.SetBus(player, busName);
        adapter.SetVolumeDb(player, options.VolumeDb);
        adapter.SetPitchScale(player, Mathf.Max(0.1f, pitch));
        adapter.Play(player);

        return player;
    }

    private TNode CreateNode<TNode>(Stack<TNode> pool) where TNode : Node, new()
    {
        var player = new TNode();
        container.AddChild(player);

        player.Connect("finished", Callable.From(() =>
        {
            if (pool.Count >= Capacity) player.QueueFree();
            else pool.Push(player);
        }));

        return player;
    }
}
