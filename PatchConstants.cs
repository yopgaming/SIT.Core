using BepInEx.Logging;
using Comfort.Common;
using EFT;
using FilesChecker;
using HarmonyLib;
using Newtonsoft.Json;
using SIT.Core.Misc;
using SIT.Tarkov.Core.AI;
using SIT.Tarkov.Core.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static SIT.Core.Misc.PaulovJsonConverters;

namespace SIT.Tarkov.Core
{
    public static class PatchConstants
    {
        public static BindingFlags PrivateFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        private static Type[] _eftTypes;
        public static Type[] EftTypes
        {
            get
            {
                if (_eftTypes == null)
                {
                    _eftTypes = typeof(AbstractGame).Assembly.GetTypes().OrderBy(t => t.Name).ToArray();
                }

                return _eftTypes;
            }
        }
        public static Type[] FilesCheckerTypes { get; private set; }
        public static Type LocalGameType { get; private set; }
        public static Type ExfilPointManagerType { get; private set; }
        public static Type BackendInterfaceType { get; private set; }
        public static Type SessionInterfaceType { get; private set; }

        public static Type StartWithTokenType { get; private set; }

        public static Type PoolManagerType { get; set; }

        public static Type JobPriorityType { get; set; }

        public static Type PlayerInfoType { get; set; }
        public static Type PlayerCustomizationType { get; set; }

        public static Type SpawnPointSystemInterfaceType { get; set; }
        public static Type SpawnPointArrayInterfaceType { get; set; }
        public static Type SpawnPointSystemClassType { get; set; }

        public static Type BackendStaticConfigurationType { get; set; }
        public static object BackendStaticConfigurationConfigInstance { get; set; }

        public static class CharacterControllerSettings
        {
            public static object CharacterControllerInstance { get; set; }
            public static CharacterControllerSpawner.Mode ObservedPlayerMode { get; set; }
            public static CharacterControllerSpawner.Mode ClientPlayerMode { get; set; }
            public static CharacterControllerSpawner.Mode BotPlayerMode { get; set; }
        }


        /// <summary>
        /// A Key/Value dictionary of storing & obtaining an array of types by name
        /// </summary>
        public static readonly Dictionary<string, Type[]> TypesDictionary = new();

        /// <summary>
        /// A Key/Value dictionary of storing & obtaining a type by name
        /// </summary>
        public static Dictionary<string, Type> TypeDictionary { get; } = new();

        /// <summary>
        /// A Key/Value dictionary of storing & obtaining a method by type and name
        /// </summary>
        public static readonly Dictionary<(Type, string), MethodInfo> MethodDictionary = new();

        private static string backendUrl;
        /// <summary>
        /// Method that returns the Backend Url (Example: https://127.0.0.1)
        /// </summary>
        private static string RealWSURL;    //did i do this right?
        //It appears to be successful :D
        public static string GetBackendUrl()
        {
            if (string.IsNullOrEmpty(backendUrl))
            {
                backendUrl = BackendConnection.GetBackendConnection().BackendUrl;
            }
            return backendUrl;
        }
        public static string GetREALWSURL() //cut the server address obtained from GetBackendUrl and convert it to "ws://" w
        {
            if (string.IsNullOrEmpty(RealWSURL))
            {
                RealWSURL = BackendConnection.GetBackendConnection().BackendUrl;
                int colonIndex = RealWSURL.LastIndexOf(':');
                if (colonIndex != -1)
                {
                    RealWSURL = RealWSURL.Substring(0, colonIndex);
                }
                RealWSURL = RealWSURL.Replace("http", "ws");
            }
            return RealWSURL;
        }
        public static string GetPHPSESSID()
        {
            if (BackendConnection.GetBackendConnection() == null)
                Logger.LogError("Cannot get Backend Info");

            return BackendConnection.GetBackendConnection().PHPSESSID;
        }



        public static ManualLogSource Logger { get; private set; }

        public static Type GroupingType { get; }
        public static Type JsonConverterType { get; }
        public static JsonConverter[] JsonConverterDefault { get; }

