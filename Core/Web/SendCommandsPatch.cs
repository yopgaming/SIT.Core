using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Core.Web
{
    /// <summary>
    /// This patch removes the wait to push changes from Inventory
    /// </summary>
    internal class SendCommandsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(BackEndSession2), "TrySendCommands");
        }

        //private static DateTime lastSent = DateTime.MinValue;

        [PatchPrefix]
        public static bool Prefix(
            //ref BackEndSession2 __instance,
            //ref List<object> ___list_0,
            //ref Queue<object> ___queue_0,
            ref float ___float_0
            //TaskCompletionSource<object> ___taskCompletionSource_0,
            // Dictionary<string, IProfileUpdater> ___dictionary_0
            )
        {
            ___float_0 = 0;
            return true;
            //try
            //{
            //    if (___list_0 != null)
            //    {
            //        if (___list_0.Count > 0 
            //            && lastSent < DateTime.Now.AddSeconds(-1)
            //            && __instance.QueueStatus == EOperationQueueStatus.Idle)
            //        {
            //            Logger.LogDebug("Sending Inventory Updates to Server");
            //            //__instance.QueueStatus = EOperationQueueStatus.AwaitingResponse;
            //            Dictionary<string, object> packet = new();
            //            packet.Add("data", ___list_0);
            //            var result = Request.Instance.PostJson("/client/game/profile/items/moving", packet.ToJson());

            //            var resultResponseDataDict = result.ParseJsonTo<Dictionary<string, object>>();

            //            var resultData = resultResponseDataDict["data"].ToString();
            //            ////Logger.LogDebug(resultData);

            //            //var queueData = resultData.ParseJsonTo<QueueData>();
            //            //lastSent = DateTime.Now;
            //            //___list_0.Clear();

            //            //if (___taskCompletionSource_0 != null)
            //            //{
            //            //    ___taskCompletionSource_0.TrySetResult(true);
            //            //}

            //            //__instance.QueueStatusChanged.Invoke();
            //            //__instance.QueueStatus = EOperationQueueStatus.Idle;

            //            //foreach (var (key, changes2) in queueData.ProfileChanges)
            //            //{
            //            //    if (___dictionary_0.TryGetValue(key, out var value2))
            //            //    {
            //            //        Logger.LogInfo($"update {key}");
            //            //        value2.UpdateProfile(changes2);
            //            //    }
            //            //}

            //            //__instance.TrySendCommands();
            //        }
            //    }

            //    //___queue_0.Clear();
            //    return true;
            //}
            //catch ( Exception ex )
            //{
            //    Logger.LogError(ex);
            //    return true;
            //}
        }


        [Serializable]
        private sealed class QueueData
        {
            [JsonProperty("profileChanges")]
            public Dictionary<string, Changes> ProfileChanges;

            [JsonProperty("warnings")]
            public InventoryWarning[] InventoryWarnings;

            [Serializable]
            public sealed class InventoryWarning : IInventoryWarning
            {
                [Serializable]
                private struct InsufficientNumberOfItemsData
                {
                    [JsonProperty("itemId")]
                    public string ItemId;

                    [JsonProperty("requestedCount")]
                    public int RequestedCount;

                    [JsonProperty("actualCount")]
                    public int ActualCount;

                    public string LocalizedMessage
                    {
                        get
                        {
                            if (ActualCount <= 0)
                            {
                                return "Item is out of stock".Localized();
                            }
                            return string.Format("Trading/InsufficientNumberOfItemsInStock{}{}".Localized(), RequestedCount, ActualCount);
                        }
                    }
                }

                [Serializable]
                private struct IncorrectClientPriceData
                {
                    private const string INCORRECT_CLIENT_PRICE = "Trading/IncorrectClientPrice{0}{1}";

                    [JsonProperty("traderCurrency")]
                    public string TraderCurrencyId { get; private set; }

                    [JsonProperty("requestedCount")]
                    public int RequestedCount { get; private set; }

                    [JsonProperty("actualCount")]
                    public int ActualCount { get; private set; }

                    public string LocalizedMessage
                    {
                        get
                        {
                            ECurrencyType currencyTypeById = GClass2181.GetCurrencyTypeById(TraderCurrencyId);
                            return string.Format("Trading/IncorrectClientPrice{0}{1}".Localized(), $"Backend price: {RequestedCount} {currencyTypeById}", $"Client price: {ActualCount} {currencyTypeById}");
                        }
                    }
                }

                [JsonProperty("data")]
                private UnparsedData _data;

                [JsonProperty("index")]
                public int RequestIndex { get; private set; }

                [JsonProperty("errmsg")]
                public string ErrorMessage { get; private set; }

                [JsonProperty("code")]
                public string ErrorCode { get; private set; }

                int IInventoryWarning.ErrorCode
                {
                    get
                    {
                        if (!int.TryParse(ErrorCode, out var result))
                        {
                            return 0;
                        }
                        return result;
                    }
                }

                [JsonProperty("msg")]
                private string String_0
                {
                    set
                    {
                        ErrorMessage = value;
                    }
                }

                public bool TryGetMessage(out string header, out string description)
                {
                    header = ErrorCode;
                    object obj;
                    if (!GClass1351.TryParse(ErrorCode, out var error))
                    {
                        string errorMessage = ErrorMessage;
                        if (errorMessage == null)
                        {
                            obj = null;
                        }
                        else
                        {
                            obj = errorMessage.Localized();
                            if (obj != null)
                            {
                                goto IL_0035;
                            }
                        }
                        obj = ErrorCode;
                        goto IL_0035;
                    }
                    if (error <= EBackendErrorCode.OfferNotFound)
                    {
                        if (error <= EBackendErrorCode.PriceChanged)
                        {
                            if (error != EBackendErrorCode.NoRoomInStash)
                            {
                                if (error != EBackendErrorCode.PriceChanged)
                                {
                                    goto IL_011c;
                                }
                                description = "Price changed error".Localized();
                            }
                            else
                            {
                                header = "no_free_space_title".Localized();
                                description = "no_free_space_message".Localized();
                            }
                        }
                        else if (error != EBackendErrorCode.TraderDisabled)
                        {
                            if (error != EBackendErrorCode.OfferNotFound)
                            {
                                goto IL_011c;
                            }
                            description = "Request error: 1503 - Offer not found 1503".Localized();
                        }
                        else
                        {
                            description = "TraderError/TraderDisabled".Localized();
                        }
                    }
                    else if (error <= EBackendErrorCode.InsufficientNumberInStock)
                    {
                        if (error != EBackendErrorCode.NotEnoughSpace)
                        {
                            if (error != EBackendErrorCode.InsufficientNumberInStock)
                            {
                                goto IL_011c;
                            }
                            description = _data.ParseJsonTo<InsufficientNumberOfItemsData>(Array.Empty<JsonConverter>()).LocalizedMessage;
                        }
                        else
                        {
                            description = "Warnings/Inventory/NotEnoughSpaceInStash".Localized();
                        }
                    }
                    else
                    {
                        if (error == EBackendErrorCode.IncorrectClientPrice)
                        {
                            description = "";
                            return false;
                        }
                        if (error != EBackendErrorCode.ExaminationFailed)
                        {
                            goto IL_011c;
                        }
                        description = "Warnings/Inventory/ExaminationFailed".Localized();
                    }
                    goto IL_014f;
                IL_0035:
                    description = (string)obj;
                    return true;
                IL_011c:
                    description = (string.IsNullOrEmpty(ErrorMessage) ? error.Localized(EStringCase.None) : ErrorMessage.Localized());
                    goto IL_014f;
                IL_014f:
                    return true;
                }

                public IResult ToResult()
                {
                    if (string.IsNullOrEmpty(ErrorMessage) && string.IsNullOrEmpty(ErrorCode))
                    {
                        return SuccessfulResult.New;
                    }
                    return new FailedResult(ErrorMessage, GClass1351.GetIntCode(ErrorCode));
                }
            }
        }
    }
}
