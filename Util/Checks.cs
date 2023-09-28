using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AdminMenu.Util;

public class Checks
{
    public static bool AdminPanelActive() => AdminMenuPlugin.AdminUI != null && AdminMenuPlugin.AdminUI.activeSelf;

    public static void CheckPlayer()
    {
        if (Utilities.gInst.player.IsDead) return;

        switch (AdminUI.unlimitedHealth)
        {
            case true:
            {
                Utilities.gInst.player.Health = 1000000f;
                Utilities.gInst.player.MaxHealth = 1000000f;
                Utilities.gInst.player.health = Utilities.gInst.player.maxHealth;
                Utilities.gInst.player.targetHealth = Utilities.gInst.player.maxHealth;
                Utilities.gInst.player.BodyTemperature = 100f;
                if (!(Utilities.gInst.player.Bleeding <= 0.0))
                    Utilities.gInst.player.Bleeding = 0.0f;
                break;
            }
            case false:
                //AdminMenuPlugin.AdminMenuLogger.LogInfo("Player health: " + Utilities.gInst.player.Health);
                //AdminMenuPlugin.AdminMenuLogger.LogInfo("Player max health: " + Utilities.gInst.player.MaxHealth);
                Global.code.Player.Health = 160f;
                Global.code.Player.MaxHealth = 160f;
                break;
        }

        if (AdminUI.unlimitedAir)
        {
            Utilities.gInst.player.Air = Utilities.gInst.player.MaxAir;
        }

        if (AdminUI.unlimitedEnergy)
        {
            Utilities.gInst.player.energy = Utilities.gInst.player.MaxEnergy;
        }

        if (AdminUI.unlimitedStamina)
        {
            Utilities.gInst.player.stamina = 130f;
        }

        if (AdminUI.noHunger)
        {
            Utilities.gInst.player.hunger = 100f;
        }

        if (AdminUI.noThirst)
        {
            Utilities.gInst.player.thirst = 100f;
        }

        if (AdminUI.transform && Input.GetKeyDown(KeyCode.LeftControl))
        {
            Utilities.gInst.player.transform.position += 0.25f * Utilities.gInst.player.mainCamera.transform.forward;
            AdminMenuPlugin.AdminMenuLogger.LogInfo("Teleported player forward, current position: " + Utilities.gInst.player.transform.position);
        }
    }

    public static void CheckWeapon()
    {
        if (Utilities.gInst.player.weaponInHand == null) return;

        if (AdminUI.unlimitedAmmo)
        {
            PlayerWeapons.code.CurrentWeaponBehaviorComponent.bulletsLeft = PlayerWeapons.code.CurrentWeaponBehaviorComponent.bulletsPerClip;
        }

        if (AdminUI.weaponNoRecoil)
        {
            Utilities.gInst.player.weaponInHand.recoil = 0.0f;
            Utilities.gInst.player.weaponInHand.horizontalKick = 0.0f;
            Utilities.gInst.player.weaponInHand.verticalKick = 0.0f;
            Utilities.gInst.player.weaponInHand.viewClimbSide = 0.0f;
            Utilities.gInst.player.weaponInHand.viewClimbUp = 0.0f;
        }

        if (AdminUI.weaponNoSpread)
        {
            Utilities.gInst.player.weaponInHand.spread = 0.0f;
        }
    }

    public static void CheckESP()
    {
        if (Event.current.type != EventType.Repaint || !AdminUI.ShouldEsp())
            return;

        if (AdminUI.enemyESP)
            DisplayInfoForEntities(Utilities.wsInst.allCharacters.items, ProcessCharacter);
        if (AdminUI.playerESP)
            DisplayInfoForEntities(Utilities.wsInst.allPlayerDummies.items, ProcessCharacterPlayer);

        if (AdminUI.fishESP)
            DisplayInfoForEntities(Utilities.wsInst.fishes.items, ProcessFish);

        if (AdminUI.birdESP)
            DisplayInfoForEntities(Utilities.wsInst.birds.items, ProcessBird);

        if (AdminUI.lootESP)
        {
            DisplayInfoForEntities(Utilities.wsInst.worldScavengables, ProcessWorldScavengable);
            DisplayInfoForEntities(Utilities.wsInst.worldCollectableContinuingInteractions, ProcessWorldCollectableContinuingInteraction);
            DisplayInfoForEntities(Utilities.wsInst.worldChests, ProcessWorldChest);
        }
    }

    private static void DisplayInfoForEntities<T>(IEnumerable<T> entities, Action<T> processEntity)
    {
        foreach (T entity in entities)
        {
            if (entity as Object) // Cast to Unity's Object to use the implicit bool check.
            {
                processEntity(entity);
            }
        }
    }


