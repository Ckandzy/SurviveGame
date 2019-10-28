using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CraftingPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Image itemIcon;
    [SerializeField] Text itemIntroduction;
    [SerializeField] List<BaseItemSlot> materialSlotUIs;
    [SerializeField] RectTransform materialSlotUIParent;
    [SerializeField] BaseItemSlot materialSlotUIPrefab;

    [Header("Public Variables")]
    public ItemContainer ItemContainer;

    private CraftingRecipe craftingRecipe;
    public CraftingRecipe CraftingRecipe
    {
        get { return craftingRecipe; }
        set { SetCraftingRecipe(value); }
    }

    public event Action<BaseItemSlot> OnPointerEnterEvent;
    public event Action<BaseItemSlot> OnPointerExitEvent;

    private void OnValidate()
    {
        Init();
        if (craftingRecipe == null)
            gameObject.SetActive(false);
    }

    private void Init()
    {
        materialSlotUIParent.GetComponentsInChildren<BaseItemSlot>(includeInactive: true, result: materialSlotUIs);
    }

    private void SetMaterialSlotEvent(BaseItemSlot materialSlot)
    {
        materialSlot.OnPointerEnterEvent += slot => OnPointerEnterEvent(slot);
        materialSlot.OnPointerExitEvent += slot => OnPointerExitEvent(slot);
    }

    public void OnCraftButtonClick()
    {
        if (craftingRecipe != null && ItemContainer != null)
        {
            craftingRecipe.Craft(ItemContainer);
        }
    }

    private void SetCraftingRecipe(CraftingRecipe newCraftingRecipe)
    {
        craftingRecipe = newCraftingRecipe;

        if (craftingRecipe != null)
        {
            itemIcon.sprite = craftingRecipe.Results[0].Item.Icon;
            UpdateMaterialSlot();
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void UpdateMaterialSlot()
    {
        for (int i = 0; i < craftingRecipe.Materials.Count; i++)
        {
            if (materialSlotUIs.Count == i)
            {
                materialSlotUIs.Add(Instantiate(materialSlotUIPrefab, materialSlotUIParent, false));
            }
            else if (materialSlotUIs[i] == null)
            {
                materialSlotUIs[i] = Instantiate(materialSlotUIPrefab, materialSlotUIParent, false);
            }

            materialSlotUIs[i].Item = craftingRecipe.Materials[i].Item;
            materialSlotUIs[i].Amount = craftingRecipe.Materials[i].Amount;
            SetMaterialSlotEvent(materialSlotUIs[i]);
            materialSlotUIs[i].gameObject.SetActive(true);
        }

        for (int i = CraftingRecipe.Materials.Count; i < materialSlotUIs.Count; i++)
        {
            materialSlotUIs[i].Item = null;
            materialSlotUIs[i].gameObject.SetActive(false);
        }
    }
}
