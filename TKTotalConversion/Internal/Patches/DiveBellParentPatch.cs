using System;
using System.Collections;
using System.Linq;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TKTotalConversion.Internal.Patches;

[HarmonyPatch(typeof(DiveBellParent))]
public class DiveBellParentPatch {
    [HarmonyPatch(nameof(DiveBellParent.GetSpawn))]
    [HarmonyPrefix]
    private static bool OverrideSpawnLocations(DiveBellParent __instance, ref SpawnPoint __result)
    {
        var state = Random.state;
        Random.InitState(GameAPI.seed);
        var currentDay = __instance.testDay;
        if (SurfaceNetworkHandler.RoomStats != null)
            currentDay = GameAPI.CurrentDay;

        var startIdx = currentDay + Random.Range(0, __instance.transform.childCount);
        for (var i = 0; i < __instance.transform.childCount; i++)
        {
            __instance.transform.GetChild(i).gameObject.SetActive(false);
        }
        var idx = 0;
        foreach (var player in PhotonNetwork.CurrentRoom.Players.OrderBy(play => play.Key))
        {
            var cur = __instance.transform.GetChild((startIdx + idx) % __instance.transform.childCount);
            cur.gameObject.SetActive(true);
            if (player.Value.IsLocal)
            {
                __result = cur.GetComponentInChildren<SpawnPoint>();

                var bell = __result.GetComponentInParent<DivingBell>();
                bell.StartCoroutine(DelayedStuff(bell));
            }
            idx++;
        }
        Random.state = state;
        return false;
    }

    private static IEnumerator DelayedStuff(DivingBell bell)
    {
        yield return new WaitUntil(() => PlayerHandler.instance.playerAlive.Contains(Player.localPlayer));
        if (Player.localPlayer.TryGetInventory(out var inv) && 
            inv.TryAddItem(new ItemDescriptor(AssetManager.VN1, new ItemInstanceData(Guid.NewGuid())), out var slot))
            Player.localPlayer.refs.view.RPC("RPC_SelectSlot", Player.localPlayer.refs.view.Owner, [slot.SlotID]);
        yield return new WaitUntil(() => PlayerHandler.instance.playerAlive.Count == PhotonNetwork.CurrentRoom.PlayerCount);
        bell.AttemptSetOpen(true);
    }
}