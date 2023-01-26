using RimWorld;
using Verse;
using Verse.AI;

namespace TilledSoil
{
    public class WorkGiver_GatherDirtBags : WorkGiver_ConstructAffectFloor
    {
        protected override DesignationDef DesDef => DefOfTS.GatherDirtBags;
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override Job JobOnCell(Pawn pawn, IntVec3 cell, bool forced = false)
        {
            return JobMaker.MakeJob(DefOfTS.GatherDirtJob, cell);
        }
    }
}
