using EFT;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Core.SP.PlayerPatches.Health
{
    public class HealthControllerHelpers
    {
        public static Type GetDamageInfoType()
        {
            return PatchConstants.EftTypes.Single(
                x =>
                ReflectionHelpers.GetAllMethodsForType(x).Any(y => y.Name == "GetOverDamage")
                );
        }

        public static DamageInfo ReadyMadeDamageInstance;

        public static DamageInfo CreateDamageInfoTypeFromDict(Dictionary<string, object> dict)
        {
            ReadyMadeDamageInstance = new DamageInfo();
            ReflectionHelpers.GetFieldFromType(ReadyMadeDamageInstance.GetType(), "Damage").SetValue(ReadyMadeDamageInstance, float.Parse(dict["damage"].ToString()));
            ReflectionHelpers.GetFieldFromType(ReadyMadeDamageInstance.GetType(), "DamageType").SetValue(ReadyMadeDamageInstance, Enum.Parse(typeof(EDamageType), dict["damageType"].ToString()));
            ReflectionHelpers.GetFieldFromType(ReadyMadeDamageInstance.GetType(), "ArmorDamage").SetValue(ReadyMadeDamageInstance, float.Parse(dict["armorDamage"].ToString()));
            ReflectionHelpers.GetFieldFromType(ReadyMadeDamageInstance.GetType(), "DidArmorDamage").SetValue(ReadyMadeDamageInstance, float.Parse(dict["didArmorDamage"].ToString()));
            ReflectionHelpers.GetFieldFromType(ReadyMadeDamageInstance.GetType(), "DidBodyDamage").SetValue(ReadyMadeDamageInstance, float.Parse(dict["didBodyDamage"].ToString()));
            return ReadyMadeDamageInstance;
        }

        public static MethodInfo GetHealthControllerChangeHealthMethod(object healthController)
        {
            return healthController.GetType()
                .GetMethod("ChangeHealth", BindingFlags.Public | BindingFlags.Instance);
        }

        public static void ChangeHealth(object healthController, EBodyPart bodyPart, float value, object damageInfo)
        {
            GetHealthControllerChangeHealthMethod(healthController).Invoke(healthController, new object[] { bodyPart, value, damageInfo });
        }

        public static object GetActiveHealthController(object player)
        {
            object activeHealthController = ReflectionHelpers.GetFieldOrPropertyFromInstance<object>(player, "ActiveHealthController", false);
            return activeHealthController;
        }

        public static bool IsAlive(object healthController)
        {
            bool isAlive = ReflectionHelpers.GetFieldOrPropertyFromInstance<bool>(healthController, "IsAlive", false);
            return isAlive;
        }

        /// <summary>
        /// Gets the Body Part Health Value struct for provided health controller
        /// </summary>
        /// <param name="healthController"></param>
        /// <param name="bodyPart"></param>
        /// <returns></returns>
        public static EFT.HealthSystem.ValueStruct GetBodyPartHealth(object healthController, EBodyPart bodyPart)
        {
            var getbodyparthealthmethod = healthController.GetType().GetMethod("GetBodyPartHealth"
                , BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.FlattenHierarchy
                );
            if (getbodyparthealthmethod == null)
            {
                PatchConstants.Logger.LogInfo("HealthListener:GetBodyPartHealth not found!");
                return new EFT.HealthSystem.ValueStruct();
            }

            //PatchConstants.Logger.LogInfo("GetBodyPartHealth found!");

            return (EFT.HealthSystem.ValueStruct)getbodyparthealthmethod.Invoke(healthController, new object[] { bodyPart, false });
        }
    }
}
