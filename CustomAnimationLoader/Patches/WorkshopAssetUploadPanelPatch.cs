using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ColossalFramework.Packaging;
using HarmonyLib;
using UnityEngine;

namespace CustomAnimationLoader.Patches
{
    /// <summary>
    /// Patch for the asset upload process. Add an extra tag for assets with custom animations
    /// </summary>
    public static class WorkshopAssetUploadPanelPatch
    {
        private const string AssetWorkshopTag = "Custom Animation";

        public static void Apply(Harmony harmony)
        {
            var prefix = typeof(WorkshopAssetUploadPanelPatch).GetMethod("UpdateItemPrefix");
            harmony.Patch(OriginalMethod, new HarmonyMethod(prefix), null, null);
            Debug.Log("WorkshopAssetUploadPanelPatch applied");
        }

        public static void Revert(Harmony harmony)
        {
            harmony.Unpatch(OriginalMethod, HarmonyPatchType.Prefix);
        }

        private static MethodInfo OriginalMethod => typeof(WorkshopAssetUploadPanel).GetMethod("UpdateItem", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void UpdateItemPrefix(Package.Asset ___m_TargetAsset, string ___m_ContentPath, ref string[] ___m_Tags)
        {
            if (___m_TargetAsset.type == UserAssetType.CustomAssetMetaData)
            {
                var animationBundlePath = Path.Combine(___m_ContentPath, AssetAnimationLoader.AnimationBundleFileName);
                if (File.Exists(animationBundlePath))
                {
                    if (!___m_Tags.Contains(AssetWorkshopTag))
                    {
                        var tagList = new List<string>(___m_Tags);
                        tagList.Add(AssetWorkshopTag);
                        ___m_Tags = tagList.ToArray();
                    }
                }
                else
                {
                    if (___m_Tags.Contains(AssetWorkshopTag))
                    {
                        var tagList = new List<string>(___m_Tags);
                        tagList.Remove(AssetWorkshopTag);
                        ___m_Tags = tagList.ToArray();
                    }
                }
            }

        }
    }
}
