using ChampionFeats.Config;
using HarmonyLib;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace ChampionFeats.Components
{
    /*
     * Butchery of the highest order here...
     * 
     * ... I'm sure there is a saner way of doing this. Essentially, during the Act 1 fight with Minagho, the PC has to be able to take sufficient
     * damage to drop below 10% health in order to trigger the completion of the combat and the subsequent cutscene to move on.
     * 
     * I tried a number of things:
     * - Subclass AddDamageResistancePhysical and override CalculateValue - TableTop Tweaks, however, when using the Damage Resistance rule, uses its
     *   own DR classes and refuses to recognize any Vanilla override that it doesn't already have a mapping for, and I don't want to have a dependency, soft
     *   or otherwise, on that mod
     * - TurnOff/TurnOn the feature at start and finish of the scene - Couldn't get it to work, and I think this is a failure on my part. It's probably the 
     *   most appropriate approach, but I was only ever able to disable it and never could get it to consistently reenable
     * - Subclass ContextValue and override Calculate - close to what you see here, but the base class methods are not virtual, and so my overrides ("new" actually)
     *   wouldn't get called.
     * - Harmony patch ContextValue, detect if being called from ContextValueDR, and behave appropriately - basically, I used Harmony to force Calculate
     *   and it's ilk to behave like virtuals. It's icky because now my code is being called on ContextValue.Calculate for EVERYTHING, adding, at minimum,
     *   the type check to every such call, nevermind the additional plumbing to add the patch logic in. But it does work, and provides the added
     *   benefit of dynamically returning the appropriate value in response to settings changes without having to do anything else.
     */
    [Serializable]
    public class ContextValueDR : ContextValue
    {
        public static int GetDRValue(UnitEntityData caster)
        {
            if (caster == null)
            {
                return 0;
            }
            if (Blueprints.HasNPCImmortalityBuff(caster))
            {
                return 0;
            }
            return (((caster.Progression.CharacterLevel - 1) / Main.settings.ScalingDRLevelsPerStep) + 1) * Main.settings.ScalingDRBonusPerStep;
        }
    }

    [HarmonyPatch(typeof(ContextValue), nameof(ContextValue.Calculate), new Type[] { typeof(MechanicsContext) })]
    class ContextValue_Patch_Calculate1
    {
        static void Postfix(ContextValue __instance, ref int __result, MechanicsContext context)
        {
            if (__instance is ContextValueDR)
            {
                UnitEntityData _caster = context?.MaybeCaster;
                if (_caster != null)
                {
                    __result = ContextValueDR.GetDRValue(_caster);
                }
            }
        }
    }

    [HarmonyPatch(typeof(ContextValue), nameof(ContextValue.Calculate), new Type[] { typeof(BlueprintScriptableObject), typeof(UnitEntityData) })]
    class ContextValue_Patch_Calculate2
    {
        static void Postfix(ContextValue __instance, ref int __result, UnitEntityData caster)
        {
            if (__instance is ContextValueDR)
            {
                if (caster != null)
                {
                    __result = ContextValueDR.GetDRValue(caster);
                }
            }
        }
    }

    [HarmonyPatch(typeof(ContextValue), nameof(ContextValue.Calculate), new Type[] { typeof(MechanicsContext), typeof(BlueprintScriptableObject), typeof(UnitEntityData) })]
    class ContextValue_Patch_Calculate3
    {
        static void Postfix(ContextValue __instance, ref int __result, MechanicsContext context, UnitEntityData caster)
        {
            if (__instance is ContextValueDR)
            {
                UnitEntityData _caster = context?.MaybeCaster ?? caster;
                if (_caster != null)
                {
                    __result = ContextValueDR.GetDRValue(_caster);
                }
            }
        }
    }
}
