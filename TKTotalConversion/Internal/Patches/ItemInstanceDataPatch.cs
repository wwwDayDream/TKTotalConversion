using System;
using HarmonyLib;
using TKTotalConversion.VN1;

namespace TKTotalConversion.Internal.Patches;

[HarmonyPatch(typeof(ItemInstanceData))]
internal class ItemInstanceDataPatch {

    [HarmonyPatch(nameof(ItemInstanceData.GetEntryType))]
    [HarmonyPrefix]
    private static bool IDToEntryData(byte identifier, ref ItemDataEntry __result)
    {
        if (identifier != VN1RifleData.Identifier) return true;
        
        __result = new VN1RifleData();
        return false;
    }

    [HarmonyPatch(nameof(ItemInstanceData.GetEntryIdentifier))]
    [HarmonyPrefix]
    private static bool EntryDataToID(Type type, ref byte __result)
    {
        if (type != typeof(VN1RifleData)) return true;
        
        __result = VN1RifleData.Identifier;
        return false;
    }

}