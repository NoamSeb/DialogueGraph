using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    //public DialogueGraph graph;
//
//    private Dictionary<string, Node> nodeLookup;
//    private Node currentNode;
//
//    public void StartDialogue(DialogueGraph graphAsset)
//    {
//        graph = graphAsset;
//        BuildLookup();
//        currentNode = GetStartNode();
//        ProcessNode(currentNode);
//    }
//
//    void BuildLookup()
//    {
//        nodeLookup = new Dictionary<string, BaseNode>();
//        foreach (var node in graph.nodes)
//        {
//            nodeLookup.Add(node.GUID, node);
//        }
//    }
//    
//    Node GetStartNode()
//    {
//        return graph.nodes.Find(n => n is StartNode);
//    }
//
//    
//    void ProcessNode(Node node)
//    {
//        currentNode = node;
//
//        switch (node)
//        {
//            case DialogueNode dialogue:
//                UIManager.Instance.ShowDialogue(dialogue.text);
//                break;
//
//            case ChoiceNode choice:
//                UIManager.Instance.ShowChoices(choice);
//                break;
//
//            case ActionNode action:
//                ExecuteAction(action);
//                GoToNextNode(0);
//                break;
//        }
//    }



}



