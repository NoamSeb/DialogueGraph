using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class DialogueGraphView : UnityEditor.Experimental.GraphView.GraphView
{
    private readonly Vector2 _defaultNodeSize = new Vector2(150, 200);
        
    public DialogueGraphView()
    {
        styleSheets.Add(Resources.Load<StyleSheet>("Dialogue"));
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        
        this.AddManipulator(CreateNodeContextualMenu("Add node"));
        //this.AddManipulator(CreateNodeContextualMenu("test"));
        
        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
        
        AddElement(GenerateEntryPointNode());
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        ports.ForEach(port =>
        {
            if (startPort != port && startPort.node != port.node)
            {
                compatiblePorts.Add(port);
            }
        });
        
        return compatiblePorts;
    }

    private Port GeneratePort(DialogueNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
    }

    private DialogueNode GenerateEntryPointNode()
    {
        var node = new DialogueNode
        {
            title = "Start",
            GUID = Guid.NewGuid().ToString(),
            DialogueText = "EntryPoint",
            EntryPoint = true
        };
        
        var generatePort = GeneratePort(node, Direction.Output, Port.Capacity.Multi);
        generatePort.portName = "Next";
        node.outputContainer.Add(generatePort);
        
        node.RefreshExpandedState();
        node.RefreshPorts();
        
        node.SetPosition(new Rect(100, 200, 100, 150));
        return node;
    }

    public void CreateNode(string nodeName, Vector2 position = default(Vector2))
    {
        AddElement(CreateDialogueNode(nodeName,  position));
    }

    //remplacer DialogueNode par Node pour plus de généralité
    public DialogueNode CreateDialogueNode(string nodeName, Vector2 position = default(Vector2))
    {
        var dialogueNode = new DialogueNode
        {
            title = nodeName,
            DialogueText = nodeName,
            GUID = Guid.NewGuid().ToString()
        };
        
        var inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        dialogueNode.inputContainer.Add(inputPort);

        var button = new Button(clickEvent: () => { AddChoicePort(dialogueNode); });
        button.text = "New Choice";
        dialogueNode.titleContainer.Add(button);
        
        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
        
        dialogueNode.SetPosition(new Rect(position, _defaultNodeSize));
        
        return dialogueNode;
    }

    public void AddChoicePort(DialogueNode dialogueNode, string overridenPortName = "")
    {
        var generatePort = GeneratePort(dialogueNode, Direction.Output);

        var oldLabel = generatePort.contentContainer.Q<Label>("type");
        generatePort.contentContainer.Remove(oldLabel);
        
        var outputPortCount = dialogueNode.outputContainer.Query("connector").ToList().Count;
        //generatePort.portName  = $"Choice {outputPortCount}";
        
        var choicePortName = string.IsNullOrEmpty(overridenPortName) ? $"Choice {outputPortCount+1}" : overridenPortName;
        generatePort.portName =  choicePortName;
        
        var textField = new TextField
        {
            name = string.Empty, 
            value = choicePortName
        };
        textField.RegisterValueChangedCallback(evt => generatePort.portName = evt.newValue);
        generatePort.contentContainer.Add(new Label("   "));
        generatePort.contentContainer.Add(textField);
        
        var deleteButton = new Button(clickEvent: () => RemovePort(dialogueNode, generatePort))
        {
            text = "X"
        };
        generatePort.contentContainer.Add(deleteButton);
        
        dialogueNode.outputContainer.Add(generatePort);
        
        dialogueNode.RefreshPorts();
        dialogueNode.RefreshExpandedState();
    }

    private void RemovePort(DialogueNode dialogueNode, Port generatePort)
    {
        var targetEdge = edges.ToList().Where(x => x.output.portName == generatePort.portName && x.output.node == generatePort.node);
        if (targetEdge.Any())
        {
            var edge = targetEdge.First();
            edge.input.Disconnect(edge);
            RemoveElement(targetEdge.First());
        }
        
        
        dialogueNode.outputContainer.Remove(generatePort);
        dialogueNode.RefreshPorts();
        dialogueNode.RefreshExpandedState();
    }

    private IManipulator CreateNodeContextualMenu(string actionTitle)
    {
        ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
            menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => CreateNode("Dialogue Node", actionEvent.eventInfo.localMousePosition)));

        return contextualMenuManipulator;
    }
}
