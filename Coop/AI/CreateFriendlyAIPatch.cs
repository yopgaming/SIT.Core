using SIT.Core.Misc;
using System.Reflection;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.AI
{
    /// <summary>
    /// A Stay in Tarkov feature to create some friendly team AI
    /// </summary>
    public class CreateFriendlyAIPatch : ModulePatch
    {
        public static bool? ShouldFriendlyAI = null;
        public static int? NumberOfFriendlies = 0;
        public static int? MaxNumberOfFriendlies = 1;

        public static EFT.LocalPlayer MyPlayer;
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(EFT.LocalPlayer), "Init");
        }

        public CreateFriendlyAIPatch()
        {
            //ShouldFriendlyAI = JsonConvert.DeserializeObject<bool>(Request.Instance.PostJson("/client/raid/createFriendlyAI", null, true));
        }

        [PatchPostfix]
        public static
            async
            void
            PatchPostfix(Task __result, EFT.LocalPlayer __instance)
        {
            await __result;

            Logger.LogInfo("CreateFriendlyAIPatch.PatchPostfix");

            //    //if (!ShouldFriendlyAI.HasValue)
            //    //{
            //    //    var result = Request.Instance.PostJson("/client/raid/createFriendlyAI", JsonConvert.SerializeObject(new Dictionary<string, object>()));
            //    //    Logger.LogInfo("CreateFriendlyAIPatch.PatchPostfix.Result=" + result);
            //    //    if(bool.TryParse(result, out bool resultB))
            //    //    {
            //    //        ShouldFriendlyAI = resultB;
            //    //    }
            //    //}
            //    //if (ShouldFriendlyAI == false)
            //    //{
            //    //    Logger.LogInfo("CreateFriendlyAIPatch.PatchPostfix.Friendly AI is turned OFF");
            //    //    return;
            //    //}
            //    //if (ShouldFriendlyAI.Value == true)
            //    //{

            var aiData = __instance.AIData; //ReflectionHelpers.GetFieldOrPropertyFromInstance<object>(__instance, "AIData", false);
            var botsGroup = __instance.BotsGroup; // ReflectionHelpers.GetFieldOrPropertyFromInstance<object>(__instance, "BotsGroup", false);

            if (__instance.Profile.AccountId == PatchConstants.GetPHPSESSID())
                MyPlayer = __instance;
            else if (__instance.IsAI
                && aiData != null
                && botsGroup != null
                )
            {
                Logger.LogInfo("CreateFriendlyAIPatch.PatchPostfix.Spawning AI");
                if (NumberOfFriendlies < MaxNumberOfFriendlies)
                {
                    if (__instance.Side == MyPlayer.Side)
                    {
                        Logger.LogInfo($"CreateFriendlyAIPatch.PatchPostfix.Creating Friendly AI #{NumberOfFriendlies}");

                        //ReflectionHelpers.GetMethodForType(botsGroup.GetType(), "RemoveInfo").Invoke(botsGroup, new object[] { MyPlayer });
                        //ReflectionHelpers.GetMethodForType(botsGroup.GetType(), "AddNeutral").Invoke(botsGroup, new object[] { MyPlayer });
                        //__instance.BotsGroup.RemoveInfo(MyPlayer);
                        //__instance.BotsGroup.AddNeutral(MyPlayer);
                        //__instance.Teleport(MyPlayer.Position, true);
                        NumberOfFriendlies++;
                    }
                }
            }
        }
    }
}
