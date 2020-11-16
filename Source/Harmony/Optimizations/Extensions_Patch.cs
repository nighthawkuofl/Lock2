using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using Locks2.Core;
using RimWorld;
using Verse;

namespace Locks2.Harmony
{
    [HarmonyPatch(typeof(Extensions), nameof(Extensions.GetConfig))]
    public static class Extensions_Patch
    {
        static FieldInfo fLockComp;

        /*
         * 
         * 
         * ----------------------------------------------------------------------------------
        [UsedImplicitly]
        static LockConfig GetConfig(Building_Door door)
        {
            LockComp comp = door.lockComp;
            if (comp == null)
            {
                comp = door.lockComp = door.GetComp<LockComp>();
            }
            if (comp.config == null)
            {
                comp.config = new LockConfig();
                comp.config.Initailize();
            }
            return comp.config;
        }
        */

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var op1 = generator.DeclareLocal(typeof(LockComp));
            var op2 = generator.DeclareLocal(typeof(LockComp));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, fLockComp);
            yield return new CodeInstruction(OpCodes.Stloc, op1.LocalIndex);

            yield return new CodeInstruction(OpCodes.Ldloc_S, op1.LocalIndex);
            var l1 = generator.DefineLabel();
            yield return new CodeInstruction(OpCodes.Brtrue_S, l1);

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ThingWithComps), nameof(ThingWithComps.GetComp), generics: new[] { typeof(LockComp) }));

            yield return new CodeInstruction(OpCodes.Dup);
            yield return new CodeInstruction(OpCodes.Stloc_S, op2.LocalIndex);
            yield return new CodeInstruction(OpCodes.Stfld, fLockComp);

            yield return new CodeInstruction(OpCodes.Ldloc_S, op2.LocalIndex);
            yield return new CodeInstruction(OpCodes.Stloc_S, op1.LocalIndex);

            yield return new CodeInstruction(OpCodes.Ldloc_S, op1.LocalIndex) { labels = new List<Label>() { l1 } };
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(LockComp), nameof(LockComp.config)));
            var l2 = generator.DefineLabel();

            yield return new CodeInstruction(OpCodes.Brtrue_S, l2);

            yield return new CodeInstruction(OpCodes.Ldloc_S, op1.LocalIndex);
            yield return new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(LockConfig)));
            yield return new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(LockComp), nameof(LockComp.config)));

            yield return new CodeInstruction(OpCodes.Ldloc_S, op1.LocalIndex);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(LockComp), nameof(LockComp.config)));
            yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(LockConfig), nameof(LockConfig.Initailize)));

            yield return new CodeInstruction(OpCodes.Ldloc_S, op1.LocalIndex) { labels = new List<Label>() { l2 } };
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(LockComp), nameof(LockComp.config)));
            yield return new CodeInstruction(OpCodes.Ret);

            Log.Message("LOCKS2: switched to native fields");
            yield break;
        }

        static void PrepareForNativeFields()
        {
            fLockComp = AccessTools.Field(typeof(Building_Door), "lockComp");
        }

        static bool Prepare()
        {
            var type = typeof(Building_Door);
            if (type.GetFields().Any((f) => f.Name == "lockComp"))
            {
                Log.Message("LOCKS2: Prepatcher active");
                PrepareForNativeFields();
                return true;
            }
            else
            {
                Log.Message("LOCKS2: Prepatcher is not active");
                return false;
            }
        }
    }
}
