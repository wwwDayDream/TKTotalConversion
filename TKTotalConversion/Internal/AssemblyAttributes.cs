using System;
using System.Reflection;
using CessilCellsCeaChells.CeaChore;
using HarmonyLib;
using TKTotalConversion;
using TKTotalConversion.Internal;

[assembly: RequiresField(typeof(Player), AssemblyInjections.TKTC_PlayerUpgradablesName, typeof(PlayerUpgradables))]

namespace TKTotalConversion.Internal;

internal static class AssemblyInjections {
    internal const string TKTC_PlayerUpgradablesName = "TKTotalConversion_Upgradables";
    
    private static FieldInfo? TKTC_PlayerUpgradablesField;
    internal static (Func<Player, PlayerUpgradables> Get, Action<Player, PlayerUpgradables> Set) TKTC_PlayerUpgradables {
        get
        {
            if (TKTC_PlayerUpgradablesField == null)
                TKTC_PlayerUpgradablesField = AccessTools.Field(typeof(Player), TKTC_PlayerUpgradablesName);

            return (ply => (PlayerUpgradables)TKTC_PlayerUpgradablesField.GetValue(ply),
                (ply, upgradables) => TKTC_PlayerUpgradablesField.SetValue(ply, upgradables));
        }
    }
}