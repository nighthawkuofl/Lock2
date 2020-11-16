using System;
using System.Collections.Generic;
using System.Linq;
using Ionic.Zlib;
using RimWorld;
using UnityEngine;
using Verse;
using static Locks2.Core.LockConfig;

namespace Locks2.Core
{
    public class LockComp : ThingComp
    {
        public LockConfig config;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref config, "conifg");
            if (config != null && config.door == null)
            {
                config.door = parent;
            }
            if (config?.rules?.Count == 0)
            {
                config.Initailize();
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Finder.clip != null && Finder.clip != config)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "Paste",
                    defaultDesc = "Paste lock configuration",
                    activateSound = SoundDefOf.Designate_Claim,
                    icon = TexButton.Paste,
                    action = () =>
                    {
                        foreach (Thing thing in Find.Selector.selected)
                        {
                            if (!(thing is Building door))
                            {
                                continue;
                            }
                            var config = door.GetConfig();
                            if (config == null || config == Finder.clip)
                            {
                                continue;
                            }
                            config.rules.Clear();
                            foreach (var rule in Finder.clip.rules)
                            {
                                config.rules.Add(rule.Duplicate());
                            }
                        }
                    }
                };
            }
            if (Find.Selector.selected.Count == 1 && Find.Selector.selected.First() is Building)
            {
                yield return new Command_Action()
                {
                    defaultLabel = "copy",
                    defaultDesc = "Copy current lock configuration",
                    activateSound = SoundDefOf.Designate_Claim,
                    icon = TexButton.Copy,
                    action = () =>
                    {
                        var building = Find.Selector.selected.First() as Building;
                        var config = building?.GetConfig();
                        Finder.clip = config;
                    }
                };
            }
        }
    }
}
