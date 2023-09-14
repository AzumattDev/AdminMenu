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


    [HarmonyPatch(typeof(Global), nameof(Global.Update))]
    static class GlobalCSPatch
    {
        static void Postfix(Global __instance)
        {
            if (!UIGameMenuAwakePatch.Admin || !Utilities.gInst) return;
            if (AdminMenuPlugin.openuiHotkey.Value.IsDown())
            {
                AdminMenuPlugin.AdminUI.SetActive(!AdminMenuPlugin.AdminUI.activeSelf);
                Utilities.TurnOnUI();
            }

            if (Checks.AdminPanelActive() && !__instance.uiDialogue.IsActive)
            {
                Utilities.TurnOnUI();
                Cursor.visible = true;
                Cursor.SetCursor(RM.code.cursorNormal, Vector2.zero, CursorMode.Auto);
                Cursor.lockState = CursorLockMode.None;
            }

            Checks.CheckPlayer();
            Checks.CheckWeapon();

            if (Utilities.fpspInst != null && AdminUI.noClip)
            {
                if (!Input.anyKey)
                    return;
                Transform transform1 = Utilities.fpspInst.transform;
                Transform transform2 = Utilities.camInst.transform;
                float num1 = Utilities.GetKey(KeyCode.W) - Utilities.GetKey(KeyCode.S);
                float num2 = Utilities.GetKey(KeyCode.D) - Utilities.GetKey(KeyCode.A);
                transform1.position += (transform1.up * (Utilities.GetKey(KeyCode.Space) - Utilities.GetKey(KeyCode.LeftControl)) + transform2.forward * num1 + transform2.right * num2).normalized * (float)(10.0 + 15.0 * Utilities.GetKey(KeyCode.LeftShift)) * Time.deltaTime;
            }
        }
    }

    [HarmonyPatch(typeof(GlobalData), nameof(GlobalData.Spawned))]
    static class PlayerDummySpawnedPatch
    {
        static void Postfix(GlobalData __instance)
        {
            try
            {
                if (!__instance.Runner.SessionInfo.Properties.TryGetValue("serverHostPlayerIdentifier", out SessionProperty sessionProperty))
                {
                    return;
                }

                Guid guid;
                if (!Guid.TryParse(sessionProperty, out guid))
                {
                    return;
                }

                bool flag = false;
                foreach (Transform transform in WorldScene.code.allPlayerDummies.items.ToList<Transform>())
                {
                    PlayerDummy playerDummy;
                    PlayerDummy playerDummy2;
                    if (transform && transform.TryGetComponent<PlayerDummy>(out playerDummy) && playerDummy.Object && playerDummy.Object.IsValid && !(playerDummy.CharacterGuid != guid))
                    {
                        if (guid == Utilities.gInst.Player.playerDummy.CharacterGuid)
                        {
                            AdminMenuPlugin.AdminMenuLogger.LogInfo($"Setting Admin to true for {playerDummy.CharacterName} ({playerDummy.CharacterGuid})");
                            // UIGameMenuAwakePatch.CheatPanelRoot.SetActive(true);
                            UIGameMenuAwakePatch.Admin = true;
                        }
                    }
                    else if (transform && transform.TryGetComponent<PlayerDummy>(out playerDummy2) && playerDummy2.Object && playerDummy2.Object.IsValid && (playerDummy2.CharacterGuid != guid))
                    {
                        if (guid != Utilities.gInst.Player.playerDummy.CharacterGuid)
                        {
                            if (playerDummy2.CharacterGuid == Utilities.gInst.Player.playerDummy.CharacterGuid)
                            {
                                AdminMenuPlugin.AdminMenuLogger.LogInfo($"Setting Admin to false for {playerDummy2.CharacterName} ({playerDummy2.CharacterGuid})");
                            }

                            UIGameMenuAwakePatch.CheatPanelRoot.SetActive(false);
                            UIGameMenuAwakePatch.Admin = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // Ignored for singleplayer
            }
        }
    }

    [HarmonyPatch(typeof(UIGameMenu), nameof(UIGameMenu.Awake))]
    static class UIGameMenuAwakePatch
    {
        public static GameObject CheatPanelRoot = null!;
        public static bool Admin = true;

        static void Postfix(UIGameMenu __instance)
        {
            CheatPanelRoot = __instance.CheatPanelRoot;
            if (!Admin) return;
            //CheatPanelRoot.SetActive(true);
            AdminMenuPlugin.AdminUI = Object.Instantiate(AdminMenuPlugin.AdminUI, Utilities.gInst.uiCombat.transform, false);
            AdminMenuPlugin.AdminUI.SetActive(false);
        }
    }


    [HarmonyPatch(typeof(PlayerDummy), nameof(PlayerDummy.NoticePlayerJoined_RPC))]
    static class PlayerDummyNoticePlayerJoinedRPCPatch
    {
        static void Postfix(PlayerDummy __instance, ref string characterName)
        {
            foreach (Transform transform in WorldScene.code.allPlayerDummies.items.ToList<Transform>())
            {
                if (transform && transform.gameObject.activeSelf && transform.TryGetComponent<PlayerDummy>(out PlayerDummy component) && (bool)(Object)component.Object && !(component == Global.code.Player.playerDummy))
                {
                    if (AdminUI.allowPlayers) continue;
                    Mainframe.code.M_GlobalData.RPC_DisconnectPlayer(component.PlayerRef);
                    Utilities.gInst.uiCombat.AddHint($"Player {characterName} kicked from the game.", Color.yellow);
                    AdminMenuPlugin.AdminMenuLogger.LogInfo($"Player {characterName} ({component.CharacterGuid}) kicked from the game.");
                }
            }
        }
    }
}