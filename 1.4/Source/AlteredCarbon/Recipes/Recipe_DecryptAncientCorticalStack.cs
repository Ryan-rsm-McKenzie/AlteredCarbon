﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class UnfinishedAncientStack : UnfinishedThing
    {
        public override string LabelNoCount
        {
            get
            {
                return GenLabel.ThingLabel(this, 1);
            }
        }
    }
    public class Recipe_DecryptAncientCorticalStack : RecipeWorker
    {
        public override void ConsumeIngredient(Thing ingredient, RecipeDef recipe, Map map)
        {
            base.ConsumeIngredient(ingredient, recipe, map);
        }
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            List<Pair<Action, Func<float>>> actions = new List<Pair<Action, Func<float>>>();
            actions.Add(new Pair<Action, Func<float>>(delegate
            {
                var pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.AncientSoldier, Faction.OfAncients);
                var skills = DefDatabase<SkillDef>.AllDefs.Where(x => !pawn.skills.GetSkill(x).TotallyDisabled && pawn.skills.GetSkill(x).Level < 15).InRandomOrder().Take(2);
                foreach (var skill in skills)
                {
                    var skillRecord = pawn.skills.GetSkill(skill);
                    skillRecord.Level = Rand.RangeInclusive(15, 20);
                    skillRecord.passion = (Passion)Rand.RangeInclusive(1, 2);
                }
                var corticalStack = ThingMaker.MakeThing(AC_DefOf.VFEU_FilledCorticalStack) as CorticalStack;
                corticalStack.PersonaData.CopyFromPawn(pawn, AC_DefOf.VFEU_FilledCorticalStack, copyRaceGenderInfo: true);
                corticalStack.PersonaData.stackGroupID = AlteredCarbonManager.Instance.GetStackGroupID(corticalStack);
                AlteredCarbonManager.Instance.RegisterStack(corticalStack);
                GenPlace.TryPlaceThing(corticalStack, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                Messages.Message("AC.FixedAncientStack".Translate(), corticalStack, MessageTypeDefOf.PositiveEvent);
            }, () => 0.1f));
            actions.Add(new Pair<Action, Func<float>>(delegate
            {
                var emptyStack = ThingMaker.MakeThing(AC_DefOf.VFEU_EmptyCorticalStack);
                GenPlace.TryPlaceThing(emptyStack, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                Messages.Message("AC.GainedEmptyCorticalStack".Translate(), emptyStack, MessageTypeDefOf.PositiveEvent);
            }, () => 0.5f));
            actions.Add(new Pair<Action, Func<float>>(delegate
            {
                var advComponent = ThingMaker.MakeThing(ThingDefOf.ComponentSpacer);
                advComponent.stackCount = 2;
                GenPlace.TryPlaceThing(advComponent, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);

                var plasteel = ThingMaker.MakeThing(ThingDefOf.Plasteel);
                plasteel.stackCount = 5;
                GenPlace.TryPlaceThing(plasteel, billDoer.Position, billDoer.Map, ThingPlaceMode.Near);
                Messages.Message("AC.FailedCorticalStackDestroyed".Translate(), MessageTypeDefOf.NeutralEvent);
            }, () => 
            {
                // Chance: 50% with skill level 8, each skill level lowers the chance by 5% until its 0%.
                float chance = 0.5f;
                var intelSkill = billDoer.skills.GetSkill(SkillDefOf.Intellectual)?.Level ?? 0;
                if (intelSkill > 8)
                {
                    var bonus = intelSkill - 8;
                    chance -= (bonus * 5) / 100f;
                    if (chance < 0)
                    {
                        chance = 0;
                    }
                }
                return chance;
            }));

            if (actions.TryRandomElementByWeight(x => x.Second(), out var result))
            {
                result.First();
            }
        }

        //private void AddResearchProgress(ResearchProjectDef proj, float researchProgressMultiplier)
        //{
        //    if (proj != null)
        //    {
        //        var prevProj = Find.ResearchManager.currentProj;
        //        var prerequisites = GetNonFinishedPrerequisites(proj).ToList();
        //        foreach (var prerequisite in prerequisites)
        //        {
        //            Find.ResearchManager.currentProj = prerequisite;
        //            Find.ResearchManager.FinishProject(prerequisite, doCompletionDialog: false);
        //        }
        //        Dictionary<ResearchProjectDef, float> dictionary = Find.ResearchManager.progress;
        //        if (dictionary.ContainsKey(proj))
        //        {
        //            dictionary[proj] += (proj.baseCost - Find.ResearchManager.GetProgress(proj)) * researchProgressMultiplier;
        //        }
        //        else
        //        {
        //            dictionary[proj] = proj.baseCost * researchProgressMultiplier;
        //        }
        //        if (proj.IsFinished)
        //        {
        //            Find.ResearchManager.currentProj = proj;
        //            Find.ResearchManager.FinishProject(proj, doCompletionDialog: true);
        //        }
        //        Find.ResearchManager.currentProj = prevProj;
        //    }
        //}
        //
        //private IEnumerable<ResearchProjectDef> GetNonFinishedPrerequisites(ResearchProjectDef proj)
        //{
        //    if (proj.prerequisites != null)
        //    {
        //        for (int i = 0; i < proj.prerequisites.Count; i++)
        //        {
        //            if (!proj.prerequisites[i].IsFinished)
        //            {
        //                yield return proj.prerequisites[i];
        //            }
        //        }
        //    }
        //    if (proj.hiddenPrerequisites != null)
        //    {
        //        for (int j = 0; j < proj.hiddenPrerequisites.Count; j++)
        //        {
        //            if (!proj.hiddenPrerequisites[j].IsFinished)
        //            {
        //                yield return proj.hiddenPrerequisites[j];
        //            }
        //        }
        //    }
        //}
        //
        //private bool TryGetUnfinishedSpacerResearch(out ResearchProjectDef researchProjectDef)
        //{
        //    return DefDatabase<ResearchProjectDef>.AllDefs.Where(x => x.techLevel > TechLevel.Spacer && !x.IsFinished).TryRandomElement(out researchProjectDef);
        //}
    }
}

