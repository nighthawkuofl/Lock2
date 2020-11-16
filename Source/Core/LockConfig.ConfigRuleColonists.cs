using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Locks2.Core
{
    public partial class LockConfig
    {
        public class ConfigRuleColonists : IConfigRule
        {
            public bool enabled = true;
            public HashSet<Pawn> blackSet = new HashSet<Pawn>();

            private List<Pawn> removalPawns = new List<Pawn>();

            public override float Height => enabled ? blackSet.Count * 25 + 75f : 54;

            public override bool Allows(Pawn pawn)
            {
                if (enabled && (pawn?.RaceProps?.Humanlike ?? false) && (pawn.factionInt?.IsPlayer ?? false) && !blackSet.Contains(pawn) && !pawn.IsPrisoner)
                {
                    return true;
                }
                return false;
            }

            public override IConfigRule Duplicate()
            {
                return new ConfigRuleColonists() { enabled = enabled, blackSet = new HashSet<Pawn>(blackSet) };
            }

            public override void DoContent(IEnumerable<Pawn> pawns, Rect rect)
            {
                Text.Font = GameFont.Small;
                Widgets.CheckboxLabeled(rect.TopPartPixels(25), "Locks2ColonistsFilter".Translate(), ref enabled);
                Text.Font = GameFont.Tiny;
                if (enabled)
                {
                    Widgets.Label(rect.TopPartPixels(50).BottomPartPixels(25), "Locks2ColonistsFilterBlacklist".Translate());
                    var rowRect = rect.TopPartPixels(75).BottomPartPixels(25);
                    removalPawns.Clear();
                    foreach (Pawn pawn in blackSet)
                    {
                        if (Widgets.ButtonText(rowRect, pawn.Name.ToString()))
                        {
                            Find.CurrentMap.reachability.ClearCache();
                            removalPawns.Add(pawn);
                        }
                        rowRect.y += 25;
                    }
                    foreach (Pawn pawn in removalPawns)
                    {
                        Find.CurrentMap.reachability.ClearCache();
                        blackSet.Remove(pawn);
                    }
                    if (Widgets.ButtonText(rowRect, "+"))
                    {
                        Find.CurrentMap.reachability.ClearCache();
                        DoExtraContent((p) =>
                        {
                            blackSet.Add(p);
                            Find.CurrentMap.reachability.ClearCache();
                        }, pawns.Where(p => !blackSet.Contains(p)));
                    }
                }
            }

            public override void ExposeData()
            {
                Scribe_Values.Look(ref enabled, "enabled", true);
                if (Scribe.mode == LoadSaveMode.Saving)
                {
                    blackSet.RemoveWhere(p => p == null || p.Destroyed || p.Dead);
                }
                Scribe_Collections.Look(ref blackSet, "blackset", LookMode.Reference);
                if (blackSet == null)
                {
                    blackSet = new HashSet<Pawn>();
                }
            }

            private void DoExtraContent(Action<Pawn> onSelection, IEnumerable<Pawn> pawns)
            {
                Find.WindowStack.Add(new PawnSelection_Window(pawns, onSelection));
            }
        }
    }
}
