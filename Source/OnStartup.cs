using DivineFramework;
using RimWorld;
using Verse;

namespace TilledSoil
{
    [StaticConstructorOnStartup]
    public class OnStartup
    {
        static OnStartup()
        {
            TilledSoilMod.settings.ExposeData();
            if (ModsConfig.IsActive("mlie.soilrelocationframework") || ModsConfig.IsActive("udderlyevelyn.soilrelocation"))
            {
                TilledSoilSettings.SoilRelocationActive = true;
                TilledSoilSettings.DirtBag = DefDatabase<ThingDef>.GetNamed("SR_Soil");
            }
            else
            {
                TilledSoilSettings.DirtBag = DefDatabase<ThingDef>.GetNamed("DirtBag");
            }
            TilledSoilSettings.tillList = [TerrainAffordanceDefOf.Light, DefOfTS.GrowSoil,];
            TilledSoilSettings.dirtList = [TerrainAffordanceDefOf.Light, TerrainAffordanceDefOf.SmoothableStone,];
            TilledSoilMod.settings.UpdateSettings();

            VersionCheck.NeededVersion(new("0.1"), "TilledSoil.ModTitle".Translate());
        }
    }
}