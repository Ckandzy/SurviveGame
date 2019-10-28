using System;
using System.Collections.Generic;
using UnityEngine;

public class CraftingWindow : MonoBehaviour
{
	[Header("References")]
	[SerializeField] CraftingRecipeUI recipeUIPrefab;
	[SerializeField] RectTransform recipeUIParent;
    [Space]
    [SerializeField] CraftingPanel craftingPanel;
    [SerializeField] List<CraftingRecipeUI> craftingRecipeUIs;

	[Header("Public Variables")]
	public ItemContainer ItemContainer;
	public List<CraftingRecipe> CraftingRecipes;

	public event Action<BaseItemSlot> OnPointerEnterEvent;
	public event Action<BaseItemSlot> OnPointerExitEvent;

	private void OnValidate()
	{
		Init();
	}

	private void Start()
	{
		Init();

		foreach (CraftingRecipeUI craftingRecipeUI in craftingRecipeUIs)
		{
            SetCraftingRecipeUIEvent(craftingRecipeUI);
        }
    }

	private void Init()
	{
		recipeUIParent.GetComponentsInChildren<CraftingRecipeUI>(includeInactive: true, result: craftingRecipeUIs);
		UpdateCraftingRecipes();
        craftingPanel.OnPointerEnterEvent += slot => OnPointerEnterEvent(slot);
        craftingPanel.OnPointerExitEvent += slot => OnPointerExitEvent(slot);
    }

    private void SetCraftingRecipeUIEvent(CraftingRecipeUI craftingRecipeUI)
    {
        craftingRecipeUI.OnPointerEnterEvent += slot => OnPointerEnterEvent(slot);
        craftingRecipeUI.OnPointerExitEvent += slot => OnPointerExitEvent(slot);
        craftingRecipeUI.OnPointerLeftClickEvent += SelectRecipe;
    }

    private void SelectRecipe(CraftingRecipe recipe)
    {
        craftingPanel.CraftingRecipe = recipe;
        craftingPanel.ItemContainer = ItemContainer;
    }

	public void UpdateCraftingRecipes()
	{
		for (int i = 0; i < CraftingRecipes.Count; i++)
		{
			if (craftingRecipeUIs.Count == i)
			{
				craftingRecipeUIs.Add(Instantiate(recipeUIPrefab, recipeUIParent, false));
			}
			else if (craftingRecipeUIs[i] == null)
			{
				craftingRecipeUIs[i] = Instantiate(recipeUIPrefab, recipeUIParent, false);
			}

			craftingRecipeUIs[i].ItemContainer = ItemContainer;
			craftingRecipeUIs[i].CraftingRecipe = CraftingRecipes[i];

            SetCraftingRecipeUIEvent(craftingRecipeUIs[i]);
        }

		for (int i = CraftingRecipes.Count; i < craftingRecipeUIs.Count; i++)
		{
			craftingRecipeUIs[i].CraftingRecipe = null;
		}
	}
}
