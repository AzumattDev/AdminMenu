using System;
using System.Linq;
using AdminMenu.Util;
using Fusion;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AdminMenu;

[HarmonyPatch(typeof(Global), nameof(Global.Update))]
static class GlobalCSPatch
{
    static void Postfix(Global __instance)
    {
        if (!UIGameMenuAwakePatch.Admin || !Utilities.GInst) return;
        if (AdminMenuPlugin.OpenuiHotkey.Value.IsDown())
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

        if (Utilities.FpspInst != null && AdminUI.noClip)
        {
            if (!Input.anyKey)
                return;
            Transform transform1 = Utilities.FpspInst.transform;
            Transform transform2 = Utilities.CamInst.transform;
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
                    if (guid == Utilities.GInst.Player.playerDummy.CharacterGuid)
                    {
                        AdminMenuPlugin.AdminMenuLogger.LogInfo($"Setting Admin to true for {playerDummy.CharacterName} ({playerDummy.CharacterGuid})");
                        // UIGameMenuAwakePatch.CheatPanelRoot.SetActive(true);
                        UIGameMenuAwakePatch.Admin = true;
                    }
                }
                else if (transform && transform.TryGetComponent<PlayerDummy>(out playerDummy2) && playerDummy2.Object && playerDummy2.Object.IsValid && (playerDummy2.CharacterGuid != guid))
                {
                    if (guid != Utilities.GInst.Player.playerDummy.CharacterGuid)
                    {
                        if (playerDummy2.CharacterGuid == Utilities.GInst.Player.playerDummy.CharacterGuid)
                        {
                            AdminMenuPlugin.AdminMenuLogger.LogInfo($"Setting Admin to false for {playerDummy2.CharacterName} ({playerDummy2.CharacterGuid})");
                        }

                        //UIGameMenuAwakePatch.CheatPanelRoot.SetActive(false);
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

[HarmonyPatch(typeof(UICombat), nameof(UICombat.Start))]
static class UIGameMenuAwakePatch
{
   // public static GameObject CheatPanelRoot = null!;
    public static bool Admin = true;

    static void Postfix(UICombat __instance)
    {
        //CheatPanelRoot = __instance.CheatPanelRoot; // Removed this in v0.200
        if (!Admin) return;
        //CheatPanelRoot.SetActive(true);
        if (AdminMenuPlugin.AdminUI != null) return;
        if (AdminMenuPlugin.AdminUITemp == null)
        {
            AdminMenuPlugin.AdminMenuLogger.LogInfo("AdminUITemp is null, loading assets");
            AdminMenuPlugin.LoadAssets();
        }
        AdminMenuPlugin.AdminUI = Object.Instantiate(AdminMenuPlugin.AdminUITemp, Utilities.GInst.uiCombat.transform, false);
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
                Mainframe.code.GlobalData.DisconnectPlayerAsync(component.PlayerRef);
                Utilities.GInst.uiCombat.AddHint($"Player {characterName} kicked from the game.", Color.yellow);
                AdminMenuPlugin.AdminMenuLogger.LogInfo($"Player {characterName} ({component.CharacterGuid}) kicked from the game.");
            }
        }
    }
}

[HarmonyPatch(typeof(Global), nameof(Global.HandleKeys))]
static class GlobalHandleKeysPatch
{
    static bool Prefix(Global __instance)
    {
        if (__instance.Player.IsBusy || __instance.player.IsDead || !GlobalDataHelper.IsGlobalDataValid)
        {
            return true;
        }

        if (!Checks.AdminPanelActive() || __instance.uiDialogue.IsActive) return true;
        if (InputManager.Instance.EscapePressed)
            __instance.ButtonEscape();
        /*if (Input.GetKeyDown(KeyCode.Home))
            __instance.uiGameMenu.FasterTime();*/

        return false;
    }
}

[HarmonyPatch(typeof(PlayerCharacter), nameof(PlayerCharacter.UpdateKeys))]
static class PlayerCharacterUpdateKeysPatch
{
    static bool Prefix(PlayerCharacter __instance)
    {
        if (!Checks.AdminPanelActive()) return true;
        return false;
    }
}

[HarmonyPatch(typeof(PlayerCharacter), nameof(PlayerCharacter.AddHealth))]
static class PlayerAddHealthPatch
{
    static bool Prefix(PlayerCharacter __instance, float point,
        int HitPart,
        Vector3 damageSource,
        bool canBleed,
        bool showBiteFx,
        bool IsMeleeWeapon)
    {
        // Outright prevent damage (hopefully)
        return !(point < 0) || HitPart == 3 || !AdminUI.unlimitedHealth;
    }
}