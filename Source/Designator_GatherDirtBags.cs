using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            defaultLabel = "Gather Dirt";
            defaultDesc = "GatherDirt";
            icon = ContentFinder<Texture2D>.Get("UI/Designators/Haul");
            useMouseIcon = true;
            soundDragSustain = SoundDefOf.Designate_DragStandard;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_SmoothSurface;
        }

        public override AcceptanceReport CanDesignateThing(Thing t) => false;

        public override AcceptanceReport CanDesignateCell(IntVec3 cell)
        {
            if (!cell.InBounds(Map))
            {
                return false;
            }
            if (cell.Fogged(Map))
            {
                return false;
            }
            if (Map.designationManager.DesignationAt(cell, DefOfTS.GatherDirtBags) != null)
            {
                return "SurfaceBeingSmoothed".Translate();
            }
            if (cell.GetEdifice(Map) != null)
            {
                return "Building in the way";
            }
            if (!cell.GetTerrain(Map).affordances.Contains(TerrainAffordanceDefOf.GrowSoil))
            {
                return "Must designate soil";
            }
            return AcceptanceReport.WasAccepted;
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            Map.designationManager.AddDesignation(new Designation(c, DefOfTS.GatherDirtBags));
        }

        public override void SelectedUpdate()
        {
            GenUI.RenderMouseoverBracket();
        }
    }
}
