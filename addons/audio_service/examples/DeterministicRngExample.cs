// DeterministicRngExample.cs
//
// Demonstrates seeding a PooledBusHandler's RNG so PitchVariance rolls
// are reproducible -- useful for automated tests or deterministic replays.

using Godot;

namespace AudioService.Examples;

public partial class DeterministicRngExample : Node
{
    [Export] public AudioStream FootstepSound;

    public override void _Ready()
    {
        // Same seed always produces the same sequence of pitch variance rolls.
        AudioHost.Instance.GetPooledBus("SFX")?.SeedRng(seed: 12345);
    }

    public void PlayFootstep()
    {
        AudioHost.Instance.PlaySfx(FootstepSound, new AudioOptions(PitchVariance: 0.1f));
    }
}
