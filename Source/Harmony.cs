using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
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

#if v1_6
    //Prevent dirt from being placed on top of soil
    [HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanPlaceBlueprintAt))]
    public static class GenConstruct_CanPlaceBlueprintAt
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new(instructions, generator);
            matcher.DeclareLocal(typeof(TerrainDef), out LocalBuilder existingTerrainLocal);
            // Find the call to TerrainAt
            matcher.MatchStartForward([
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(TerrainGrid), nameof(TerrainGrid.TerrainAt), [typeof(IntVec3)])),
                    new CodeMatch(OpCodes.Ldloc_3),
                    new CodeMatch(OpCodes.Bne_Un_S)]
                );
            matcher.InsertAfterAndAdvance(
                //Store the existing terrain as a local variable
                new CodeInstruction(OpCodes.Stloc, existingTerrainLocal),
                //Put it back on the stack
                new CodeInstruction(OpCodes.Ldloc, existingTerrainLocal.LocalIndex)
                );
            matcher.Advance(1);
            //Copy the instruction for loading the new terrain on the stack
            CodeInstruction newterrain = matcher.Instruction;
            matcher.Advance(1);
            //Put a label on the original branch
            matcher.CreateLabel(out Label branchLabel);
            //Put the terrains on the stack again
            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldloc, existingTerrainLocal.LocalIndex),
                new CodeInstruction(newterrain.opcode, newterrain.operand)
                );

            //Call the helper
            matcher.Insert(
                CodeInstruction.Call(typeof(GenConstruct_CanPlaceBlueprintAt), nameof(Intercept)),
                //Jump to the original branch if the helper returns false
                new CodeInstruction(OpCodes.Brfalse_S, branchLabel),
                //If true, which means it's my special case, remove new terrain from the stack and add existing, so that existing existing is on the stack
                //Then the original branch will be see them as the same
                new CodeInstruction(OpCodes.Pop),
                new CodeInstruction(OpCodes.Ldloc, existingTerrainLocal.LocalIndex)
                );

            return matcher.InstructionEnumeration();
        }
        public static bool Intercept(TerrainDef existing, TerrainDef newTerrain)
        {
            if (newTerrain == DefOfTS.Dirt && existing == TerrainDefOf.Soil)
            {
                return true;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(WorkGiver_ConstructDeliverResources), "ShouldRemoveExistingFloorFirst")]
    public static class WorkGiver_ConstructDeliverResources_ShouldRemoveExistingFloorFirst
    {
        public static void Postfix(Blueprint blue, ref bool __result)
        {
            if (blue.def.entityDefToBuild == TilledSoilSettings.TilledSoil)
            {
                TerrainDef existing = blue.Map.terrainGrid.TopTerrainAt(blue.Position);
                if (existing == DefOfTS.Dirt)
                {
                    __result = false;
                }
            }
        }
    }
#endif
}