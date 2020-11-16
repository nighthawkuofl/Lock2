using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Locks2.Core
{
    public partial class LockConfig
    {
        public class ConfigRuleAnimals : IConfigRule
        {
            public bool enabled = true;

            public override float Height => 54;

            public override bool Allows(Pawn pawn)
            {
                if (enabled && (pawn?.RaceProps?.Animal ?? false) && (pawn.factionInt?.IsPlayer ?? false))
                {
                    return true;
                }
                return false;
            }

            public override void DoContent(IEnumerable<Pawn> pawns, Rect rect, Action notifySelectionBegan, Action notifySelectionEnded)
            {
                var before = enabled;
                Widgets.CheckboxLabeled(rect, "Locks2Animals".Translate(), ref enabled);
                if (before != enabled)
                {
                    Find.CurrentMap.reachability.ClearCache();
                }
            }

            public override IConfigRule Duplicate()
            {
                return new ConfigRuleAnimals() { enabled = enabled };
            }

            public override void ExposeData()
            {
                Scribe_Values.Look(ref enabled, "enabled", true);
            }
        }
    }
}
