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

        RegisterPooledBus("SFX");
        RegisterStreamedBus("Music");
    }

    #region Register

    public void RegisterStreamedBus(StringName busName)
    {
        AssertBus(busName);
        streamedBuses[busName] = new StreamedBusHandler(this, busName);
    }

    public void RegisterStreamedBus<T>(StringName busName, T handler) where T : StreamedBusHandler
    {
        AssertBus(busName);
        streamedBuses[busName] = handler;
    }
    
    public void RegisterPooledBus<T>(StringName busName, T handler) where T : PooledBusHandler
    {
        AssertBus(busName);
        pooledBuses[busName] = handler;
    }
    
    public void RegisterPooledBus(StringName busName, int capacity = 8)
    {
        AssertBus(busName);
        pooledBuses[busName] = new PooledBusHandler(this, busName, capacity);
    }

    #endregion

    #region Bus Capture

    public T GetPooledBus<T>(StringName busName) where T : PooledBusHandler
    {
        if (pooledBuses.TryGetValue(busName, out var handler) && handler is T t)
            return t;

        GD.PushError($"[AudioService] Pooled bus '{busName}' is not registered.");
        return null;
    }

    public T GetStreamedBus<T>(StringName busName) where T : StreamedBusHandler
    {
        if (streamedBuses.TryGetValue(busName, out var handler) && handler is T t)
            return t;

        GD.PushError($"[AudioService] Streamed bus '{busName}' is not registered.");
        return null;
    }

    public PooledBusHandler GetPooledBus(StringName busName)
    {
        return GetPooledBus<PooledBusHandler>(busName);
    }

    public StreamedBusHandler GetStreamedBus(StringName busName)
    {
        return GetStreamedBus<StreamedBusHandler>(busName);
    }
    
    #endregion

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

    private static bool AssertBus(StringName busName)
    {
        int busIndex = AudioServer.GetBusIndex(busName);
        if (busIndex == -1)
        {
            GD.PushWarning($"[AudioService] Bus '{busName}' does not exist in Godot's AudioServer layout.");
            return false;
        }
        return true;
    }
}
