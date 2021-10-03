using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using System;
using Kingmaker.Blueprints.Items.Armors;
namespace ChampionFeats.Components
{

    [TypeId("b8379d7a188f4ef997d636479aba8a2c")]
    class AddACFromArmor : UnitFactComponentDelegate,
           IUnitActiveEquipmentSetHandler,
           IGlobalSubscriber,
           ISubscriber,
           IUnitEquipmentHandler,
           IUnitBuffHandler
    {

        public override void OnTurnOn()
        {
            base.OnTurnOn();
            UpdateModifier();
        }

        public override void OnTurnOff()
        {
            base.OnTurnOff();
            DeactivateModifier();
        }

        public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
        {
            if (slot.Owner != Owner)
            {
                return;
            }
            UpdateModifier();
        }

        public void HandleUnitChangeActiveEquipmentSet(UnitDescriptor unit)
        {
            UpdateModifier();
        }

        private void UpdateModifier()
        {
            DeactivateModifier();
            ActivateModifier();
        }

        private void ActivateModifier()
        {
            if (Owner.Body.Armor.HasArmor && Owner.Body.Armor.Armor.Blueprint.IsArmor)
            {
                Owner.Stats.AC.AddModifierUnique(CalculateModifier(), base.Runtime, ModifierDescriptor.UntypedStackable);
            }
        }

        private void DeactivateModifier()
        {
            Owner.Stats.AC.RemoveModifiersFrom(base.Runtime);
        }

        private int CalculateModifier()
        {
            int itemBonus;
            switch (Owner.Body.Armor.Armor.Blueprint.ProficiencyGroup)
            {
                case ArmorProficiencyGroup.Light:
                case ArmorProficiencyGroup.LightBarding:
                    itemBonus = LightBonus;
                    break;
                case ArmorProficiencyGroup.Medium:
                case ArmorProficiencyGroup.MediumBarding:
                    itemBonus = MediumBonus;
                    break;
                case ArmorProficiencyGroup.Heavy:
                case ArmorProficiencyGroup.HeavyBarding:
                    itemBonus = HeavyBonus;
                    break;
                default:
                    itemBonus = 0;
                    break;
            }

            itemBonus *= Math.Max(1 + (Fact.Owner.Progression.CharacterLevel / 5), 1);

            return itemBonus;
        }

        public void HandleBuffDidAdded(Buff buff)
        {
            UpdateModifier();
        }

        public void HandleBuffDidRemoved(Buff buff)
        {
            UpdateModifier();
        }

        public int LightBonus;
        public int MediumBonus;
        public int HeavyBonus;
        public int StepLevel = 5;
    }
}
