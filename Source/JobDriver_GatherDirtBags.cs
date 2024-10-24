using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace TilledSoil
{
    public class JobDriver_GatherDirtBags : JobDriver_AffectFloor
    {
        private float workLeft;
        protected override int BaseWorkAmount => 1000;
        protected override DesignationDef DesDef => DefOfTS.GatherDirtBags;
        protected override StatDef SpeedStat => StatDefOf.PlantWorkSpeed;

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOn(() => !job.ignoreDesignations && Map.designationManager.DesignationAt(TargetLocA, DesDef) == null);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
            Toil doWork = ToilMaker.MakeToil("GatherDirtBag");
            doWork.initAction = delegate
            {
                workLeft = BaseWorkAmount;
            };
            doWork.tickAction = delegate
            {
                float num = (SpeedStat != null && !SpeedStat.Worker.IsDisabledFor(doWork.actor)) ? doWork.actor.GetStatValue(SpeedStat) : 1f;
                num *= 1.7f;
                workLeft -= num;
                doWork.actor.skills?.Learn(SkillDefOf.Plants, 0.1f);
                if (workLeft < 0f)
                {
                    DoEffect(TargetLocA);
                    Map.designationManager.DesignationAt(TargetLocA, DesDef)?.Delete();
                    ReadyForNextToil();
                }
            };
            doWork.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            doWork.WithProgressBar(TargetIndex.A, () => 1f - (workLeft / BaseWorkAmount));
            doWork.defaultCompleteMode = ToilCompleteMode.Never;
            doWork.WithEffect(EffecterDefOf.ConstructDirt, TargetIndex.A);
            doWork.PlaySustainerOrSound(SoundDefOf.Interact_ConstructDirt);
            doWork.activeSkill = () => SkillDefOf.Plants;
            yield return doWork;
        }

        protected override void DoEffect(IntVec3 c)
        {
            Thing thing = ThingMaker.MakeThing(TilledSoilSettings.DirtBag);
            GenPlace.TryPlaceThing(thing, TargetLocA, pawn.Map, ThingPlaceMode.Near);
            Map.snowGrid.SetDepth(TargetLocA, 0f);
            //Get rid of plants
            Utilities.DestroyPlantsAt(c, Map);
        }
    }
}
