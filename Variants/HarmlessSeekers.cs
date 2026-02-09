using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using static ExtendedVariants.Module.ExtendedVariantsModule;

namespace ExtendedVariants.Variants {
    /// <summary>
    /// Variant that makes Seekers harmless - they won't damage or kill the player.
    /// This implementation only prevents player death without affecting Seeker physics
    /// or how Seekers interact with the world.
    /// </summary>
    class HarmlessSeekers : AbstractExtendedVariant {
        
        public HarmlessSeekers() : base(variantType: typeof(bool), defaultVariantValue: false) { }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            // Hook Player.Die() to check if death was caused by a Seeker
            On.Celeste.Player.Die += onPlayerDie;
        }

        public override void Unload() {
            On.Celeste.Player.Die -= onPlayerDie;
        }

        /// <summary>
        /// Intercept player death and check if it was caused by a Seeker.
        /// This approach doesn't interfere with any collision physics - it only
        /// prevents the death outcome.
        /// </summary>
        private PlayerDeadBody onPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats) {
            if (GetVariantValue<bool>(Variant.HarmlessSeekers)) {
                // Check if the player is currently colliding with a Seeker
                // We do this by checking if there's a Seeker entity touching the player
                bool collidingWithSeeker = false;
                
                foreach (Entity entity in self.Scene.Tracker.GetEntities<Seeker>()) {
                    if (entity is Seeker seeker && self.CollideCheck(seeker)) {
                        collidingWithSeeker = true;
                        Logger.Log(LogLevel.Debug, "ExtendedVariantMode/HarmlessSeekers", 
                            "Prevented death from Seeker collision");
                        break;
                    }
                }
                
                // If colliding with a Seeker, don't die
                if (collidingWithSeeker) {
                    // Optionally give the player a small bounce-back for feedback
                    // This won't affect the Seeker's physics at all
                    if (self.Speed.X != 0 || self.Speed.Y != 0) {
                        self.Speed *= -0.5f; // Gentle bounce in opposite direction
                    } else {
                        // If player has no velocity, give a small push away
                        self.Speed = new Vector2(50f, -50f);
                    }
                    
                    return null; // Don't create a dead body, player stays alive
                }
            }
            
            // Normal death for non-Seeker causes or when variant is disabled
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }
    }
}
