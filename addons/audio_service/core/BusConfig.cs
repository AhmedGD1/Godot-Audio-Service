using Godot;

namespace AudioService;

public enum BusBehaviorMode
{
    Pooled,
    Streamed
}

public record struct BusConfig(StringName BusName, BusBehaviorMode Mode);
