using System.Collections.Generic;
using AudioService.Handlers;
using Godot;

namespace AudioService;

public partial class AudioHost : Node
{
    public static AudioHost Instance { get; private set; }

    private readonly Dictionary<StringName, PooledBusHandler> pooledBuses = [];
    private readonly Dictionary<StringName, StreamedBusHandler> streamedBuses = [];

    public override void _EnterTree()
    {
        Instance = this;

        RegisterBus(new BusConfig("SFX", BusBehaviorMode.Pooled));
        RegisterBus(new BusConfig("Music", BusBehaviorMode.Streamed));
    }

    public void RegisterBus(BusConfig config)
    {
        int busIndex = AudioServer.GetBusIndex(config.BusName);
        if (busIndex == -1)
        {
            GD.PushWarning($"[AudioService] Bus '{config.BusName}' does not exist in Godot's AudioServer layout.");
            return;
        }

        if (config.Mode == BusBehaviorMode.Streamed)
            streamedBuses[config.BusName] = new StreamedBusHandler(this, config.BusName);
        else
            pooledBuses[config.BusName] = new PooledBusHandler(this, config.BusName);
    }

    public PooledBusHandler GetPooledBus(StringName busName)
    {
        if (pooledBuses.TryGetValue(busName, out var handler))
            return handler;

        GD.PushError($"[AudioService] Pooled bus '{busName}' is not registered.");
        return null;
    }

    public StreamedBusHandler GetStreamedBus(StringName busName)
    {
        if (streamedBuses.TryGetValue(busName, out var handler))
            return handler;

        GD.PushError($"[AudioService] Streamed bus '{busName}' is not registered.");
        return null;
    }

    #pragma warning disable CA1822
    public void SetBusVolume(StringName busName, float linearVolume)
    {
        int idx = AudioServer.GetBusIndex(busName);
        if (idx != -1)
            AudioServer.SetBusVolumeDb(idx, Mathf.LinearToDb(Mathf.Max(0.0001f, linearVolume)));
    }

    public float GetBusVolume(StringName busName)
    {
        int idx = AudioServer.GetBusIndex(busName);
        return idx != -1 ? Mathf.DbToLinear(AudioServer.GetBusVolumeDb(idx)) : 0f;
    }

    public void SetBusMute(StringName busName, bool isMuted)
    {
        int idx = AudioServer.GetBusIndex(busName);
        if (idx != -1)
            AudioServer.SetBusMute(idx, isMuted);
    }
    #pragma warning restore CA1822
}
