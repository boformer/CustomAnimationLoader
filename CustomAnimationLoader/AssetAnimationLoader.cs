using System;
using System.Collections.Generic;
using System.IO;
using ColossalFramework;
using ColossalFramework.Packaging;
using ColossalFramework.UI;
using UnityEngine;

namespace CustomAnimationLoader
{
    // TODO Idea: add support for particles combined with a normal renderer (no animation)

    public class AssetAnimationLoader : Singleton<AssetAnimationLoader>
    {
        public const string AnimationBundleFileName = "animations.unity3d";

        private GameObject _prefabCollection;

        private readonly AssetBundleCache _bundleCache = new AssetBundleCache();

        private readonly List<PrefabInfo> _animatedPrefabs = new List<PrefabInfo>();

        private readonly List<string> _assetErrors = new List<string>();

        #region Lifecycle
        public void Awake()
        {
            Initialize();

            LoadingManager.instance.m_metaDataReady += OnMetaDataReady;
            LoadingManager.instance.m_simulationDataReady += OnSimulationDataReady;
            LoadingManager.instance.m_levelUnloaded += OnLevelUnloaded;
        }

        public void OnDestroy()
        {
            LoadingManager.instance.m_metaDataReady -= OnMetaDataReady;
            LoadingManager.instance.m_simulationDataReady -= OnSimulationDataReady;
            LoadingManager.instance.m_levelUnloaded -= OnLevelUnloaded;

            Reset();
        }

        public void OnMetaDataReady()
        {
            Initialize();
        }

        public void OnSimulationDataReady()
        {
            try
            {
                // The GameObjects of animated prefabs must be enabled for the animation to work
                foreach (var prefab in _animatedPrefabs)
                {
                    prefab.gameObject.SetActive(true);
                }

                _animatedPrefabs.Clear();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void OnLevelUnloaded()
        {
            Reset();
        }
        #endregion

        public void Initialize()
        {
            if (_prefabCollection == null)
            {
                _prefabCollection = new GameObject("AnimationPrefabCollection");
                _prefabCollection.transform.parent = transform;
                _prefabCollection.SetActive(false);
            }
        }

        public void Reset()
        {
            if (_prefabCollection != null)
            {
                Destroy(_prefabCollection);
                _prefabCollection = null;
            }

            _bundleCache.UnloadAll();

            _animatedPrefabs.Clear();
            _assetErrors.Clear();
        }

        // returns the replacement prefab with applied animations, or null
        public BuildingInfo ProcessBuildingAsset(Package package, BuildingInfo prefab)
        {
            var assetName = GetAssetName(prefab.name);

            if (prefab.gameObject.GetComponent<Renderer>() == null)
            {
                return null;
            }

            try
            {
                var crpPath = package.packagePath;
                if (crpPath == null) return null;

                var animationBundlePath = Path.Combine(Path.GetDirectoryName(crpPath), AnimationBundleFileName);
                if (!File.Exists(animationBundlePath)) return null;

                Debug.Log($"Found animation bundle for asset {prefab.name}");

                var bundle = Loading.BundleCache.LoadBundleFromFile(animationBundlePath);
                if (bundle == null)
                {
                    Debug.LogError($"Failed to load animation bundle for '{prefab.name}'");
                    _assetErrors.Add($"{assetName}: Duplicate AssetBundle name or broken/incompatible bundle file. Possible solution: "
                                                  + "\n* If you are the publisher of the asset, make sure to disable or delete the local version of the asset!"
                                                  + "\n* In Unity Editor, choose a unique name for your bundle (like the current timestamp) and build it. "
                                                  + "Then rename the bundle file to animations.unity3d and move it next to your .crp file.");
                    return null;
                }

                if (bundle.name == "animations.unity3d")
                {
                    _assetErrors.Add($"{assetName}: Animation bundles must have a unique internal name (NOT animations.unity3d). Solution: "
                                                  + "In Unity Editor, choose a unique name for your bundle (like the current timestamp) and build it. "
                                                  + "Then rename the bundle file to animations.unity3d and move it next to your .crp file.");
                }

                var animationPrefab = bundle.LoadAsset<GameObject>($"Assets/{package.packageName}.{assetName}.prefab");
                if (animationPrefab == null)
                {
                    animationPrefab = bundle.LoadAsset<GameObject>($"Assets/{assetName}.prefab");
                    if (animationPrefab == null)
                    {
                        return null;
                    }
                }

                var replacementPrefab = BuildPrefabWithAnimation(prefab, animationPrefab);
                _animatedPrefabs.Add(replacementPrefab);

                return replacementPrefab;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while applying animation bundle for '{prefab.name}'");
                Debug.LogException(e);
                _assetErrors.Add($"{assetName}: {e.Message}");
                return null;
            }
        }

        private BuildingInfo BuildPrefabWithAnimation(BuildingInfo assetPrefab, GameObject animationPrefab)
        {
            Debug.Log($"Building prefab with animation for {assetPrefab.name}");

            var assetGameObject = assetPrefab.gameObject;
            var customGameObject = Instantiate(animationPrefab, _prefabCollection.transform);
            customGameObject.name = assetGameObject.name;
            customGameObject.tag = assetGameObject.tag;
            customGameObject.layer = assetGameObject.layer;

            // Use materials generated by the game
            var assetRenderer = assetGameObject.GetComponent<Renderer>();
            var customRenderer = customGameObject.GetComponentInChildren<SkinnedMeshRenderer>(); // the renderer sits in a child GameObject!
            customRenderer.material = assetRenderer.material;
            customRenderer.materials = assetRenderer.materials;
            customRenderer.sharedMaterial = assetRenderer.sharedMaterial;
            customRenderer.sharedMaterials = assetRenderer.sharedMaterials;

            // Copy BuildingInfo
            var customPrefab = customGameObject.AddComponent<BuildingInfo>().GetCopyOf(assetPrefab);

            // Set renderer/mesh overrides
            customPrefab.m_overrideMainRenderer = customRenderer;
            customPrefab.m_overrideMainMesh = customRenderer.sharedMesh;

            // Copy AI
            var assetAi = assetGameObject.GetComponent<BuildingAI>();
            var customAi = customGameObject.AddComponent(assetAi.GetType()).GetCopyOf(assetAi);

            return customPrefab;
        }

        private static string GetAssetName(string prefabName)
        {
            var assetName = prefabName;

            if (assetName.EndsWith("_Data"))
            {
                assetName = assetName.Substring(0, assetName.Length - 5);
            }

            return assetName;
        }

        public void MaybeShowAssetErrorsModal()
        {
            if (_assetErrors.Count == 0) return;

            var errorMessage = "The animations of the following assets failed to load. "
                               + "Please report the error to the asset creator:\n\n"
                               + $"{string.Join("\n\n", _assetErrors.ToArray())}";

            UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel")
                .SetMessage("Custom Animation Loader", errorMessage, false);
        }
    }
}
