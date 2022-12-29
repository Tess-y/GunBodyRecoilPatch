using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnboundLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace GunBodyRecoilPatch {
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class Main: BaseUnityPlugin {
        private const string ModId = "Root.Gun.bodyRecoil.Patch";
        private const string ModName = "GunBodyRecoilPatch";
        public const string Version = "0.0.1";
        public static Main instance { get; private set; }

        void Awake() {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        void Start() {
            instance=this;
        }
    }

    [HarmonyPatch(typeof(Gun), "Start")]
    public class PatchGunStart {
        public static void Postfix(Gun __instance) {
            __instance.bodyRecoil=1;
        }
    }
    [HarmonyPatch(typeof(Gun), "ResetStats")]
    public class PatchGunReset {
        public static void Postfix(Gun __instance) {
            __instance.bodyRecoil=1;
        }
    }
    [HarmonyPatch(typeof(Gun), "DoAttack")]
    public class PatchGunAttack {
        public static void Postfix(Gun __instance) {
            if(__instance.player is Player player&&player.GetComponent<PlayerVelocity>() is PlayerVelocity playerVelocity) {
                float mass = (float)playerVelocity.GetFieldValue("mass");
                Vector2 velocity = (Vector2)playerVelocity.GetFieldValue("velocity");
                float d = mass/100f;
                float d2 = 25f*__instance.recoilMuiltiplier;
                float charge = 1;
                if(__instance.useCharge) {
                    charge=__instance.currentCharge*__instance.chargeRecoilTo;
                }
                player.data.healthHandler.CallTakeForce(-player.data.input.aimDirection*d2*d*__instance.bodyRecoil*charge, ForceMode2D.Impulse, false, false, 0f);
            }
        }
    }
    [HarmonyPatch(typeof(ApplyCardStats), "CopyGunStats")]
    public class PatchCopyGunStats {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            List<CodeInstruction> code = instructions.ToList();
            int i;
            for(i=0; i<code.Count; i++) {
                if(code[i].opcode == OpCodes.Ldfld&&code[i].operand.ToString().Contains("bodyRecoil")) {
                    i-=2; 
                    break;
                }
            }
            code.RemoveRange(i, 7);
            return code;
        }
        public static void Postfix(Gun copyFromGun, Gun copyToGun) {
            copyToGun.recoilMuiltiplier*=copyFromGun.recoilMuiltiplier;
            copyToGun.bodyRecoil+=copyFromGun.bodyRecoil;
        }
    }
}
