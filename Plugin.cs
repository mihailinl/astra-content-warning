using Astra.Sdk;
using BepInEx;
using UnityEngine;

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
    /// Every game type (<c>MainCamera</c>, <c>Player</c>, <c>VideoCamera</c>, <c>Mirror</c>) is
    /// reached by NAME through <see cref="Astra.Sdk.GameType"/>: this plugin compiles against the
    /// Astra SDK, BepInEx and Unity only — never the game's own assemblies.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(AstraSdk.Guid, AstraSdk.Dependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        static readonly GameType MainCameraType = GameType.Find("MainCamera");
        static readonly GameType VideoCameraType = GameType.Find("VideoCamera");
        static readonly GameType MirrorType = GameType.Find("Mirror");
        static readonly Member<Camera> Cam = MainCameraType.Member<Camera>("Cam");

        void Awake()
        {
            var astra = AstraSdk.Register(MyPluginInfo.PLUGIN_GUID, "Content Warning");
            // A little taller than you: at your own height she read small next to the players.
            astra.Defaults.MatchPlayerHeight = 1.2f;
            astra.Defaults.TeleportDistance = 25f;
            astra.UseCamera(() => Cam.Get(MainCameraType.Static<object>("instance")));
            astra.UsePlayer(_ => CwPlayer.Locate());
            // The video camera in your hand films her too (selfie mode included), and mirrors show
            // her. Neither type is known at compile time, so the parent search uses the non-generic
            // Component.GetComponentInParent(Type) overload with GameType's resolved Type.
            astra.AlsoDrawInto(cam =>
                (VideoCameraType.Exists && cam.GetComponentInParent(VideoCameraType.ClrType) != null) ||
                (MirrorType.Exists && cam.GetComponentInParent(MirrorType.ClrType) != null));
            astra.OnFrame(f =>
            {
                var p = CwPlayer.Local;
                if (p == null) return;
                var d = CwPlayer.Data(p);
                f.Params.Set("crouching", CwPlayer.IsCrouching.Get(d))
                    .Set("sprinting", CwPlayer.IsSprinting.Get(d))
                    .Set("in_bell", CwPlayer.IsInDiveBell.Get(d))
                    .Set("upside_down", CwPlayer.IsHangingUpsideDown.Get(d));
            });
            Logger.LogInfo("Astra is coming down to the Old World");
        }
    }
}
