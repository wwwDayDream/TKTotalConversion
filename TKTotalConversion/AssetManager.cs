using UnityEngine;

namespace TKTotalConversion;

internal static class AssetManager {
    private const string VN1BundleName = "Bundles.vn1.assetbundle";
    private const string VN1RifleAssetName = "Assets/TKTC/VN1Rifle.asset";

    private static AssetBundle? vn1Bundle;
    private static Item? vn1 = null;
    
    private static AssetBundle VN1Bundle => vn1Bundle ??=
        AssetBundle.LoadFromStream(typeof(AssetManager).Assembly.GetManifestResourceStream(typeof(AssetManager).Namespace + "." + VN1BundleName));
    
    internal static Item? VN1 => vn1 ??= VN1Bundle.LoadAsset<Item>(VN1RifleAssetName);
}