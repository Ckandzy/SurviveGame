using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CraftingRecipeUI : MonoBehaviour, IPointerClickHandler
{
	[Header("References")]
	[SerializeField] BaseItemSlot itemSlot;
    [SerializeField] Text itemName;

    [Header("Public Variables")]
	public ItemContainer ItemContainer;

	private CraftingRecipe craftingRecipe;
	public CraftingRecipe CraftingRecipe {
		get { return craftingRecipe; }
		set { SetCraftingRecipe(value); }
	}

	public event Action<BaseItemSlot> OnPointerEnterEvent;
	public event Action<BaseItemSlot> OnPointerExitEvent;
    public event Action<CraftingRecipe> OnPointerLeftClickEvent;

    private void OnValidate()
	{
		itemSlot = GetComponentInChildren<BaseItemSlot>(includeInactive: true);
	}


	private void Start()
	{
        itemSlot.OnPointerEnterEvent += slot => OnPointerEnterEvent(slot);
        itemSlot.OnPointerExitEvent += slot => OnPointerExitEvent(slot);
    }

    private void SetCraftingRecipe(CraftingRecipe newCraftingRecipe)
	{
		craftingRecipe = newCraftingRecipe;

		if (craftingRecipe != null)
		{
            SetResultSlot(craftingRecipe.Results[0]);
            gameObject.SetActive(true);
		}
		else
		{
			gameObject.SetActive(false);
		}
	}

	private void SetResultSlot(ItemAmount itemAmount)
	{
        itemSlot.Item = itemAmount.Item;
        itemSlot.Amount = itemAmount.Amount;

        itemName.text = itemSlot.Item.ItemName;

        itemSlot.transform.parent.gameObject.SetActive(true);
	}

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData != null && eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log(craftingRecipe.name);
            OnPointerLeftClickEvent?.Invoke(craftingRecipe);
        }
    }
}
