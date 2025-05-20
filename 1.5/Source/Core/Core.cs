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
    public static class RomanceAgeOverride{
        public static float getRomanceAgeOverride(Pawn pawn){
            float expectancyLiftHuman = ThingDefOf.Human.race.lifeExpectancy;
            float expectancyLife = pawn.RaceProps.lifeExpectancy;
            // float expectancyLife2 = otherPawn.RaceProps.lifeExpectancy;
            float RomanceAge = pawn.ageTracker.AgeBiologicalYearsFloat;
            // float age2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;
            float minAge = GetMinRomanceAgeFromPreceptLabels(pawn);
            if(expectancyLife > expectancyLiftHuman && RomanceAge > expectancyLiftHuman){
                RomanceAge /= expectancyLife;
                if(RomanceAge < minAge){
                    RomanceAge = minAge;
                }
            }         
            return RomanceAge;
        }
    
    public static int defaultMinAge = 14;
    public static int GetMinRomanceAgeFromPreceptLabels(Pawn pawn)
    {
        
        int AgeFromTrait =  Phephilia.Core.TraitOverride.getTraitRomanceAge(pawn);  

        if(AgeFromTrait != -1){
            return AgeFromTrait;
        }
        if (pawn?.Ideo?.PreceptsListForReading == null){
            Log.Warning("Cant get PreceptLabels of "+ pawn.Name);
            return defaultMinAge;
        }

        foreach (var precept in pawn.Ideo.PreceptsListForReading)
        {
            // Log.Warning("pawn " + pawn.Name + " has precept " + precept.def.defName);
            switch (precept.def.defName)
            {
                case "MinAgeforRomance_7":
                    return 7;
                case "MinAgeforRomance_10":
                    return 10;
                case "MinAgeforRomance_14":
                    return 14;
                case "MinAgeforRomance_18":
                    return 18;
                default:
                    continue;
                // 可扩展更多
            }
        }
        return defaultMinAge; // 没有找到任何指定的戒律时默认不限制
    }
}
}