using DivineFramework;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace TilledSoil
{
    public class TilledSoilMod : Mod
    {
        public static TilledSoilSettings settings;

        public TilledSoilMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<TilledSoilSettings>();
            Harmony harmony = new("divineDerivative.tilledsoil");
            harmony.PatchAll();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            settings.UpdateSettings();
        }

        public override string SettingsCategory() => "TilledSoil.ModTitle".Translate();

        internal SettingsHandler<TilledSoilSettings> settingsHandler = new();

        public override void DoSettingsWindowContents(Rect canvas)
        {
            Listing_Standard list = new();
            list.Begin(canvas);
            if (!settingsHandler.Initialized)
            {
                settingsHandler.width = canvas.width;
                settings.SetUpHandler(settingsHandler);
            }

            settingsHandler.Draw(list);

            list.End();
        }
    }
}