using HarmonyLib;
using ColossalFramework.Plugins;

namespace CustomAnimationLoader.Patches {
    public static class Patcher {
        private const string HarmonyId = "boformer.CustomAnimationLoader";
        private static bool patched = false;

        private static bool lsmPatchApplied = false;
        private static bool lsmTestPatchApplied = false;
        private static bool lsmKlytePatchApplied = false;
        private static bool lsmRevisitedPatchApplied = false;

        public static void PatchAll() {
            if (patched) return;

            patched = true;
            var harmony = new Harmony(HarmonyId);

            WorkshopAssetUploadPanelPatch.Apply(harmony);

            PackageAssetPatch.Apply(harmony);

            if (LsmAssetDeserializerPatch.OriginalMethod != null) {
                LsmAssetDeserializerPatch.Apply(harmony);
                lsmPatchApplied = true;
            }

            if (LsmTestAssetDeserializerPatch.OriginalMethod != null) {
                LsmTestAssetDeserializerPatch.Apply(harmony);
                lsmTestPatchApplied = true;
            }

            if (LsmKlyteAssetDeserializerPatch.OriginalMethod != null) {
                LsmKlyteAssetDeserializerPatch.Apply(harmony);
                lsmKlytePatchApplied = true;
            }

            if (LsmRevisitedAssetDeserializerPatch.OriginalMethod != null)
            {
                LsmRevisitedAssetDeserializerPatch.Apply(harmony);
                lsmRevisitedPatchApplied = true;
            }
        }

        public static void UnpatchAll() {
            if (!patched) return;

            var harmony = new Harmony(HarmonyId);

            WorkshopAssetUploadPanelPatch.Revert(harmony);

            PackageAssetPatch.Revert(harmony);

            if (lsmPatchApplied) {
                LsmAssetDeserializerPatch.Revert(harmony);
                lsmPatchApplied = false;
            }

            if (lsmTestPatchApplied) {
                LsmTestAssetDeserializerPatch.Revert(harmony);
                lsmTestPatchApplied = false;
            }

            if (lsmKlytePatchApplied) {
                LsmKlyteAssetDeserializerPatch.Revert(harmony);
                lsmKlytePatchApplied = false;
            }

            if (lsmRevisitedPatchApplied)
            {
                LsmRevisitedAssetDeserializerPatch.Revert(harmony);
                lsmKlytePatchApplied = false;
            }

            patched = false;
        }
    }
}
