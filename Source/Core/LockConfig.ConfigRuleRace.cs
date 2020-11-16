using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Locks2.Core
{
    public partial class LockConfig
    {
        public class ConfigRuleRace : ConfigRuleGuests
        {
            public HashSet<ThingDef> whiteSet = new HashSet<ThingDef>();

            private List<ThingDef> removalKinds = new List<ThingDef>();
            private static IEnumerable<ThingDef> racesDefs;

            public override float Height => enabled ? whiteSet.Count * 25 + 75f : 54;

            public override bool Allows(Pawn pawn)
            {
                if (enabled && whiteSet.Contains(pawn.def) && (pawn.IsColonist || base.Allows(pawn)))
                {
                    return true;
                }
                return false;
            }

            public override IConfigRule Duplicate()
            {
                return new ConfigRuleRace() { enabled = enabled, whiteSet = new HashSet<ThingDef>(whiteSet) };
            }

            public override void DoContent(IEnumerable<Pawn> pawns, Rect rect)
            {
                if (racesDefs == null)
                {
                    racesDefs = DefDatabase<ThingDef>.AllDefs.Where(def => def.race != null);
                }
                Text.Font = GameFont.Small;
                Widgets.CheckboxLabeled(rect.TopPartPixels(25), "Locks2RaceFilter".Translate(), ref enabled);
                Text.Font = GameFont.Tiny;
                if (enabled)
                {
                    Widgets.Label(rect.TopPartPixels(50).BottomPartPixels(25), "Locks2RaceFilterWhitelist".Translate());
                    var rowRect = rect.TopPartPixels(75).BottomPartPixels(25);
                    removalKinds.Clear();
                    foreach (ThingDef def in whiteSet)
                    {
                        if (Widgets.ButtonText(rowRect, def.label))
                        {
                            Find.CurrentMap.reachability.ClearCache();
                            removalKinds.Add(def);
                        }
                        rowRect.y += 25;
                    }
                    foreach (ThingDef def in removalKinds)
                    {
                        Find.CurrentMap.reachability.ClearCache();
                        whiteSet.Remove(def);
                    }
                    if (Widgets.ButtonText(rowRect, "+"))
                    {
                        Find.CurrentMap.reachability.ClearCache();
                        DoExtraContent((def) =>
                        {
                            whiteSet.Add(def as ThingDef);
                            Find.CurrentMap.reachability.ClearCache();
                        }, racesDefs.Where(def => !whiteSet.Contains(def)));
                    }
                }
            }

            public override void ExposeData()
            {
                base.ExposeData();
                Scribe_Collections.Look(ref whiteSet, "whiteset", LookMode.Def);
                if (whiteSet == null)
                {
                    whiteSet = new HashSet<ThingDef>();
                }
            }

            private void DoExtraContent(Action<Def> onSelection, IEnumerable<ThingDef> defs)
            {
                Find.WindowStack.Add(new DefSelection_Window(defs, onSelection));
            }
        }
    }
}
