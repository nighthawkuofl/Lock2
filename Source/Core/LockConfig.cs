using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Locks2.Core
{
    public partial class LockConfig : IExposable
    {
        public Thing door;
        public List<IConfigRule> rules;

        private Dictionary<int, Pair<bool, int>> cache = new Dictionary<int, Pair<bool, int>>(100);

        public bool Allows(Pawn pawn)
        {
            var key = pawn.GetKey();
            if (cache.TryGetValue(key, out var store) && GenTicks.TicksGame - store.second < Finder.settings.cacheTimeOut)
            {
                return store.first;
            }
            bool result = AllowsInternal(pawn);
            cache[key] = new Pair<bool, int>(result, GenTicks.TicksGame);
            return result;
        }

        public void Dirty()
        {
            cache.Clear();
        }

        public void Dirty(Pawn pawn)
        {
            cache.Remove(pawn.thingIDNumber);
        }

        private bool AllowsInternal(Pawn pawn)
        {
            foreach (var rule in rules)
            {
                if (rule.Allows(pawn)) return true;
            }
            return false;
        }

        public abstract class IConfigRule : IExposable
        {
            public abstract float Height { get; }
            public abstract bool Allows(Pawn pawn);
            public abstract void ExposeData();
            public abstract void DoContent(IEnumerable<Pawn> pawns, Rect rect);
            public abstract IConfigRule Duplicate();
        }

        public void Initailize()
        {
            rules = new List<IConfigRule>();
            if (Finder.settings.defaultRules.Count > 0)
            {
                foreach (var type in Finder.settings.defaultRules)
                {
                    rules.Add(Activator.CreateInstance(type) as IConfigRule);
                }
            }
            else
            {
                rules.Add(new ConfigRuleAnimals());
                rules.Add(new ConfigRuleColonists());
                rules.Add(new ConfigRuleGuests());
                rules.Add(new ConfigRuleIgnorDrafted());
            }
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref door, "door", saveDestroyedThings: false);
            Scribe_Collections.Look(ref rules, "rules", LookMode.Deep);
            if (Scribe.mode != LoadSaveMode.Saving && (rules == null || rules.Count == 0))
            {
                Initailize();
            }
        }
    }
}
