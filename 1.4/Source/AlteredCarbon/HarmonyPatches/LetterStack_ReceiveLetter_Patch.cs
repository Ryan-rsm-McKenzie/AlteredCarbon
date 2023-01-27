﻿using System;
using HarmonyLib;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(LetterStack), "ReceiveLetter", new Type[]
    {
        typeof(Letter),
        typeof(string),
    })]
    internal static class LetterStack_ReceiveLetter_Patch
    {
        private static void Prefix(Letter let)
        {
            if (let.def == AC_DefOf.HumanPregnancy && let.lookTargets.PrimaryTarget.Thing is Pawn pawn && 
                pawn.HasCorticalStack(out var hediff) && pawn.CanThink())
            {
                var origPawn = hediff.PersonaData.origPawn;
                if (origPawn != null && origPawn.gender != pawn.gender) 
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.VFEU_WrongGenderPregnant);
                }
            }
        }
    }
}

