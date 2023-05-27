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

        /*
         * Why this? Because I want the bonuses to help handle skill checks that are untrained or that have
         * skill rank checks that ignore bonuses. By applying directly to the skill base value, skills
         * will appear trained and will have significantly improved ranks to meet any rank requirements.
         */
        public static bool AddToBaseValue
        {
            get
            {
                return Main.settings.AddAsSkillRanks;
            }
        }

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
                if (AddToBaseValue)
                {
                    unit.Stats.GetStat(skillType).BaseValue -= modToRemove.AdditionalBaseValue; // because reasons
                }
                else
                {
                    unit.Stats.GetStat(skillType).RemoveModifier(modToRemove);
                }
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
                SkillModifier skillModifier = GetNewModifier(unit, runtime);
                unit.Stats.GetStat<ModifiableValue>(type).AddModifier(skillModifier);
                if (AddToBaseValue)
                {
                    unit.Stats.GetStat<ModifiableValue>(type).BaseValue += skillModifier.AdditionalBaseValue; // because reasons
                }
            }
        }

        private static SkillModifier GetNewModifier(UnitEntityData unit, EntityFactComponent runtime)
        {
            if (AddToBaseValue)
            {
                return new SkillModifier()
                {
                    ModValue = 0, // bear with me
                    AdditionalBaseValue = (((unit.Progression.CharacterLevel - 1) / Main.settings.ScalingSkillsLevelsPerStep) + 1) * Main.settings.ScalingSkillsBonusPerLevel,
                    StackMode = ModifiableValue.StackMode.ForceStack,
                    ModDescriptor = Kingmaker.Enums.ModifierDescriptor.UntypedStackable,
                    Source = runtime.Fact
                };
            }
            else
            {
                return new SkillModifier()
                {
                    ModValue = (((unit.Progression.CharacterLevel - 1) / Main.settings.ScalingSkillsLevelsPerStep) + 1) * Main.settings.ScalingSkillsBonusPerLevel,
                    StackMode = ModifiableValue.StackMode.ForceStack,
                    ModDescriptor = Kingmaker.Enums.ModifierDescriptor.UntypedStackable,
                    Source = runtime.Fact
                };
            }
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
        public int AdditionalBaseValue;
    }
}
