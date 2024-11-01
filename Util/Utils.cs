﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AdminMenu.Util
{
    public static class Utilities
    {
        private static Dictionary<int, RingArray> ringDict = new();
        private static GUIStyle __style = new();
        private static GUIStyle __outlineStyle = new();
        private static Texture2D drawingTex;
        private static Color lastTexColour;

        public static Camera CamInst => GInst.player.MainCamera;

        public static PlayerCharacter PCInst => GInst.player;

        public static Storage StorInst => GInst.player.playerStorage;

        public static WeaponBehavior WbInst => PlayerWeapons.code.CurrentWeaponBehaviorComponent;

        public static Global GInst => Global.code;

        public static WorldScene WsInst => WorldScene.code;

        public static FPSPlayer FpspInst => FPSPlayer.code;

        public static RM RmInst => RM.code;

        public static void TurnOnUI()
        {
            Utilities.GInst.OnGUI = true;
            //RPGCamera.code.blur.enabled = Checks.AdminPanelActive() && !Utilities.GInst.dialogueManager.IsInDialogue;
        }

        public static void RemoveDrops()
        {
            // foreach DiscardedItem near the player in a configurable radius, destroy it
            foreach (var item in UnityEngine.Object.FindObjectsOfType<DiscardedItem>())
            {
                if (Vector3.Distance(item.transform.position, Utilities.PCInst.transform.position) <= AdminMenuPlugin.RemoveDropsRadius.Value)
                {
                    if (item.Runner.IsServer || item.Object.HasStateAuthority && item.Object.HasInputAuthority || item.Runner.IsHostPlayer(Global.code.Player.playerDummy.PlayerRef))
                    {
                        // This client has authority to despawn the object
                        item.Runner.Despawn(item.Object);
                    }
                    else
                    {
                        try
                        {
                            item.Runner.Despawn(item.Object);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        public static bool IsOnScreen(Vector3 position) => position.y > 0.00999999977648258 && position.y < Screen.height - 5.0 && position.z > 0.00999999977648258;

        public static GUIStyle StringStyle { get; set; } = new(GUI.skin.label);

        public static Color Color
        {
            get => GUI.color;
            set => GUI.color = value;
        }

        public static void DrawLine(Vector2 from, Vector2 to, float thickness, Color color)
        {
            Color = color;
            DrawLine(from, to, thickness);
        }

        public static void DrawLine(Vector2 from, Vector2 to, float thickness)
        {
            Vector2 normalized = (to - from).normalized;
            double angle = Mathf.Atan2(normalized.y, normalized.x) * 57.2957801818848;
            GUIUtility.RotateAroundPivot((float)angle, from);
            DrawBox(from, Vector2.right * (from - to).magnitude, thickness, false);
            GUIUtility.RotateAroundPivot((float)-angle, from);
        }

        public static void CornerBox(
            Vector2 Head,
            float Width,
            float Height,
            float thickness,
            Color color,
            bool outline)
        {
            int width = (int)(Width / 4.0);
            int height = width;
            if (outline)
            {
                RectFilled((float)(Head.x - Width / 2.0 - 1.0), Head.y - 1f, width + 2, 3f, Color.black);
                RectFilled((float)(Head.x - Width / 2.0 - 1.0), Head.y - 1f, 3f, height + 2, Color.black);
                RectFilled((float)(Head.x + Width / 2.0 - width - 1.0), Head.y - 1f, width + 2, 3f, Color.black);
                RectFilled((float)(Head.x + Width / 2.0 - 1.0), Head.y - 1f, 3f, height + 2, Color.black);
                RectFilled((float)(Head.x - Width / 2.0 - 1.0), (float)(Head.y + (double)Height - 4.0), width + 2, 3f, Color.black);
                RectFilled((float)(Head.x - Width / 2.0 - 1.0), (float)(Head.y + (double)Height - height - 4.0), 3f, height + 2, Color.black);
                RectFilled((float)(Head.x + Width / 2.0 - width - 1.0), (float)(Head.y + (double)Height - 4.0), width + 2, 3f, Color.black);
                RectFilled((float)(Head.x + Width / 2.0 - 1.0), (float)(Head.y + (double)Height - height - 4.0), 3f, height + 3, Color.black);
            }

            RectFilled(Head.x - Width / 2f, Head.y, width, 1f, color);
            RectFilled(Head.x - Width / 2f, Head.y, 1f, height, color);
            RectFilled(Head.x + Width / 2f - width, Head.y, width, 1f, color);
            RectFilled(Head.x + Width / 2f, Head.y, 1f, height, color);
            RectFilled(Head.x - Width / 2f, (float)(Head.y + (double)Height - 3.0), width, 1f, color);
            RectFilled(Head.x - Width / 2f, (float)(Head.y + (double)Height - height - 3.0), 1f, height, color);
            RectFilled(Head.x + Width / 2f - width, (float)(Head.y + (double)Height - 3.0), width, 1f, color);
            RectFilled(Head.x + Width / 2f, (float)(Head.y + (double)Height - height - 3.0), 1f, height + 1, color);
        }

        internal static void RectFilled(float x, float y, float width, float height, Color color)
        {
            if (!drawingTex)
                drawingTex = new Texture2D(1, 1);
            if (color != lastTexColour)
            {
                drawingTex.SetPixel(0, 0, color);
                drawingTex.Apply();
                lastTexColour = color;
            }

            GUI.DrawTexture(new Rect(x, y, width, height), drawingTex);
        }

        public static void DrawBox(
            Vector2 position,
            Vector2 size,
            float thickness,
            Color color,
            bool centered = true)
        {
            Color = color;
            DrawBox(position, size, thickness, centered);
        }

        public static void DrawBox(Vector2 position, Vector2 size, float thickness, bool centered = true)
        {
            if (centered)
            {
                Vector2 vector2 = position - size / 2f;
            }

            GUI.DrawTexture(new Rect(position.x, position.y, size.x, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(position.x, position.y, thickness, size.y), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(position.x + size.x, position.y, thickness, size.y), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(position.x, position.y + size.y, size.x + thickness, thickness), Texture2D.whiteTexture);
        }

        public static void DrawCross(Vector2 position, Vector2 size, float thickness, Color color)
        {
            Color = color;
            DrawCross(position, size, thickness);
        }

        public static void DrawCross(Vector2 position, Vector2 size, float thickness)
        {
            GUI.DrawTexture(new Rect(position.x - size.x / 2f, position.y, size.x, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(position.x, position.y - size.y / 2f, thickness, size.y), Texture2D.whiteTexture);
        }

        public static void DrawDot(Vector2 position, Color color)
        {
            Color = color;
            DrawDot(position);
        }

        public static void DrawDot(Vector2 position) => DrawBox(position - Vector2.one, Vector2.one * 2f, 1f);

        public static void DrawString(
            Vector2 pos,
            string text,
            Color color,
            bool center = true,
            int size = 12,
            FontStyle fontStyle = FontStyle.Bold,
            int depth = 1)
        {
            __style.fontSize = size;
            __style.richText = true;
            __style.normal.textColor = color;
            __style.fontStyle = fontStyle;
            __outlineStyle.fontSize = size;
            __outlineStyle.richText = true;
            __outlineStyle.normal.textColor = new Color(0.0f, 0.0f, 0.0f, 1f);
            __outlineStyle.fontStyle = fontStyle;
            GUIContent content1 = new(text);
            GUIContent content2 = new(text);
            if (center)
                pos.x -= __style.CalcSize(content1).x / 2f;
            switch (depth)
            {
                case 0:
                    GUI.Label(new Rect(pos.x, pos.y, 300f, 25f), content1, __style);
                    break;
                case 1:
                    GUI.Label(new Rect(pos.x + 1f, pos.y + 1f, 300f, 25f), content2, __outlineStyle);
                    GUI.Label(new Rect(pos.x, pos.y, 300f, 25f), content1, __style);
                    break;
                case 2:
                    GUI.Label(new Rect(pos.x + 1f, pos.y + 1f, 300f, 25f), content2, __outlineStyle);
                    GUI.Label(new Rect(pos.x - 1f, pos.y - 1f, 300f, 25f), content2, __outlineStyle);
                    GUI.Label(new Rect(pos.x, pos.y, 300f, 25f), content1, __style);
                    break;
                case 3:
                    GUI.Label(new Rect(pos.x + 1f, pos.y + 1f, 300f, 25f), content2, __outlineStyle);
                    GUI.Label(new Rect(pos.x - 1f, pos.y - 1f, 300f, 25f), content2, __outlineStyle);
                    GUI.Label(new Rect(pos.x, pos.y - 1f, 300f, 25f), content2, __outlineStyle);
                    GUI.Label(new Rect(pos.x, pos.y + 1f, 300f, 25f), content2, __outlineStyle);
                    GUI.Label(new Rect(pos.x, pos.y, 300f, 25f), content1, __style);
                    break;
            }
        }

        public static void DrawCircle(
            Vector2 position,
            float radius,
            int numSides,
            bool centered = true,
            float thickness = 1f)
        {
            DrawCircle(position, radius, numSides, Color.white, centered, thickness);
        }

        public static void DrawCircle(
            Vector2 position,
            float radius,
            int numSides,
            Color color,
            bool centered = true,
            float thickness = 1f)
        {
            RingArray ringArray = !ringDict.ContainsKey(numSides) ? (ringDict[numSides] = new RingArray(numSides)) : ringDict[numSides];
            Vector2 vector2 = centered ? position : position + Vector2.one * radius;
            for (int index = 0; index < numSides - 1; ++index)
                DrawLine(vector2 + ringArray.Positions[index] * radius, vector2 + ringArray.Positions[index + 1] * radius, thickness, color);
            DrawLine(vector2 + ringArray.Positions[0] * radius, vector2 + ringArray.Positions[ringArray.Positions.Length - 1] * radius, thickness, color);
        }

        public static bool IsEnemyVisible(Vector3 position, Vector3 target)
        {
            Vector3 origin = position;
            Vector3 direction = target - origin;
            float num1 = direction.magnitude * 0.9f;
            Ray ray = new(origin, direction);
            const int num2 = 512;
            const int num3 = 2048;
            const int num4 = 4;
            const int num5 = 65536;
            const int num6 = 131072;
            const int num7 = 1048576;
            const int num8 = 8388608;
            const int num9 = 16;
            const int num10 = 256;
            const int num11 = ~(num2 | num3 | num4 | num5 | num6 | num7 | num9 | num8 | num10);
            double maxDistance = num1;
            return Physics.Raycast(ray, (float)maxDistance, num11, QueryTriggerInteraction.UseGlobal);
        }

        public static string NameReplacer(string pText)
        {
            pText = pText.Replace("(Clone)", "");
            return new Regex("", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant).Replace(pText, string.Empty);
        }

        public static string WeaponReplacer(string pText)
        {
            pText = pText.Replace("(WeaponRaycast)", "").Replace("A1_", "").Replace("A2_", "").Replace("A3_", "").Replace("A4_", "").Replace("A5_", "").Replace("B2_", "").Replace("C1_", "").Replace("C2_", "").Replace("G1_", "");
            return new Regex("", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant).Replace(pText, string.Empty);
        }

        public static string LootReplacer(string pText)
        {
            pText = pText.Replace("0", "").Replace("1", "").Replace("2", "").Replace("3", "").Replace("4", "").Replace("5", "").Replace("6", "").Replace("7", "").Replace("8", "").Replace("9", "").Replace("(", "").Replace(")", "").Replace("_", "");
            return new Regex("", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant).Replace(pText, string.Empty);
        }

        internal static float GetKey(KeyCode key) => !Input.GetKey(key) ? 0.0f : 1f;


        public static void CustomUseItem(Item item, bool isQuickSlot = false)
        {
            if (GInst.player.IsBusy)
                return;
            FPSPlayer.code.SwimmerMove = false;
            PlayerWeapons.code.StartWithQuickSlot = false;
            Global.code.uiCombat.HideHint();
            if (!item)
            {
                if (GInst.player.weaponInHand & isQuickSlot) PlayerWeapons.code.HolsterCurrentWeapon();
            }
            else
            {
                if (item != null)
                {
                    if (item.TryGetComponent<Food>(out var component))
                    {
                        if (component.bleedAmount > 0.0 && GInst.player.Bleeding <= 0.0)
                            return;
                        GInst.player.Hunger += component.foodAmount;
                        GInst.player.Thirst += component.waterAmount;
                        GInst.player.Energy += component.foodAmount * 0.2f;
                        GInst.player.Energy += component.waterAmount * 0.2f;
                        GInst.player.Stamina += component.energyAmount;
                        GInst.player.Stamina += component.staminaAmount;
                        GInst.player.Energy += component.energyAmount;
                        GInst.player.Bleeding -= component.bleedAmount;
                        if (!component.Buff.localizedItemName.IsEmpty)
                        {
                            Utility.Instantiate<BuffInstance>(RM.code.BuffPrefab).GetComponent<BuffInstance>().InitBuff(component);
                        }

                        GInst.player.AddHealth(component.healthAmount, 3, Vector3.zero, false, false, false);
                        if (component.poisonAmount > 0)
                        {
                            GInst.player.AddHealth(-component.poisonAmount, 2, Vector3.zero, false, false, false);
                            RM.code.PlayOneShot(GInst.player.sndFoodPoisoning, 1f);
                        }

                        GInst.player.UpdateBuffHint();
                        --item.Amount;
                        if (component.itemReceivedAfterEat && UnityEngine.Random.Range(0, 100) < component.itemReceiveChance)
                        {
                            GInst.player.playerStorage.TryAddItem(Utility.Instantiate(component.itemReceivedAfterEat));
                        }

                        if (item.sndUse)
                        {
                            RM.code.PlayOneShot(item.sndUse, UnityEngine.Random.Range(0.9f, 1.1f));
                        }
                        else if (component.foodAmount > 0)
                        {
                            RM.code.PlayOneShot(GInst.player.sndEat, UnityEngine.Random.Range(0.9f, 1.1f));
                        }
                        else if (component.waterAmount > 0.0)
                        {
                            RM.code.PlayOneShot(GInst.player.sndDrink, UnityEngine.Random.Range(0.9f, 1.1f));
                        }
                        else if (component.healthAmount > 0)
                        {
                            RM.code.PlayOneShot(GInst.player.sndUseBandage, UnityEngine.Random.Range(0.9f, 1.1f));
                        }
                        //gInst.player.StartCoroutine(gInst.player.RefreshInventoryUI());
                    }
                    else if (item.GetComponent<WeaponRaycast>())
                    {
                        GInst.player.QuitConnectWire();
                        if (GInst.player.MyBuildController.m_BuildingHelpers.m_CurrentPreview)
                            GInst.player.MyBuildController.SetSelectedPiece(null);
                        bool flag = !(FPSRigidBodyWalker.code.isUnderWater && item.ItemID == RM.code.Torch.ItemID);
                        if (flag)
                        {
                            if (GInst.player.weaponInHand && GInst.player.weaponInHand._item == item && PlayerWeapons.code.CurrentWeaponBehaviorComponent
                                && PlayerWeapons.code.CurrentWeaponBehaviorComponent.WeaponItem.ItemID == item.ItemID && PlayerWeapons.code.CurrentWeaponBehaviorComponent.InitDone)
                            {
                                PlayerWeapons.code.HolsterCurrentWeapon();
                            }
                            else
                            {
                                PlayerWeapons.code.StartWithQuickSlot = isQuickSlot;
                                PlayerWeapons.code.SelectWeaponByPrefab(item.transform);
                            }

                            Global.code.uiCombat.HideFishPenal();
                        }

                        GInst.player.CS();
                    }
                    else if (item.TryGetComponent<BuildingPiece>(out BuildingPiece _))
                    {
                        if (RM.code.ItemDictionary.TryGetValue(item.ItemID, out Item obj))
                        {
                            GInst.player.CanSnap = false;
                            GInst.player.MyBuildController.SetSelectedPiece(obj.GetComponent<BuildingPiece>());
                            if (Global.code.uiInventory.gameObject.activeSelf)
                                Global.code.uiInventory.Close();
                            GInst.player.DecorationPieceID = item.ItemID;
                            if (!isQuickSlot)
                                GInst.player.Invoke("ChangeInventoryState", 0.01f);
                        }
                        else
                            GInst.player.LogError($"ItemID {item.ItemID} does not exist");
                    }
                    else
                    {
                        if (item.TryGetComponent<BuildingContainer>(out BuildingContainer component1))
                        {
                            BuildingPiece buildPiece = component1.GetBuildPiece();
                            if (buildPiece)
                            {
                                GInst.player.CanSnap = false;
                                GInst.player.MyBuildController.SetSelectedPiece(buildPiece);
                                if (Global.code.uiInventory.gameObject.activeSelf)
                                    Global.code.uiInventory.Close();
                                GInst.player.curBuildingContainer = component1;
                                if (!isQuickSlot)
                                    GInst.player.Invoke("ChangeInventoryState", 0.01f);
                            }
                        }
                        else
                        {
                            if (item.TryGetComponent<Blueprint>(out Blueprint component2) && GlobalDataHelper.IsGlobalDataValid && Mainframe.code.GlobalData.AddLearnedBlueprint(item.ItemID))
                            {
                                if (Global.code)
                                    Global.code.uiCombat.OpenBlueprintPenal(component2, Global.code.uiInventory.gameObject.activeSelf);

                                UniTaskVoid uniTaskVoid;
                                uniTaskVoid = GInst.player.RefreshInventoryUI();
                                uniTaskVoid.Forget();
                            }
                        }
                    }

                    if (GInst.player.weaponInHand)
                        return;
                    Global.code.uiCombat.tmpAmmo.text = "∞";
                }
            }
        }

        public static Transform? FindChild(Transform aParent, string aName, IterativeSearchType searchType = IterativeSearchType.DepthFirst)
        {
            switch (searchType)
            {
                case IterativeSearchType.DepthFirst:
                    Stack<Transform> transformStack = new Stack<Transform>();
                    Transform child1 = aParent;
                    do
                    {
                        for (int index = child1.childCount - 1; index >= 0; --index)
                            transformStack.Push(child1.GetChild(index));
                        if (transformStack.Count > 0)
                            child1 = transformStack.Pop();
                        else
                            goto label_8;
                    } while (child1.name != aName);

                    return child1;
                    label_8:
                    return null;
                case IterativeSearchType.BreadthFirst:
                    Queue<Transform> transformQueue = new Queue<Transform>();
                    Transform child2 = aParent;
                    do
                    {
                        int childCount = child2.childCount;
                        for (int index = 0; index < childCount; ++index)
                            transformQueue.Enqueue(child2.GetChild(index));
                        if (transformQueue.Count > 0)
                            child2 = transformQueue.Dequeue();
                        else
                            goto label_16;
                    } while (child2.name != aName);

                    return child2;
                    label_16:
                    return null;
                default:
                    AdminMenuPlugin.AdminMenuLogger.LogError((object)"Search type not implemented!");
                    return null;
            }
        }

        public enum IterativeSearchType
        {
            DepthFirst,
            BreadthFirst,
        }

        private class RingArray
        {
            public Vector2[] Positions { get; private set; }

            public RingArray(int numSegments)
            {
                Positions = new Vector2[numSegments];
                float num = 360f / numSegments;
                for (int index = 0; index < numSegments; ++index)
                {
                    float f = (float)Math.PI / 180f * num * index;
                    Positions[index] = new Vector2(Mathf.Sin(f), Mathf.Cos(f));
                }
            }
        }
    }
}