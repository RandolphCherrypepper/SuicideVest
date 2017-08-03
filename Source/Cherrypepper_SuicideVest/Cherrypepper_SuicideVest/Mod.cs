using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Randolph_Cherrypepper
{

    public class DangerousApparelTags : DefModExtension
    {
        protected List<string> tags;

        public List<string> Tags
        {
            get
            {
                return tags;
            }
        }
    }

    public class ModWearableExplosive : Verse.Mod
    {

        public ModWearableExplosive(ModContentPack content) : base(content)
        {
            base.GetSettings<ModWearableExplosiveSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            ModWearableExplosiveSettings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return Translator.Translate("ModName");
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            // After writing the settings, immediately react to some of the changes made.
            if (LoadedModManager.GetMod<ModWearableExplosive>().GetSettings<ModWearableExplosiveSettings>().SpawnWithRaids)
            {
                // Activate Spawn with Raids.
                // Search all ThingDefs for apparel with the WearableExplosive tag.
                foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
                {
                    if (def != null && def.apparel != null && def.apparel.tags != null && def.apparel.tags.Contains("WearableExplosive"))
                    {
                        // Apply tags specified in DangerousApparelTags to the apparel tags
                        if (def.GetModExtension<DangerousApparelTags>() != null)
                        {
                            Log.Message("Adding Raids to explosive: " + def.defName);
                            foreach (string tag in def.GetModExtension<DangerousApparelTags>().Tags)
                            {
                                if (!def.apparel.tags.Contains(tag))
                                    def.apparel.tags.Add(tag);
                            }
                        }
                    }
                }
            }
            else
            {
                // Deactivate Spawn with Raids.
                // Search all ThingDefs for apparel with the WearableExplosive tag.
                foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
                {
                    if (def != null && def.apparel != null && def.apparel.tags != null && def.apparel.tags.Contains("WearableExplosive"))
                    {
                        // Remove tags specified in DangerousApparelTags to the apparel tags
                        if (def.GetModExtension<DangerousApparelTags>() != null)
                        {
                            Log.Message("Removing Raids to explosive: " + def.defName);
                            foreach (string tag in def.GetModExtension<DangerousApparelTags>().Tags)
                            {
                                if (def.apparel.tags.Contains(tag))
                                    def.apparel.tags.Remove(tag);
                            }
                        }
                    }
                }
            }
        }

    }

    public class ModWearableExplosiveSettings : ModSettings
    {

        protected static bool spawnWithRaids = false;

        public bool SpawnWithRaids
        {
            get
            {
                return spawnWithRaids;
            }
        }

        public static void DoSettingsWindowContents(Rect rect)
        {
            // Make a list of options with checkboxes and whatnot.
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(rect);
            listing_Standard.CheckboxLabeled(Translator.Translate("SpawnWithRaids"), ref spawnWithRaids);
            listing_Standard.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref spawnWithRaids, "SpawnWithRaids", false, false);
        }

    }

}