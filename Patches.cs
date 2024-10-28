using System.Linq;
using AdminMenu.Util;
using HarmonyLib;
using KWS;
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

        if (Checks.AdminPanelActive() && !__instance.DialogueManager.IsInDialogue)
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

[HarmonyPatch(typeof(UICombat), nameof(UICombat.Start))]
static class UIGameMenuAwakePatch
{
    public static bool Admin = true;

    static void Postfix(UICombat __instance)
    {
        if (Mainframe.code.Networking.IsHostAndInGameSession || Mainframe.code.NetworkGameManager.ServerAdministration.IsServerAdmin)
        {
            AdminMenuPlugin.AdminMenuLogger.LogDebug($"Setting Admin to true");
            UIGameMenuAwakePatch.Admin = true;
            if (AdminMenuPlugin.AdminUI != null) return;
            if (AdminMenuPlugin.AdminUITemp == null)
            {
                AdminMenuPlugin.AdminMenuLogger.LogInfo("AdminUITemp is null, loading assets");
                AdminMenuPlugin.LoadAssets();
            }

            Transform? allowPlayers = Utilities.FindChild(AdminMenuPlugin.AdminUITemp.transform, "AllowPlayers");
            if (allowPlayers != null) allowPlayers.gameObject.SetActive(false);

            AdminMenuPlugin.AdminUI = Object.Instantiate(AdminMenuPlugin.AdminUITemp, Utilities.GInst.uiCombat.transform, false);
            AdminMenuPlugin.AdminUI.SetActive(false);
        }
        else
        {
            if (UIGameMenuAwakePatch.Admin)
                AdminMenuPlugin.AdminMenuLogger.LogDebug($"Setting Admin to false");
            UIGameMenuAwakePatch.Admin = false;
        }
    }
}

/*[HarmonyPatch(typeof(PlayerDummy), nameof(PlayerDummy.NoticePlayerJoined_RPC))]
static class PlayerDummyNoticePlayerJoinedRPCPatch
{
    static void Postfix(PlayerDummy __instance, ref string characterName)
    {
        foreach (Transform transform in WorldScene.code.allPlayerDummies.items.ToList<Transform>())
        {
            if (transform && transform.gameObject.activeSelf && transform.TryGetComponent<PlayerDummy>(out PlayerDummy component) && (bool)(Object)component.Object && !(component == Global.code.Player.playerDummy))
            {
                if (AdminUI.allowPlayers) continue;
                // TODO: Fix this
                //Mainframe.code.GlobalData.DisconnectPlayerAsync(component.PlayerRef);
                Utilities.GInst.uiCombat.AddHint($"Player {characterName} kicked from the game.", Color.yellow);
                AdminMenuPlugin.AdminMenuLogger.LogInfo($"Player {characterName} ({component.CharacterGuid}) kicked from the game.");
            }
        }
    }
}*/

[HarmonyPatch(typeof(Global), nameof(Global.HandleKeys))]
static class GlobalHandleKeysPatch
{
    static bool Prefix(Global __instance)
    {
        if (__instance.Player.IsBusy || __instance.player.IsDead || !GlobalDataHelper.IsGlobalDataValid)
        {
            return true;
        }

        if (!Checks.AdminPanelActive() || __instance.DialogueManager.IsInDialogue) return true;
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
    static bool Prefix(PlayerCharacter __instance, float point, int HitPart, Vector3 damageSource, bool canBleed, bool showBiteFx, bool IsMeleeWeapon)
    {
        // Outright prevent damage (hopefully)
        return !(point < 0) || HitPart == 3 || !AdminUI.unlimitedHealth;
    }
}

[HarmonyPatch(typeof(Craftable), nameof(Craftable.IsAbleToCraft), typeof(Craftable))]
static class CraftableIsAbleToCraftPatch
{
    static void Postfix(Craftable __instance, ref bool __result)
    {
        if (Utilities.GInst.FreeBuild)
            __result = true;
    }
}

[HarmonyPatch(typeof(Craftable), nameof(Craftable.IsAbleToCraft), typeof(CraftingItemRequirement[]))]
static class CraftableIsAbleToCraftRequirementsPatch
{
    static void Postfix(Craftable __instance, ref bool __result)
    {
        if (Utilities.GInst.FreeBuild)
            __result = true;
    }
}

[HarmonyPatch(typeof(PlayerCharacter), nameof(PlayerCharacter.ConsumeItem))]
static class PlayerCharacterConsumeItemPatch
{
    static void Postfix(PlayerCharacter __instance, ref bool __result)
    {
        if (Utilities.GInst.FreeBuild)
            __result = true;
    }
}