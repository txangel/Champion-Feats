using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using ChampionFeats.Extensions;
using ChampionFeats.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.UnitLogic.Mechanics.Properties;
using Kingmaker.UnitLogic;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Designers.Mechanics.Facts;
using ChampionFeats.Utilities;
using ChampionFeats.Components;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Enums.Damage;
using Kingmaker.Designers.Mechanics.Recommendations;
using Kingmaker.EntitySystem.Stats;

namespace ChampionFeats
{
    class ChampionFeatsPatcher
    {
        [HarmonyPatch(typeof(BlueprintsCache), "Init")]
        public static class BlueprintPatcher
        {
            static bool Initialized;

            [HarmonyPriority(Priority.LowerThanNormal)]
            static void Postfix()
            {
                if (Initialized) return;
                Initialized = true;
                Main.Log("Adding Champion Feats");

                AddChampionDefences(); //int wis and cha
                AddChampionOffences();
                AddChampionMagics();
            }

            private static string getStepString(int step)
            {
                string stepString = "";
                switch (step)
                {
                    case 1:
                        stepString = "For each";
                        break;
                    case 2:
                        stepString = "At 2nd level, and every 2nd";
                        break;
                    case 3:
                        stepString = "At 3rd level, and every 3rd";
                        break;
                    default:
                        stepString = String.Format("At {0}th level, and every {0}th", step);
                        break;
                }
                return stepString;
            }

            private static string getStepStringWOffset(int step)
            {
                string stepString = "";
                switch (step)
                {
                    case 1:
                        stepString = "For each";
                        break;
                    case 2:
                        stepString = "At 3rd level, and every 2nd";
                        break;
                    case 3:
                        stepString = "At 4th level, and every 3rd";
                        break;
                    default:
                        stepString = String.Format("At {0}th level, and every {1}th", step, step - 1);
                        break;
                }
                return stepString;
            }

            private static ContextRankConfig.CustomProgressionItem[] makeCustomProgression(int levelsPerStep, int bonusPerStep, int levelStepOffset = 0)
            {
                // levelsPerStep must be within [1,40]
                if (levelsPerStep < 1)
                {
                    levelsPerStep = 1;
                }
                if (levelsPerStep > 40)
                {
                    levelsPerStep = 40;
                }
                // max level 40
                int steps = 40 / levelsPerStep + 1;

                ContextRankConfig.CustomProgressionItem[] items = new ContextRankConfig.CustomProgressionItem[steps];
                int bonus = bonusPerStep;
                int level = levelsPerStep - 1 + levelStepOffset;

                for (int step = 0; step < steps; step++)
                {
                    items[step] = new ContextRankConfig.CustomProgressionItem()
                    {
                        ProgressionValue = bonus,
                        BaseValue = level
                    };

                    bonus += bonusPerStep;
                    level += levelsPerStep;
                }

                items[steps - 1].BaseValue = 99;

                return items;
            }

