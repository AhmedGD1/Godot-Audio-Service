// PositionalSfxExample.cs
//
// Demonstrates spatial audio: playing a pooled sound at a world position
// so it pans/attenuates with distance from the listener. Useful for
// impacts, footsteps, explosions.
//
// Attach to a Node2D or Node3D and call the matching method.

using Godot;

namespace AudioService.Examples;

public partial class PositionalSfxExample : Node
{
    [Export] public AudioStream ImpactSound;

    public void PlayImpactAt2D(Vector2 worldPosition)
    {
        AudioHost.Instance.PlaySfx2D(ImpactSound, worldPosition);
    }

    public void PlayImpactAt3D(Vector3 worldPosition)
    {
        AudioHost.Instance.PlaySfx3D(ImpactSound, worldPosition);
    }
}
