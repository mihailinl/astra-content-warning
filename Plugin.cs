using Astra.Sdk;
using BepInEx;

namespace AstraContentWarning
{
    /// <summary>
    /// Astra for Content Warning: she comes down to the Old World with you.
    /// <list type="bullet">
    /// <item>she accompanies YOUR player and is drawn into the camera you look through;</item>
    /// <item>she is a little taller than your player and follows you through the Old World;</item>
    /// <item>the video camera in your hand films her too, selfie mode included, and mirrors show her;</item>
    /// <item>raw facts for her animation set every frame — crouching, sprinting, in the diving bell,
    /// hanging upside down. What they look like is the set's business.</item>
    /// </list>
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(AstraSdk.Guid, AstraSdk.Dependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        void Awake()
        {
            var astra = AstraSdk.Register(MyPluginInfo.PLUGIN_GUID, "Content Warning");
            // A little taller than you: at your own height she read small next to the players.
            astra.Defaults.MatchPlayerHeight = 1.2f;
            astra.Defaults.TeleportDistance = 25f;
            astra.UseCamera(() => MainCamera.instance != null ? MainCamera.instance.Cam : null);
            astra.UsePlayer(_ => CwPlayer.Locate());
            // The video camera in your hand films her too (selfie mode included), and mirrors show her.
            astra.AlsoDrawInto(cam => cam.GetComponentInParent<VideoCamera>() != null || cam.GetComponentInParent<Mirror>() != null);
            astra.OnFrame(f =>
            {
                var p = CwPlayer.Local;
                if (p == null) return;
                f.Params.Set("crouching", p.data.isCrouching)
                    .Set("sprinting", p.data.isSprinting)
                    .Set("in_bell", p.data.isInDiveBell)
                    .Set("upside_down", p.data.isHangingUpsideDown);
            });
            Logger.LogInfo("Astra is coming down to the Old World");
        }
    }
}
