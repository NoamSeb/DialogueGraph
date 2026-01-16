using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class dialogueContainer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private Image panelBackground;

    public void InitializeDialogueContainer(string dialogue, string speakerName, Sprite backgroundSprite)
    {
        var childContainer = transform.GetChild(0);
        if (childContainer == null) return;
        
        
        dialogueText.SetText(dialogue);
        speakerNameText.SetText(speakerName);
        panelBackground.sprite = backgroundSprite;
    }
}
