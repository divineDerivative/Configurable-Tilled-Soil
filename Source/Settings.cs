using UnityEngine;
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace TilledSoil
{
    public class TilledSoilSettings : ModSettings
    {
        public int fertility = 120;
        public string canTillOn;
        public string canTurnIntoDirt;
        public bool requireCost = false;
        public int soilCost = 1;
        public int workAmount = 500;

        public float Fertility => fertility / 100f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref fertility, "Fertility", 120);
            Scribe_Values.Look(ref canTillOn, "CanTillOn", "GrowSoil");
            Scribe_Values.Look(ref canTurnIntoDirt, "CanTurnIntoDirt", "SmoothableStone");
            Scribe_Values.Look(ref requireCost, "RequireCost", defaultValue: false, forceSave: true);
            Scribe_Values.Look(ref soilCost, "SoilCost", 1);
            Scribe_Values.Look(ref workAmount, "WorkAmount", 500);
            base.ExposeData();
        }

        public void ResetSettings()
        {
            fertility = 120;
            canTillOn = "GrowSoil";
            canTurnIntoDirt = "SmoothableStone";
            requireCost = false;
            soilCost = 1;
            workAmount = 500;
        }

        public void UpdateSettings()
        {
            TerrainDef tilledSoil = TerrainDef.Named("TilledSoil");
            tilledSoil.fertility = Fertility;
            tilledSoil.SetStatBaseValue(StatDefOf.WorkToBuild, workAmount);
            tilledSoil.terrainAffordanceNeeded = DefDatabase<TerrainAffordanceDef>.GetNamed(canTillOn);

            if (requireCost)
            {
                tilledSoil.costList = new List<ThingDefCountClass>
                {
                    new ThingDefCountClass(ThingDefOf.WoodLog, soilCost)
                };
            }
            else
            {
                tilledSoil.costList = new List<ThingDefCountClass>();
            }

            TerrainDef dirt = TerrainDef.Named("Dirt");
            dirt.terrainAffordanceNeeded = DefDatabase<TerrainAffordanceDef>.GetNamed(canTurnIntoDirt);
        }
    }

    public class TilledSoilMod : Mod
    {
        public static TilledSoilSettings settings;
        private string soilCostBuffer;
        private string workAmountBuffer;
        private string fertilityBuffer;
        const float gap2 = 24f;
        const float gap3 = 16f;

        public TilledSoilMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<TilledSoilSettings>();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            settings.UpdateSettings();
        }

        public override string SettingsCategory() => "Configurable Tilled Soil";

        public override void DoSettingsWindowContents(Rect canvas)
        {

            Listing_Standard list = new Listing_Standard
            {
                ColumnWidth = canvas.width / 3f - 100f
            };
            list.Begin(canvas);
            list.Label("Fertility " + settings.fertility.ToString() + "%");
            list.GapLine(15f);
            list.Label("Terrain affordance for tilling");
            list.GapLine(gap2 + 1f);
            list.Label("Terrain affordance for dirt");
            //list.Label("NOTE: Placed dirt is permanent. It cannot be removed to revert to the original floor.");
            list.GapLine(gap3);
            list.Label("Requires Wood");
            list.GapLine();
            if (settings.requireCost)
            {
                list.Label("Construction cost in wood");
                list.GapLine();
            }
            list.Label("Work amount required");
            list.GapLine();

            if (list.ButtonText(Translator.Translate("RestoreToDefaultSettings")))
            {
                settings.ResetSettings();
                fertilityBuffer = settings.fertility.ToString();
                workAmountBuffer = settings.workAmount.ToString();
                soilCostBuffer = settings.soilCost.ToString();
            }

            list.NewColumn();
            list.IntEntry(ref settings.fertility, ref fertilityBuffer);
            list.GapLine();

            List<TerrainAffordanceDef> tillList = new List<TerrainAffordanceDef>
            {
                TerrainAffordanceDefOf.Light,
                TerrainAffordanceDefOf.GrowSoil,
            };
            if (list.ButtonTextLabeled(null, DefDatabase<TerrainAffordanceDef>.GetNamed(settings.canTillOn).label))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (TerrainAffordanceDef terrain in tillList)
                {
                    options.Add(new FloatMenuOption(terrain.label, delegate
                    {
                        settings.canTillOn = terrain.defName;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            list.GapLine();

            List<TerrainAffordanceDef> dirtList = new List<TerrainAffordanceDef>
            {
                TerrainAffordanceDefOf.Light,
                TerrainAffordanceDefOf.SmoothableStone,
            };
            if (list.ButtonTextLabeled(null, DefDatabase<TerrainAffordanceDef>.GetNamed(settings.canTurnIntoDirt).label))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (TerrainAffordanceDef terrain in dirtList)
                {
                    options.Add(new FloatMenuOption(terrain.label, delegate
                    {
                        settings.canTurnIntoDirt = terrain.defName;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            list.GapLine(14f);
            list.CheckboxLabeled("", ref settings.requireCost);
            list.GapLine();
            if (settings.requireCost)
            {
                list.TextFieldNumeric(ref settings.soilCost, ref soilCostBuffer, 0f, 100f);
                list.GapLine();
            }
            list.TextFieldNumeric(ref settings.workAmount, ref workAmountBuffer, 1f, 10000f);

            list.NewColumn();
            list.ColumnWidth = canvas.width / 3f + 200f;
            Rect rect = list.Label("100% = Normal soil, 140% = Rich soil, 120% is default.");
            list.GapLine(16f);
            if (settings.canTillOn == "Light")
            {
                list.Label("Can till anywhere a normal floor can be placed.");
            }
            else if (settings.canTillOn == "GrowSoil")
            {
                list.Label("Can only till in growable soil.");
            }
            else
            {
                Log.Error("Unexpected value of canTillOn: " + settings.canTillOn);
            }
            list.GapLine(gap2);
            if (settings.canTurnIntoDirt == "Light")
            {
                list.Label("Can place dirt anywhere a normal floor can be placed.");
            }
            else if (settings.canTurnIntoDirt == "SmoothableStone")
            {
                list.Label("Can only place dirt on rough stone.");
            }
            else
            {
                Log.Error("Unexpected value of canTurnIntoDirt: " + settings.canTurnIntoDirt);
            }
            list.GapLine(gap3);
            list.Label("Does the Tilled Soil require wood to construct? Restart Required.");
            list.GapLine();
            if (settings.requireCost)
            {
                list.Label("How much wood is required for 1 tile.");
                list.GapLine();
            }
            list.Label("How much work is required to construct the soil. 100 = Concrete, 500 = Flagstone.");
            list.End();
        }
    }

    [StaticConstructorOnStartup]
    public class OnStartup
    {
        static OnStartup()
        {
            TilledSoilMod.settings.ExposeData();
            TilledSoilMod.settings.UpdateSettings();
        }
    }
}