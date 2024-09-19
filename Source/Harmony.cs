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
        }
    }
}