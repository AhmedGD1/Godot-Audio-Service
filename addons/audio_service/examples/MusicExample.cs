// MusicExample.cs
//
// Demonstrates the streamed "Music" bus: crossfading between tracks and
// stopping music with a fade-out. Unlike PlaySfx, music playback is not
// pooled — there are exactly two alternating channels (A/B) so that
// starting a new track automatically crossfades out of whatever was
// previously playing.
//
// Attach to any Node, assign the two tracks in the inspector, and use
// the exported methods (e.g. wire them to UI buttons) to try it out.

using Godot;

namespace AudioService.Examples;

public partial class MusicExample : Node
{
    [Export] public AudioStream MenuTheme;
    [Export] public AudioStream GameplayTheme;

    public override void _Ready()
    {
        // Start menu music immediately with a 2 second fade-in.
        AudioHost.Instance.PlayMusic(MenuTheme, fadeDuration: 2f);
    }

    // Call this when the player starts a level — crossfades from
    // whatever is currently playing (MenuTheme) into GameplayTheme.
    public void OnLevelStarted()
    {
        AudioHost.Instance.PlayMusic(GameplayTheme, fadeDuration: 1.5f);
    }

    // Call this when returning to the menu.
    public void OnReturnedToMenu()
    {
        AudioHost.Instance.PlayMusic(MenuTheme, fadeDuration: 1.5f);
    }

    // Call this to fade music out completely (e.g. on game over).
    public void OnGameOver()
    {
        AudioHost.Instance.StopMusic(fadeDuration: 3f);
    }

    // The Music bus is just a convenience wrapper around a general
    // "streamed bus" concept — you can use the same pattern for any
    // other streamed bus you register yourself (see CustomBusExample.cs),
    // via the lower-level PlayStream/StopStream API:
    public void PlayOnCustomBus(AudioStream stream, StringName busName)
    {
        AudioHost.Instance.PlayStream(busName, stream, fadeDuration: 1f);
    }
}