    private static void ProcessCharacter(Object obj)
    {
        Transform? transform = obj as Transform;
        if (transform == null) return;

        Character component = transform.GetComponent<Character>();
        if (component != null)
        {
            var position = transform.transform.position;
            Vector3 screenPoint1 = Utilities.camInst.WorldToScreenPoint(position);
            var position1 = component.eye.transform.position;
            Vector3 screenPoint2 = Utilities.camInst.WorldToScreenPoint(position1);
            Vector3 screenPoint3 = Utilities.camInst.WorldToScreenPoint(component.transform.position);
            float f = Vector3.Distance(Utilities.pcInst.transform.position, position);
            float health = component.health;
            string pText = component.weapon.ToString();
            float num1 = Mathf.Abs(screenPoint2.y - screenPoint3.y);
            bool flag = !Utilities.IsEnemyVisible(Utilities.camInst.transform.position, position1);
            float num2;
            if (Utilities.IsOnScreen(screenPoint1) && f < (double)Variables.FEnemies)
            {
                if (flag)
                {
                    if (f < 150.0)
                        Utilities.CornerBox(new Vector2(screenPoint2.x, (float)(Screen.height - (double)screenPoint2.y - 20.0)), num1 / 2f, num1 + 20f, 2f, Color.red, true);
                    Utilities.DrawString(new Vector2(screenPoint1.x, Screen.height - screenPoint1.y), Utilities.NameReplacer(transform.name), Color.red, fontStyle: FontStyle.Normal);
                    Utilities.DrawString(new Vector2(screenPoint1.x, (float)(Screen.height - (double)screenPoint1.y + 12.0)), Utilities.WeaponReplacer(pText), Color.HSVToRGB(0.1055556f, 0.29f, 1f), fontStyle: FontStyle.Normal);
                    Vector2 pos1 = new(screenPoint1.x, (float)(Screen.height - (double)screenPoint1.y + 24.0));
                    num2 = Mathf.Round(health);
                    string text1 = num2.ToString() + " HP";
                    Color white = Color.white;
                    Utilities.DrawString(pos1, text1, white, fontStyle: FontStyle.Normal);
                    Vector2 pos2 = new(screenPoint1.x, (float)(Screen.height - (double)screenPoint1.y + 36.0));
                    num2 = Mathf.Round(f);
                    string text2 = num2.ToString() + "m";
                    Color yellow = Color.yellow;
                    Utilities.DrawString(pos2, text2, yellow, fontStyle: FontStyle.Normal);
                }
                else if (!flag)
                {
                    if (f < 150.0)
                        Utilities.CornerBox(new Vector2(screenPoint2.x, (float)(Screen.height - (double)screenPoint2.y - 20.0)), num1 / 2f, num1 + 20f, 2f, Color.HSVToRGB(0.1083333f, 1f, 1f), true);
                    Utilities.DrawString(new Vector2(screenPoint1.x, Screen.height - screenPoint1.y), Utilities.NameReplacer(transform.name), Color.HSVToRGB(0.1083333f, 1f, 1f), fontStyle: FontStyle.Normal);
                    Utilities.DrawString(new Vector2(screenPoint1.x, (float)(Screen.height - (double)screenPoint1.y + 12.0)), Utilities.WeaponReplacer(pText), Color.HSVToRGB(0.1055556f, 0.29f, 1f), fontStyle: FontStyle.Normal);
                    Vector2 pos3 = new(screenPoint1.x, (float)(Screen.height - (double)screenPoint1.y + 24.0));
                    num2 = Mathf.Round(health);
                    string text3 = num2.ToString() + " HP";
                    Color white = Color.white;
                    Utilities.DrawString(pos3, text3, white, fontStyle: FontStyle.Normal);
                    Vector2 pos4 = new(screenPoint1.x, (float)(Screen.height - (double)screenPoint1.y + 36.0));
                    num2 = Mathf.Round(f);
                    string text4 = num2.ToString() + "m";
                    Color yellow = Color.yellow;
                    Utilities.DrawString(pos4, text4, yellow, fontStyle: FontStyle.Normal);
                }
            }
        }
    }

