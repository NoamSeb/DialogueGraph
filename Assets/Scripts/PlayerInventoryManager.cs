using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class PlayerInventoryManager : MonoBehaviour
{
    
    public static PlayerInventoryManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    [Header("PLAYER INVENTORY")]
    public List<ConditionsSC> inventoryItems = new List<ConditionsSC>();

    public bool DoesPlayerFillCondition(ConditionsSC condition)
    {
        foreach (ConditionsSC playerItem in inventoryItems)
        {
            if (playerItem.conditionItem == condition.conditionItem)
            {
                Debug.Log($"Comparing Player Item: {playerItem.conditionItem} with Condition Item: {condition.conditionItem}");
                if (playerItem.conditionValue >= condition.conditionValue)
                {
                    Debug.Log($"Player fills condition: {condition.conditionItem} with value {condition.conditionValue}");
                    return true;
                }
                return false;
            }
        }
        return false;
    }
    
}
