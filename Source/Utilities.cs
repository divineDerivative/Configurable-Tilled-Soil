using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace TilledSoil
{
    internal static class Utilities
    {
        internal static void DestroyPlantsAt(IntVec3 cell, Map map)
        {
            List<Thing> thingList = cell.GetThingList(map);
            //Use a for loop and go backwards to avoid modifying the list while iterating through it
            for (int num = thingList.Count - 1; num >= 0; num--)
            {
                if (thingList[num].def.category == ThingCategory.Plant)
                {
                    thingList[num].Destroy();
                }
            }
        }
    }
}
