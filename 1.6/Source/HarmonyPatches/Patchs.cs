using Verse;
using HarmonyLib;
using RimWorld;
// using RimFood.Core;
using System.Collections.Generic;
using System;
// using System.Linq;
using UnityEngine;
// using Phephilia.HarmonyPatches;
// using System.Drawing.Text;
// using Verse.AI;
using Phephilia.Core;

namespace Phephilia.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), "SecondaryRomanceChanceFactor")]
    public static class Postfix_RomanceAgeFix
    {
        [HarmonyPostfix]
        public static void SecondaryRomanceChanceFactor_Postfix(Pawn ___pawn, Pawn otherPawn, ref float __result)
        {
            Pawn pawn = ___pawn;
            int minAgeA = Core.RomanceAgeOverride.GetMinRomanceAgeFromPreceptLabels(pawn);
            int minAgeB = Core.RomanceAgeOverride.GetMinRomanceAgeFromPreceptLabels(otherPawn);
            // Log.Warning("minAgeA:  " + pawn.Name + " with " + otherPawn.Name + " has "+ minAgeA + " and minAgeB: " + minAgeB);
            int requiredMinAge = Math.Max(minAgeA, minAgeB);

            // Log.Warning("requiredMinAge:  " + pawn.Name + " with " + otherPawn.Name + " has "+ requiredMinAge);  
            
            if (pawn.ageTracker.AgeBiologicalYears < requiredMinAge || otherPawn.ageTracker.AgeBiologicalYears < requiredMinAge)
            {
                __result = 0f;
                // Log.Warning("SecondaryRomanceChanceFactor: " + __result);
                // return false;
                return; 
            }
            float num = 1f;
            foreach (PawnRelationDef relation in pawn.GetRelations(otherPawn))
            {
                num *= relation.romanceChanceFactor;
            }
            float num2 = 1f;
            HediffWithTarget hediffWithTarget = (HediffWithTarget)pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicLove);
            if (hediffWithTarget != null && hediffWithTarget.target == otherPawn)
            {
                num2 = 10f;
            }
            float num3 = 1f;
            if (ModsConfig.BiotechActive && pawn.genes != null && (otherPawn.story?.traits == null || !otherPawn.story.traits.HasTrait(TraitDefOf.Kind)))
            {
                List<Gene> genesListForReading = pawn.genes.GenesListForReading;
                for (int i = 0; i < genesListForReading.Count; i++)
                {
                    if (genesListForReading[i].def.missingGeneRomanceChanceFactor != 1f &&
                        (otherPawn.genes == null || !otherPawn.genes.HasActiveGene(genesListForReading[i].def)))
                    {
                        num3 *= genesListForReading[i].def.missingGeneRomanceChanceFactor;
                    }
                }
            }
            float baseChance = SecondaryLovinChanceFactor(pawn, otherPawn, requiredMinAge);
            __result = baseChance * num * num2 * num3;
            // Log.Warning("SecondaryRomanceChanceFactor: A" + pawn.Name + "with" + otherPawn.Name + "has "+ __result);  
            return;
        }
        private static float SecondaryLovinChanceFactor(Pawn pawn, Pawn otherPawn, float minRequiredAge)
        {
            if (pawn == otherPawn)
                return 0f;

            if (pawn.story != null && pawn.story.traits != null)
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.Asexual))
                    return 0f;

                if (!pawn.story.traits.HasTrait(TraitDefOf.Bisexual))
                {
                    if (pawn.story.traits.HasTrait(TraitDefOf.Gay))
                    {
                        if (otherPawn.gender != pawn.gender)
                            return 0f;
                    }
                    else if (otherPawn.gender == pawn.gender)
                    {
                        return 0f;
                    }
                }
            }
            // 替换原来固定的 <16f 判断
            if (pawn.ageTracker.AgeBiologicalYearsFloat < minRequiredAge || otherPawn.ageTracker.AgeBiologicalYearsFloat < minRequiredAge)
                return 0f;
            return LovinAgeFactor(pawn, otherPawn) * PrettinessFactor(otherPawn);
        }
        private static float PrettinessFactor(Pawn otherPawn)
        {
            float beauty = 0f;
            if (otherPawn.RaceProps.Humanlike)
                beauty = otherPawn.GetStatValue(StatDefOf.PawnBeauty);

            if (beauty < 0f) return 0.3f;
            if (beauty > 0f) return 2.3f;
            return 1f;
        }
        private static float LovinAgeFactor(Pawn pawn, Pawn otherPawn)
        {
                float num = 1f;
                
                // float expectancyLiftHuman = ThingDefOf.Human.race.lifeExpectancy;
                float expectancyLife1 = pawn.RaceProps.lifeExpectancy;
                float expectancyLife2 = otherPawn.RaceProps.lifeExpectancy;
                float age1 = Core.RomanceAgeOverride.getRomanceAgeOverride(pawn);
                float age2 = Core.RomanceAgeOverride.getRomanceAgeOverride(otherPawn);

                float malemin = expectancyLife1 * .375f;
                float malelower = expectancyLife1 * .25f;
                float maleupper = expectancyLife1 * .075f;
                float malemax = expectancyLife1 * .25f;

                float femalemin = expectancyLife2 * .1875f;
                float femalelower = expectancyLife2 * .1f;
                float femaleupper = expectancyLife2 * .1875f;
                float femalemax = expectancyLife2 * .5f;

                if (pawn.gender == Gender.Male)
                {
                    float min = age1 - malemin;
                    float lower = age1 - malelower;
                    float upper = age1 + maleupper;
                    float max = age1 + malemax;
                    num = GenMath.FlatHill(0.2f, min, lower, upper, max, 0.2f, age2);
                }
                else if (pawn.gender == Gender.Female)
                {
                    float min2 = age1 - femalemin;
                    float lower2 = age1 - femalelower;
                    float upper2 = age1 + femaleupper;
                    float max2 = age1 + femalemax;
                    num = GenMath.FlatHill(0.2f, min2, lower2, upper2, max2, 0.2f, age2);
                }
                return num;
        }
    }

    [HarmonyPatch(typeof(RelationsUtility), "RomanceEligible")]
    public static class Postfix_RomanceEligible
    {
        [HarmonyPostfix]
        public static void Patch_RomanceEligible(ref AcceptanceReport __result, Pawn pawn, bool initiator, bool forOpinionExplanation)
        {

            if (pawn.ageTracker.AgeBiologicalYearsFloat < Core.RomanceAgeOverride.GetMinRomanceAgeFromPreceptLabels(pawn))
            {
                __result = false; // 直接拒绝
                return;
            }
            if (pawn.IsPrisoner)
            {
                if (!initiator || forOpinionExplanation)
                {
                    __result = AcceptanceReport.WasRejected;
                    return;
                }
                __result = TranslatorFormattedStringExtensions.Translate("CantRomanceInitiateMessagePrisoner", pawn).CapitalizeFirst();
                return;
            }
            else
            {
                if (pawn.Downed && !forOpinionExplanation)
                {
                    __result = (initiator ? TranslatorFormattedStringExtensions.Translate("CantRomanceInitiateMessageDowned", pawn).CapitalizeFirst() : Translator.Translate("CantRomanceTargetDowned"));
                    return;
                }
                Pawn_StoryTracker story = pawn.story;
                bool flag2;
                if (story == null)
                {
                    flag2 = false;
                }
                else
                {
                    TraitSet traits = story.traits;
                    bool? flag3 = ((traits != null) ? new bool?(traits.HasTrait(TraitDefOf.Asexual)) : null);
                    bool flag4 = true;
                    flag2 = (flag3.GetValueOrDefault() == flag4) & (flag3 != null);
                }
                if (flag2)
                {
                    if (!initiator || forOpinionExplanation)
                    {
                        __result = AcceptanceReport.WasRejected;
                        return;
                    }
                    __result = TranslatorFormattedStringExtensions.Translate("CantRomanceInitiateMessageAsexual", pawn).CapitalizeFirst();
                    return;
                }
                else if (initiator && !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
                {
                    if (!forOpinionExplanation)
                    {
                        __result = TranslatorFormattedStringExtensions.Translate("CantRomanceInitiateMessageTalk", pawn).CapitalizeFirst();
                        return;
                    }
                    __result = AcceptanceReport.WasRejected;
                    return;
                }
                else
                {
                    if (pawn.Drafted && !forOpinionExplanation)
                    {
                        __result = (initiator ? TranslatorFormattedStringExtensions.Translate("CantRomanceInitiateMessageDrafted", pawn).CapitalizeFirst() : Translator.Translate("CantRomanceTargetDrafted"));
                        return;
                    }
                    if (initiator && pawn.IsSlave)
                    {
                        if (!forOpinionExplanation)
                        {
                            __result = TranslatorFormattedStringExtensions.Translate("CantRomanceInitiateMessageSlave", pawn).CapitalizeFirst();
                            return;
                        }
                        __result = AcceptanceReport.WasRejected;
                        return;
                    }
                    else
                    {
                        if (pawn.MentalState != null)
                        {
                            __result = ((initiator && !forOpinionExplanation) ? TranslatorFormattedStringExtensions.Translate("CantRomanceInitiateMessageMentalState", pawn).CapitalizeFirst() : Translator.Translate("CantRomanceTargetMentalState"));
                            return;
                        }
                        __result = true;
                        return;
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(RelationsUtility), "RomanceEligiblePair")]
    public static class Postfix_RomanceEligiblePair
    {
        [HarmonyPostfix]
        public static void Patch_RomanceEligiblePair(ref AcceptanceReport __result, Pawn initiator, Pawn target, bool forOpinionExplanation)
        {
            if (initiator == target)
            {
                __result= false;

                return;
            }
        
            int minAgeTarget = Core.RomanceAgeOverride.GetMinRomanceAgeFromPreceptLabels(target);
            if (forOpinionExplanation && target.ageTracker.AgeBiologicalYearsFloat < minAgeTarget)
            {
                // Log.Message("CantRomanceTargetYoung");
                // __result = "CantRomanceTargetYoung".Translate();
                __result = "This is minAgeTarget";

                return;
            }
            return;
        }
    } 

    [HarmonyPatch(typeof(CompAbilityEffect_WordOfLove),"ValidateTarget")]
    public static class Postfix_ValidateTarget{
        [HarmonyPostfix]  
        private static void Patch_ValidateTarget(ref bool __result, CompAbilityEffect_WordOfLove __instance, LocalTargetInfo target, bool showMessages = true)
        {
            if (!(target != null))
            {
                return;
            }
            Pawn casterPawn = __instance.CasterPawn;
            Pawn pawn = target.Pawn;
            if (casterPawn == pawn || casterPawn == null || pawn == null)
            {
                __result = false;
                return;
            }
            bool flag = target.IsValid && (__instance.Props.range <= 0f || IntVec3Utility.DistanceTo(target.Cell, LocalTargetInfo.Invalid.Cell) <= __instance.Props.range) && (!__instance.Props.requiresLineOfSight || GenSight.LineOfSight(LocalTargetInfo.Invalid.Cell, target.Cell, __instance.parent.pawn.Map, false, null, 0, 0));
            __result = flag;
        }
    }
}


