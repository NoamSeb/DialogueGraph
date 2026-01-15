using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;


[Serializable]
    public class DSChoiceSaveData
    {
        public DSChoiceSaveData()
        {
        }
        
        [field: SerializeField] public string DropDownKey { get; set; }
        [field: SerializeField] public string NodeID { get; set; }

    }
