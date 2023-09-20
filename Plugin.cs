using System;
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
        internal const string ModVersion = "1.1.1";
        internal const string Author = "Azumatt";
        private const string ModGUID = Author + "." + ModName;
        private readonly Harmony _harmony = new(ModGUID);
        public static readonly ManualLogSource AdminMenuLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        public static AssetBundle? AssetBundle;
        public static GameObject AdminUI = null!;
        public static ConfigEntry<KeyboardShortcut> openuiHotkey = null!;

        public void Awake()
        {
            openuiHotkey = Config.Bind("1 - General", "OpenUIHotkey", new KeyboardShortcut(KeyCode.F3), "The hotkey to open the admin UI");
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            LoadAssets();
            DontDestroyOnLoad(AdminUI);
        }

        void OnGUI()
        {
            if (!UIGameMenuAwakePatch.Admin) return;
            if (openuiHotkey.Value.IsDown())
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