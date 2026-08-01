using System.Collections.Generic;
using AudioService.Adapters;
using Godot;

namespace AudioService.Handlers;

public partial class PooledBusHandler
{
    private readonly StringName busName;
    private readonly Node container;

    private readonly Stack<AudioStreamPlayer> pool1D = new();
    private readonly Stack<AudioStreamPlayer2D> pool2D = new();
    private readonly Stack<AudioStreamPlayer3D> pool3D = new();

    private readonly int capacity;

    private RandomNumberGenerator rng;

    public PooledBusHandler(Node owner, StringName busName, int capacity, RandomNumberGenerator rng = null)
    {
        this.busName = busName;
        this.capacity = capacity;

        this.rng = rng ?? new RandomNumberGenerator { Seed = GD.Randi() };

        container = new Node { Name = $"{busName}_Pool" };
        owner.AddChild(container);
    }

    public void SeedRng(ulong seed) => rng = new RandomNumberGenerator { Seed = seed };

    protected virtual TNode Acquire<TNode>(Stack<TNode> pool) where TNode : Node, new()
        => pool.TryPop(out var p) && GodotObject.IsInstanceValid(p) ? p : CreateNode(pool);

    protected virtual void Release<TNode>(Stack<TNode> pool, TNode player) where TNode : Node
    {
        if (pool.Count >= capacity) player.QueueFree();
        else pool.Push(player);
    }

    #region Play

    public virtual AudioStreamPlayer Play(AudioStream stream, AudioOptions options)
    {
        return Commit(pool1D, stream, options, new Player1DAdapter());
    }

    public virtual AudioStreamPlayer2D Play2D(AudioStream stream, Vector2 position, AudioOptions options)
    {
        var player = Commit(pool2D, stream, options, new Player2DAdapter());
        player.GlobalPosition = position;
        return player;
    }

    public virtual AudioStreamPlayer3D Play3D(AudioStream stream, Vector3 position, AudioOptions options)
    {
        var player = Commit(pool3D, stream, options, new Player3DAdapter());
        player.GlobalPosition = position;
        return player;
    }

    #endregion

    public TNode Commit<TNode, TAdapter>(Stack<TNode> pool, AudioStream stream, AudioOptions options, TAdapter adapter)
        where TNode : Node, new()
        where TAdapter : struct, IAudioPlayerAdapter<TNode>
    {
        if (stream == null)
        {
            GD.PushError($"[Audio Service] Can't play a null pooled stream of bus: {busName}.");
            return null;
        }

        var player = Acquire(pool);
        var pitch = options.PitchScale;

        if (options.PitchVariance > 0f)
            pitch += rng.RandfRange(-options.PitchVariance, options.PitchVariance);

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
        player.Connect("finished", Callable.From(() => Release(pool, player)));
        return player;
    }
}
