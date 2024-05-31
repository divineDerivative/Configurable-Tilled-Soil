using HarmonyLib;
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
}