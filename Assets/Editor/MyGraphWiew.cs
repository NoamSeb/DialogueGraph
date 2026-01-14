using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;


public class MyGraphWiew : UnityEditor.Experimental.GraphView.GraphView
{
    public MyGraphWiew()
    {
        GenerateGridBackground();
        ApplyStyles();
        AddManipulators();
    }
    private void AddManipulators()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(CreateNodeContextualMenu("Add Choice Node"));
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts = new List<Port>();
        ports.ForEach(port =>
        {
            if (startPort == port) { return; }
            if (startPort.node == port.node) { return; }
            if (startPort.direction == port.direction) {return;}
            
            compatiblePorts.Add(port);
        });
        return compatiblePorts;
    }

    private IManipulator CreateNodeContextualMenu(string actionTitle)
    {
        ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
            menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => 
                CreateNode(actionEvent.eventInfo.localMousePosition)));
        return contextualMenuManipulator;
    }
    
    private MyNode CreateNode(Vector2 positionSpawn)
    {
        Type nodeType = Type.GetType($"ChoiceNode");
        if (nodeType == null){Debug.LogError("ERROR ON LOAD NODE");}
        
        MyNode node = (MyNode) Activator.CreateInstance(nodeType);
        
        if(node == null){Debug.LogError("ERROR ON CREATE NODE INSTANCE");}
        
        AddElement(node);
        node?.Initialize(positionSpawn);
        node?.Draw();
        return node;
    }

    private void GenerateGridBackground()
    {
        GridBackground gridBackground = new GridBackground();
        gridBackground.StretchToParentSize();
        Insert(0, gridBackground);
    }
    
    private void ApplyStyles()
    {
        styleSheets.Add((StyleSheet)EditorGUIUtility.Load("Styles/DSGraphViewStyles.uss"));
        styleSheets.Add((StyleSheet)EditorGUIUtility.Load("Styles/DSNodeStyles.uss"));
    }
}
