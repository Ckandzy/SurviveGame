using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightReward : MonoBehaviour
{
    public List<Reward> rewards;

    public void DropReward()
    {
        foreach (var reward in rewards)
        {
            if (UnityEngine.Random.Range(0f, 1f) < reward.probability)
            {
                ItemDropInScene itemDropInScene = Instantiate(reward.itemAmount.Item.itemDropInSceneOBJ, transform.position + new Vector3(0, 1, 0), Quaternion.identity).GetComponentInChildren<ItemDropInScene>();
                itemDropInScene.item = reward.itemAmount.Item;
                itemDropInScene.amount = reward.itemAmount.Amount;
            }
        }
    }
}

[Serializable]
public class Reward
{
    public ItemAmount itemAmount;
    public float probability;
}
