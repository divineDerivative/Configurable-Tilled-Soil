using Verse;
using HarmonyLib;

namespace TilledSoil
{
    [HarmonyPatch(typeof(Building), nameof(Building.SpawnSetup))]
    public static class Building_SpawnSetup
    {
        public static void Postfix(Map map, bool respawningAfterLoad, Building __instance)
        {
            Designator_GatherDirtBags.Notify_BuildingSpawned(__instance);
        }
    }
}