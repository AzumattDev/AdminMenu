using System;
using System.IO;
using System.Linq;
using System.Reflection;
using AdminMenu.Util;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Fusion;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AdminMenu
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class AdminMenuPlugin : BaseUnityPlugin
    {
        internal const string ModName = "AdminMenu";
        internal const string ModVersion = "1.2.3";
        internal const string Author = "Azumatt";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
        private readonly Harmony _harmony = new(ModGUID);
        public static readonly ManualLogSource AdminMenuLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        public static AssetBundle? AssetBundle;
        public static GameObject AdminUI = null!;
        public static ConfigEntry<KeyboardShortcut> OpenuiHotkey = null!;
        public static ConfigEntry<Toggle> HideEmptyChests = null!;
        public static ConfigEntry<Toggle> HideBrokenScavengeables = null!;
        public static ConfigEntry<Toggle> HideCollectibles = null!;

        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public void Awake()
        {
            OpenuiHotkey = Config.Bind("1 - General", "OpenUIHotkey", new KeyboardShortcut(KeyCode.F3), "The hotkey to open the admin UI");
            HideEmptyChests = Config.Bind("1 - General", "Hide Empty Chests", Toggle.On, "Hide chests that have no items in them");
            HideBrokenScavengeables = Config.Bind("1 - General", "Hide Broken Scavengeables", Toggle.On, "Hide scavengeables that have nothing left to give.");
            HideCollectibles = Config.Bind("1 - General", "Hide Collectibles", Toggle.On, "Hide collectibles that have nothing left to give.");
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            LoadAssets();
            DontDestroyOnLoad(AdminUI);
            SetupWatcher();
        }

        private void OnDestroy()
        {
            Config.Save();
        }

        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                AdminMenuLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                AdminMenuLogger.LogError($"There was an issue loading your {ConfigFileName}");
                AdminMenuLogger.LogError("Please check your config entries for spelling and format!");
            }
        }

        void OnGUI()
        {
            if (!UIGameMenuAwakePatch.Admin) return;
            if (OpenuiHotkey.Value.IsDown())
            {
                AdminUI.SetActive(!AdminUI.activeSelf);
                Utilities.TurnOnUI();
            }

            if (Checks.AdminPanelActive() && Input.GetKeyDown(KeyCode.Escape))
            {
                AdminUI.SetActive(false);
            }

            if (!Utilities.gInst) return;
            if (Checks.AdminPanelActive() && !Utilities.gInst.uiDialogue.IsActive)
            {
                Utilities.TurnOnUI();
            }

            Checks.CheckESP();
        }

        private static AssetBundle GetAssetBundleFromResources(string filename)
        {
            var execAssembly = Assembly.GetExecutingAssembly();
            var resourceName = execAssembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(filename));

            using (var stream = execAssembly.GetManifestResourceStream(resourceName))
            {
                return AssetBundle.LoadFromStream(stream);
            }
        }

        public static void LoadAssets()
        {
            AssetBundle = GetAssetBundleFromResources("sunkenadminui");
            AdminUI = AssetBundle.LoadAsset<GameObject>("AdminUI");
            AssetBundle.Unload(false);
        }
    }
}