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

        //public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
        //{
        //    return !c.IsForbidden(pawn) && pawn.Map.designationManager.DesignationAt(c, DesDef) != null && pawn.CanReserve(c, 1, -1, null, forced);
        //}
    }
}
