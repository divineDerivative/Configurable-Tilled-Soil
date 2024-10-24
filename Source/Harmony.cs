using HarmonyLib;
using RimWorld;
using Verse;

namespace TilledSoil
{
    [HarmonyPatch(typeof(Building), nameof(Building.SpawnSetup))]
    public static class Building_SpawnSetup
    {
        public static void Postfix(Building __instance)
        {
            Designator_GatherDirtBags.Notify_BuildingSpawned(__instance);
        }
    }

    [HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
    public static class DefGenerator_GenerateImpliedDefs_PreResolve
    {
        public static void Postfix()
        {
            TilledSoilSettings.TilledSoil = TerrainDef.Named("TilledSoil");
            TilledSoilSettings.TilledSoil.SetStatBaseValue(StatDefOf.WorkToBuild, TilledSoilMod.settings.workAmount);
            TilledSoilSettings.cachedWorkAmount = TilledSoilMod.settings.workAmount;

            if (TilledSoilSettings.VFEActive)
            {
                TilledSoilSettings.PackedDirt = TerrainDef.Named("VFEArch_PlayerPackedDirt");
            }
        }
    }

    [HarmonyPatch(typeof(Root_Play), nameof(Root_Play.Start))]
    public static class Root_Play_Start
    {
        public static void Prefix()
        {
            TilledSoilSettings.cachedSoilCost = TilledSoilMod.settings.soilCost;
            TilledSoilSettings.cachedPackedDirtCost = TilledSoilMod.settings.packedDirtCost;
        }
    }

    [HarmonyPatch(typeof(TerrainGrid), nameof(TerrainGrid.SetTerrain))]
    public static class TerrainGrid_SetTerrain
    {
        public static void Postfix(IntVec3 c, TerrainDef newTerr, Map ___map)
        {
            if (newTerr == TilledSoilSettings.TilledSoil && TilledSoilMod.settings.tillingDestroysPlants)
            {
                Utilities.DestroyPlantsAt(c, ___map);
            }
        }
    }
}