using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT;
using EFT.UI;
//using GamePanelHUDCore.Patches;
//using GamePanelHUDCore.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace GamePanelHUDCore
{

	[BepInPlugin("com.kmyuhkyuk.GamePanelHUDCore", "kmyuhkyuk-GamePanelHUDCore", "2.5.1")]
	public class GamePanelHUDCorePlugin : BaseUnityPlugin
	{
		public class HUDCoreClass
		{
			public class AssetData<T>
			{
				public IReadOnlyDictionary<string, T> Asset;

				public IReadOnlyDictionary<string, T> Init;

				public AssetData(Dictionary<string, T> asset, Dictionary<string, T> init)
				{
					Asset = asset;
					Init = init;
				}
			}

			public Player YourPlayer;

			public GameUI YourGameUI;

			public GameWorld TheWorld;

			public bool AllHUDSW;

			public static readonly GameObject GamePanlHUDPublic;

			public static readonly string ModPath;

			public static readonly Version GameVersion;

			private static readonly ManualLogSource LogSource;

			public bool HasPlayer => YourPlayer != null;

			public static event Action<GameWorld> WorldStart;

			public static event Action<GameWorld> WorldDispose;

			static HUDCoreClass()
			{
				GamePanlHUDPublic = new GameObject("GamePanlHUDPublic", new Type[2]
				{
				typeof(Canvas),
				typeof(CanvasScaler)
				});
				ModPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BepInEx/plugins/kmyuhkyuk-GamePanelHUD");
				LogSource = BepInEx.Logging.Logger.CreateLogSource("HUDCore");
				FileVersionInfo fileVersionInfo = Process.GetCurrentProcess().MainModule.FileVersionInfo;
				GameVersion = new Version(fileVersionInfo.FileMajorPart, fileVersionInfo.ProductMinorPart, fileVersionInfo.ProductBuildPart, fileVersionInfo.FilePrivatePart);
				Canvas component = GamePanlHUDPublic.GetComponent<Canvas>();
				component.renderMode = ((RenderMode)0);
				component.sortingOrder = 1;
				component.additionalShaderChannels = ((AdditionalCanvasShaderChannels)25);
				GameObject.DontDestroyOnLoad(GamePanlHUDPublic);
			}

			public static string GetBundlePath(string bundlename)
			{
				return Path.Combine(ModPath, "bundles", bundlename);
			}

			public static AssetData<GameObject> LoadHUD(string bundlename, string initassetname)
			{
				return LoadHUD(bundlename, new string[1] { initassetname });
			}

			public static AssetData<GameObject> LoadHUD(string bundlename, string[] initassetname)
			{
				//AssetBundle val = BundleHelp.LoadBundle(LogSource, GetBundlePath(bundlename));
				//Dictionary<string, GameObject> asset = val.LoadAllAssets<GameObject>().ToDictionary((GameObject x) => x.name, (GameObject x) => x);
				//Dictionary<string, GameObject> init = new Dictionary<string, GameObject>();
				//foreach (string initassetname2 in initassetname)
				//{
				//	InitAsset(asset, init, initassetname2);
				//}
				//val.Unload(false);
				//return new AssetData<GameObject>(asset, init);
				return null;
			}

			private static void InitAsset(Dictionary<string, GameObject> asset, Dictionary<string, GameObject> init, string initassetname)
			{
				init.Add(initassetname, GameObject.Instantiate<GameObject>(asset[initassetname], GamePanlHUDPublic.transform));
			}

			public static void GameWorldDispose(GameWorld world)
			{
				if (HUDCoreClass.WorldDispose != null)
				{
					HUDCoreClass.WorldDispose(world);
				}
			}

			public static void GameWorldStart(GameWorld world)
			{
				if (HUDCoreClass.WorldStart != null)
				{
					HUDCoreClass.WorldStart(world);
				}
			}

			public void Set(Player yourplayer, GameUI yourgameui, GameWorld theworld, bool hudsw)
			{
				YourPlayer = yourplayer;
				YourGameUI = yourgameui;
				TheWorld = theworld;
				AllHUDSW = hudsw;
			}
		}

		public class HUDClass<T, V>
		{
			public T Info;

			public V SettingsData;

			public bool HUDSW;

			public void Set(T info, V settingsdata, bool hudsw)
			{
				Info = info;
				SettingsData = settingsdata;
				HUDSW = hudsw;
			}
		}

		public class SettingsData
		{
			public ConfigEntry<bool> KeyAllHUDAlways;

			public ConfigEntry<bool> KeyDebugMethodTime;
		}

		//public static readonly IUpdateManger UpdateManger = new IUpdateManger();

		//internal static GameUI YourGameUI;

		//internal static Player YourPlayer;

		//internal static GameWorld TheWorld;

		public static readonly HUDCoreClass HUDCore = new HUDCoreClass();

		//private bool AllHUDSW;

		private readonly SettingsData SettingsDatas = new SettingsData();

		private void Start()
		{
			Logger.LogInfo("Loaded: kmyuhkyuk-GamePanelHUDCore");
			//ModUpdateCheck.ServerCheck();
			//ModUpdateCheck.DrawNeedUpdate(Config, (Info.get_Metadata().get_Version());
			//SettingsDatas.KeyAllHUDAlways = (Config.Bind<bool>("主设置 Main Settings", "所有指示栏始终显示 All HUD Always display", false, (ConfigDescription)null);
			//SettingsDatas.KeyDebugMethodTime = (Config.Bind<bool>("主设置 Main Settings", "调试所有指示栏调用时间 Debug All HUD Method Invoke Time", false, new ConfigDescription("", (AcceptableValueBase)null, new object[1]
			//{
			//	new ConfigurationManagerAttributes
			//	{
			//		IsAdvanced = true
			//	}
			//}));
			//(new PlayerPatch()).Enable();
			//(new GameWorldAwakePatch()).Enable();
			//(new GameWorldOnGameStartedPatch()).Enable();
			//(new GameWorldDisposePatch()).Enable();
			//(new GameUIPatch()).Enable();
			//(new MainApplicationPatch()).Enable();
			//new TriggerWithIdPatch().Enable();
			//LocalizedHelp.Init();
			//GrenadeType.Init();
			//GetMag.Init();
			//RoleHelp.Init();
			//RuToEn.Init();
		}

		private void Update()
		{
			//if (SettingsDatas.KeyAllHUDAlways.Value)
			//{
			//	AllHUDSW = true;
			//}
			//else if (YourGameUI != null && YourGameUI.BattleUiScreen != null)
			//{
			//	AllHUDSW = ((Component)YourGameUI.BattleUiScreen).gameObject.activeSelf;
			//}
			//else
			//{
			//	AllHUDSW = false;
			//}
			//HUDCore.Set(YourPlayer, YourGameUI, TheWorld, AllHUDSW);
			//UpdateManger.NeedMethodTime = SettingsDatas.KeyDebugMethodTime.Value;
			//UpdateManger.Update();
		}
	}

}