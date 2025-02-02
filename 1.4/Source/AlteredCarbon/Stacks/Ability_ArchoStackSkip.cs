﻿using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace AlteredCarbon
{
    [HotSwappable]
    public class Ability_ArchoStackSkip : Ability
    {
        public Hediff_CorticalStack Hediff_CorticalStack => pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.AC_ArchoStack) as Hediff_CorticalStack;
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            var pawnTarget = target.Pawn;
            if (pawnTarget != null)
            {
                if (ACUtils.CanImplantStackTo(Hediff_CorticalStack.def, pawnTarget))
                {
                    return true;
                }
            }
            return false;
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var target = targets.FirstOrDefault(x => x.Thing is Pawn);
            var pawnTarget = target.Thing as Pawn;
            if (pawnTarget != null)
            {
                var sourceHediff = Hediff_CorticalStack;

                if (pawnTarget.Faction != null && CasterPawn.Faction != null && pawnTarget.Faction != CasterPawn.Faction)
                {
                    pawnTarget.Faction.TryAffectGoodwillWith(CasterPawn.Faction, -80, reason: AC_DefOf.AC_UsedArchoStack, lookTarget: pawnTarget);
                }

                if (pawnTarget.HasCorticalStack(out var stackHediff))
                {
                    stackHediff.preventKill = true;
                    pawnTarget.health.RemoveHediff(stackHediff);
                }

                BodyPartRecord neckRecord = pawnTarget.GetNeck();
                var copyHediff = HediffMaker.MakeHediff(sourceHediff.def, pawnTarget, neckRecord) as Hediff_CorticalStack;
                copyHediff.PersonaData = sourceHediff.PersonaData;
                copyHediff.PersonaData.CopyFromPawn(pawn, sourceHediff.SourceStack);
                pawnTarget.health.AddHediff(copyHediff, neckRecord);
                copyHediff.PersonaData.OverwritePawn(pawnTarget, null);

                sourceHediff.preventSpawningStack = true;
                pawn.health.RemoveHediff(sourceHediff);
                sourceHediff.preventSpawningStack = false;

                copyHediff.skipAbility.cooldown = sourceHediff.skipAbility.cooldown;
                AlteredCarbonManager.Instance.deadPawns.Add(pawn);
                pawn.GetComp<CompAbilities>().currentlyCasting = null;
            }
        }
    }
}