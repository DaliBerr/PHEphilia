using Verse;
using HarmonyLib;
using RimWorld;
using System;


namespace Phephilia.Core{

	public static class Utils
	{
		internal static void CaptureIncestuousInFunc()
		{
			Utils.FuncIncestuous = AccessTools.MethodDelegate<Func<Pawn, Pawn, bool>>(AccessTools.Method(typeof(RelationsUtility), "Incestuous", new Type[]
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
        // public static float getRomanceAgeOverride(Pawn pawn){
        //     float expectancyLiftHuman = ThingDefOf.Human.race.lifeExpectancy;
        //     float expectancyLife = pawn.RaceProps.lifeExpectancy;
        //     // float expectancyLife2 = otherPawn.RaceProps.lifeExpectancy;
        //     float RomanceAge = pawn.ageTracker.AgeBiologicalYearsFloat;
        //     // float age2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;
        //     float minAge = GetMinRomanceAgeFromPreceptLabels(pawn);
        //     if(expectancyLife > expectancyLiftHuman && RomanceAge > expectancyLiftHuman){
        //         RomanceAge /= expectancyLife;
        //         if(RomanceAge < minAge){
        //             RomanceAge = minAge;
        //         }
        //     }         
        //     return RomanceAge;
        // }

        public static float getRomanceAgeOverride(Pawn pawn)
        {
            float RomanceAge = GetEquivalentHumanAge(pawn);
            float minAge = GetMinRomanceAgeFromPreceptLabels(pawn);
            if (RomanceAge < minAge) {
                RomanceAge = minAge;
            }

            return RomanceAge;
        }
        public static float GetEquivalentHumanAge(Pawn pawn)
        {
            if (pawn == null || pawn.RaceProps == null)
                return 0f;
            float humanLifeExpectancy = ThingDefOf.Human.race.lifeExpectancy;
            float pawnExpectancyLife = pawn.RaceProps.lifeExpectancy;
            float age = pawn.ageTracker.AgeBiologicalYearsFloat;
            float factor = pawn.GetStatValue(StatDef.Named("LifespanFactor"));
            if (factor <= 0f)
                factor = 1f;
            return age / (pawnExpectancyLife * factor) * humanLifeExpectancy ;
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
            }
        }
        return defaultMinAge;
    }
}
}