    private static void ProcessCharacterPlayer(Object obj)
    {
        Transform? transform = obj as Transform;
        if (transform == null) return;

        PlayerDummy component = transform.GetComponent<PlayerDummy>();
        if (component != null && component.Object && component.Object.IsValid && (component.CharacterGuid != Utilities.gInst.Player.playerDummy.CharacterGuid))
        {
            var position = transform.transform.position;
            Vector3 screenPoint1 = Utilities.camInst.WorldToScreenPoint(position);
            var position1 = component.eye.transform.position;
            Vector3 screenPoint2 = Utilities.camInst.WorldToScreenPoint(position1);
            Vector3 screenPoint3 = Utilities.camInst.WorldToScreenPoint(component.transform.position);
            float f = Vector3.Distance(Utilities.pcInst.transform.position, position);
            //float health = component;
            //string pText = component.weapon.ToString();
            float num1 = Mathf.Abs(screenPoint2.y - screenPoint3.y);
            bool flag = !Utilities.IsEnemyVisible(Utilities.camInst.transform.position, position1);
            float num2;
            if (Utilities.IsOnScreen(screenPoint1) && f < (double)Variables.FEnemies)
            {
                if (flag)
                {
                    if (f < 150.0)
                        Utilities.CornerBox(new Vector2(screenPoint2.x, (float)(Screen.height - (double)screenPoint2.y - 20.0)),
                            num1 / 2f, num1 + 20f, 2f, Color.green, true);
                    Utilities.DrawString(new Vector2(screenPoint1.x, Screen.height - screenPoint1.y), Utilities.NameReplacer(component.CharacterName), Color.green, fontStyle: FontStyle.Normal);
                    Vector2 pos1 = new(screenPoint1.x, (float)(Screen.height - (double)screenPoint1.y + 24.0));
                    Color white = Color.white;
                    Vector2 pos2 = new(screenPoint1.x, (float)(Screen.height - (double)screenPoint1.y + 36.0));
                    num2 = Mathf.Round(f);
                    string text2 = num2.ToString() + "m";
                    Color yellow = Color.yellow;
                    Utilities.DrawString(pos2, text2, yellow, fontStyle: FontStyle.Normal);
                }
                else if (!flag)
                {
                    if (f < 150.0)
                        Utilities.CornerBox(new Vector2(screenPoint2.x, (float)(Screen.height - (double)screenPoint2.y - 20.0)),
                            num1 / 2f, num1 + 20f, 2f, Color.HSVToRGB(0.1083333f, 1f, 1f), true);
                    Utilities.DrawString(new Vector2(screenPoint1.x, Screen.height - screenPoint1.y), Utilities.NameReplacer(component.CharacterName),
                        Color.HSVToRGB(0.1083333f, 1f, 1f), fontStyle: FontStyle.Normal);
                    Vector2 pos3 = new(screenPoint1.x, (float)(Screen.height - (double)screenPoint1.y + 24.0));
                    Color white = Color.white;
                    Vector2 pos4 = new(screenPoint1.x, (float)(Screen.height - (double)screenPoint1.y + 36.0));
                    num2 = Mathf.Round(f);
                    string text4 = num2.ToString() + "m";
                    Color yellow = Color.yellow;
                    Utilities.DrawString(pos4, text4, yellow, fontStyle: FontStyle.Normal);
                }
            }
        }
    }

    private static void ProcessFish(Object obj)
    {
        Transform? transform = obj as Transform; // Attempt to cast
        if (transform == null) return;

        FishAI? component = transform.GetComponent<FishAI>();
        if (component != null)
        {
            var position = transform.transform.position;
            Vector3 screenPoint = Utilities.camInst.WorldToScreenPoint(position);
            float f = Vector3.Distance(Utilities.pcInst.transform.position, position);
            float health = component.Health;
            if (Utilities.IsOnScreen(screenPoint) && f < (double)Variables.FFishes)
            {
                Utilities.DrawString(new Vector2(screenPoint.x, Screen.height - screenPoint.y), Utilities.NameReplacer(transform.name), Color.HSVToRGB(0.5f, 1f, 1f), fontStyle: FontStyle.Normal);
                Vector2 pos5 = new(screenPoint.x, (float)(Screen.height - (double)screenPoint.y + 12.0));
                float num = Mathf.Round(health);
                string text5 = num.ToString() + " HP";
                Color white = Color.white;
                Utilities.DrawString(pos5, text5, white, fontStyle: FontStyle.Normal);
                Vector2 pos6 = new(screenPoint.x, (float)(Screen.height - (double)screenPoint.y + 24.0));
                num = Mathf.Round(f);
                string text6 = num.ToString() + "m";
                Color yellow = Color.yellow;
                Utilities.DrawString(pos6, text6, yellow, fontStyle: FontStyle.Normal);
            }
        }
    }