        private static ISession _backEndSession;
        public static ISession BackEndSession
        {
            get
            {
                if (_backEndSession == null && Singleton<TarkovApplication>.Instantiated)
                {
                    _backEndSession = Singleton<TarkovApplication>.Instance.GetClientBackEndSession();
                }

                if (_backEndSession == null && Singleton<ClientApplication<ISession>>.Instantiated)
                {
                    _backEndSession = Singleton<ClientApplication<ISession>>.Instance.GetClientBackEndSession();
                }

                return _backEndSession;
            }
        }





        public static JsonConverter[] GetJsonConvertersBSG()
        {
            return JsonConverterDefault;
        }

        public static List<JsonConverter> GetJsonConvertersPaulov()
        {
            var converters = new List<JsonConverter>();
            converters.Add(new DateTimeOffsetJsonConverter());
            converters.Add(new SimpleCharacterControllerJsonConverter());
            converters.Add(new CollisionFlagsJsonConverter());
            converters.Add(new PlayerJsonConverter());
            return converters;
        }

        public static JsonSerializerSettings GetJsonSerializerSettings()
        {
            var converters = JsonConverterDefault;
            converters.AddItem(new DateTimeOffsetJsonConverter());
            converters.AddItem(new SimpleCharacterControllerJsonConverter());
            converters.AddItem(new CollisionFlagsJsonConverter());
            converters.AddItem(new NotesJsonConverter());
            var paulovconverters = GetJsonConvertersPaulov();
            converters.AddRangeToArray(paulovconverters.ToArray());

            return new JsonSerializerSettings()
            {
                Converters = converters,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                FloatFormatHandling = FloatFormatHandling.DefaultValue,
                FloatParseHandling = FloatParseHandling.Double,
                Error = (serializer, err) =>
                {
                    Logger.LogError(err.ErrorContext.Error.ToString());
                }
            };
        }
        public static JsonSerializerSettings GetJsonSerializerSettingsWithoutBSG()
        {
            var converters = GetJsonConvertersPaulov();

            return new JsonSerializerSettings()
            {
                Converters = converters,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Error = (serializer, err) =>
                {
                    Logger.LogError(err.ErrorContext.Error.ToString());
                }
            };
        }

        public static string SITToJson(this object o)
        {


            return JsonConvert.SerializeObject(o
                    , GetJsonSerializerSettings()
                );
        }

        public static async Task<string> SITToJsonAsync(this object o)
        {
            return await Task.Run(() =>
            {
                return SITToJson(o);
            });
        }

        public static T SITParseJson<T>(this string str)
        {
            return JsonConvert.DeserializeObject<T>(str
                    , new JsonSerializerSettings()
                    {
                        Converters = JsonConverterDefault
                        ,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    }
                    );
        }

        public static bool TrySITParseJson<T>(this string str, out T result)
        {
            try
            {
                result = JsonConvert.DeserializeObject<T>(str
                        , new JsonSerializerSettings()
                        {
                            Converters = JsonConverterDefault
                            ,
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        }
                        );
                return true;
            }
            catch
            {
                result = default(T);
                return false;
            }
        }

        public static object GetPlayerProfile(object __instance)
        {
            var instanceProfile = __instance.GetType().GetProperty("Profile"
                , BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).GetValue(__instance);
            if (instanceProfile == null)
            {
                Logger.LogInfo("ReplaceInPlayer:PatchPostfix: Couldn't find Profile");
                return null;
            }
            return instanceProfile;
        }

