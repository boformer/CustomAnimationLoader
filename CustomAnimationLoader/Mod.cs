using CitiesHarmony.API;
using CustomAnimationLoader.Patches;
using ICities;

namespace CustomAnimationLoader
{
    public class Mod : IUserMod
    {
        public string Name => "Custom Animation Loader (CAL)";
        public string Description => "Loads custom animations for assets";

        public void OnEnabled()
        {
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled) Patcher.UnpatchAll();
        }

        public static bool IsInGame {
            get {
                if (SimulationManager.instance == null || SimulationManager.instance.m_metaData == null) return false;
                var updateMode = SimulationManager.instance.m_metaData.m_updateMode;
                return updateMode == SimulationManager.UpdateMode.NewGameFromMap ||
                       updateMode == SimulationManager.UpdateMode.NewGameFromScenario ||
                       updateMode == SimulationManager.UpdateMode.LoadGame;
            }
        }
    }

    public class Loading : LoadingExtensionBase
    {
        public static readonly AssetBundleCache BundleCache = new AssetBundleCache();
        
        public override void OnCreated(ILoading loading)
        {
            AssetAnimationLoader.Ensure();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            AssetAnimationLoader.instance.MaybeShowAssetErrorsModal();
        }
    }
}
