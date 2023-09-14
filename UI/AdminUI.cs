using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using AdminMenu.Util;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AdminMenu
{
    public class AdminUI : MonoBehaviour
    {
        public static AdminUI Instance = null!;
        public RectTransform Canvas = null!;
        public GameObject AdminMenu = null!;
        public ScrollRect ScrollViewLeft = null!;
        public RectTransform LeftButtonList = null!;
        public RectTransform LeftSpawnerList = null!;
        public RectTransform DefaultRightView = null!;
        public RectTransform SpawnerView = null!;
        public Transform spawnerPlaceholder = null!;
        public SpawnerPlaceholder spawnerPlaceholderComp = null!;
        public static Color RedTextColor { get; private set; }
        public static Color GreenTextColor = new Color();
        public List<GameObject> buttons = new();

        public const string Act = "Active";
        public const string Inact = "Inactive";
        public static bool allowPlayers = false;
        public static bool unlimitedHealth = false;
        public static bool unlimitedStamina = false;
        public static bool unlimitedEnergy = false;
        public static bool unlimitedAir = false;
        public static bool unlimitedAmmo = false;
        public static bool noHunger = false;
        public static bool noThirst = false;
        public static bool playerESP = false;
        public static bool enemyESP = false;
        public static bool fishESP = false;
        public static bool birdESP = false;
        public static bool lootESP = false;
        public static bool weaponNoRecoil = false;
        public static bool weaponNoSpread = false;
        public static bool transform = false;
        public static bool noClip = false;
        public static float timeOfDay = 0f;
        public static bool instantKill = false;

        [Header("Main Buttons")] 
        public Button playerCheats = null!;
        public Button visualCheats = null!;
        public Button weaponCheats = null!;
        public Button miscCheats = null!;
        public Button spawnerCheats = null!;


        [Header("Left - Buttons")] 
        public Button UnlimitedHealth = null!;
        public Button UnlimitedStamina = null!;
        public Button UnlimitedEnergy = null!;
        public Button UnlimitedOxygen = null!;
        public Button UnlimitedAmmo = null!;
        public Button FreeBuild = null!;
        public Button FreeResearch = null!;
        public Button NoHunger = null!;
        public Button NoThirst = null!;
        public Button PlayerESP = null!;
        public Button EnemyESP = null!;
        public Button FishESP = null!;
        public Button BirdESP = null!;
        public Button LootESP = null!;
        public Button WeaponNoRecoil = null!;
        public Button WeaponNoSpread = null!;
        public Button Transform = null!;
        public Button NoClip = null!;
        public Slider TimeOfDay = null!;

        [Header("Right - Buttons")] 
        public Button AllowPlayers = null!;
        public Button InstantKill = null!;
        public Button SavePlayer = null!;
        public Button LoadPlayer = null!;
        public Button SaveWorld = null!;
        public Button LoadWorld = null!;
        public Button TriggerAutoSave = null!;
        public Button LoadAutoSave = null!;

        [Header("Button Descriptions")] 
        public Text unlimitedHealthDescription = null!;
        public Text unlimitedStaminaDescription = null!;
        public Text unlimitedEnergyDescription = null!;
        public Text unlimitedOxygenDescription = null!;
        public Text unlimitedAmmoDescription = null!;
        public Text freeBuildDescription = null!;
        public Text freeResearchDescription = null!;
        public Text noHungerDescription = null!;
        public Text noThirstDescription = null!;
        public Text playerEspDescription = null!;
        public Text enemyEspDescription = null!;
        public Text fishEspDescription = null!;
        public Text birdEspDescription = null!;
        public Text lootEspDescription = null!;
        public Text weaponNoRecoilDescription = null!;
        public Text weaponNoSpreadDescription = null!;
        public Text transformDescription = null!;
        public Text noClipDescription = null!;
        public Text instantKillDescription = null!;
        public Text allowPlayersDescription = null!;


        public void Awake()
        {
            GreenTextColor = ColorUtility.TryParseHtmlString("#25C835", out Color color) ? color : Color.green;
            RedTextColor = unlimitedHealthDescription.color;
            CreateEventListeners();
            Refresh(RM.code.allItems.items);
        }


        public void CreateEventListeners()
        {
            UnlimitedHealth.onClick.AddListener(UnlimitedHealth_OnClick);
            UnlimitedStamina.onClick.AddListener(UnlimitedStamina_OnClick);
            UnlimitedEnergy.onClick.AddListener(UnlimitedEnergy_OnClick);
            UnlimitedOxygen.onClick.AddListener(UnlimitedOxygen_OnClick);
            UnlimitedAmmo.onClick.AddListener(UnlimitedAmmo_OnClick);
            FreeBuild.onClick.AddListener(FreeBuild_OnClick);
            FreeResearch.onClick.AddListener(FreeResearch_OnClick);
            NoHunger.onClick.AddListener(NoHunger_OnClick);
            NoThirst.onClick.AddListener(NoThirst_OnClick);
            PlayerESP.onClick.AddListener(PlayerESP_OnClick);
            EnemyESP.onClick.AddListener(EnemyESP_OnClick);
            FishESP.onClick.AddListener(FishESP_OnClick);
            BirdESP.onClick.AddListener(BirdESP_OnClick);
            LootESP.onClick.AddListener(LootESP_OnClick);
            WeaponNoRecoil.onClick.AddListener(WeaponNoRecoil_OnClick);
            WeaponNoSpread.onClick.AddListener(WeaponNoSpread_OnClick);
            Transform.onClick.AddListener(Transform_OnClick);
            NoClip.onClick.AddListener(NoClip_OnClick);
            TimeOfDay.onValueChanged.AddListener(TimeOfDay_OnValueChanged);

            AllowPlayers.onClick.AddListener(AllowPlayers_OnClick);
            InstantKill.onClick.AddListener(InstantKill_OnClick);
            SavePlayer.onClick.AddListener(SavePlayer_OnClick);
            LoadPlayer.onClick.AddListener(LoadPlayer_OnClick);
            SaveWorld.onClick.AddListener(SaveWorld_OnClick);
            LoadWorld.onClick.AddListener(LoadWorld_OnClick);
            TriggerAutoSave.onClick.AddListener(TriggerAutoSave_OnClick);
            LoadAutoSave.onClick.AddListener(LoadAutoSave_OnClick);

            
            playerCheats.onClick.AddListener(() => ToggleSubButtons(playerCheats));
            visualCheats.onClick.AddListener(() => ToggleSubButtons(visualCheats));
            weaponCheats.onClick.AddListener(() => ToggleSubButtons(weaponCheats));
            miscCheats.onClick.AddListener(() => ToggleSubButtons(miscCheats));
            spawnerCheats.onClick.AddListener(ToggleSpawnerList);
        }

        public void ToggleSubButtons(Button buttonClicked)
        {
            if (!LeftButtonList.gameObject.activeSelf)
            {
                LeftButtonList.gameObject.SetActive(true);
                if(LeftSpawnerList.gameObject.activeSelf)
                    LeftSpawnerList.gameObject.SetActive(false);
                if(SpawnerView.gameObject.activeSelf)
                    SpawnerView.gameObject.SetActive(false);
                ScrollViewLeft.content = LeftButtonList;
            }
            // Determine the category of the clicked button
            string category = DetermineButtonCategory(buttonClicked);

            // Toggle the respective buttons on and off
            foreach (var button in buttons)
            {
                button.SetActive(button.name.Contains(category));
            }
        }
        
        public void ToggleSpawnerList()
        {
            if (!LeftSpawnerList.gameObject.activeSelf)
            {
                LeftSpawnerList.gameObject.SetActive(true);
                if(LeftButtonList.gameObject.activeSelf)
                    LeftButtonList.gameObject.SetActive(false);
                ScrollViewLeft.content = LeftSpawnerList;
            }
            // TODO: Finish the search and object pooling to re-enable this
           //if (!SpawnerView.gameObject.activeSelf)
           //{
           //    SpawnerView.gameObject.SetActive(true);
           //    if(DefaultRightView.gameObject.activeSelf)
           //        DefaultRightView.gameObject.SetActive(false);
           //}
        }

        private string DetermineButtonCategory(Button button)
        {
            string[] categories = { "Player", "Visual", "Weapon", "Misc" };

            foreach (string category in categories)
            {
                if (button.name.Contains(category))
                {
                    return category;
                }
            }

            // Return an empty string if no match found
            return string.Empty;
        }

        public void Refresh(List<Transform> items)
        {
            foreach (Transform transform1 in items)
            {
                if (transform1)
                {
                    Item component = transform1.GetComponent<Item>();
                    if (component == null)
                        continue;
                    Transform transform2 = GameObject.Instantiate<Transform>(this.spawnerPlaceholder, this.LeftSpawnerList, true);
                    transform2.gameObject.SetActive(true);
                    transform2.localScale = Vector3.one;
                    var placeholderComp = transform2.GetComponent<SpawnerPlaceholder>();
                    if (component.icon)
                        placeholderComp.Icon.texture = component.icon;
                    placeholderComp.DisplayName.text = $"{component.DisplayName} ({component.ItemID})";
                    placeholderComp.InternalName.text = component.GetDisplayDescription();
                    placeholderComp.Cost.text = component.DisplayName;
                    if (component.GetComponent<Equipment>())
                        placeholderComp.Armor.text = component.GetComponent<Equipment>().armor.ToString();
                    if (component.GetComponent<WeaponRaycast>())
                        placeholderComp.Damage.text = component.GetComponent<WeaponRaycast>().damage.ToString(CultureInfo.InvariantCulture);
                    
                }
            }
        }
        
        public void UnlimitedHealth_OnClick()
        {
            unlimitedHealth = !unlimitedHealth;
            unlimitedHealthDescription.color = unlimitedHealth ? GreenTextColor : RedTextColor;
            unlimitedHealthDescription.text = unlimitedHealth ? Act : Inact;
        }

        public void UnlimitedStamina_OnClick()
        {
            unlimitedStamina = !unlimitedStamina;
            unlimitedStaminaDescription.color = unlimitedStamina ? GreenTextColor : RedTextColor;
            unlimitedStaminaDescription.text = unlimitedStamina ? Act : Inact;
            
        }

        public void UnlimitedEnergy_OnClick()
        {
            unlimitedEnergy = !unlimitedEnergy;
            unlimitedEnergyDescription.color = unlimitedEnergy ? GreenTextColor : RedTextColor;
            unlimitedEnergyDescription.text = unlimitedEnergy ? Act : Inact;
        }

        public void UnlimitedOxygen_OnClick()
        {
            unlimitedAir = !unlimitedAir;
            unlimitedOxygenDescription.color = unlimitedAir ? GreenTextColor : RedTextColor;
            unlimitedOxygenDescription.text = unlimitedAir ? Act : Inact;
        }

        public void UnlimitedAmmo_OnClick()
        {
            unlimitedAmmo = !unlimitedAmmo;
            unlimitedAmmoDescription.color = unlimitedAmmo ? GreenTextColor : RedTextColor;
            unlimitedAmmoDescription.text = unlimitedAmmo ? Act : Inact;
        }

        public void FreeBuild_OnClick()
        {
            Utilities.gInst.FreeBuild = !Utilities.gInst.FreeBuild;
            freeBuildDescription.color = Utilities.gInst.FreeBuild ? GreenTextColor : RedTextColor;
            freeBuildDescription.text = Utilities.gInst.FreeBuild ? Act : Inact;
        }

        public void FreeResearch_OnClick()
        {
            Utilities.gInst.UnLockAllResearch = !Utilities.gInst.UnLockAllResearch;
            freeResearchDescription.color = Utilities.gInst.UnLockAllResearch ? GreenTextColor : RedTextColor;
            freeResearchDescription.text = Utilities.gInst.UnLockAllResearch ? Act : Inact;
        }

        public void NoHunger_OnClick()
        {
            noHunger = !noHunger;
            noHungerDescription.color = noHunger ? GreenTextColor : RedTextColor;
            noHungerDescription.text = noHunger ? Act : Inact;
        }

        public void NoThirst_OnClick()
        {
            noThirst = !noThirst;
            noThirstDescription.color = noThirst ? GreenTextColor : RedTextColor;
            noThirstDescription.text = noThirst ? Act : Inact;
        }

        public void PlayerESP_OnClick()
        {
            playerESP = !playerESP;
            playerEspDescription.color = playerESP ? GreenTextColor : RedTextColor;
            playerEspDescription.text = playerESP ? Act : Inact;
        }

        public void EnemyESP_OnClick()
        {
            enemyESP = !enemyESP;
            enemyEspDescription.color = enemyESP ? GreenTextColor : RedTextColor;
            enemyEspDescription.text = enemyESP ? Act : Inact;
        }

        public void FishESP_OnClick()
        {
            fishESP = !fishESP;
            fishEspDescription.color = fishESP ? GreenTextColor : RedTextColor;
            fishEspDescription.text = fishESP ? Act : Inact;
        }

        public void BirdESP_OnClick()
        {
            birdESP = !birdESP;
            birdEspDescription.color = birdESP ? GreenTextColor : RedTextColor;
            birdEspDescription.text = birdESP ? Act : Inact;
        }

        public void LootESP_OnClick()
        {
            lootESP = !lootESP;
            lootEspDescription.color = lootESP ? GreenTextColor : RedTextColor;
            lootEspDescription.text = lootESP ? Act : Inact;
        }

        public void WeaponNoRecoil_OnClick()
        {
            weaponNoRecoil = !weaponNoRecoil;
            weaponNoRecoilDescription.color = weaponNoRecoil ? GreenTextColor : RedTextColor;
            weaponNoRecoilDescription.text = weaponNoRecoil ? Act : Inact;
        }

        public void WeaponNoSpread_OnClick()
        {
            weaponNoSpread = !weaponNoSpread;
            weaponNoSpreadDescription.color = weaponNoSpread ? GreenTextColor : RedTextColor;
            weaponNoSpreadDescription.text = weaponNoSpread ? Act : Inact;
        }

        public void Transform_OnClick()
        {
            transform = !transform;
            transformDescription.color = transform ? GreenTextColor : RedTextColor;
            transformDescription.text = transform ? Act : Inact;
        }

        public void NoClip_OnClick()
        {
            noClip = !noClip;
            noClipDescription.color = noClip ? GreenTextColor : RedTextColor;
            noClipDescription.text = noClip ? Act : Inact;
        }

        public void TimeOfDay_OnValueChanged(float value)
        {
            EnviroSkyMgr.instance.SetTimeOfDay(value);
        }

        public void AllowPlayers_OnClick()
        {
            allowPlayers = !allowPlayers;
            allowPlayersDescription.color = allowPlayers ? GreenTextColor : RedTextColor;
            allowPlayersDescription.text = allowPlayers ? "Allowing player entry" : "Denying player entry";
        }

        public void InstantKill_OnClick()
        {
            if (Utilities.gInst.Player.weaponInHand == null)
                return;
            Utilities.gInst.Player.weaponInHand.damage = 1000000f;
            instantKill = !instantKill;
            instantKillDescription.color = instantKill ? GreenTextColor : RedTextColor;
            instantKillDescription.text = instantKill ? Act : Inact;
        }


        public void SavePlayer_OnClick()
        {
            Utilities.gInst.uiCombat.AddHint(nameof(SavePlayer), Color.red);
            Mainframe.code.SaveManager.SavePlayer();
        }

        public void LoadPlayer_OnClick()
        {
            Utilities.gInst.uiCombat.AddHint(nameof(LoadPlayer), Color.red);
            Mainframe.code.saveManager.LoadLocalPlayer();
        }

        public void SaveWorld_OnClick()
        {
            Utilities.gInst.uiCombat.AddHint(nameof(SaveWorld), Color.red);
            Mainframe.code.SaveManager.SaveWorld();
        }

        public void LoadWorld_OnClick()
        {
            Utilities.gInst.uiCombat.AddHint("Load World", Color.red);
            Mainframe.code.SaveManager.LoadWorldAsync();
        }

        public void TriggerAutoSave_OnClick()
        {
            Utilities.gInst.uiCombat.AddHint("Autosaving", Color.red);
            Utilities.gInst.uiGameMenu.TriggerAutoSave();
        }

        public void LoadAutoSave_OnClick()
        {
            Utilities.gInst.uiCombat.AddHint("Loading Autosave", Color.red);
            Mainframe.code.SaveManager.LoadGame(true);
        }

        public static bool ShouldEsp()
        {
            return enemyESP || playerESP || fishESP || birdESP || lootESP;
        }
    }
}