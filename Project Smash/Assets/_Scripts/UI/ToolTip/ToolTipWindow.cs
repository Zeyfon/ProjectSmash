﻿using PSmash.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PSmash.Inventories;
using PSmash.CraftingSystem;
using PSmash.UI.CraftingSytem;
using TMPro;

namespace PSmash.UI
{
    public class ToolTipWindow : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup = null;
        [SerializeField] float fadeOutTime = 0.25f;
        [SerializeField] float fadeInTime = 1;
        [SerializeField] ToolTipCraftingItemsHandler requiredMaterialsUpdater = null;
        [SerializeField] TextMeshProUGUI descriptionText = null;
        [SerializeField] TextMeshProUGUI nameText = null;

        Coroutine coroutine;
        bool isInitialized = false;
        private void Awake()
        {
            CraftingSystemUI.onSelectionChange += UpdateToolTipInfo;

        }

        private void OnDestroy()
        {
            CraftingSystemUI.onSelectionChange -= UpdateToolTipInfo;

        }

        public void UpdateToolTipInfo(SkillSlot skillSlot)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
            if (skillSlot == null)
                Debug.LogWarning("The skillSlot selected is empty");

            if (isInitialized)
            {
                coroutine = StartCoroutine(InfoUpdate(skillSlot));
            }
            isInitialized = true;
        }

        IEnumerator InfoUpdate(SkillSlot skillSlotGameObject)
        {
            Fader fader = new Fader();
            yield return fader.FadeIn(canvasGroup, fadeOutTime);

            yield return InfoChange(skillSlotGameObject);
            yield return fader.FadeOut(canvasGroup, fadeInTime);
        }

        IEnumerator InfoChange(SkillSlot skillSlot)
        {
            if (skillSlot == null)
                Debug.LogWarning("The skillSlot selected is empty");
            transform.position = skillSlot.transform.position;
            Dictionary<CraftingItem, int> requiredCraftingMaterials;
            requiredCraftingMaterials = skillSlot.GetCraftingItemsRequirement();
            descriptionText.text = skillSlot.GetSkill().GetDescription();
            nameText.text = skillSlot.GetSkill().GetItem().GetDisplayName();
            //requiredMaterialsUpdater.SetSkillSlotInfo(requiredCraftingMaterials);
            yield return null;
        }
    }
}
