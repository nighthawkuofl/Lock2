using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using static Locks2.Core.LockConfig;

namespace Locks2.Core
{
    public static class Extensions
    {
        private static Dictionary<int, int> _pawnSignature = new Dictionary<int, int>();
        private static Dictionary<int, LockComp> _cache = new Dictionary<int, LockComp>();

        public static LockConfig GetConfig(this Building door)
        {
            if (_cache.TryGetValue(door.thingIDNumber, out var comp))
            {
                return comp.config;
            }
            comp = door.GetComp<LockComp>();
            if (comp == null)
            {
                if (Finder.debug)
                {
                    var message = string.Format("LOCKS2: Partially patched door {0}, {1}", door.def.defName, door);
                    Log.Warning(message);
                }
                return null;
            }
            if (comp.config == null)
            {
                comp.config = new LockConfig() { door = door };
                comp.config.Initailize();
            }
            return (_cache[door.thingIDNumber] = comp).config;
        }

        public static int GetKey(this Pawn pawn)
        {
            int hash;
            if (!_pawnSignature.TryGetValue(pawn.thingIDNumber, out hash))
            {
                hash = _pawnSignature[pawn.thingIDNumber] = Rand.Int;
            }
            hash = pawn.thingIDNumber.GetHashCode() ^ (hash << 1);
            return hash;
        }

        public static void Notify_Dirty(this Pawn pawn)
        {
            _pawnSignature[pawn.thingIDNumber] = Rand.Int;
        }
    }
}
