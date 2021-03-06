﻿using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace EquipmentAndQuickSlots
{
    //private void UpdateGui(Player player, ItemDrop.ItemData dragItem)
    [HarmonyPatch(typeof(InventoryGrid), "UpdateGui", new Type[] {typeof(Player), typeof(ItemDrop.ItemData)})]
    public static class InventoryGrid_UpdateGui_Patch
    {
        private static void Postfix(InventoryGrid __instance, Player player, ItemDrop.ItemData dragItem, List<InventoryGrid.Element> ___m_elements)
        {
            if (__instance.name != "PlayerGrid")
            {
                return;
            }

            if (EquipmentAndQuickSlots.QuickSlotsEnabled.Value)
            {
                var quickSlotBkg = GetOrCreateBackground(__instance, "QuickSlotBkg");
                quickSlotBkg.anchoredPosition = new Vector2(480, -173);
                quickSlotBkg.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 240);
                quickSlotBkg.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 90);
                quickSlotBkg.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                var existingBkg = __instance.transform.parent.Find("QuickSlotBkg");
                if (existingBkg != null)
                {
                    GameObject.Destroy(existingBkg.gameObject);
                }
            }

            if (EquipmentAndQuickSlots.EquipmentSlotsEnabled.Value)
            {
                var equipmentBkg = GetOrCreateBackground(__instance, "EquipmentBkg");
                equipmentBkg.anchoredPosition = new Vector2(485, 10);
                equipmentBkg.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 210);
                equipmentBkg.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 260);
                equipmentBkg.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                var existingBkg = __instance.transform.parent.Find("EquipmentBkg");
                if (existingBkg != null)
                {
                    GameObject.Destroy(existingBkg.gameObject);
                }
            }

            float horizontalSpacing = __instance.m_elementSpace + 10;
            float verticalSpacing = __instance.m_elementSpace + 10;
            string[] equipNames = { "Head", "Chest", "Legs", "Shoulders", "Utility" };
            Vector2[] equipPositions = {
                new Vector2(), // Head
                new Vector2(0, -verticalSpacing), // Chest
                new Vector2(0, -2 * verticalSpacing), // Legs
                new Vector2(horizontalSpacing, -0.5f * verticalSpacing), // Shoulders
                new Vector2(horizontalSpacing, -1.5f * verticalSpacing), // Utility
            };

            var y = EquipmentAndQuickSlots.GetBonusInventoryRowIndex();
            for (int i = 0; i < EquipmentAndQuickSlots.EquipSlotCount; ++i)
            {
                var x = i;
                var element = GetElement(___m_elements, x, y);

                if (!EquipmentAndQuickSlots.EquipmentSlotsEnabled.Value)
                {
                    element.m_go.SetActive(false);
                    continue;
                }
                else
                {
                    element.m_go.SetActive(true);
                }

                var bindingText = element.m_go.transform.Find("binding").GetComponent<Text>();
                bindingText.enabled = true;
                bindingText.horizontalOverflow = HorizontalWrapMode.Overflow;
                bindingText.text = equipNames[i];
                bindingText.rectTransform.anchoredPosition = new Vector2(32, 5);

                Vector2 offset = new Vector2(692, -20);
                (element.m_go.transform as RectTransform).anchoredPosition = offset + equipPositions[i];
            }

            for (int i = 0; i < EquipmentAndQuickSlots.QuickUseSlotCount; ++i)
            {
                var x = EquipmentAndQuickSlots.QuickUseSlotIndexStart + i;
                var element = GetElement(___m_elements, x, y);
                if (EquipmentAndQuickSlots.QuickSlotsEnabled.Value)
                {
                    element.m_go.SetActive(true);
                    var bindingText = element.m_go.transform.Find("binding").GetComponent<Text>();
                    bindingText.enabled = true;
                    bindingText.horizontalOverflow = HorizontalWrapMode.Overflow;
                    bindingText.text = EquipmentAndQuickSlots.GetBindingLabel(i);

                    Vector2 offset = new Vector2(310, 1);
                    Vector2 position = new Vector2(x * __instance.m_elementSpace, 4 * -__instance.m_elementSpace);
                    (element.m_go.transform as RectTransform).anchoredPosition = offset + position;
                }
                else
                {
                    element.m_go.SetActive(false);
                }
            }
        }

        private static RectTransform GetOrCreateBackground(InventoryGrid __instance, string name)
        {
            var existingBkg = __instance.transform.parent.Find(name);
            if (existingBkg == null)
            {
                var bkg = __instance.transform.parent.Find("Bkg").gameObject;
                var background = GameObject.Instantiate(bkg, bkg.transform.parent);
                background.name = name;
                background.transform.SetSiblingIndex(bkg.transform.GetSiblingIndex() + 1);
                existingBkg = background.transform;
            }

            return existingBkg as RectTransform;
        }

        private static InventoryGrid.Element GetElement(List<InventoryGrid.Element> elements, int x, int y)
        {
            var inventoryWidth = Player.m_localPlayer.GetInventory().GetWidth();
            return elements[y * inventoryWidth + x];
        }
    }

    
}
