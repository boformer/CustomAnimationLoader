using ColossalFramework.Plugins;
using CustomAnimationLoader.Patches;
using Harmony;
using ICities;

namespace CustomAnimationLoader
{
    public class Mod : IUserMod
    {
        private HarmonyInstance _harmony;

        public string Name => "Custom Animation Loader (CAL)";
        public string Description => "Loads custom animations for assets";

        public void OnEnabled()
        {
            _harmony = HarmonyInstance.Create("boformer.CustomAnimationLoader.WorkshopTags");
            WorkshopAssetUploadPanelPatch.Apply(_harmony);
        }

        public void OnDisabled()
        {
            WorkshopAssetUploadPanelPatch.Revert(_harmony);
        }
    }

    public class Loading : LoadingExtensionBase
    {
        public static readonly AssetBundleCache BundleCache = new AssetBundleCache();

        private HarmonyInstance _harmony;

        private bool _lsmPatchApplied = false;
        private bool _lsmTestPatchApplied = false;
        
        public override void OnCreated(ILoading loading)
        {
            _harmony = HarmonyInstance.Create("boformer.CustomAnimationLoader");

            PackageAssetPatch.Apply(_harmony);

            if (IsLoadingScreenModEnabled)
            {
                LsmAssetDeserializerPatch.Apply(_harmony);
                _lsmPatchApplied = true;
            }

            if (IsLoadingScreenModTestEnabled)
            {
                LsmTestAssetDeserializerPatch.Apply(_harmony);
                _lsmTestPatchApplied = true;
            }

            AssetAnimationLoader.Ensure();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            AssetAnimationLoader.instance.MaybeShowAssetErrorsModal();
        }

        public override void OnReleased()
        {
            PackageAssetPatch.Revert(_harmony);

            if (_lsmPatchApplied)
            {
                LsmAssetDeserializerPatch.Revert(_harmony);
                _lsmPatchApplied = false;
            }

            if (_lsmTestPatchApplied)
            {
                LsmTestAssetDeserializerPatch.Revert(_harmony);
                _lsmTestPatchApplied = false;
            }
        }

        private static bool IsLoadingScreenModEnabled => IsModEnabled(667342976uL);

        private static bool IsLoadingScreenModTestEnabled => IsModEnabled(833779378uL);

        private static bool IsModEnabled(ulong workshopId)
        {
            foreach (var current in PluginManager.instance.GetPluginsInfo())
            {
                if (current.isEnabled && current.publishedFileID.AsUInt64 == workshopId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
