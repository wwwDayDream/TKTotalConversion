using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace TKTotalConversion.Patches;

[HarmonyPatch(typeof(Player))]
internal class ZorroPlayerPatch {
    [HarmonyPatch(nameof(Player.Update))]
    [HarmonyPostfix]
    private static void GiveGun(Player __instance)
    {
        if (!__instance.IsLocal) return;
        if (!UnityInput.Current.GetKeyUp(KeyCode.F6)) return;

        if (Player.localPlayer.TryGetInventory(out var inv) && inv.TryAddItem(new ItemDescriptor(AssetManager.VN1, new ItemInstanceData(Guid.NewGuid())), out var slot))
            Player.localPlayer.refs.view.RPC("RPC_SelectSlot", Player.localPlayer.refs.view.Owner, [slot.SlotID]);
    }
}