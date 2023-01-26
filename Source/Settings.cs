using UnityEngine;
using RimWorld;
using Verse;
using System.Collections.Generic;
using HarmonyLib;

namespace TilledSoil
{
    public class TilledSoilSettings : ModSettings
    {
        public int fertility = 120;
        public string canTillOn;
        public string canTurnIntoDirt;
        public bool requireCost = true;
        public int soilCost = 1;
        public int workAmount = 500;

        public float Fertility => fertility / 100f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref fertility, "Fertility", 120);
            Scribe_Values.Look(ref canTillOn, "CanTillOn", "GrowSoil");
            Scribe_Values.Look(ref canTurnIntoDirt, "CanTurnIntoDirt", "SmoothableStone");
            Scribe_Values.Look(ref requireCost, "RequireCost", defaultValue: true, forceSave: true);
            Scribe_Values.Look(ref soilCost, "SoilCost", 1);
            Scribe_Values.Look(ref workAmount, "WorkAmount", 500);
            base.ExposeData();
        }

        public void ResetSettings()
        {
            fertility = 120;
            canTillOn = "GrowSoil";
            canTurnIntoDirt = "SmoothableStone";
            requireCost = true;
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
                    new ThingDefCountClass(DefOfTS.DirtBag, soilCost)
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

        public override string SettingsCategory() => "TilledSoil.ModTitle".Translate();

        public override void DoSettingsWindowContents(Rect canvas)
        {

            Listing_Standard list = new Listing_Standard
            {
                ColumnWidth = canvas.width / 3f - 100f
            };
            //Left hand column, labels for settings
            list.Begin(canvas);
            list.Label("TilledSoil.FertilityLabel".Translate(settings.fertility.ToString()));
            list.Gap(15f);
            list.Label("TilledSoil.AffordanceTill".Translate());
            list.Gap(gap2 + 1f);
            list.Label("TilledSoil.AffordanceDirt".Translate());
            list.Gap(gap3);
            list.Label("TilledSoil.RequireCost".Translate());
            list.Gap();
            if (settings.requireCost)
            {
                list.Label("TilledSoil.DirtCost".Translate());
                list.Gap();
            }
            list.Label("TilledSoil.WorkAmount".Translate());
            
            list.Gap();

            //Middle column, where settings can be changed
            list.NewColumn();
            list.IntEntry(ref settings.fertility, ref fertilityBuffer);
            if (settings.fertility > 1000) { settings.fertility = 1000; }
            list.Gap();

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
            list.Gap();

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
            list.Gap(14f);
            list.CheckboxLabeled("", ref settings.requireCost);
            list.Gap();
            if (settings.requireCost)
            {
                list.TextFieldNumeric(ref settings.soilCost, ref soilCostBuffer, 0f, 100f);
                list.Gap();
            }
            list.TextFieldNumeric(ref settings.workAmount, ref workAmountBuffer, 1f, 10000f);
            list.Gap();

            if (list.ButtonText(Translator.Translate("RestoreToDefaultSettings")))
            {
                settings.ResetSettings();
                fertilityBuffer = settings.fertility.ToString();
                workAmountBuffer = settings.workAmount.ToString();
                soilCostBuffer = settings.soilCost.ToString();
            }

            //Right hand column, with explanatory notes
            list.NewColumn();
            list.ColumnWidth = canvas.width / 3f + 180f;
            Rect rect = list.Label("TilledSoil.FertilityExplaination".Translate());
            list.Gap(16f);
            if (settings.canTillOn == "Light")
            {
                list.Label("TilledSoil.AffordanceTillLight".Translate());
            }
            else if (settings.canTillOn == "GrowSoil")
            {
                list.Label("TilledSoil.AffordanceTillGrowable".Translate());
            }
            else
            {
                Log.Error("Unexpected value of canTillOn: " + settings.canTillOn);
            }
            list.Gap(gap2);
            if (settings.canTurnIntoDirt == "Light")
            {
                list.Label("TilledSoil.AffordanceDirtLight".Translate());
            }
            else if (settings.canTurnIntoDirt == "SmoothableStone")
            {
                list.Label("TilledSoil.AffordanceDirtSmoothable".Translate());
            }
            else
            {
                Log.Error("Unexpected value of canTurnIntoDirt: " + settings.canTurnIntoDirt);
            }
            list.Gap(gap3);
            list.Label("TilledSoil.RequireCostExplanation".Translate());
            list.Gap();
            if (settings.requireCost)
            {
                list.Label("TilledSoil.DirtCostExplanation".Translate());
                list.Gap();
            }
            list.Label("TilledSoil.WorkAmountExplanation".Translate());
            list.Gap();
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
            Harmony harmony = new Harmony("divineDerivative.tilledsoil");
            harmony.PatchAll();
        }
    }
}