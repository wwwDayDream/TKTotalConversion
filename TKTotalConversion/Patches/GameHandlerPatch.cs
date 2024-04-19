using System;
using System.Linq;
using HarmonyLib;
using Zorro.Core;

namespace TKTotalConversion.Patches;

[HarmonyPatch(typeof(GameHandler))]
internal class GameHandlerPatch {
    [HarmonyPatch(nameof(GameHandler.Start))]
    [HarmonyPostfix]
    [HarmonyPriority(999)]
    internal static void InjectItemsIntoDatabase()
    {
        if (AssetManager.VN1 == null)
            throw new Exception("AssetManager.VN1 is not present and so can't be added to the ItemDatabase!");
        
        SingletonAsset<ItemDatabase>.Instance.lastLoadedItems.Add(AssetManager.VN1);
        SingletonAsset<ItemDatabase>.Instance.lastLoadedPersistentIDs.Add(AssetManager.VN1.persistentID);
        var objs = SingletonAsset<ItemDatabase>.Instance.Objects.ToList();
        objs.Add(AssetManager.VN1);
        SingletonAsset<ItemDatabase>.Instance.Objects = objs.ToArray();
    }
}