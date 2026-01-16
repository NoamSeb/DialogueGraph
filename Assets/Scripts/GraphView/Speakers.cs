using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Speakers", menuName = "Scriptable Objects/Speakers")]
public class Speakers : ScriptableObject
{
    public List<SpeakerInfo> speakers;
}

[System.Serializable]
public enum Espeaker
{
    None,
    Noam,
    Gab,
    Margot
}



[System.Serializable]
public enum EUIDisposition
{
    classique,
    envers
}