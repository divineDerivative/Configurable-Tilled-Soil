using DivineFramework;
using DivineFramework.UI;
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
            ModManagement.RegisterMod("TilledSoil.ModTitle", typeof(TilledSoilMod).Assembly.GetName().Name, new("0.8.1.0"), "<color=#567b2a>[TilledSoil]</color>");
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            settings.UpdateSettings();
        }

        public override string SettingsCategory() => "TilledSoil.ModTitle".Translate();

        internal SettingsHandler<TilledSoilSettings> settingsHandler = new();
        internal SettingsHandler<TilledSoilSettings> integrationHandler = new();

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

            if (TilledSoilSettings.VFEActive)
            {
                if (!integrationHandler.Initialized)
                {
                    integrationHandler.width = canvas.width;
                    settings.SetUpIntegrationHandler(integrationHandler);
                }
                integrationHandler.Draw(list);
            }

            list.End();
        }
    }
}