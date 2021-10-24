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
                    bp.SetDescription("Whether it's from practice or tutoring, your ability to defend yourself in armor surpasses most. You gain +1/+2/+3 AC while wearing light/medium/heavy armor. At every fifth character level beyond that, this bonus doubles.");
                    bp.m_DescriptionShort = bp.m_Description;
                    bp.AddComponent(Helpers.Create<AddACFromArmor>(c => {
                        c.LightBonus = 1;
                        c.MediumBonus = 2;
                        c.HeavyBonus = 3;
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
                    bp.SetDescription("You stand firm and resist whatever physical harm comes for you, no matter what it is. You gain +5 DR/-, and every 5th level increases it by +5.");
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
                            Value = 5
                            
                        };


                        c.Pool = new ContextValue()
                        {
                            Value = 12
                        };

                    }));
 
                });

                var RankConfig = Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, ContextRankProgression.Custom, AbilityRankType.Default, 5, null, 0, 5);
                RankConfig.m_UseMin = true;
                var customProg = new ContextRankConfig.CustomProgressionItem[]
                {
                     new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 5,
                           BaseValue = 0
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 10,
                           BaseValue = 1
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 15,
                           BaseValue = 2
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 20,
                           BaseValue = 3
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 25,
                           BaseValue = 4
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 30,
                           BaseValue = 5
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 35,
                           BaseValue = 6
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 40,
                           BaseValue = 7
                       },
                       new ContextRankConfig.CustomProgressionItem{
                           ProgressionValue = 45,
                           BaseValue = 8
                       }
                };
                RankConfig.m_CustomProgression = customProg;
                ChampionDefenceDR.AddComponent(RankConfig);
                if (!Main.settings.FeatsAreMythic)
                {
                    FeatTools.AddAsFeat(ChampionDefenceAC);
                    FeatTools.AddAsFeat(ChampionDefenceDR);
                }
                else
                {
                    FeatTools.AddAsMythicFeats(ChampionDefenceAC);
                    FeatTools.AddAsMythicFeats(ChampionDefenceDR);
                }

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
                    bp.SetDescription("Whether it's from practice or tutoring, your ability to hit targets surpasses most. You gain +1 attack bonus, and that bonus increases every two levels starting at level 3.");
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
                        
                        bp.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, ContextRankProgression.StartPlusDivStep, AbilityRankType.Default, null, null, 3, 2));
                    }));


                });


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
                    bp.SetDescription("Your weapon attacks strike hard, no matter how tough the foe. You gain +1 damage to attacks, with an extra +1 every second level starting at level 3.");
                    bp.m_DescriptionShort = bp.m_Description;
                    bp.AddComponent(Helpers.Create<AddScalingDamageBonus>(c => {
                        c.value = new ContextValue()
                        {
                            Value = 1
                        };
                        c.Bonus = new ContextValue()
                        {
                            ValueType = ContextValueType.Rank
                        };
                    }));
                    bp.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, ContextRankProgression.StartPlusDivStep, AbilityRankType.Default, null, null, 3, 2));
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
                    bp.SetDescription("Your magical arts strike hard, no matter how tough the foe. You gain +1 damage to spell attacks per damage die, with an extra +1 every third character level starting at character level 4.");
                    bp.m_DescriptionShort = bp.m_Description;
                    bp.AddComponent(Helpers.Create<AddScalingSpellDamage>(c => {
                        c.Value = new ContextValue()
                        {
                            Value = 1,
                            ValueType = ContextValueType.Rank
                        };
                    }));
                    bp.AddComponent(Helpers.CreateContextRankConfig(ContextRankBaseValueType.CharacterLevel, ContextRankProgression.StartPlusDivStep, AbilityRankType.Default, null, null, 4, 3));
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
                    bp.SetDescription("Your magical arts are overwhelming for enemies to deal with. You gain +1 to the DC of your spells, with an extra +1 for every character level.");
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
                    bp.SetDescription("Your magical arts are trained to pierce even the thickest of protections. Half your character level (or +1 at the lowest) is added as bonus spell penetration. If you have Spell Penetration, it's full character level instead.");
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
