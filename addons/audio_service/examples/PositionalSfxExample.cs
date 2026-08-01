// PositionalSfxExample.cs
//
// Demonstrates spatial audio: playing pooled SFX at a world position in
// both 2D and 3D. Useful for impact sounds, footsteps, explosions, etc.
// where the sound should attenuate/pan based on distance from the listener.
//
// Attach to a Node2D (for the 2D path) or a Node3D (for the 3D path) —
// the two calls are independent, so delete whichever branch doesn't
// apply to your project.

using Godot;

namespace AudioService.Examples;

public partial class PositionalSfxExample : Node
{
    [Export] public AudioStream ImpactSound;

    // --- 2D example -----------------------------------------------------

    public void PlayImpactAt2D(Vector2 worldPosition)
    {
        // Returns the pooled AudioStreamPlayer2D in case you want to read
        // its state, but you do NOT need to hold onto it — it returns
        // itself to the pool automatically when the stream finishes.
        AudioHost.Instance.PlaySfx2D(ImpactSound, worldPosition, new AudioOptions(
            VolumeDb: -2f,
            PitchVariance: 0.08f
        ));
    }

    // --- 3D example -----------------------------------------------------

    public void PlayImpactAt3D(Vector3 worldPosition)
    {
        AudioHost.Instance.PlaySfx3D(ImpactSound, worldPosition, new AudioOptions(
            VolumeDb: -2f,
            PitchVariance: 0.08f
        ));
    }
}
