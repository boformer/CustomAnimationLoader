using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimationLoader
{
    public class AssetBundleCache
    {
        private readonly Dictionary<string, AssetBundle> _loadedBundles = new Dictionary<string, AssetBundle>();

        // can return null
        public AssetBundle LoadBundleFromFile(string path)
        {
            if (!_loadedBundles.TryGetValue(path, out var bundle))
            {
                bundle = AssetBundle.LoadFromFile(path);
                if (bundle != null)
                {
                    _loadedBundles[path] = bundle;
                }
            }

            return bundle;
        }

        public void UnloadAll()
        {
            foreach (var bundle in _loadedBundles.Values)
            {
                bundle.Unload(true);
            }

            _loadedBundles.Clear();
        }
    }
}
