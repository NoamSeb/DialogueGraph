using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


[Serializable]
public class SpeakerInfo
{
    public const string AssetExtension = "simpleg";
    
    public string Name;
    public Espeaker speakEnum;
    public HumeurSpeaker Humeur;
    public List<SpriteHumeur> SpritesHumeur;
    
}

[System.Serializable]
public class SpriteHumeur
{
    public Sprite sprite;
    public HumeurSpeaker humeur;
}

public enum HumeurSpeaker
{
    Neutre,
    Joie,
    Triste
}
