using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Locks2.Core
{
    public partial class LockConfig
    {
        public class ConfigRuleGameCondition : IConfigRule
        {
            public bool enabled = false;

            public override float Height => enabled ? 75 + conditionsSet.Count * 25 : 54;

            public List<GameConditionDef> removalSet = new List<GameConditionDef>();
            public HashSet<GameConditionDef> conditionsSet = new HashSet<GameConditionDef>();

            public override bool Allows(Pawn pawn)
            {
                if (enabled && !AnyConditionActive(pawn.Map) && !pawn.IsPrisoner && !(pawn.IsWildMan() && pawn.factionInt == null))
                {
                    return true;
                }
                return false;
            }

            public override IConfigRule Duplicate()
            {
                return new ConfigRuleGameCondition() { enabled = enabled, conditionsSet = new HashSet<GameConditionDef>(conditionsSet) };
            }

            private bool AnyConditionActive(Map map)
            {
                var conditionManager = map.gameConditionManager;
                foreach (var def in conditionsSet)
                    if (conditionManager.ConditionIsActive(def))
                    {
                        return true;
                    }
                return false;
            }

            public override void DoContent(IEnumerable<Pawn> pawns, Rect rect, Action notifySelectionBegan, Action notifySelectionEnded)
            {
                Text.Font = GameFont.Small;
                Widgets.CheckboxLabeled(rect.TopPartPixels(25), "Locks2GameConditionFilter".Translate(), ref enabled);
                Text.Font = GameFont.Tiny;
                if (enabled)
                {
                    Widgets.Label(rect.TopPartPixels(50).BottomPartPixels(25), "Locks2DenyIf".Translate());
                    var rowRect = rect.TopPartPixels(75).BottomPartPixels(25);
                    removalSet.Clear();
                    foreach (GameConditionDef def in conditionsSet)
                    {
                        if (Widgets.ButtonText(rowRect, def.label))
                        {
                            removalSet.Add(def);
                            Find.CurrentMap.reachability.ClearCache();
                        }
                        rowRect.y += 25;
                    }
                    foreach (GameConditionDef def in removalSet)
                    {
                        Find.CurrentMap.reachability.ClearCache();
                        conditionsSet.Remove(def);
                    }
                    if (Widgets.ButtonText(rowRect, "+"))
                    {
                        Find.CurrentMap.reachability.ClearCache();
                        notifySelectionBegan();
                        DoExtraContent((def) =>
                        {
                            conditionsSet.Add(def);
                            Find.CurrentMap.reachability.ClearCache();
                        }, DefDatabase<GameConditionDef>.AllDefs.Where(def => !conditionsSet.Contains(def)), notifySelectionEnded);
                    }
                }
            }

            public override void ExposeData()
            {
                Scribe_Values.Look(ref enabled, "enabled", true);
                Scribe_Collections.Look(ref conditionsSet, "conditionsSet", LookMode.Def);
            }

            private void DoExtraContent(Action<GameConditionDef> onSelection, IEnumerable<GameConditionDef> conditions, Action notifySelectionEnded)
            {
                ITab_Lock.currentSelector = new Selector_DefSelection(conditions, (def) => onSelection(def as GameConditionDef), true, notifySelectionEnded);
            }
        }
    }
}
