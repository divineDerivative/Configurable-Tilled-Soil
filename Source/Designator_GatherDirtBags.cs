using RimWorld;
using UnityEngine;
using Verse;

namespace TilledSoil
{
    public class Designator_GatherDirtBags : Designator_Cells
    {
        public override int DraggableDimensions => 2;
        public override bool DragDrawMeasurements => true;

        public Designator_GatherDirtBags()
        {
            defaultLabel = "TilledSoil.DesignatorLabel".Translate();
            defaultDesc = "TilledSoil.DesignatorDesc".Translate();
            icon = ContentFinder<Texture2D>.Get("TilledSoil_Shovel");
            useMouseIcon = true;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_SmoothSurface;
        }

        public override AcceptanceReport CanDesignateThing(Thing t) => false;

        public override AcceptanceReport CanDesignateCell(IntVec3 cell)
        {
            if (!cell.InBounds(Map) || cell.Fogged(Map))
            {
                return false;
            }

            if (Map.designationManager.DesignationAt(cell, DefOfTS.GatherDirtBags) != null)
            {
                return "TilledSoil.AlreadyMarked".Translate();
            }
            if (cell.GetEdifice(Map) != null)
            {
                return "TilledSoil.Building".Translate();
            }
            if (!cell.GetTerrain(Map).affordances.Contains(DefOfTS.GrowSoil))
            {
                return "TilledSoil.DesignateSoil".Translate();
            }
            return true;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            Map.designationManager.AddDesignation(new Designation(c, DefOfTS.GatherDirtBags));
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }

        public static void Notify_BuildingSpawned(Building b)
        {
            foreach (IntVec3 cell in b.OccupiedRect())
            {
                Designation designation = b.Map.designationManager.DesignationAt(cell, DefOfTS.GatherDirtBags);
                if (designation != null)
                {
                    b.Map.designationManager.RemoveDesignation(designation);
                }
            }
        }
    }
}
