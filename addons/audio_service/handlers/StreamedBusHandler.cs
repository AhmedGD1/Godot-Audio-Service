using Godot;

namespace AudioService.Handlers;

public sealed class StreamedBusHandler
{
    private readonly Node owner;

    private readonly AudioStreamPlayer channelA;
    private readonly AudioStreamPlayer channelB;

    private AudioStreamPlayer activeChannel;
    private Tween tween;

    public StreamedBusHandler(Node owner, StringName busName)
    {
        this.owner = owner;

        channelA = new AudioStreamPlayer { Bus = busName };
        channelB = new AudioStreamPlayer { Bus = busName };

        owner.AddChild(channelA);
        owner.AddChild(channelB);
    }

    public void PlayStream(AudioStream stream, float fadeDuration = 1f)
    {
        var targetChannel = (activeChannel == channelA) ? channelB : channelA;
        var outgoingChannel = activeChannel;

        targetChannel.Stream = stream;
        targetChannel.Play();

        activeChannel = targetChannel;

        tween = RecreateTween().SetParallel();
        tween.TweenProperty(targetChannel, "volume_db", 0f, fadeDuration).From(-80f);

        if (outgoingChannel != null && outgoingChannel.Playing)
        {
            tween.TweenProperty(outgoingChannel, "volume_db", -80f, fadeDuration).From(0f);
            tween.Chain().TweenCallback(Callable.From(outgoingChannel.Stop));
        }
    }

    public void StopStream(float fadeDuration = 1f)
    {
        if (activeChannel == null)
            return;

        tween = RecreateTween();
        tween.TweenProperty(activeChannel, "volume_db", -80f, fadeDuration).From(0f);
        tween.TweenCallback(Callable.From(activeChannel.Stop));
    }

    private Tween RecreateTween()
    {
        if (tween.IsValid())
            tween.Kill();
        return owner.CreateTween();
    }
}
