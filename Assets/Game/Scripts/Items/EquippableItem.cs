using UnityEngine;
using GameLogic.CharacterStats;

public enum EquipmentType
{
	Helmet,
	Chest,
	Boots,
	Weapon1,
	Weapon2,
	Accessory1,
	Accessory2,
}

[CreateAssetMenu(menuName = "Items/Equippable Item")]
public class EquippableItem : Item
{
	public int DamageBonus;
	public int DefenseBonus;
	public int HealthBonus;
	public int MagicBonus;
	[Space]
	public float DamagePercentBonus;
	public float DefensePercentBonus;
	public float HealthPercentBonus;
	public float MagicPercentBonus;
	[Space]
	public EquipmentType EquipmentType;

	public override Item GetCopy()
	{
		return Instantiate(this);
	}

	public override void Destroy()
	{
		Destroy(this);
	}

	public void Equip(Character c)
	{
		if (DamageBonus != 0)
			c.statDamage.AddModifier(new StatModifier(DamageBonus, StatModType.Flat, this));
		if (DefenseBonus != 0)
			c.statDefense.AddModifier(new StatModifier(DefenseBonus, StatModType.Flat, this));
		if (HealthBonus != 0)
			c.statHealth.AddModifier(new StatModifier(HealthBonus, StatModType.Flat, this));
		if (MagicBonus != 0)
			c.statMagic.AddModifier(new StatModifier(MagicBonus, StatModType.Flat, this));

		if (DamagePercentBonus != 0)
			c.statDamage.AddModifier(new StatModifier(DamagePercentBonus, StatModType.PercentMult, this));
		if (DefensePercentBonus != 0)
			c.statDefense.AddModifier(new StatModifier(DefensePercentBonus, StatModType.PercentMult, this));
		if (HealthPercentBonus != 0)
			c.statHealth.AddModifier(new StatModifier(HealthPercentBonus, StatModType.PercentMult, this));
		if (MagicPercentBonus != 0)
			c.statMagic.AddModifier(new StatModifier(MagicPercentBonus, StatModType.PercentMult, this));
	}

	public void Unequip(Character c)
	{
		c.statDamage.RemoveAllModifiersFromSource(this);
		c.statDefense.RemoveAllModifiersFromSource(this);
		c.statHealth.RemoveAllModifiersFromSource(this);
		c.statMagic.RemoveAllModifiersFromSource(this);
	}

	public override string GetItemType()
	{
		return EquipmentType.ToString();
	}

	public override string GetDescription()
	{
		sb.Length = 0;
		AddStat(DamageBonus, "Damage");
		AddStat(DefenseBonus, "Defense");
		AddStat(HealthBonus, "Health");
		AddStat(MagicBonus, "Magic");

		AddStat(DamagePercentBonus, "Damage", isPercent: true);
		AddStat(DefensePercentBonus, "Defense", isPercent: true);
		AddStat(HealthPercentBonus, "Health", isPercent: true);
		AddStat(MagicPercentBonus, "Magic", isPercent: true);

		return sb.ToString();
	}

	private void AddStat(float value, string statName, bool isPercent = false)
	{
		if (value != 0)
		{
			if (sb.Length > 0)
				sb.AppendLine();

			if (value > 0)
				sb.Append("+");

			if (isPercent) {
				sb.Append(value * 100);
				sb.Append("% ");
			} else {
				sb.Append(value);
				sb.Append(" ");
			}
			sb.Append(statName);
		}
	}
}