    private static void ProcessBird(Object obj)
    {
        Transform? transform = obj as Transform; // Attempt to cast
        if (transform == null) return;

        Bird? component = transform.GetComponent<Bird>();
        if (component != null)
        {
            Vector3 screenPoint = Utilities.camInst.WorldToScreenPoint(transform.transform.position);
            float f = Vector3.Distance(Utilities.pcInst.transform.position, transform.transform.position);
            float health = component.health;
            if (Utilities.IsOnScreen(screenPoint) && f < (double)Variables.FBirds)
            {
                Utilities.DrawString(new Vector2(screenPoint.x, Screen.height - screenPoint.y), Utilities.NameReplacer(transform.name), Color.HSVToRGB(0.07777778f, 0.275f, 1f), fontStyle: FontStyle.Normal);
                Vector2 pos7 = new(screenPoint.x, (float)(Screen.height - (double)screenPoint.y + 12.0));
                float num = Mathf.Round(health);
                string text7 = num.ToString() + " HP";
                Color white = Color.white;
                Utilities.DrawString(pos7, text7, white, fontStyle: FontStyle.Normal);
                Vector2 pos8 = new(screenPoint.x, (float)(Screen.height - (double)screenPoint.y + 24.0));
                num = Mathf.Round(f);
                string text8 = num.ToString() + "m";
                Color yellow = Color.yellow;
                Utilities.DrawString(pos8, text8, yellow, fontStyle: FontStyle.Normal);
            }
        }
    }

    private static void ProcessWorldScavengable(Scavengeable scavengable)
    {
        if (AdminMenuPlugin.HideBrokenScavengeables.Value == AdminMenuPlugin.Toggle.On && scavengable.ChopAmount <= 0)
        {
            return;
        }

        var position = scavengable.transform.position;
        Vector3 screenPoint = Utilities.camInst.WorldToScreenPoint(position);
        float distance = Vector3.Distance(Utilities.pcInst.transform.position, position);

        if (Utilities.IsOnScreen(screenPoint) && distance < (double)Variables.FLoot)
        {
            Utilities.DrawString(new Vector2(screenPoint.x, Screen.height - screenPoint.y),
                Utilities.LootReplacer(scavengable.name[0].ToString().ToUpper()) + Utilities.LootReplacer(scavengable.name).Substring(1),
                Color.HSVToRGB(0.1055556f, 0.29f, 1f),
                fontStyle: FontStyle.Normal);

            Utilities.DrawString(new Vector2(screenPoint.x, (float)(Screen.height - (double)screenPoint.y + 12.0)),
                Mathf.Round(distance).ToString() + "m",
                Color.yellow,
                fontStyle: FontStyle.Normal);
        }
    }

    private static void ProcessWorldChest(Chest chest)
    {
        if (AdminMenuPlugin.HideEmptyChests.Value == AdminMenuPlugin.Toggle.On && chest._storage.IsEmpty())
        {
            return;
        }

        var position = chest.transform.position;
        Vector3 screenPoint = Utilities.camInst.WorldToScreenPoint(position);
        float distance = Vector3.Distance(Utilities.pcInst.transform.position, position);

        if (Utilities.IsOnScreen(screenPoint) && distance < (double)Variables.FLoot)
        {
            Utilities.DrawString(new Vector2(screenPoint.x, Screen.height - screenPoint.y),
                Utilities.LootReplacer(chest.name[0].ToString().ToUpper()) + Utilities.LootReplacer(chest.name).Substring(1),
                Color.HSVToRGB(0.05277778f, 0.719f, 0.627f),
                fontStyle: FontStyle.Normal);

            Utilities.DrawString(new Vector2(screenPoint.x, (float)(Screen.height - (double)screenPoint.y + 12.0)),
                Mathf.Round(distance).ToString() + "m",
                Color.yellow,
                fontStyle: FontStyle.Normal);
        }
    }

    private static void ProcessWorldCollectableContinuingInteraction(CollectableContinuingInteraction interaction)
    {
        if (AdminMenuPlugin.HideCollectibles.Value == AdminMenuPlugin.Toggle.On && !interaction.gameObject.activeSelf)
        {
            return;
        }
        
        var position = interaction.transform.position;
        Vector3 screenPoint = Utilities.camInst.WorldToScreenPoint(position);
        float distance = Vector3.Distance(Utilities.pcInst.transform.position, position);

        if (Utilities.IsOnScreen(screenPoint) && distance < (double)Variables.FLoot)
        {
            Utilities.DrawString(new Vector2(screenPoint.x, Screen.height - screenPoint.y),
                Utilities.LootReplacer(interaction.name),
                Color.HSVToRGB(0.1666667f, 0.102f, 0.961f),
                fontStyle: FontStyle.Normal);

            Utilities.DrawString(new Vector2(screenPoint.x, (float)(Screen.height - (double)screenPoint.y + 12.0)),
                Mathf.Round(distance).ToString() + "m",
                Color.yellow,
                fontStyle: FontStyle.Normal);
        }
    }
}