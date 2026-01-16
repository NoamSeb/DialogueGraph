using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;


[Serializable]
public class DSChoiceSaveData
{
    public string NodeID;
    [SerializeField] private string _dropDownKeyChoice;

    [field: SerializeField] public List<ConditionsSC> Conditions { get; set; } = new List<ConditionsSC>();

    public void SaveDropDownKeyChoice(string key) => _dropDownKeyChoice = key;
    public string GetDropDownKeyChoice() => _dropDownKeyChoice;
}

