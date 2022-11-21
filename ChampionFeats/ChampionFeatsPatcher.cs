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

            public static ContextRankConfig.CustomProgressionItem[] makeCustomProgression(int levelsPerStep, int bonusPerStep, int levelStepOffset = 0)
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
                int level = levelsPerStep + levelStepOffset;

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
                var ChampionDefenceAC = Helpers.CreateBlueprint<BlueprintFeature>(AddACFromArmor.BLUEPRINTNAME, bp => {
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

                    bp.AddComponent(Helpers.Create<AddMechanicsFeature>(amf =>
                    {
                        amf.m_Feature = AddMechanicsFeature.MechanicsFeatureType.ImmunityToArmorSpeedPenalty;
                    }));


                });


                var ChampionDefenceDR = Helpers.CreateBlueprint<BlueprintFeature>(AddScalingDamageResistance.BLUEPRINTNAME, bp => {
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

                    bp.AddComponent(Helpers.Create<AddDamageResistancePhysical>(c =>
                    {
                        c.name = "RMChampionGuardBuff";
                        c.MinEnhancementBonus = 5;
                        // I apologize for the brute force
                        c.Value = new ContextValueDR()
                        {
                            ValueType = ContextValueType.Simple,
                            // not sure about this
                            //Value = Main.settings.ScalingDRBonusPerStep
                            Value = 1
                        };
                    }));

                });


                var ChampionDefenceSaves = Helpers.CreateBlueprint<BlueprintFeature>(AddScalingSavingThrows.BLUEPRINTNAME, bp =>
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
                    string stepString = getStepString(Main.settings.ScalingSaveLevelsPerStep);
                    bp.SetDescription(string.Format("Your natural ability to avoid danger protects you from harm. You gain +{0} to all saving throws. {1} level beyond that, increases it by +{0}.",
                        Main.settings.ScalingSaveBonusPerLevel, stepString));
                    bp.m_DescriptionShort = bp.m_Description;

                    bp.AddComponent(Helpers.Create<AddScalingSavingThrows>());
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

            static void AddChampionOffences()
            {
                var ChampionSkills = Helpers.CreateBlueprint<BlueprintFeature>(AddScalingSkillBonus.BLUEPRINTNAME, bp =>
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
                    bp.SetName("Champion Skills");
                    string stepString = getStepString(Main.settings.ScalingSkillsLevelsPerStep);
                    string descString = string.Format("Your prowess knows no boundaries, picking up new skills at an inexplicable pace. You gain +{0} to all skills. {1} level beyond that, this bonus increases by +{0}.",
                        Main.settings.ScalingSkillsBonusPerLevel, stepString);
                    if (AddScalingSkillBonus.AddToBaseValue)
                    {
                        descString += " Bonuses from this feat will be applied directly as skill ranks and will not appear separately as a bonus during skill checks.";
                    }
                    bp.SetDescription(descString);
                    bp.m_DescriptionShort = bp.m_Description;

                    bp.AddComponent(Helpers.Create<AddScalingSkillBonus>());
                    bp.AddComponent(Helpers.Create<AddIdentifyBonus>(ib =>
                    {
                        ib.AllowUsingUntrainedSkill = true;
                    }));
                    bp.AddComponent(Helpers.Create<AddMechanicsFeature>(mf =>
                    {
                        mf.m_Feature = AddMechanicsFeature.MechanicsFeatureType.MakeKnowledgeCheckUntrained;
                    }));
                });

                var ChampionOffenceAB = Helpers.CreateBlueprint<BlueprintFeature>(AddScalingAttackBonus.BLUEPRINTNAME, bp => {
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
                    bp.AddComponent(Helpers.Create<AddScalingAttackBonus>(c => {}));

                });


                var ChampionOffenceDam = Helpers.CreateBlueprint<BlueprintFeature>(AddScalingDamageBonus.BLUEPRINTNAME, bp => {
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
                    bp.AddComponent(Helpers.Create<AddScalingDamageBonus>(c => {}));
                });

                if (!Main.settings.FeatsAreMythic)
                {
                    FeatTools.AddAsFeat(ChampionSkills);
                    FeatTools.AddAsFeat(ChampionOffenceAB);
                    FeatTools.AddAsFeat(ChampionOffenceDam);
                }
                else
                {
                    FeatTools.AddAsMythicFeats(ChampionSkills);
                    FeatTools.AddAsMythicFeats(ChampionOffenceAB);
                    FeatTools.AddAsMythicFeats(ChampionOffenceDam);
                }
            }
            static void AddChampionMagics()
            {
                var SpellPen = Resources.GetBlueprint<BlueprintFeature>("ee7dc126939e4d9438357fbd5980d459");

                var ChampionOffenceSpellDam = Helpers.CreateBlueprint<BlueprintFeature>(AddScalingSpellDamage.BLUEPRINTNAME, bp => {
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
                    bp.AddComponent(Helpers.Create<AddScalingSpellDamage>(c => {}));
                    bp.AddComponent(Helpers.Create<RecommendationRequiresSpellbook>());
                    bp.AddComponent(Helpers.Create<FeatureTagsComponent>(c => {
                        c.FeatureTags = FeatureTag.Magic;
                    }));
                });

                var ChampionOffenceSpellDC = Helpers.CreateBlueprint<BlueprintFeature>(AddScalingSpellDC.BLUEPRINTNAME, bp => {
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
                    bp.AddComponent(Helpers.Create<AddScalingSpellDC>(c => {}));
                    bp.AddComponent(Helpers.Create<RecommendationRequiresSpellbook>());
                    bp.AddComponent(Helpers.Create<FeatureTagsComponent>(c => {
                        c.FeatureTags = FeatureTag.Magic;
                    }));
                });

                var ChampionOffenceSpellPen = Helpers.CreateBlueprint<BlueprintFeature>(AddScalingSpellPenetration.BLUEPRINTNAME, bp => {
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
                        c.m_SpellPen = SpellPen.ToReference<BlueprintUnitFactReference>();
                    }));
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
