using Astra.Sdk;
using UnityEngine;

namespace AstraContentWarning
{
    /// <summary>Content Warning's local player, as the Astra SDK sees a player.</summary>
    static class CwPlayer
    {
        /// <summary>The top of the head above the head position, as a fraction of the head height.</summary>
        const float HeadTop = 0.1f;

        /// <summary>YOUR player (never another one), or null in a menu, while loading or dead.</summary>
        public static Player Local
        {
            get
            {
                var p = Player.localPlayer;
                return p != null && p.data != null && !p.data.dead && p.data.physicsAreReady ? p : null;
            }
        }

        public static PlayerInfo? Locate()
        {
            var p = Local;
            if (p == null) return null;
            var d = p.data;
            var head = p.HeadPosition();
            // On the ground the game knows the ground point; in the air, a standing height below the head.
            var feet = d.isGrounded ? d.groundPos : head + Vector3.down * 1.6f;
            float standing = head.y - d.groundPos.y;
            var look = new Vector3(d.lookDirection.x, 0, d.lookDirection.z);
            return new PlayerInfo
            {
                Feet = feet,
                Forward = look,
                Root = p.gameObject,
                Grounded = d.isGrounded,
                // The head over the ground of a STANDING player only: a crouch never shrinks her.
                Height = d.isGrounded && !d.isCrouching && standing > 0.3f ? standing * (1 + HeadTop) : 0,
            };
        }
    }
}
