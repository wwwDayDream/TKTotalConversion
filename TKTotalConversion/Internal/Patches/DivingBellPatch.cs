using HarmonyLib;

namespace TKTotalConversion.Internal.Patches;

[HarmonyPatch(typeof(DivingBell))]
public class DivingBellPatch {
    [HarmonyPatch(nameof(DivingBell.GoToSurface))]
    [HarmonyPrefix]
    private static bool OnlyGoToSurfaceIfOneAlive()
    {
        return PlayerHandler.instance.playerAlive.Count == 1;
    }
    
    [HarmonyPatch(nameof(DivingBell.AttemptSetOpen))]
    [HarmonyPrefix]
    private static bool BlockClosingBellUntilOneAlive(bool open)
    {
        return open || PhotonGameLobbyHandler.IsSurface || PlayerHandler.instance.playerAlive.Count == 1;
    }
}