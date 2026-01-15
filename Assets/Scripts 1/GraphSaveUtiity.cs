using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GraphSaveUtiity
{
    private DialogueGraphView _targetGraphView;
    private DialogueContainer _containerCache;
    
    private List<Edge> edges => _targetGraphView.edges.ToList();
    private List<DialogueNode> nodes => _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();
    
    public static GraphSaveUtiity GetInstance(DialogueGraphView targetGraphView)
    {
        return new GraphSaveUtiity
        {
            _targetGraphView =  targetGraphView
        };
    }

    public void SaveGraph(string graphName)
    {
        if (!edges.Any())
        {
            return;
        }
        
        var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
        
        var connectedPorts = edges.Where(x=> x.input.node != null).ToArray();

        for (int i = 0; i < connectedPorts.Length; i++)
        {
            var outputNode = connectedPorts[i].output.node as DialogueNode;
            var inputNode = connectedPorts[i].input.node as DialogueNode;
            
            dialogueContainer.NodeLinks.Add(new NodeLinkData
            {
                baseNodeGuid = outputNode.GUID,
                portName = connectedPorts[i].output.portName,
                targetNodeGuid = inputNode.GUID
            });
        }

        foreach (var dialogueNode in nodes.Where(node => !node.EntryPoint))
        {
            dialogueContainer.DialogueNodeData.Add(new DialogueNodeData
            {
                Guid = dialogueNode.GUID,
                dialogueText = dialogueNode.DialogueText,
                position = dialogueNode.GetPosition().position
            });
        }

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Resources");
        }
        
        AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/{graphName}.asset");
        AssetDatabase.SaveAssets();
    }

    public void LoadGraph(string graphName)
    {
        _containerCache = Resources.Load<DialogueContainer>(graphName);
        if (_containerCache == null)
        {
            EditorUtility.DisplayDialog("Graph Not Found", "Graph Not Found", "OK");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
    }

    private void ClearGraph()
    {
        nodes.Find(x => x.EntryPoint).GUID = _containerCache.NodeLinks[0].baseNodeGuid;

        foreach (var node in nodes)
        {
            if (node.EntryPoint)
            {
                continue;
            }
            edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));
            
            _targetGraphView.RemoveElement(node);
        }
    }
    
    private void CreateNodes()
    {
        foreach (var nodeData in _containerCache.DialogueNodeData)
        {
            var tempNode = _targetGraphView.CreateDialogueNode(nodeData.dialogueText);
            tempNode.GUID = nodeData.Guid;
            _targetGraphView.AddElement(tempNode);
            
            var nodePorts = _containerCache.NodeLinks.Where(x => x.baseNodeGuid ==  nodeData.Guid).ToList();
            nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.portName));
        }
    }
    
    private void ConnectNodes()
    {
        throw new System.NotImplementedException();
    }
}


