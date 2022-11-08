using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ChampionFeats.Components
{
    [AllowedOn(typeof(BlueprintBuff))]
    [AllowedOn(typeof(BlueprintFeature))]
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [AllowMultipleComponents]
    [TypeId("9804852F50534445A3A51769CC00DD80")]
    public class AddScalingSkillBonus : UnitFactComponentDelegate
    {
        public const string BLUEPRINTNAME = "RMChampionFeatSkills";

        public static void OnSettingsSave()
        {
            UpdateSkillBonuses();
        }

        private static void UpdateSkillBonuses()
        {
            foreach (var unit in Game.Instance.State.Units)
            {
                Feature feature = unit.GetFeature(Resources.GetModBlueprint<BlueprintFeature>(BLUEPRINTNAME));
                if (feature != null)
                {
                    feature.TurnOff();
                    feature.TurnOn();
                }
            }
        }

        private static void RemoveSkillBonus(UnitEntityData unit, StatType skillType)
        {
            SkillModifier modToRemove = null;
            foreach (var list in unit.Stats.GetStat(skillType).ModifierList)
            {
                foreach (var mod in list.Value)
                {
                    if (mod is SkillModifier)
                    {
                        modToRemove = (SkillModifier)mod;
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
                unit.Stats.GetStat(skillType).RemoveModifier(modToRemove);
            }
        }

        private static void RemoveSkillBonuses(UnitEntityData unit, EntityFactComponent runtime)
        {
            foreach (StatType type in StatTypeHelper.Skills)
            {
                RemoveSkillBonus(unit, type);
            }
        }

        private static void AddSkillBonuses(UnitEntityData unit, EntityFactComponent runtime)
        {
            foreach (StatType type in StatTypeHelper.Skills)
            {
                unit.Stats.GetStat<ModifiableValue>(type).AddModifier(GetNewModifier(unit, runtime));
            }
        }

        private static ModifiableValue.Modifier GetNewModifier(UnitEntityData unit, EntityFactComponent runtime)
        {
            return new SkillModifier()
            {
                ModDescriptor = Kingmaker.Enums.ModifierDescriptor.UntypedStackable,
                Source = runtime.Fact,
                ModValue = (((unit.Progression.CharacterLevel - 1) / Main.settings.ScalingSkillsLevelsPerStep) + 1) * Main.settings.ScalingSkillsBonusPerLevel
            };
        }

        public override void OnTurnOff()
        {
            RemoveSkillBonuses(base.Owner, base.Runtime);
        }

        public override void OnTurnOn()
        {
            AddSkillBonuses(base.Owner, base.Runtime);
        }
    }

    public class SkillModifier : ModifiableValue.Modifier
    {
    }
}
