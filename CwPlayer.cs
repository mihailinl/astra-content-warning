using System;
using System.Reflection;
using Astra.Sdk;
using UnityEngine;

namespace AstraContentWarning
{
    /// <summary>
    /// Content Warning's local player, as the Astra SDK sees a player. Reached by NAME
    /// (<see cref="Astra.Sdk.GameType"/>): this plugin never compiles against the game's own
    /// assemblies (verified against the installed game with <c>tools/check-game-members</c> in
    /// astra-bepinex).
    /// </summary>
    static class CwPlayer
    {
        /// <summary>The top of the head above the head position, as a fraction of the head height.</summary>
        const float HeadTop = 0.1f;

        static readonly GameType PlayerType = GameType.Find("Player");
        static readonly GameType PlayerDataType = GameType.Find("PlayerData");

        static readonly Member<object> DataMember = PlayerType.Member<object>("data");
        static readonly Member<bool> Dead = PlayerDataType.Member<bool>("dead");
        static readonly Member<bool> PhysicsAreReady = PlayerDataType.Member<bool>("physicsAreReady");
        static readonly Member<bool> IsGrounded = PlayerDataType.Member<bool>("isGrounded");
        static readonly Member<Vector3> GroundPos = PlayerDataType.Member<Vector3>("groundPos");
        static readonly Member<Vector3> LookDirection = PlayerDataType.Member<Vector3>("lookDirection");

        /// <summary>Raw facts <see cref="AstraContentWarning.Plugin"/> reads every frame, off
        /// <see cref="Data"/>.</summary>
        public static readonly Member<bool> IsCrouching = PlayerDataType.Member<bool>("isCrouching");
        public static readonly Member<bool> IsSprinting = PlayerDataType.Member<bool>("isSprinting");
        public static readonly Member<bool> IsInDiveBell = PlayerDataType.Member<bool>("isInDiveBell");
        public static readonly Member<bool> IsHangingUpsideDown = PlayerDataType.Member<bool>("isHangingUpsideDown");

        // HeadPosition() is a METHOD, not a field or a property, so Astra.Sdk.GameType (members
        // only) does not cover it: this one lookup is plain reflection on GameType.ClrType, cached
        // the same way (found once, invoked every call), never thrown through.
        static readonly MethodInfo HeadPositionMethod = PlayerType.ClrType?.GetMethod(
            "HeadPosition", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);

        static Vector3 HeadPosition(object player)
        {
            try { return player != null && HeadPositionMethod != null ? (Vector3)HeadPositionMethod.Invoke(player, null) : Vector3.zero; }
            catch { return Vector3.zero; }
        }

        /// <summary>YOUR player (never another one), or null in a menu, while loading or dead.</summary>
        public static object Local
        {
            get
            {
                var p = PlayerType.Static<object>("localPlayer");
                if (p == null) return null;
                var d = DataMember.Get(p);
                return d != null && !Dead.Get(d) && PhysicsAreReady.Get(d) ? p : null;
            }
        }

        /// <summary>The player's <c>data</c> (a game-specific type, read only through further
        /// <see cref="GameType"/> members).</summary>
        public static object Data(object player) => DataMember.Get(player);

        public static PlayerInfo? Locate()
        {
            var p = Local;
            var root = p as Component;
            if (root == null) return null;
            var d = DataMember.Get(p);
            var head = HeadPosition(p);
            var groundPos = GroundPos.Get(d);
            bool grounded = IsGrounded.Get(d);
            // On the ground the game knows the ground point; in the air, a standing height below the head.
            var feet = grounded ? groundPos : head + Vector3.down * 1.6f;
            float standing = head.y - groundPos.y;
            var lookDir = LookDirection.Get(d);
            var look = new Vector3(lookDir.x, 0, lookDir.z);
            return new PlayerInfo
            {
                Feet = feet,
                Forward = look,
                Root = root.gameObject,
                Grounded = grounded,
                // The head over the ground of a STANDING player only: a crouch never shrinks her.
                Height = grounded && !IsCrouching.Get(d) && standing > 0.3f ? standing * (1 + HeadTop) : 0,
            };
        }
    }
}
