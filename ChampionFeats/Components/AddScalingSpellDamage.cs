﻿using System;
using System.Linq;
using ChampionFeats.Config;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;
using UnityEngine.Serialization;

namespace ChampionFeats.Components
{
    [AllowedOn(typeof(BlueprintUnitFact), false)]
    [TypeId("34f01b4e062075c408523fbf92871dc5")]
    class AddScalingSpellDamage : UnitBuffComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IInitiatorRulebookSubscriber
    {
        public const string BLUEPRINTNAME = "RMChampionFeatOffenceSpellDam";

        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            if (evt.Reason.Ability == null)
            {
                return;
            }
            if (evt.Reason.Context == null || !evt.Reason.Ability.Blueprint.IsSpell)
            {
                return;
            }
            if (Blueprints.HasNPCImmortalityBuff(Fact.Owner))
            {
                return;
            }

            foreach (BaseDamage baseDamage in evt.DamageBundle)
            {
                if(baseDamage.Dice.BaseFormula.Dice == Kingmaker.RuleSystem.DiceType.Zero) // trying to account for getting spells that shouldn't be damaging, apparently?
                {
                    continue;
                }
                int calcBonus = (((this.Fact.Owner.Progression.CharacterLevel - 1) / Main.settings.ScalingSpellDamageLevelsPerStep) + 1) * Main.settings.ScalingSpellDamageBonusPerStep;
                int bonus = calcBonus * baseDamage.Dice.BaseFormula.m_Rolls;
                baseDamage.AddModifier(Math.Max(1, bonus), Fact);
            }
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
        }
    }
}
