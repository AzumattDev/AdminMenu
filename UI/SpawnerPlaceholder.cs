using AdminMenu;
using AdminMenu.Util;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpawnerPlaceholder : MonoBehaviour
{
    public static SpawnerPlaceholder Instance = null!;
    public ExtButton Btn = null!;
    public RawImage Icon = null!;
    public Image Bkg = null!;
    public HorizontalLayoutGroup AdditionalInfoHlg = null!;
    public Text DisplayName = null!;
    public Text InternalName = null!;
    public Text Cost = null!;
    public Text Damage = null!;
    public Text Armor = null!;
    public Text Level = null!;

    [Header("Spawner View Right")] 
    public GameObject SpawnerViewRight = null!;
    public TMP_InputField Search = null!;
    public TMP_InputField Amount = null!;
    public Text AmountText = null!;
    public Text ShowFavoritesText = null!;
    public Text ShowFavoritesDescription = null!;
    public Toggle ShowFavorites = null!;
    public Text PutInInventoryText = null!;
    public Text PutInInventoryDescription = null!;
    public Toggle PutInInventory = null!;


    public void Awake()
    {
        Instance = this;
        ShowFavorites.onValueChanged.AddListener(ShowFavoritesChanged);
        PutInInventory.onValueChanged.AddListener(PutInInventoryChanged);
        Btn.OnRightClick += RightClickItem;
    }

    /* Doing this for now until fully implemented*/
    public void Update()
    {
        PutInInventory.isOn = true;
    }


    // Create onClick event for the Btn that spawns the item based on the internal name
    public void SpawnItem()
    {
        AdminMenuPlugin.AdminMenuLogger.LogInfo($"SpawnItem: {DisplayName.text}");

        if (PutInInventory == null || !PutInInventory.isOn)
            return;

        if (RM.code == null || RM.code.allItems == null)
        {
            AdminMenuPlugin.AdminMenuLogger.LogError("RM or its properties are null!");
            return;
        }

        int amountToSpawn = 1;
        if (Amount != null && int.TryParse(Amount.text, out int parsedAmount))
        {
            amountToSpawn = parsedAmount;
        }

        foreach (Transform allItemsItem in RM.code.allItems.items)
        {
            if (allItemsItem != null && allItemsItem.TryGetComponent<Item>(out Item? item))
            {
                if (item.DisplayName == Cost.text)
                {
                    if (Global.code.Player.playerStorage != null)
                    {
                        for (int i = 0; i < amountToSpawn; ++i)
                        {
                           if (!Global.code.Player.playerStorage.AddItem(Utility.Instantiate<Item>(item), true))
                            Global.code.Player.quickSlotStorage.AddItem(Utility.Instantiate<Item>(item), true);
                        }
                        return;
                    }
                    else
                    {
                        AdminMenuPlugin.AdminMenuLogger.LogError("playerStorage is null!");
                    }
                }
            }
        }
    }


    public void RightClickItem()
    {
        if (string.IsNullOrWhiteSpace(Cost.text))
            return;
        foreach (Transform allItemsItem in RM.code.allItems.items)
        {
            if (allItemsItem != null && allItemsItem.TryGetComponent<Item>(out Item? item2))
            {
                if (!item2) continue;
                if (item2.DisplayName != Cost.text) continue;
                try
                {
                    Utilities.CustomUseItem(item2, false);
                    //Global.code.Player.UseItem(item2);
                }
                catch
                {
                    AdminMenuPlugin.AdminMenuLogger.LogInfo($"Failed to use item: {Cost.text}");
                }


                return;
            }
        }
    }

    public void ShowFavoritesChanged(bool value)
    {
        ShowFavorites.isOn = value;
        ShowFavoritesDescription.text = value ? AdminUI.Act : AdminUI.Inact;
        ShowFavoritesDescription.color = value ? AdminUI.GreenTextColor : AdminUI.RedTextColor;
    }

    public void PutInInventoryChanged(bool value)
    {
        PutInInventory.isOn = value;
        PutInInventoryDescription.text = value ? AdminUI.Act : AdminUI.Inact;
        PutInInventoryDescription.color = value ? AdminUI.GreenTextColor : AdminUI.RedTextColor;
    }
}

[RequireComponent(typeof(Button))]
public class ExtButton : Button
{
    public delegate void RightClickAction();

    public event RightClickAction OnRightClick = null!;

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick?.Invoke();
        }
        else
        {
            base.OnPointerClick(eventData);
        }
    }
}