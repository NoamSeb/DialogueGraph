using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class MyNode : Node
{
    public string dialogueName;
    public string skipStateNode;
    public List<string> dialogueChoices = new List<string>();
    public string dialogueText;
    
    // REFERENCE
    
    private TextField _stateNodeTextField = new TextField("NODE STATE ======>");
    private Button _addButtonChoice = new Button() { text = "+" };
    
    private bool _isSkipNode = false;

    public virtual void Initialize(Vector2 postionSpawn)
    {
        dialogueName = "Dialogue Name";
        dialogueText = "This is the dialogue text.";
        
        SetPosition(new Rect(postionSpawn, Vector2.zero));
        SetupBaseChoices(dialogueChoices);
        
        ApplyStylesToNode();
    }
    
    private void ApplyStylesToNode()
    {
        this.AddToClassList("ds-node");
        titleContainer.AddToClassList("ds-node__title-container");
        inputContainer.AddToClassList("ds-node__input-container");
        outputContainer.AddToClassList("ds-node__output-container");
        mainContainer.AddToClassList("ds-node__main-container");
        extensionContainer.AddToClassList("ds-node__extension-container");
    }
    
    public virtual void Draw()
    {
        /* MAIN CONTAINER */
        
        _addButtonChoice.clicked += AddChoice;
        
        mainContainer.Insert(1,_addButtonChoice);

        
        /* TITLE CONTAINER */
        
        
        TextField dialogueNameTextField = new TextField()
        {
            value = dialogueName,
        };
        
        dialogueNameTextField.AddToClassList("ds-node__textfield");
        
        titleContainer.Insert(0, dialogueNameTextField);
        
        /* MAIN CONTAINER */
        
        Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
        inputPort.portName = "Dialogue Connection";

        Button switchNodeType = new Button() { text = "SwitchNodeType" };
        switchNodeType.clicked += SwitchNodeType;
        
        SwitchNodeType();
        
        
        inputContainer.Insert(0, _stateNodeTextField);
        inputContainer.Add(switchNodeType);
        inputContainer.Add(inputPort);
        inputContainer.Add(inputPort);
        
        
        
        /* EXTENSION CONTAINER */
        
        VisualElement customDataContainer = new VisualElement();
        
        Foldout textFoldout = new Foldout()
        {
            text = "Foldout Text"
        };

        TextField textField = new TextField()
        {
            value = dialogueText
        };
        
        textFoldout.Add(textField);
        
        customDataContainer.Add(textFoldout);
        
        extensionContainer.Add(customDataContainer);
        
        ShowChoices();
        
        RefreshExpandedState();
    }

    
    private void SwitchNodeType()
    {
        dialogueChoices.Clear();
        outputContainer.Clear();
        RefreshExpandedState();
        
        if (!_isSkipNode)
        {
            _stateNodeTextField.value = "SKIP";
            mainContainer.Remove(_addButtonChoice);

            AddChoice("Continue");
        }
        else
        {
            _stateNodeTextField.value = "CHOICE";
            mainContainer.Insert(1,_addButtonChoice);
        }
        _isSkipNode = !_isSkipNode;
    }
    
    private void AddChoice(string choiceText)
    {
        if (_isSkipNode) return;
        dialogueChoices.Add(choiceText);
        ShowChoices();
        RefreshExpandedState();
    }    private void AddChoice()
    {
        if (_isSkipNode) return;
        dialogueChoices.Add("New Choice");
        ShowChoices();
        RefreshExpandedState();
    }
    
    
    
    public virtual void SetupBaseChoices(List<string> choices)
    {
        dialogueChoices = choices;
    }

    private void DeleteChoice(Port choicePort)
    {
        int index = outputContainer.IndexOf(choicePort);
        if (index >= 0 && index < dialogueChoices.Count)
        {
            dialogueChoices.RemoveAt(index);
            outputContainer.Remove(choicePort);
            RefreshExpandedState();
        }
    }
    
    private void ShowChoices()
    {
        outputContainer.Clear();
        foreach (var choice in dialogueChoices)
        {
            Port choicePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            choicePort.portName = "";
            
            outputContainer.Add(choicePort);

            Button deleteChoiceButton = new Button() { text = "X" };
            TextField choiceTextField = new TextField()
            {
                value = choice
            };
            
            deleteChoiceButton.clicked += () => { DeleteChoice(choicePort); };
            
            choiceTextField.AddToClassList("ds-node__textfield");
            choiceTextField.AddToClassList("ds-node__choice-textfield");
            choiceTextField.AddToClassList("ds-node__textfield__hidden");
            
            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);
        }

        RefreshExpandedState();
    }

}
