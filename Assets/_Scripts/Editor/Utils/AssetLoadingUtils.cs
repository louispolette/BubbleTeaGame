using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Utils.AssetLoading
{
    public class AssetLoadingUtils
    {
        public static VisualTreeAsset LoadVisualTreeAsset(string path)
        {
            var asset = Resources.Load<VisualTreeAsset>(path);

            return asset;
        }
    }
}
