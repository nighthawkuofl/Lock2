using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using Locks2.Core;
using RimWorld;
using Verse;
using Verse.AI;

namespace Locks2.Harmony
{
    [HarmonyPatch(typeof(Building_Door), nameof(Building_Door.PawnCanOpen))]
    static class Building_Door_PawnCanOpen_Patch
    {
        static bool Prefix(Building_Door __instance, Pawn p, ref bool __result)
        {
            if (__instance.Faction == null)
            {
                __result = true; return false;
            }
            if (!(__instance.Map?.IsPlayerHome ?? false) || p == null)
            {
                return true;
            }
            var config = Finder.currentConfig = __instance.GetConfig();
            if (config == null) return true;
            if (config.Allows(p))
            {
                __result = true;
            }
            else
            {
                __result = false;
            }
            return false;
        }
    }

    [HarmonyPatch]
    static class Building_Door_Expanded_PawnCanOpen_Patch
    {
        static bool Prepare()
        {
            return AccessTools.Method("Building_DoorExpanded:PawnCanOpen") != null;
        }

        static MethodBase TargetMethod()
        {
            return AccessTools.Method("Building_DoorExpanded:PawnCanOpen");
        }

        static bool Prefix(Building __instance, Pawn p, ref bool __result)
        {
            if (__instance.Faction == null)
            {
                __result = true; return false;
            }
            if (!(__instance.Map?.IsPlayerHome ?? false) || p == null)
            {
                return true;
            }
            var config = Finder.currentConfig = __instance.GetConfig();
            if (config == null) return true;
            if (config.Allows(p))
            {
                __result = true;
            }
            else
            {
                __result = false;
            }
            return false;
        }
    }
}
