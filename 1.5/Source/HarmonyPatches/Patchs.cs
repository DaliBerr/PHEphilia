using Verse;
using HarmonyLib;
using RimWorld;
// using RimFood.Core;
using System.Collections.Generic;
using System;
// using System.Linq;
using UnityEngine;
using Phephilia.HarmonyPatches;
using System.Drawing.Text;
using Verse.AI;
// using phepholia.Core;

namespace Phephilia.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), "SecondaryRomanceChanceFactor")]
    public static class Prefix_MinAgeForRomance
    {
        [HarmonyPostfix]
        public static void SecondaryRomanceChanceFactor_Prefix(Pawn ___pawn, Pawn otherPawn, ref float __result)
        {

            Pawn pawn = ___pawn;

            int minAgeA = GetMinRomanceAgeFromPreceptLabels(pawn);
            int minAgeB = GetMinRomanceAgeFromPreceptLabels(otherPawn);
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
            float age1 = pawn.ageTracker.AgeBiologicalYearsFloat;
            float age2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;

            if (pawn.gender == Gender.Male)
            {
                float min = age1 - 30f;
                float lower = age1 - 10f;
                float upper = age1 + 3f;
                float max = age1 + 10f;
                num = GenMath.FlatHill(0.2f, min, lower, upper, max, 0.2f, age2);
            }
            else if (pawn.gender == Gender.Female)
            {
                float min2 = age1 - 10f;
                float lower2 = age1 - 3f;
                float upper2 = age1 + 10f;
                float max2 = age1 + 30f;
                num = GenMath.FlatHill(0.2f, min2, lower2, upper2, max2, 0.2f, age2);
            }
            // float minAgeA = GetMinRomanceAgeFromPreceptLabels(pawn);
            // float minAgeB = GetMinRomanceAgeFromPreceptLabels(otherPawn);
            // float lerp1 = Mathf.InverseLerp(minAgeA, minAgeA+4f, age1);
            // float lerp2 = Mathf.InverseLerp(minAgeB,minAgeB+4f, age2);
            // return num * lerp1 * lerp2;
            return num;
        }
        // private static float LovinAgeFactor(Pawn pawn, Pawn otherPawn)
        // {
        //     float age1 = pawn.ageTracker.AgeBiologicalYearsFloat;
        //     float age2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;

        //     // 获取各自的最小恋爱年龄（根据 Precept）
        //     float minAge1 = Prefix_MinAgeForRomance.GetMinRomanceAgeFromPreceptLabels(pawn);
        //     float minAge2 = Prefix_MinAgeForRomance.GetMinRomanceAgeFromPreceptLabels(otherPawn);

        //     // 以自身年龄为中心，允许一定范围的年龄差（如 ±10 年）
        //     float center = age1;
        //     float toleranceLower = center - 10f;
        //     float toleranceUpper = center + 10f;

        //     // 但年龄必须大于对方的最小合法年龄
        //     toleranceLower = Math.Max(toleranceLower, minAge2);

        //     // FlatHill: 顶峰是年龄相近时，偏大或偏小都衰减
        //     float baseFactor = GenMath.FlatHill(
        //         min: toleranceLower - 10f,
        //         lower: toleranceLower,
        //         upper: toleranceUpper,
        //         max: toleranceUpper + 10f,
        //         // zeroOutside: 0.2f,
        //         x: age2
        //     );

        //     // 额外拉升：年龄越接近最小合法年龄越弱，越成熟越强
        //     float maturity1 = Mathf.InverseLerp(minAge1, minAge1 + 4f, age1);
        //     float maturity2 = Mathf.InverseLerp(minAge2, minAge2 + 4f, age2);

        //     return baseFactor * maturity1 * maturity2;
        // }

    }

    [HarmonyPatch(typeof(RelationsUtility), "RomanceEligible")]
    public static class Postfix_RomanceEligible
    {
        [HarmonyPostfix]
        public static void Patch_RomanceEligible(ref AcceptanceReport __result, Pawn pawn, bool initiator, bool forOpinionExplanation)
        {
            // if (!__result.Accepted) return; // 若原逻辑已拒绝则跳过
            if (pawn.ageTracker.AgeBiologicalYearsFloat < Prefix_MinAgeForRomance.GetMinRomanceAgeFromPreceptLabels(pawn))
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
        
            int minAgeTarget = Prefix_MinAgeForRomance.GetMinRomanceAgeFromPreceptLabels(target);
            if (forOpinionExplanation && target.ageTracker.AgeBiologicalYearsFloat < minAgeTarget)
            {
                Log.Message("CantRomanceTargetYoung");
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
    // [HarmonyPatch(typeof(RelationsUtility), "RomanceEligiblePair")]
    // public static class Postfix_RomanceEligiblePair
    // {
    // [HarmonyPostfix]
    // public static void Patch_RomanceEligiblePair(ref AcceptanceReport __result, Pawn initiator, Pawn target, bool forOpinionExplanation)
    // {
    //     if (initiator == target)
    //     {
    //         Log.Message("RomanceEligiblePair");
    //         __result= false;

    //         return;
    //     }

    //     DirectPawnRelation directPawnRelation = LovePartnerRelationUtility.ExistingLoveRealtionshipBetween(initiator, target, allowDead: false);
    //     if (directPawnRelation != null)
    //     {
    //         string genderSpecificLabel = directPawnRelation.def.GetGenderSpecificLabel(target);
    //         // __result= "RomanceChanceExistingRelation".Translate(initiator.Named("PAWN"), genderSpecificLabel.Named("RELATION"));
    //         Log.Message("RomanceChanceExistingRelation");
    //         __result = "This is directPawnRelation";

    //         return;
    //     }

    //     if (!RelationsUtility.RomanceEligible(initiator, initiator: true, forOpinionExplanation))
    //     {
    //         Log.Message("RomanceEligibleInitiator");
    //         __result= false;

    //         return;
    //     }

    //     int minAgeTarget = Prefix_MinAgeForRomance.GetMinRomanceAgeFromPreceptLabels(target);
    //     if (forOpinionExplanation && target.ageTracker.AgeBiologicalYearsFloat < minAgeTarget)
    //     {
    //                     Log.Message("CantRomanceTargetYoung");
    //         // __result = "CantRomanceTargetYoung".Translate();
    //         __result = "This is minAgeTarget";

    //         return;
    //     }
    //     if (Phephilia.Core.PrivateStaticMethodCapture.FuncIncestuous(initiator, target))
    //     {
    //                     Log.Message("CantRomanceTargetIncest");
    //         // __result = "CantRomanceTargetIncest".Translate();
    //         __result = "This is FuncIncestuous";

    //         return;
    //     }

    //     if (forOpinionExplanation && target.IsPrisoner)
    //     {
    //                     Log.Message("CantRomanceTargetPrisoner");
    //         // __result = "CantRomanceTargetPrisoner".Translate();
    //         __result = "This is target.IsPrisoner";

    //         return;
    //     }

    //     if (!RelationsUtility.AttractedToGender(initiator, target.gender) || !RelationsUtility.AttractedToGender(target, initiator.gender))
    //     {
    //         if (!forOpinionExplanation)
    //         {
    //                             Log.Message("AttractedToGender");
    //             // __result = AcceptanceReport.WasRejected;
    //             __result = "this is AttractedToGender";

    //             return;
    //         }
    //         Log.Message("CantRomanceTargetSexuality");
    //         // __result = "CantRomanceTargetSexuality".Translate();
    //         __result = "This is AttractedToGender";

    //         return;
    //     }

    //     AcceptanceReport acceptanceReport = RelationsUtility.RomanceEligible(target, initiator: false, forOpinionExplanation);
    //     if (!acceptanceReport)
    //     {
    //                     Log.Message("RomanceEligibleTarget");
    //         __result = acceptanceReport;

    //         return;
    //     }

    //     if (target.relations.OpinionOf(initiator) <= 5)
    //     {
    //         // __result = "CantRomanceTargetOpinion".Translate();
    //         Log.Message("CantRomanceTargetOpinion");
    //         __result = "This is target.relations.OpinionOf";
    //         return;
    //     }

    //     if (!forOpinionExplanation && InteractionWorker_RomanceAttempt.SuccessChance(initiator, target, 1f) <= 0f)
    //     {
    //         // __result = "CantRomanceTargetZeroChance".Translate();
    //         Log.Message("CantRomanceTargetZeroChance");
    //         __result = "This is InteractionWorker_RomanceAttempt";
    //         return;
    //     }

    //     if ((!forOpinionExplanation && !initiator.CanReach(target, PathEndMode.Touch, Danger.Deadly)) || target.IsForbidden(initiator))
    //     {
    //         // __result = "CantRomanceTargetUnreachable".Translate();
    //         Log.Message("CantRomanceTargetUnreachable");
    //         __result = "This is target.IsForbidden";
    //         return;
    //     }

    //     if (initiator.relations.IsTryRomanceOnCooldown)
    //     {
    //         // __result = "RomanceOnCooldown".Translate();
    //         Log.Message("RomanceOnCooldown");
    //         __result = "This is IsTryRomanceOnCooldown";
    //         return;
    //     }
    //     Log.Message("RomanceEligiblePair");
    //     __result = true;

    //     return;
    // }

    // }

}

// [HarmonyPatch(typeof(RelationsUtility), "RomanceEligible")]
// public static class Postfix_RomanceEligible
// {
//     [HarmonyPostfix]
//     public static void Postfix(ref AcceptanceReport __result, Pawn pawn)
//     {
//         if (!__result.Accepted) return;

//         int minAge = Prefix_MinAgeForRomance.GetMinRomanceAgeFromPreceptLabels(pawn);
//         if (pawn.ageTracker.AgeBiologicalYearsFloat < minAge)
//         {
//             __result = false;
//         }
//     }
// }

// [HarmonyPatch(typeof(RelationsUtility), "RomanceEligiblePair")]
// public static class Postfix_RomanceEligiblePair
// {
//     [HarmonyPostfix]
//     public static void Postfix(ref AcceptanceReport __result, Pawn initiator, Pawn target, bool forOpinionExplanation)
//     {
//         if (!__result.Accepted || !forOpinionExplanation) return;

//         int minAge = Prefix_MinAgeForRomance.GetMinRomanceAgeFromPreceptLabels(target);
//         if (target.ageTracker.AgeBiologicalYearsFloat < minAge)
//         {
//             __result = "CantRomanceTargetYoung".Translate();
//         }
//     }
// }
