using System;
using System.Runtime.InteropServices;
using ChampionFeats.Config;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using static Kingmaker.Blueprints.Area.FactHolder;
using static Kingmaker.Kingdom.KingdomStats;
using static UnityModManagerNet.UnityModManager.Param;

namespace ChampionFeats.Components
{
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [AllowedOn(typeof(BlueprintBuff))]
    [AllowedOn(typeof(BlueprintFeature))]
    [AllowMultipleComponents]
    [TypeId("D09E14DCC14748708E79AE101363676E")]
    public class AddScalingSavingThrows : UnitFactComponentDelegate, ISubscriber
    {
        public const string BLUEPRINTNAME = "RMChampionFeatSavingThrow";

        /*
         * Used after settings are saved, to recalculate saving throw values in case the step/bonus
         * values for Champion Saves was changed
         */
        public static void OnSettingsSave()
        {
            UpdateSavingThrows();
        }

        private static void UpdateSavingThrows()
        {
            foreach (var unit in Game.Instance.State.Units)
            {
                Feature feature = unit.GetFeature(Resources.GetModBlueprint<BlueprintFeature>(BLUEPRINTNAME));
                feature.TurnOff();
                feature.TurnOn();
            }
        }

        private static void RemoveModifier(UnitEntityData unit, StatType statType)
        {
            SaveModifier modToRemove = null;
            foreach (var list in unit.Stats.GetStat(statType).ModifierList)
            {
                foreach (var mod in list.Value)
                {
                    if (mod is SaveModifier)
                    {
                        modToRemove = (SaveModifier)mod;
                    }
                    if (modToRemove != null)
                    {
                        break;
                    }
                }
                if (modToRemove != null)
                {
                    break;
                }
            }
            if (modToRemove != null)
            {
                unit.Stats.GetStat(statType).RemoveModifier(modToRemove);
            }
        }

        private static void RemoveModifiers(UnitEntityData unit, EntityFactComponent runtime)
        {
            RemoveModifier(unit, StatType.SaveFortitude);
            RemoveModifier(unit, StatType.SaveReflex);
            RemoveModifier(unit, StatType.SaveWill);
        }

        private static void AddModifiers(UnitEntityData unit, EntityFactComponent runtime)
        {
            unit.Stats.GetStat(StatType.SaveFortitude)?.AddModifier(GetNewModifier(unit, runtime));
            unit.Stats.GetStat(StatType.SaveReflex)?.AddModifier(GetNewModifier(unit, runtime));
            unit.Stats.GetStat(StatType.SaveWill)?.AddModifier(GetNewModifier(unit, runtime));
        }

        private static ModifiableValue.Modifier GetNewModifier(UnitEntityData unit, EntityFactComponent runtime)
        {
            return new SaveModifier()
            {
                ModDescriptor = ModifierDescriptor.UntypedStackable,
                Source = runtime.Fact,
                ModValue = Main.settings.ScalingSaveBonusPerLevel * unit.Progression.CharacterLevel
            };
        }

        public override void OnTurnOff()
        {
            RemoveModifiers(base.Owner, base.Runtime);
        }

        public override void OnTurnOn()
        {
            AddModifiers(base.Owner, base.Runtime);
        }
    }

    public class SaveModifier : ModifiableValue.Modifier
    {
    }
}
