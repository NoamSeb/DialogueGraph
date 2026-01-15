using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


    [Serializable]
    public class DSChoiceSaveData
    {
        [field: SerializeField] public string Text { get; set; }
        [field: SerializeField] public string NodeID { get; set; }
    }