        public static string GetPlayerProfileAccountId(object instanceProfile)
        {
            var instanceAccountProp = instanceProfile.GetType().GetField("AccountId"
                , BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            if (instanceAccountProp == null)
            {
                Logger.LogInfo($"ReplaceInPlayer:PatchPostfix: instanceAccountProp not found");
                return null;
            }
            var instanceAccountId = instanceAccountProp.GetValue(instanceProfile).ToString();
            return instanceAccountId;
        }

        public static IDisposable StartWithToken(string name)
        {
            return ReflectionHelpers.GetAllMethodsForType(StartWithTokenType).Single(x => x.Name == "StartWithToken").Invoke(null, new object[] { name }) as IDisposable;
        }

        public static async Task InvokeAsyncStaticByReflection(MethodInfo methodInfo, object rModel, params object[] p)
        {
            if (rModel == null)
            {
                await (Task)methodInfo
                    .MakeGenericMethod(new[] { rModel.GetType() })
                    .Invoke(null, p);
            }
            else
            {
                await (Task)methodInfo
                    .Invoke(null, p);
            }
        }

        public static ClientApplication<ISession> GetClientApp()
        {
            return Singleton<ClientApplication<ISession>>.Instance;
        }

        public static TarkovApplication GetMainApp()
        {
            return GetClientApp() as TarkovApplication;
        }

        /// <summary>
        /// Invoke an async Task<object> method
        /// </summary>
        /// <param name="type"></param>
        /// <param name="outputType"></param>
        /// <param name="method"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        //public static async Task<object> InvokeAsyncMethod(Type type, Type outputType, string method, object[] param)
        //{
        //    var m = ReflectionHelpers.GetAllMethodsForType(type).First(x => x.Name == method);// foo.GetType().GetMethod(nameof(IFoo.Get));
        //    Logger.LogInfo("InvokeAsyncMethod." + m.Name);

        //    //var builder = AsyncTaskMethodBuilder.Create();

        //    var generic = m.MakeGenericMethod(outputType);
        //    var task = (Task)generic.Invoke(type, param);

        //    await task.ConfigureAwait(false);

        //    var resultProperty = task.GetType().GetProperty("Result");
        //    return resultProperty.GetValue(task);

        //}

        static PatchConstants()
        {
            if (Logger == null)
                Logger = BepInEx.Logging.Logger.CreateLogSource("SIT.Tarkov.Core.PatchConstants");

            TypesDictionary.Add("EftTypes", EftTypes);

            FilesCheckerTypes = typeof(ICheckResult).Assembly.GetTypes();
            LocalGameType = EftTypes.Single(x => x.Name == "LocalGame");
            ExfilPointManagerType = EftTypes.Single(x => x.GetMethod("InitAllExfiltrationPoints") != null);
            BackendInterfaceType = EftTypes.Single(x => x.GetMethods().Select(y => y.Name).Contains("CreateClientSession") && x.IsInterface);
            SessionInterfaceType = EftTypes.Single(x => x.GetMethods().Select(y => y.Name).Contains("GetPhpSessionId") && x.IsInterface);
            DisplayMessageNotifications.MessageNotificationType = EftTypes.Single(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public).Select(y => y.Name).Contains("DisplayMessageNotification"));
            if (DisplayMessageNotifications.MessageNotificationType == null)
            {
                Logger.LogInfo("SIT.Tarkov.Core:PatchConstants():MessageNotificationType:Not Found");
            }
            GroupingType = EftTypes.Single(x => x.GetMethods(BindingFlags.Public | BindingFlags.Static).Select(y => y.Name).Contains("CreateRaidPlayer"));
            //if (GroupingType != null)
            //{
            //  Logger.LogInfo("SIT.Tarkov.Core:PatchConstants():Found GroupingType:" + GroupingType.FullName);
            //}

            JsonConverterType = typeof(AbstractGame).Assembly.GetTypes()
               .First(t => t.GetField("Converters", BindingFlags.Static | BindingFlags.Public) != null);
            JsonConverterDefault = JsonConverterType.GetField("Converters", BindingFlags.Static | BindingFlags.Public).GetValue(null) as JsonConverter[];
            //Logger.LogInfo($"PatchConstants: {JsonConverterDefault.Length} JsonConverters found");

            StartWithTokenType = EftTypes.Single(x => ReflectionHelpers.GetAllMethodsForType(x).Count(y => y.Name == "StartWithToken") == 1);

            BotSystemHelpers.Setup();

            if (JobPriorityType == null)
            {
                JobPriorityType = EftTypes.Single(x =>
                    ReflectionHelpers.GetAllMethodsForType(x).Any(x => x.Name == "Priority")
                    );
                //Logger.LogInfo($"Loading JobPriorityType:{JobPriorityType.FullName}");
            }

            if (PlayerInfoType == null)
            {
                PlayerInfoType = EftTypes.Single(x =>
                    ReflectionHelpers.GetAllMethodsForType(x).Any(x => x.Name == "AddBan")
                    && ReflectionHelpers.GetAllMethodsForType(x).Any(x => x.Name == "RemoveBan")
                    );
                //Logger.LogInfo($"Loading PlayerInfoType:{PlayerInfoType.FullName}");
            }

            if (PlayerCustomizationType == null)
            {
                PlayerCustomizationType = ReflectionHelpers.GetFieldFromType(typeof(Profile), "Customization").FieldType;
                //Logger.LogInfo($"Loading PlayerCustomizationType:{PlayerCustomizationType.FullName}");
            }

            SpawnPointArrayInterfaceType = EftTypes.Single(x =>
                        ReflectionHelpers.GetAllMethodsForType(x).Any(x => x.Name == "CreateSpawnPoint")
                        && ReflectionHelpers.GetAllMethodsForType(x).Any(x => x.Name == "DestroySpawnPoint")
                        && x.IsInterface
                    );
            //Logger.LogInfo($"Loading SpawnPointArrayInterfaceType:{SpawnPointArrayInterfaceType.FullName}");

            BackendStaticConfigurationType = EftTypes.Single(x =>
                    ReflectionHelpers.GetAllMethodsForType(x).Any(x => x.Name == "LoadApplicationConfig")
            //&& ReflectionHelpers.GetFieldFromType(x, "BackendUrl") != null
            //&& ReflectionHelpers.GetFieldFromType(x, "Config") != null
            );

            //Logger.LogInfo($"Loading BackendStaticConfigurationType:{BackendStaticConfigurationType.FullName}");

            //if (!TypeDictionary.ContainsKey("StatisticsSession"))
            //{
            //    TypeDictionary.Add("StatisticsSession", EftTypes.OrderBy(x => x.Name).First(x =>
            //        x.IsClass
            //        && ReflectionHelpers.GetAllMethodsForType(x).Any(x => x.Name == "BeginStatisticsSession")
            //        && ReflectionHelpers.GetAllMethodsForType(x).Any(x => x.Name == "EndStatisticsSession")
            //    ));
            //    //Logger.LogInfo($"StatisticsSession:{TypeDictionary["StatisticsSession"].FullName}");
            //}

            //if (!TypeDictionary.ContainsKey("FilterCustomization"))
            //{
            //    // Gather FilterCustomization
            //    TypeDictionary.Add("FilterCustomization", EftTypes.OrderBy(x => x.Name).Last(x =>
            //        x.IsClass
            //        && ReflectionHelpers.GetAllMethodsForType(x).Any(x => x.Name == "FilterCustomization")
            //    ));
            //    Logger.LogInfo($"FilterCustomization:{TypeDictionary["FilterCustomization"].FullName}");
            //}

            // TypeDictionary.Add("Profile", EftTypes.First(x =>
            //    x.IsClass && x.FullName == "EFT.Profile"
            //));

            //TypeDictionary.Add("Profile.Customization", EftTypes.First(x =>
            //    x.IsClass
            //    && x.BaseType == typeof(Dictionary<EBodyModelPart, string>)
            //));

            //TypeDictionary.Add("Profile.Inventory", EftTypes.First(x =>
            //    x.IsClass
            //    && ReflectionHelpers.GetAllMethodsForType(x).Any(x => x.Name == "UpdateTotalWeight")
            //    && ReflectionHelpers.GetAllMethodsForType(x).Any(x => x.Name == "GetAllItemByTemplate")
            //    && ReflectionHelpers.GetAllMethodsForType(x).Any(x => x.Name == "GetItemsInSlots")
            //));
        }
    }
}
