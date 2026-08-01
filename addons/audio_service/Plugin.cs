#if TOOLS
using Godot;

namespace AudioService;

[Tool]
public partial class Plugin : EditorPlugin
{
    private const string AUTOLOAD_NAME = "AudioHost";
    private const string AUTOLOAD_PATH = "res://addons/audio_service/core/AudioHost.cs";
    
	public override void _EnterTree()
	{
	    AddAutoloadSingleton(AUTOLOAD_NAME, AUTOLOAD_PATH);
        GD.Print("[AudioService] Plugin activated. AutoLoad 'AudioHost' registered.");
	}

	public override void _ExitTree()
    {
        RemoveAutoloadSingleton(AUTOLOAD_NAME);
        GD.Print("[AudioService] Plugin deactivated. AutoLoad 'AudioHost' removed.");
    }
}
#endif
