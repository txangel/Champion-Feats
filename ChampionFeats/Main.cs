using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem;
using ChampionFeats.Config;
using ChampionFeats.Utilities;
//using Kingmaker.Blueprints.JsonSystem;

using UnityEngine;
using UnityModManagerNet;
using Kingmaker.Localization;
using System.IO;
using ChampionFeats.Components;
//using static UnityModManagerNet.UnityModManager;
namespace ChampionFeats
{
    public class Main
    {
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnToggle = new Func<UnityModManager.ModEntry, bool, bool>(Main.OnToggle);
            var harmony = new Harmony(modEntry.Info.Id);

            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
            ModSettings.ModEntry = modEntry;
            ModSettings.LoadAllSettings();
            modEntry.OnGUI = new Action<UnityModManager.ModEntry>(Main.OnGUI);
            modEntry.OnSaveGUI = new Action<UnityModManager.ModEntry>(Main.OnSaveGUI);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            PostPatchInitializer.Initialize();
            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            iAmEnabled = value;
            return true;
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
            AddScalingSavingThrows.OnSettingsSave();
        }

        private static void vert10()
        {
            GUILayout.Space(10);
            GUILayout.Label("--------------------------------------------------------");
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            if (!iAmEnabled)
            {
                return;
            }

            GUILayoutOption[] options = new GUILayoutOption[]
            {
                GUILayout.ExpandWidth(true),
                GUILayout.MaxWidth(1000)
            };
            GUILayout.Label("FOR BEST EFFECT: restart the game after changing these settings.", options);

            settings.FeatsAreMythic = GUILayout.Toggle(settings.FeatsAreMythic, "Check this to make these feats require Mythic Feat selections instead of regular Feat selections", options);

            vert10();
            GUILayout.Label("Champion Protection (AC bonus for armor):", options);
            GUILayout.Label(String.Format("Levels Per Step: {0}", settings.ScalingACLevelsPerStep), options);
            settings.ScalingACLevelsPerStep = Mathf.RoundToInt(GUILayout.HorizontalSlider(settings.ScalingACLevelsPerStep, 1, 5, options));
            GUILayout.Label(String.Format("Light Armor Bonus Per Step: {0}", settings.ScalingACArmorBonusLightPerStep), options);
            settings.ScalingACArmorBonusLightPerStep = Mathf.RoundToInt(GUILayout.HorizontalSlider(settings.ScalingACArmorBonusLightPerStep, 1, 10, options));
            GUILayout.Label(String.Format("Medium Armor Bonus Per Step: {0}", settings.ScalingACArmorBonusMediumPerStep), options);
            settings.ScalingACArmorBonusMediumPerStep = Mathf.RoundToInt(GUILayout.HorizontalSlider(settings.ScalingACArmorBonusMediumPerStep, 1, 20, options));
            GUILayout.Label(String.Format("Heavy Armor Bonus Per Step: {0}", settings.ScalingACArmorBonusHeavyPerStep), options);
            settings.ScalingACArmorBonusHeavyPerStep = Mathf.RoundToInt(GUILayout.HorizontalSlider(settings.ScalingACArmorBonusHeavyPerStep, 1, 30, options));

            vert10();
            GUILayout.Label("Champion Guard (DR):", options);
            GUILayout.Label(String.Format("Levels Per Step: {0}", settings.ScalingDRLevelsPerStep), options);
            settings.ScalingDRLevelsPerStep = Mathf.RoundToInt(GUILayout.HorizontalSlider(settings.ScalingDRLevelsPerStep, 1, 5, options));
            GUILayout.Label(String.Format("DR Bonus Per Step: {0}", settings.ScalingDRBonusPerStep), options);
            settings.ScalingDRBonusPerStep = Mathf.RoundToInt(GUILayout.HorizontalSlider(settings.ScalingDRBonusPerStep, 1, 20, options));

            vert10();
            GUILayout.Label("Champion Saves (Saving Throws):", options);
            GUILayout.Label(String.Format("Bonus Per Level: {0}", settings.ScalingSaveBonusPerLevel), options);
            settings.ScalingSaveBonusPerLevel = Mathf.RoundToInt(GUILayout.HorizontalSlider(settings.ScalingSaveBonusPerLevel, 1, 10, options));

            vert10();
            GUILayout.Label("Champion Aim (Weapon AB):", options);
            GUILayout.Label(String.Format("Levels Per Step: {0}", settings.ScalingABLevelsPerStep), options);
            settings.ScalingABLevelsPerStep = Mathf.RoundToInt(GUILayout.HorizontalSlider(settings.ScalingABLevelsPerStep, 1, 5, options));
            GUILayout.Label(String.Format("Bonus Per Step: {0}", settings.ScalingABBonusPerStep), options);
            settings.ScalingABBonusPerStep = Mathf.RoundToInt(GUILayout.HorizontalSlider(settings.ScalingABBonusPerStep, 1, 10, options));

            vert10();
            GUILayout.Label("Champion Strikes (Weapon Damage):", options);
            GUILayout.Label(String.Format("Levels Per Step: {0}", settings.ScalingDamageLevelsPerStep), options);
            settings.ScalingDamageLevelsPerStep = Mathf.RoundToInt(GUILayout.HorizontalSlider(settings.ScalingDamageLevelsPerStep, 1, 5, options));
            GUILayout.Label(String.Format("Bonus Per Step: {0}", settings.ScalingDamageBonusPerStep), options);
            settings.ScalingDamageBonusPerStep = Mathf.RoundToInt(GUILayout.HorizontalSlider(settings.ScalingDamageBonusPerStep, 1, 10, options));

            vert10();
            GUILayout.Label("Champion Spell Blasts (Spell Damage):", options);
            GUILayout.Label(String.Format("Levels Per Step: {0}", settings.ScalingSpellDamageLevelsPerStep), options);
            settings.ScalingSpellDamageLevelsPerStep = Mathf.RoundToInt(GUILayout.HorizontalSlider(settings.ScalingSpellDamageLevelsPerStep, 1, 5, options));
            GUILayout.Label(String.Format("Bonus Per Step: {0}", settings.ScalingSpellDamageBonusPerStep), options);
            settings.ScalingSpellDamageBonusPerStep = Mathf.RoundToInt(GUILayout.HorizontalSlider(settings.ScalingSpellDamageBonusPerStep, 1, 10, options));

            vert10();
            GUILayout.Label("Champion Spell Force (Spell DC):", options);
            GUILayout.Label(String.Format("Levels Per Step: {0}", settings.ScalingSpellDCLevelsPerStep), options);
            settings.ScalingSpellDCLevelsPerStep = Mathf.RoundToInt(GUILayout.HorizontalSlider(settings.ScalingSpellDCLevelsPerStep, 1, 5, options));
            GUILayout.Label(String.Format("Bonus Per Step: {0}", settings.ScalingSpellDCBonusPerStep), options);
            settings.ScalingSpellDCBonusPerStep = Mathf.RoundToInt(GUILayout.HorizontalSlider(settings.ScalingSpellDCBonusPerStep, 1, 10, options));

            vert10();
            GUILayout.Label("Champion Spell Penetration (Spell Penetration):", options);
            GUILayout.Label(String.Format("Bonus Per Level: {0}", settings.ScalingSpellPenBonusPerLevel), options);
            settings.ScalingSpellPenBonusPerLevel = Mathf.RoundToInt(GUILayout.HorizontalSlider(settings.ScalingSpellPenBonusPerLevel, 1, 10, options));

        }


        private static bool iAmEnabled;

        public static Settings settings;

        public static void Log2File(string msg)
        {
            /*
            StreamWriter streamWriter = File.AppendText("C:\\temp\\log.txt");
            streamWriter.WriteLine(msg);
            streamWriter.Flush();
            streamWriter.Close();
            */
        }

        public static void Log(string msg)
        {
            ModSettings.ModEntry.Logger.Log(msg);
        }
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDebug(string msg)
        {
            ModSettings.ModEntry.Logger.Log(msg);
        }
        public static void LogPatch(string action, [NotNull] IScriptableObjectWithAssetId bp)
        {
            Log($"{action}: {bp.AssetGuid} - {bp.name}");
        }
        public static void LogHeader(string msg)
        {
            Log($"--{msg.ToUpper()}--");
        }
        public static Exception Error(String message)
        {
            Log(message);
            return new InvalidOperationException(message);
        }

        public static LocalizedString MakeLocalizedString(string key, string value)
        {
            LocalizationManager.CurrentPack.PutString(key, value);
            LocalizedString localizedString = new LocalizedString();
            typeof(LocalizedString).GetField("m_Key", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(localizedString, key);
            return localizedString;
        }
    }
}
