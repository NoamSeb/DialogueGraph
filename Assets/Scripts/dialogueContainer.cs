using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class dialogueContainer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    public void InitializeDialogueContainer(string dialogue, string speakerName, Sprite characterSprite = null)
    {
        var childContainer = transform.GetChild(0);
        if (childContainer == null) return;
        childContainer.gameObject.SetActive(true);
        
        dialogueText.SetText(dialogue);
        speakerNameText.SetText(speakerName);
    }
    
    public void HideContainer()
    {
        var childContainer = transform.GetChild(0);
        if (childContainer == null) return;
        childContainer.gameObject.SetActive(false);
    }
}
