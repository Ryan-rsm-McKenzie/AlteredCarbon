using RimWorld;
using Verse;

namespace AlteredCarbon;

public class ThoughtWorker_Precept_Stack : ThoughtWorker_Precept
{
    public override ThoughtState ShouldHaveThought(Pawn p)
    {
        return p.HasCorticalStack(out var stackHediff) && stackHediff.def == AC_DefOf.VFEU_CorticalStack;
    }
}