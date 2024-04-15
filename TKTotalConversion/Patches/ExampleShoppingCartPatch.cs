using System;
using System.Reflection;
using HarmonyLib;

namespace TKTotalConversion.Patches;

[HarmonyPatch(typeof(ShoppingCart))]
public class ExampleShoppingCartPatch {
    [HarmonyPatch(nameof(ShoppingCart.AddItemToCart))]
    [HarmonyPostfix]
    private static void AddItemToCartPostfix(ShoppingCart __instance)
    {
        /*
         * Adding a random value to the visible price of the shopping cart typically is slightly
         * complicated due to the private setter of the CartValue property. However, as we have publicized the
         * game assembly, we do not have to worry about it, since it now is public.
         */
        __instance.CartValue += new Random().Next(0, 100);
    }
}