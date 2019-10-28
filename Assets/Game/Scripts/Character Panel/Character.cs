using UnityEngine;
using UnityEngine.UI;
using GameLogic.CharacterStats;
using Gamekit2D;

public class Character : MonoBehaviour
{
    [Header("Attribute")]
    public CharacterAttribute health;
    public CharacterAttribute magic;
    public CharacterAttribute power;
    public CharacterAttribute hunger;

    [Header("Stats")]
	public CharacterStat statHealth;
	public CharacterStat statMagic;
	public CharacterStat statPower;
	public CharacterStat statHunger;
    public CharacterStat statDamage;
    public CharacterStat statDefense;

    [Header("Public")]
	public Inventory inventory;
	public EquipmentPanel equipmentPanel;

	[Header("Serialize Field")]
	[SerializeField] CraftingWindow craftingWindow;
	[SerializeField] StatPanel statPanel;
	[SerializeField] ItemTooltip itemTooltip;
	[SerializeField] Image draggableItem;
	[SerializeField] DropItemArea dropItemArea;
	[SerializeField] QuestionDialog reallyDropItemDialog;
	[SerializeField] ItemSaveManager itemSaveManager;

    [Header("Damager&Damageable Reference")]
    public Damager damager;
    public Damageable damageable;

	private BaseItemSlot dragItemSlot;

	private void OnValidate()
	{
		if (itemTooltip == null)
			itemTooltip = FindObjectOfType<ItemTooltip>();
        if (damager == null)
            damager = GetComponent<Damager>();
        if (damageable == null)
            damageable = GetComponent<Damageable>();
	}

	private void Awake()
	{
        #region
        health.cacuValueFunc = () => statHealth.Value;
        health.cacuValueRateFunc = () => statHealth.Value * 0.1f;

        magic.cacuValueFunc = () => statMagic.Value;
        magic.cacuValueRateFunc = () => statMagic.Value * 0.01f;

        power.cacuValueFunc = () => statPower.Value;
        power.cacuValueRateFunc = () => statPower.Value * 0.01f;

        hunger.cacuValueFunc = () => statHunger.Value;
        hunger.cacuValueRateFunc = () => -statHunger.Value * 0.001f;

        health.Init();
        magic.Init();
        power.Init();
        hunger.Init();

        //damager.damageCaculator = () => (int)statHealth.Value;
        #endregion

        statPanel.SetStats(statHealth, statMagic, statPower, statHunger);
		statPanel.UpdateStatValues();

		// Setup Events:
		// Right Click
		inventory.OnRightClickEvent += InventoryRightClick;
		equipmentPanel.OnRightClickEvent += EquipmentPanelRightClick;
		// Pointer Enter
		inventory.OnPointerEnterEvent += ShowTooltip;
		equipmentPanel.OnPointerEnterEvent += ShowTooltip;
		craftingWindow.OnPointerEnterEvent += ShowTooltip;
		// Pointer Exit
		inventory.OnPointerExitEvent += HideTooltip;
		equipmentPanel.OnPointerExitEvent += HideTooltip;
		craftingWindow.OnPointerExitEvent += HideTooltip;
		// Begin Drag
		inventory.OnBeginDragEvent += BeginDrag;
		equipmentPanel.OnBeginDragEvent += BeginDrag;
		// End Drag
		inventory.OnEndDragEvent += EndDrag;
		equipmentPanel.OnEndDragEvent += EndDrag;
		// Drag
		inventory.OnDragEvent += Drag;
		equipmentPanel.OnDragEvent += Drag;
		// Drop
		inventory.OnDropEvent += Drop;
		equipmentPanel.OnDropEvent += Drop;
		dropItemArea.OnDropEvent += DropItemOutsideUI;
	}

	private void Start()
	{
		if (itemSaveManager != null)
		{
			itemSaveManager.LoadEquipment(this);
			itemSaveManager.LoadInventory(this);
		}
	}

    private void Update()
    {
        //属性更新
        if (health.isTick) health.Tick(Time.deltaTime);
        if (power.isTick) power.Tick(Time.deltaTime);
        if (hunger.isTick) hunger.Tick(Time.deltaTime);
    }

    private void OnDestroy()
	{
		if (itemSaveManager != null)
		{
			itemSaveManager.SaveEquipment(this);
			itemSaveManager.SaveInventory(this);
		}
	}

	private void InventoryRightClick(BaseItemSlot itemSlot)
	{
		if (itemSlot.Item is EquippableItem)
		{
			Equip((EquippableItem)itemSlot.Item);
		}
		else if (itemSlot.Item is UsableItem)
		{
			UsableItem usableItem = (UsableItem)itemSlot.Item;
			usableItem.Use(this);

			if (usableItem.IsConsumable)
			{
				itemSlot.Amount--;
				usableItem.Destroy();
			}
		}
	}

	private void EquipmentPanelRightClick(BaseItemSlot itemSlot)
	{
		if (itemSlot.Item is EquippableItem)
		{
			Unequip((EquippableItem)itemSlot.Item);
		}
	}

	private void ShowTooltip(BaseItemSlot itemSlot)
	{
		if (itemSlot.Item != null)
		{
			itemTooltip.ShowTooltip(itemSlot.Item);
		}
	}

	private void HideTooltip(BaseItemSlot itemSlot)
	{
		if (itemTooltip.gameObject.activeSelf)
		{
			itemTooltip.HideTooltip();
		}
	}

	private void BeginDrag(BaseItemSlot itemSlot)
	{
		if (itemSlot.Item != null)
		{
			dragItemSlot = itemSlot;
			draggableItem.sprite = itemSlot.Item.Icon;
			draggableItem.transform.position = Input.mousePosition;
			draggableItem.gameObject.SetActive(true);
		}
	}

