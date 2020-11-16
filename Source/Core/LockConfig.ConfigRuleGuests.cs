using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Locks2.Core
{
    public partial class LockConfig
    {
        public class ConfigRuleGuests : IConfigRule
        {
            public bool enabled = true;

            public override float Height => 54;

            public override bool Allows(Pawn pawn)
            {
                if (!enabled)
                {
                    return false;
                }
                if (pawn.IsColonist)
                {
                    return false;
                }
                Lord lord = LordUtility.GetLord(pawn);
                if (lord != null && lord.LordJob != null && lord.LordJob.CanOpenAnyDoor(pawn))
                {
                    return true;
                }
                if (pawn.Faction != null && FactionUtility.HostileTo(pawn.Faction, Faction.OfPlayer))
                {
                    return false;
                }
                if (pawn.Faction == null && WildManUtility.NonHumanlikeOrWildMan(pawn) && (pawn.HostFaction != Faction.OfPlayer || pawn.IsPrisoner))
                {
                    return false;
                }
                if (pawn.IsPrisoner && pawn.HostFaction == Faction.OfPlayer)
                {
                    return false;
                }
                return true;
            }

            public override IConfigRule Duplicate()
            {
                return new ConfigRuleGuests() { enabled = enabled };
            }

            public override void DoContent(IEnumerable<Pawn> pawns, Rect rect, Action notifySelectionBegan, Action notifySelectionEnded)
            {
                var before = enabled;
                Widgets.CheckboxLabeled(rect, "Locks2GuestsFilter".Translate(), ref enabled);
                if (before != enabled)
                {
                    Find.CurrentMap.reachability.ClearCache();
                }
            }

            public override void ExposeData()
            {
                Scribe_Values.Look(ref enabled, "enabled", true);
            }
        }
    }
}