            static void AddChampionDefences()
            {
                var ChampionDefenceAC = Helpers.CreateBlueprint<BlueprintFeature>("RMChampionFeatDefenceAC", bp => {
                    bp.IsClassFeature = true;
                    bp.ReapplyOnLevelUp = true;
                    if (!Main.settings.FeatsAreMythic)
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.CombatFeat, FeatureGroup.Feat };
                    }
                    else
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.MythicFeat };
                    }
                    bp.Ranks = 1;
                    bp.SetName("Champion Protection");
                    string stepString = getStepString(Main.settings.ScalingACLevelsPerStep);
                    bp.SetDescription(String.Format("Whether it's from practice or tutoring, your ability to defend yourself in armor surpasses most. You gain +{0}/+{1}/+{2} AC while wearing light/medium/heavy armor. {3} level beyond that, this bonus increases by the same increment.",
                        Main.settings.ScalingACArmorBonusLightPerStep, Main.settings.ScalingACArmorBonusMediumPerStep, Main.settings.ScalingACArmorBonusHeavyPerStep, stepString));
                    bp.m_DescriptionShort = bp.m_Description;
                    bp.AddComponent(Helpers.Create<AddACFromArmor>(c => {
                        c.LightBonus = Main.settings.ScalingACArmorBonusLightPerStep;
                        c.MediumBonus = Main.settings.ScalingACArmorBonusMediumPerStep;
                        c.HeavyBonus = Main.settings.ScalingACArmorBonusHeavyPerStep;
                        c.StepLevel = Main.settings.ScalingACLevelsPerStep;
                    }));


                });


                var ChampionDefenceDR = Helpers.CreateBlueprint<BlueprintFeature>("RMChampionFeatDefenceDR", bp => {
                    bp.IsClassFeature = true;
                    bp.ReapplyOnLevelUp = true;
                    if (!Main.settings.FeatsAreMythic)
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.CombatFeat, FeatureGroup.Feat };
                    }
                    else
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.MythicFeat };
                    }
                    bp.Ranks = 1;
                    bp.SetName("Champion Guard");
                    string stepString = getStepString(Main.settings.ScalingDRLevelsPerStep);
                    bp.SetDescription(String.Format("You stand firm and resist whatever physical harm comes for you, no matter what it is. You gain +{0} DR/-. {1} level beyond that, increases it by +{0}.",
                        Main.settings.ScalingDRBonusPerStep, stepString));
                    bp.m_DescriptionShort = bp.m_Description;
                    bp.AddComponent(Helpers.Create<AddDamageResistancePhysical>(c => {


                        c.name = "RMChampionGuardBuff";
                        c.Material = PhysicalDamageMaterial.Adamantite;
                        c.MinEnhancementBonus = 5;
                        
                        c.Alignment = DamageAlignment.Good;
                        c.Reality = DamageRealityType.Ghost;
                        c.Value = new ContextValue()
                        {
                            ValueType = ContextValueType.Rank,
                            // not sure about this
                            Value = Main.settings.ScalingDRBonusPerStep
                            
                        };


                        c.Pool = new ContextValue()
                        {
                            Value = 12
                        };

                    }));
 
                });

                var RankConfig = Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, ContextRankProgression.Custom, AbilityRankType.Default);

                var customProg = makeCustomProgression(Main.settings.ScalingDRLevelsPerStep, Main.settings.ScalingDRBonusPerStep);
              
                /*
                var customProgx = new ContextRankConfig.CustomProgressionItem[]
                {
                    // LESS THAN
                    // IT WORKS VIA LESS THAN OR EQUAL HERE
                     new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 5,
                           BaseValue = 4
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 10,
                           BaseValue = 9
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 15,
                           BaseValue = 14
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 20,
                           BaseValue = 19
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 25,
                           BaseValue = 24
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 30,
                           BaseValue = 29
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 35,
                           BaseValue = 34
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 40,
                           BaseValue = 39
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 45,
                           BaseValue = 99
                       }
                };
                */
                
                RankConfig.m_CustomProgression = customProg;
                ChampionDefenceDR.AddComponent(RankConfig);

                var ChampionDefenceSaves = Helpers.CreateBlueprint<BlueprintFeature>("RMChampionFeatSavingThrow", bp =>
                {
                    bp.IsClassFeature = true;
                    bp.ReapplyOnLevelUp = true;
                    if (!Main.settings.FeatsAreMythic)
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.CombatFeat, FeatureGroup.Feat };
                    }
                    else
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.MythicFeat };
                    }
                    bp.Ranks = 1;
                    bp.SetName("Champion Saves");
                    bp.SetDescription(string.Format("Your natural ability to avoid danger protects you from harm. You gain +{0} to all saving throws per level.", Main.settings.ScalingSaveBonusPerLevel));
                    bp.m_DescriptionShort = bp.m_Description;

                    bp.AddComponent<AddContextStatBonus>(bonus => MakeSavingThrowBonus(bonus, StatType.SaveFortitude));
                    bp.AddComponent<AddContextStatBonus>(bonus => MakeSavingThrowBonus(bonus, StatType.SaveReflex));
                    bp.AddComponent<AddContextStatBonus>(bonus => MakeSavingThrowBonus(bonus, StatType.SaveWill));

                    bp.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, ContextRankProgression.AsIs, AbilityRankType.Default, null, null, 1, 1));
                });

                if (!Main.settings.FeatsAreMythic)
                {
                    FeatTools.AddAsFeat(ChampionDefenceAC);
                    FeatTools.AddAsFeat(ChampionDefenceDR);
                    FeatTools.AddAsFeat(ChampionDefenceSaves);
                }
                else
                {
                    FeatTools.AddAsMythicFeats(ChampionDefenceAC);
                    FeatTools.AddAsMythicFeats(ChampionDefenceDR);
                    FeatTools.AddAsMythicFeats(ChampionDefenceSaves);
                }

            }

            private static void MakeSavingThrowBonus(AddContextStatBonus bonus, StatType statType)
            {
                bonus.Descriptor = ModifierDescriptor.UntypedStackable;
                bonus.Stat = statType;
                bonus.Multiplier = Main.settings.ScalingSaveBonusPerLevel;
                bonus.Value = new ContextValue()
                {
                    ValueType = ContextValueType.Rank,
                    ValueRank = AbilityRankType.Default
                };
            }

            static void AddChampionOffences()
            {
                var ChampionOffenceAB = Helpers.CreateBlueprint<BlueprintFeature>("RMChampionFeatOffenceAim", bp => {
                    bp.IsClassFeature = true;
                    bp.ReapplyOnLevelUp = true;
                    if (!Main.settings.FeatsAreMythic)
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.CombatFeat, FeatureGroup.Feat };
                    }
                    else
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.MythicFeat };
                    }
                    bp.Ranks = 1;
                    bp.SetName("Champion Aim");
                    String stepString = getStepStringWOffset(Main.settings.ScalingABLevelsPerStep);
                    bp.SetDescription(String.Format("Whether it's from practice or tutoring, your ability to hit targets surpasses most. You gain +{0} attack bonus. {1} level beyond that, increases it by +{0}.",
                        Main.settings.ScalingABBonusPerStep, stepString));
                    bp.m_DescriptionShort = bp.m_Description;
                    bp.AddComponent(Helpers.Create<AddScalingAttackBonus>(c => {
                        c.value = new ContextValue()
                        {
                            Value = 1
                        };
                        c.Bonus = new ContextValue()
                        {
                            ValueType = ContextValueType.Rank
                        };
                        c.Descriptor = ModifierDescriptor.UntypedStackable;
                        
                  //      bp.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, ContextRankProgression.StartPlusDivStep, AbilityRankType.Default, null, null, 2, 2));
                    }));

                });

                /*
                var RankConfig = Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, ContextRankProgression.Custom, AbilityRankType.Default);
                var customProg = new ContextRankConfig.CustomProgressionItem[]
                {
                     new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 1,
                           BaseValue = 2
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 2,
                           BaseValue = 4
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 3,
                           BaseValue = 6
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 4,
                           BaseValue = 8
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 5,
                           BaseValue = 10
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 7,
                           BaseValue = 12
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 8,
                           BaseValue = 14
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 9,
                           BaseValue = 16
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 10,
                           BaseValue = 18
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 11,
                           BaseValue = 20
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 12,
                           BaseValue = 22
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 13,
                           BaseValue = 24
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 14,
                           BaseValue = 26
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 15,
                           BaseValue = 28
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 17,
                           BaseValue = 30
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 19,
                           BaseValue = 32
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 21,
                           BaseValue = 34
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 22,
                           BaseValue = 36
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 23,
                           BaseValue = 38
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 24,
                           BaseValue = 99
                       }
                };
                RankConfig.m_CustomProgression = customProg;
                ChampionOffenceAB.AddComponent(RankConfig);
                */


                var ChampionOffenceDam = Helpers.CreateBlueprint<BlueprintFeature>("RMChampionFeatOffenceDam", bp => {
                    bp.IsClassFeature = true;
                    bp.ReapplyOnLevelUp = true;
                    if (!Main.settings.FeatsAreMythic)
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.CombatFeat, FeatureGroup.Feat };

                    }
                    else
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.MythicFeat };
                    }
                    bp.Ranks = 1;
                    bp.SetName("Champion Strikes");
                    String stepString = getStepStringWOffset(Main.settings.ScalingDamageLevelsPerStep);
                    bp.SetDescription(String.Format("Your weapon attacks strike hard, no matter how tough the foe. You gain +{0} damage to attacks. {1} level beyond that, increases it by +{0}.",
                        Main.settings.ScalingDamageBonusPerStep, stepString));
                    bp.m_DescriptionShort = bp.m_Description;
                    bp.AddComponent(Helpers.Create<AddScalingDamageBonus>(c => {
                        c.value = new ContextValue()
                        {
                            Value = Main.settings.ScalingDamageBonusPerStep
                        };
                        c.Bonus = new ContextValue()
                        {
                            ValueType = ContextValueType.Rank
                        };
                    }));
                    bp.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, ContextRankProgression.StartPlusDivStep, AbilityRankType.Default, null, null, 1 + Main.settings.ScalingDamageLevelsPerStep, Main.settings.ScalingDamageLevelsPerStep));
                });

                if (!Main.settings.FeatsAreMythic)
                {
                    FeatTools.AddAsFeat(ChampionOffenceAB);
                    FeatTools.AddAsFeat(ChampionOffenceDam);
                }
                else
                {
                    FeatTools.AddAsMythicFeats(ChampionOffenceAB);
                    FeatTools.AddAsMythicFeats(ChampionOffenceDam);
                }
            }
            static void AddChampionMagics()
            {
                var SpellPen = Resources.GetBlueprint<BlueprintFeature>("ee7dc126939e4d9438357fbd5980d459");

                var ChampionOffenceSpellDam = Helpers.CreateBlueprint<BlueprintFeature>("RMChampionFeatOffenceSpellDam", bp => {
                    bp.IsClassFeature = true;
                    bp.ReapplyOnLevelUp = true;
                    if (!Main.settings.FeatsAreMythic)
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.WizardFeat, FeatureGroup.Feat };
                    }
                    else
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.MythicFeat };
                    }
                    bp.Ranks = 1;
                    bp.SetName("Champion Spell Blasts");
                    String stepString = getStepStringWOffset(Main.settings.ScalingSpellDamageLevelsPerStep);
                    bp.SetDescription(String.Format("Your magical arts strike hard, no matter how tough the foe. You gain +{0} damage to spell attacks per damage die. {1} level beyond that, increases it by +{0}.",
                        Main.settings.ScalingSpellDamageBonusPerStep, stepString));
                    bp.m_DescriptionShort = bp.m_Description;
                    bp.AddComponent(Helpers.Create<AddScalingSpellDamage>(c => {
                        c.Value = new ContextValue()
                        {
                            Value = Main.settings.ScalingSpellDamageBonusPerStep,
                            ValueType = ContextValueType.Rank
                        };
                    }));
                    bp.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, ContextRankProgression.StartPlusDivStep, AbilityRankType.Default, null, null, 1 + Main.settings.ScalingSpellDamageLevelsPerStep, Main.settings.ScalingSpellDamageLevelsPerStep));
                    bp.AddComponent(Helpers.Create<RecommendationRequiresSpellbook>());
                    bp.AddComponent(Helpers.Create<FeatureTagsComponent>(c => {
                        c.FeatureTags = FeatureTag.Magic;
                    }));
                });

                var ChampionOffenceSpellDC = Helpers.CreateBlueprint<BlueprintFeature>("RMChampionFeatOffenceSpellDC", bp => {
                    bp.IsClassFeature = true;
                    bp.ReapplyOnLevelUp = true;
                    if (!Main.settings.FeatsAreMythic)
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.WizardFeat, FeatureGroup.Feat };
                    }
                    else
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.MythicFeat };
                    }
                    bp.Ranks = 1;
                    bp.SetName("Champion Spell Force");
                    String stepString = getStepStringWOffset(Main.settings.ScalingSpellDCLevelsPerStep);
                    bp.SetDescription(String.Format("Your magical arts are overwhelming for enemies to deal with. You gain +{0} to the DC of your spells. {1} level beyond that, increases it by +{0}.",
                        Main.settings.ScalingSpellDCBonusPerStep, stepString));
                    bp.m_DescriptionShort = bp.m_Description;
                    bp.AddComponent(Helpers.Create<AddScalingSpellDC>(c => {
                        c.Value = new ContextValue()
                        {
                            Value = 1,
                            ValueType = ContextValueType.Rank
                        };
                    }));
                    bp.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, ContextRankProgression.AsIs, AbilityRankType.Default));
                    bp.AddComponent(Helpers.Create<RecommendationRequiresSpellbook>());
                    bp.AddComponent(Helpers.Create<FeatureTagsComponent>(c => {
                        c.FeatureTags = FeatureTag.Magic;
                    }));
                });

                var ChampionOffenceSpellPen = Helpers.CreateBlueprint<BlueprintFeature>("RMChampionFeatOffenceSpellPen", bp => {
                    bp.IsClassFeature = true;
                    bp.ReapplyOnLevelUp = true;
                    if (!Main.settings.FeatsAreMythic)
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.WizardFeat, FeatureGroup.Feat };
                    }
                    else
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.MythicFeat };
                    }
                    bp.Ranks = 1;
                    bp.SetName("Champion Spell Penetration");
                    bp.SetDescription(String.Format("Your magical arts are trained to pierce even the thickest of protections. Half of +{0} per character level (minimum of +1) is added as bonus spell penetration. If you have Spell Penetration, it's +{0} per character level instead.",
                        Main.settings.ScalingSpellPenBonusPerLevel));
                    bp.m_DescriptionShort = bp.m_Description;
                    bp.AddComponent(Helpers.Create<AddScalingSpellPenetration>(c => {
                        c.Value = new ContextValue()
                        {
                            Value = 1,
                            ValueType = ContextValueType.Rank
                        };
                        c.m_SpellPen = SpellPen.ToReference<BlueprintUnitFactReference>();
                    }));
                    bp.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, ContextRankProgression.AsIs, AbilityRankType.Default));
                    bp.AddComponent(Helpers.Create<RecommendationRequiresSpellbook>());
                    bp.AddComponent(Helpers.Create<FeatureTagsComponent>(c => {
                        c.FeatureTags = FeatureTag.Magic;
                    }));
                });

                if (!Main.settings.FeatsAreMythic)
                {
                    FeatTools.AddAsFeat(ChampionOffenceSpellPen);
                    FeatTools.AddAsFeat(ChampionOffenceSpellDC);
                    FeatTools.AddAsFeat(ChampionOffenceSpellDam);
                }
                else
                {
                    FeatTools.AddAsMythicFeats(ChampionOffenceSpellPen);
                    FeatTools.AddAsMythicFeats(ChampionOffenceSpellDC);
                    FeatTools.AddAsMythicFeats(ChampionOffenceSpellDam);
                }

            }
        }
    }
}
