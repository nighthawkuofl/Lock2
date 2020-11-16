using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Locks2.Core
{
    public partial class LockConfig
    {
        public class ConfigRuleApparel : IConfigRule
        {
            public bool enabled = false;
            public override float Height => enabled ? 75 + apparelSet.Count * 25 : 54;
            public HashSet<ThingDef> apparelSet = new HashSet<ThingDef>();

            private List<ThingDef> removalSet = new List<ThingDef>();
            private static IEnumerable<ThingDef> allApparel;

            private Dictionary<int, Pair<bool, int>> _cache = new Dictionary<int, Pair<bool, int>>();

            public override bool Allows(Pawn pawn)
            {
                if (enabled && IsPawnWearingApparelFast(pawn))
                {
                    return true;
                }
                return false;
            }

            private bool IsPawnWearingApparelFast(Pawn pawn)
            {
                var key = pawn.GetKey();
                if (_cache.TryGetValue(key, out var store) && GenTicks.TicksGame - store.second < 60000)
                {
                    return store.first;
                }
                var result = IsPawnWearingApparelIntenal(pawn);
                _cache[key] = new Pair<bool, int>(result, GenTicks.TicksGame);
                return result;
            }

            private bool IsPawnWearingApparelIntenal(Pawn pawn)
            {
                if (pawn.apparel == null && apparelSet.Count == 0) return false;
                if (pawn.apparel == null) return false;
                foreach (var def in apparelSet)
                {
                    bool found = false;
                    foreach (var apparel in pawn.apparel.WornApparel)
                        if (def == apparel.def)
                        {
                            found = true;
                            break;
                        }
                    if (!found)
                    {
                        return false;
                    }
                }
                return true;
            }

            public override IConfigRule Duplicate()
            {
                return new ConfigRuleApparel() { enabled = enabled, apparelSet = new HashSet<ThingDef>(apparelSet) };
            }

            public override void DoContent(IEnumerable<Pawn> pawns, Rect rect, Action notifySelectionBegan, Action notifySelectionEnded)
            {
                if (allApparel == null)
                {
                    allApparel = DefDatabase<ThingDef>.AllDefs.Where(def => def.IsApparel);
                }
                Text.Font = GameFont.Small;
                Widgets.CheckboxLabeled(rect.TopPartPixels(25), "Locks2ApparelFilter".Translate(), ref enabled);
                Text.Font = GameFont.Tiny;
                if (enabled)
                {
                    Widgets.Label(rect.TopPartPixels(50).BottomPartPixels(25), "Locks2ApparelFilterBody".Translate());
                    var rowRect = rect.TopPartPixels(75).BottomPartPixels(25);
                    removalSet.Clear();
                    foreach (ThingDef def in apparelSet)
                    {
                        if (Widgets.ButtonText(rowRect, def.label))
                        {
                            removalSet.Add(def);
                            Find.CurrentMap.reachability.ClearCache();
                        }
                        rowRect.y += 25;
                    }
                    foreach (ThingDef def in removalSet)
                    {
                        apparelSet.Remove(def);
                    }
                    if (Widgets.ButtonText(rowRect, "+"))
                    {
                        Find.CurrentMap.reachability.ClearCache();
                        notifySelectionBegan.Invoke();
                        DoExtraContent((def) => apparelSet.Add(def), allApparel.Where(def => !apparelSet.Contains(def)), notifySelectionEnded);
                    }
                }
            }

            public override void ExposeData()
            {
                Scribe_Values.Look(ref enabled, "enabled", true);
                Scribe_Collections.Look(ref apparelSet, "apparelSet", LookMode.Def);
            }

            private void DoExtraContent(Action<ThingDef> onSelection, IEnumerable<ThingDef> defs, Action notifySelectionEnded)
            {
                ITab_Lock.currentSelector = new Selector_DefSelection(defs, (def) =>
                 {
                     Find.CurrentMap.reachability.ClearCache();
                     onSelection(def as ThingDef);
                 }, true, notifySelectionEnded);
            }
        }
    }
}
