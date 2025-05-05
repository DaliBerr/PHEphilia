using Verse;
using HarmonyLib;
using RimWorld;
using System;


namespace Phephilia.Core{

	public static class PrivateStaticMethodCapture
	{
		internal static void CaptureIncestuousInFunc()
		{
			PrivateStaticMethodCapture.FuncIncestuous = AccessTools.MethodDelegate<Func<Pawn, Pawn, bool>>(AccessTools.Method(typeof(RelationsUtility), "Incestuous", new Type[]
			{
				typeof(Pawn),
				typeof(Pawn)
			}, null), null, true);
		}
		internal static Func<Pawn, Pawn, bool> FuncIncestuous;
	}

    public static class TraitOverride{
        public static int getTraitRomanceAge(Pawn pawn){
            foreach (Trait trait in pawn.story.traits.allTraits){
                switch (trait.def.defName){
                    case "RomanceFetish_IgnoreAll":
                        return 0;
                    case "RomanceFetish_7":
                        return 7;
                    case "RomanceFetish_10":
                        return 10;
                    default:
                        continue;
                }
            }
            return -1;
        }
    }
}