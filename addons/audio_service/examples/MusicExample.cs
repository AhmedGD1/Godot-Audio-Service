// MusicExample.cs
//
// Demonstrates the streamed "Music" bus: crossfading between tracks and
// fading out. Starting a new track automatically crossfades out of
// whatever was playing before — no manual bookkeeping needed.
//
// Attach to any Node, assign both tracks, wire the methods to UI buttons.

using Godot;

namespace AudioService.Examples;

public partial class MusicExample : Node
{
    [Export] public AudioStream MenuTheme;
    [Export] public AudioStream GameplayTheme;

    public override void _Ready()
    {
        AudioHost.Instance.PlayMusic(MenuTheme, fadeDuration: 2f);
    }

    public void OnLevelStarted()
    {
        AudioHost.Instance.PlayMusic(GameplayTheme, fadeDuration: 1.5f);
    }

    public void OnReturnedToMenu()
    {
        AudioHost.Instance.PlayMusic(MenuTheme, fadeDuration: 1.5f);
    }

    public void OnGameOver()
    {
        AudioHost.Instance.StopMusic(fadeDuration: 3f);
    }
}
