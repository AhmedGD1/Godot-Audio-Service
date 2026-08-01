using Godot;

namespace AudioService;

public enum BusBehaviorMode
{
    Pooled,
    Streamed
}

public record BusConfig(StringName BusName, BusBehaviorMode Mode);
