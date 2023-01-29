using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon;

//TODO: test to ensure works without ideology
[HarmonyPatch(typeof(Pawn_GuiltTracker), "get_IsGuilty")]
internal static class Pawn_GuiltTracker_IsGuilty_Patch
{
    private static bool Prefix(ref bool __result, Pawn_GuiltTracker __instance, Pawn ___pawn)
    {
        if (___pawn.Ideo != null && (
                IdeoGuiltyOfStacking(___pawn) || IdeoGuiltyOfSleeving(___pawn)
                )
           )
        {
            __result = true;
            return false;
        }

        return true;
    }

    private static bool IdeoGuiltyOfSleeving(Pawn ___pawn)
    {
        return ___pawn.Ideo.precepts.Exists(precept => precept.def == AC_DefOf.AC_Sleeving_Despised) && ___pawn.UsesSleeve();
    }

    private static bool IdeoGuiltyOfStacking(Pawn ___pawn)
    {
        return ___pawn.Ideo.precepts.Exists(precept => precept.def == AC_DefOf.AC_Stacking_Despised) && ___pawn.HasStack();
    }
}