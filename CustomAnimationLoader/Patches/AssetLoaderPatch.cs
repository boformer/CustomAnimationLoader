using System;
using System.Linq;
using System.Reflection;
using ColossalFramework.Packaging;
using HarmonyLib;
using UnityEngine;

namespace CustomAnimationLoader.Patches
{
    // Vanilla Asset Loader
    public static class PackageAssetPatch
    {
        public static void Apply(Harmony harmony)
        {
            var postfix = typeof(PackageAssetPatch).GetMethod("Postfix");
            harmony.Patch(OriginalMethod, null, new HarmonyMethod(postfix), null);
        }

        public static void Revert(Harmony harmony)
        {
            harmony.Unpatch(OriginalMethod, HarmonyPatchType.Postfix);
        }

        private static MethodInfo OriginalMethod => typeof(Package.Asset)
            .GetMethods().First(m => m.Name == "Instantiate" && !m.IsGenericMethod);

        public static void Postfix(Package.Asset __instance, ref object __result)
        {
            if(!Mod.IsInGame) return;

            if (!(__result is GameObject) || __instance.package == null)
            {
                return;
            }

            var prefab = ((GameObject)__result).GetComponent<BuildingInfo>();
            if (prefab == null || prefab.name == null)
            {
                return;
            }

            var replacementPrefab = AssetAnimationLoader.instance.ProcessBuildingAsset(__instance.package, prefab);
            if (replacementPrefab != null)
            {
                __result = replacementPrefab.gameObject;
            }
        }
    }

    // LSM Asset Loader
    public static class LsmAssetDeserializerPatch
    {
        public static void Apply(Harmony harmony)
        {
            var originalMethod = OriginalMethod;
            if (originalMethod == null)
            {
                Debug.LogError("LSM AssetDeserializer#DeserializeGameObject() not found!");
                return;
            }

            var postfix = typeof(PackageAssetPatch).GetMethod("Postfix");
            harmony.Patch(originalMethod, null, new HarmonyMethod(postfix), null);
            Debug.Log("LSM patched!");
        }

        public static void Revert(Harmony harmony)
        {
            var originalMethod = OriginalMethod;
            if (originalMethod == null)
            {
                Debug.LogError("LSM AssetDeserializer#DeserializeGameObject() not found!");
                return;
            }

            harmony.Unpatch(originalMethod, HarmonyPatchType.Postfix);
        }

        public static MethodInfo OriginalMethod => Type.GetType("LoadingScreenMod.AssetDeserializer, LoadingScreenMod", false)
                ?.GetMethod("DeserializeGameObject", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Postfix(ref UnityEngine.Object __result, Package ___package)
        {
            if (!Mod.IsInGame) return;

            if (___package == null)
            {
                return;
            }

            if (__result is GameObject gameObject)
            {
                var prefab = gameObject.GetComponent<BuildingInfo>();
                if (prefab == null || prefab.name == null)
                {
                    return;
                }

                var replacementPrefab = AssetAnimationLoader.instance.ProcessBuildingAsset(___package, prefab);
                if (replacementPrefab != null)
                {
                    __result = replacementPrefab.gameObject;
                }
            }
        }
    }

    // LSM Test Asset Loader
    public static class LsmTestAssetDeserializerPatch
    {
        public static void Apply(Harmony harmony)
        {
            var originalMethod = OriginalMethod;
            if (originalMethod == null)
            {
                Debug.LogError("LSM Test AssetDeserializer#DeserializeGameObject() not found!");
                return;
            }

            var postfix = typeof(PackageAssetPatch).GetMethod("Postfix");
            harmony.Patch(originalMethod, null, new HarmonyMethod(postfix), null);
            Debug.Log("LSM Test patched!");
        }

        public static void Revert(Harmony harmony)
        {
            var originalMethod = OriginalMethod;
            if (originalMethod == null)
            {
                Debug.LogError("LSM Test AssetDeserializer#DeserializeGameObject() not found!");
                return;
            }

            harmony.Unpatch(originalMethod, HarmonyPatchType.Postfix);
        }

        public static MethodInfo OriginalMethod => Type.GetType("LoadingScreenModTest.AssetDeserializer, LoadingScreenModTest", false)
                ?.GetMethod("DeserializeGameObject", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Postfix(ref UnityEngine.Object __result, Package ___package)
        {
            if (!Mod.IsInGame) return;

            if (___package == null)
            {
                return;
            }

            if (__result is GameObject gameObject)
            {
                var prefab = gameObject.GetComponent<BuildingInfo>();
                if (prefab == null || prefab.name == null)
                {
                    return;
                }

                var replacementPrefab = AssetAnimationLoader.instance.ProcessBuildingAsset(___package, prefab);
                if (replacementPrefab != null)
                {
                    __result = replacementPrefab.gameObject;
                }
            }
        }
    }

    // Klyte LSM Asset Loader
    public static class LsmKlyteAssetDeserializerPatch {
        public static void Apply(Harmony harmony) {
            var originalMethod = OriginalMethod;
            if (originalMethod == null) {
                Debug.LogError("LSM Klyte AssetDeserializer#DeserializeGameObject() not found!");
                return;
            }

            var postfix = typeof(PackageAssetPatch).GetMethod("Postfix");
            harmony.Patch(originalMethod, null, new HarmonyMethod(postfix), null);
            Debug.Log("LSM Klyte patched!");
        }

        public static void Revert(Harmony harmony) {
            var originalMethod = OriginalMethod;
            if (originalMethod == null) {
                Debug.LogError("LSM Klyte AssetDeserializer#DeserializeGameObject() not found!");
                return;
            }

            harmony.Unpatch(originalMethod, HarmonyPatchType.Postfix);
        }

        public static MethodInfo OriginalMethod => Type.GetType("LoadingScreenMod.AssetDeserializer, LoadingScreenModKlyte", false)
                ?.GetMethod("DeserializeGameObject", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Postfix(ref UnityEngine.Object __result, Package ___package) {
            if (!Mod.IsInGame) return;

            if (___package == null) {
                return;
            }

            if (__result is GameObject gameObject) {
                var prefab = gameObject.GetComponent<BuildingInfo>();
                if (prefab == null || prefab.name == null) {
                    return;
                }

                var replacementPrefab = AssetAnimationLoader.instance.ProcessBuildingAsset(___package, prefab);
                if (replacementPrefab != null) {
                    __result = replacementPrefab.gameObject;
                }
            }
        }
    }
}
