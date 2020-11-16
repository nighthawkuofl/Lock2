using System;
using HarmonyLib;
using RimWorld;

namespace Locks2.Harmony
{
    [HarmonyPatch(typeof(Pawn_Ownership), nameof(Pawn_Ownership.ClaimBedIfNonMedical))]
    public class Pawn_Ownership_ClaimBedIfNonMedical_Patch
    {
        public static void Prefix(Pawn_Ownership __instance)
        {
            __instance?.pawn?.Map?.reachability?.ClearCache();
        }
    }

    [HarmonyPatch(typeof(Pawn_Ownership), nameof(Pawn_Ownership.UnclaimBed))]
    public class Pawn_Ownership_UnclaimBed_Patch
    {
        public static void Prefix(Pawn_Ownership __instance)
        {
            __instance?.pawn?.Map?.reachability?.ClearCache();
        }
    }
}