	private void Drag(BaseItemSlot itemSlot)
	{
		draggableItem.transform.position = Input.mousePosition;
	}

	private void EndDrag(BaseItemSlot itemSlot)
	{
		dragItemSlot = null;
		draggableItem.gameObject.SetActive(false);
	}

	private void Drop(BaseItemSlot dropItemSlot)
	{
		if (dragItemSlot == null) return;

		if (dropItemSlot.CanAddStack(dragItemSlot.Item))
		{
			AddStacks(dropItemSlot);
		}
		else if (dropItemSlot.CanReceiveItem(dragItemSlot.Item) && dragItemSlot.CanReceiveItem(dropItemSlot.Item))
		{
			SwapItems(dropItemSlot);
		}
	}

	private void AddStacks(BaseItemSlot dropItemSlot)
	{
		int numAddableStacks = dropItemSlot.Item.MaximumStacks - dropItemSlot.Amount;
		int stacksToAdd = Mathf.Min(numAddableStacks, dragItemSlot.Amount);

		dropItemSlot.Amount += stacksToAdd;
		dragItemSlot.Amount -= stacksToAdd;
	}

	private void SwapItems(BaseItemSlot dropItemSlot)
	{
		EquippableItem dragEquipItem = dragItemSlot.Item as EquippableItem;
		EquippableItem dropEquipItem = dropItemSlot.Item as EquippableItem;

		if (dropItemSlot is EquipmentSlot)
		{
			if (dragEquipItem != null) dragEquipItem.Equip(this);
			if (dropEquipItem != null) dropEquipItem.Unequip(this);
		}
		if (dragItemSlot is EquipmentSlot)
		{
			if (dragEquipItem != null) dragEquipItem.Unequip(this);
			if (dropEquipItem != null) dropEquipItem.Equip(this);
		}
		statPanel.UpdateStatValues();

		Item draggedItem = dragItemSlot.Item;
		int draggedItemAmount = dragItemSlot.Amount;

		dragItemSlot.Item = dropItemSlot.Item;
		dragItemSlot.Amount = dropItemSlot.Amount;

		dropItemSlot.Item = draggedItem;
		dropItemSlot.Amount = draggedItemAmount;
	}

	private void DropItemOutsideUI()
	{
		if (dragItemSlot == null) return;

		reallyDropItemDialog.Show();
		BaseItemSlot slot = dragItemSlot;
		reallyDropItemDialog.OnYesEvent += () => DestroyItemInSlot(slot);
	}

	private void DestroyItemInSlot(BaseItemSlot itemSlot)
	{
		// If the item is equiped, unequip first
		if (itemSlot is EquipmentSlot)
		{
			EquippableItem equippableItem = (EquippableItem)itemSlot.Item;
			equippableItem.Unequip(this);
		}

		itemSlot.Item.Destroy();
		itemSlot.Item = null;
	}

	public void Equip(EquippableItem item)
	{
		if (inventory.RemoveItem(item))
		{
			EquippableItem previousItem;
			if (equipmentPanel.AddItem(item, out previousItem))
			{
				if (previousItem != null)
				{
					inventory.AddItem(previousItem);
					previousItem.Unequip(this);
					statPanel.UpdateStatValues();
				}
				item.Equip(this);
				statPanel.UpdateStatValues();
			}
			else
			{
				inventory.AddItem(item);
			}
		}
	}

	public void Unequip(EquippableItem item)
	{
		if (inventory.CanAddItem(item) && equipmentPanel.RemoveItem(item))
		{
			item.Unequip(this);
			statPanel.UpdateStatValues();
			inventory.AddItem(item);
		}
	}

	private ItemContainer openItemContainer;

	private void TransferToItemContainer(BaseItemSlot itemSlot)
	{
		Item item = itemSlot.Item;
		if (item != null && openItemContainer.CanAddItem(item))
		{
			inventory.RemoveItem(item);
			openItemContainer.AddItem(item);
		}
	}

	private void TransferToInventory(BaseItemSlot itemSlot)
	{
		Item item = itemSlot.Item;
		if (item != null && inventory.CanAddItem(item))
		{
			openItemContainer.RemoveItem(item);
			inventory.AddItem(item);
		}
	}

	public void OpenItemContainer(ItemContainer itemContainer)
	{
		openItemContainer = itemContainer;

		inventory.OnRightClickEvent -= InventoryRightClick;
		inventory.OnRightClickEvent += TransferToItemContainer;

		itemContainer.OnRightClickEvent += TransferToInventory;

		itemContainer.OnPointerEnterEvent += ShowTooltip;
		itemContainer.OnPointerExitEvent += HideTooltip;
		itemContainer.OnBeginDragEvent += BeginDrag;
		itemContainer.OnEndDragEvent += EndDrag;
		itemContainer.OnDragEvent += Drag;
		itemContainer.OnDropEvent += Drop;
	}

	public void CloseItemContainer(ItemContainer itemContainer)
	{
		openItemContainer = null;

		inventory.OnRightClickEvent += InventoryRightClick;
		inventory.OnRightClickEvent -= TransferToItemContainer;

		itemContainer.OnRightClickEvent -= TransferToInventory;

		itemContainer.OnPointerEnterEvent -= ShowTooltip;
		itemContainer.OnPointerExitEvent -= HideTooltip;
		itemContainer.OnBeginDragEvent -= BeginDrag;
		itemContainer.OnEndDragEvent -= EndDrag;
		itemContainer.OnDragEvent -= Drag;
		itemContainer.OnDropEvent -= Drop;
	}

	public void UpdateStatValues()
	{
		statPanel.UpdateStatValues();
	}
}
