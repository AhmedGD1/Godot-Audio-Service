using System.Threading.Tasks;
using Godot;

namespace AudioService.Handlers;

public partial class StreamedBusHandler
{
    private const float MUTE = -80f;
    private const float NORMAL = 0f;
    
    private readonly Node owner;

    private readonly AudioStreamPlayer channelA;
    private readonly AudioStreamPlayer channelB;

    private AudioStreamPlayer activeChannel;
    private Tween tween;

    public StreamedBusHandler(Node owner, StringName busName)
    {
        this.owner = owner;

        channelA = new AudioStreamPlayer { Bus = busName, Name = $"{busName}_ChannelA" };
        channelB = new AudioStreamPlayer { Bus = busName, Name = $"{busName}_ChannelB" };

        owner.AddChild(channelA);
        owner.AddChild(channelB);
    }

    public virtual void PlayStream(AudioStream stream, double fadeDuration = 1f)
    {
        var targetChannel = (activeChannel == channelA) ? channelB : channelA;
        var outgoingChannel = activeChannel;
    
        targetChannel.Stream = stream;
        targetChannel.Play();
    
        activeChannel = targetChannel;
    
        tween = RecreateTween().SetParallel();
        tween.TweenProperty(targetChannel, "volume_db", NORMAL, fadeDuration).From(MUTE);
    
        if (outgoingChannel != null && outgoingChannel.Playing)
        {
            tween.TweenProperty(outgoingChannel, "volume_db", MUTE, fadeDuration).From(NORMAL);
            tween.Chain().TweenCallback(Callable.From(outgoingChannel.Stop));
        }
    }
    
    public virtual async Task PlayStreamAsync(AudioStream stream, double fadeDuration = 1f)
    {
        PlayStream(stream, fadeDuration);
    
        var player = activeChannel;
        if (player == null || !player.Playing)
            return;
    
        await activeChannel.ToSignal(player, AudioStreamPlayer.SignalName.Finished);
    }

    public virtual void StopStream(double fadeDuration = 1f)
    {
        if (activeChannel == null)
            return;

        tween = RecreateTween();
        tween.TweenProperty(activeChannel, "volume_db", MUTE, fadeDuration).From(NORMAL);
        tween.TweenCallback(Callable.From(activeChannel.Stop));
    }

    private Tween RecreateTween()
    {
        if (tween != null && tween.IsValid())
            tween.Kill();
        return owner.CreateTween();
    }
}
