﻿using PSmash.Inventories;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PSmash.UI
{
    public class ToolItemSlotUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI text = null;
        [SerializeField] Image image = null;

        public void UpdateItemInfo(Equipment.ToolSlot slot)
        {
            //print(actionableItem.name);
            //print(actionableItem.GetNumber());
            image.sprite = slot.item.GetSprite();
            if (text == null)
                return;
            text.enabled = true;
            text.text = slot.number.ToString();
        }
    }

}
