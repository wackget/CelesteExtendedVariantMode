using Celeste;
using Celeste.Mod;
using Monocle;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using ExtendedVariants.Module;

namespace ExtendedVariants.Variants {
    class MuteSeekerSounds : AbstractExtendedVariant {

        public MuteSeekerSounds() : base(typeof(bool), false) {
        }

        public override object ConvertLegacyVariantValue(int value) {
            return value != 0;
        }

        public override void Load() {
            IL.Celeste.Seeker.RegenerateCoroutine += muteSeekerSounds;
        }

        public override void Unload() {
            IL.Celeste.Seeker.RegenerateCoroutine -= muteSeekerSounds;
        }

        private static void muteSeekerSounds(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.Before,
                instr => instr.OpCode == OpCodes.Call && instr.Operand.ToString().Contains("Monocle.Audio::Play"))) {

                cursor.EmitDelegate<Func<string, string>>(sound => {
                    if (GetVariantValue<bool>(ExtendedVariantsModule.Variant.MuteSeekerSounds)) {
                        return null;
                    }
                    return sound;
                });

                cursor.Index++;
            }
        }
    }
}