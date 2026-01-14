using UnityEngine;
using UnityEditor.AssetImporters;
using Unity.GraphToolkit.Editor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphView
{
    [ScriptedImporter(1, DialogueGraph.AssetExtension)]
    public class DialogueGraphImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            DialogueGraph editorGraph = GraphDatabase.LoadGraphForImporter<DialogueGraph>(ctx.assetPath);
            RuntimeDialogueGraph runtimeGraph = ScriptableObject.CreateInstance<RuntimeDialogueGraph>();
            var nodeIdMap = new Dictionary<INode, string>();

            foreach (var node in editorGraph.GetNodes())
            {
                nodeIdMap[node] = Guid.NewGuid().ToString();
            }
            
            var startNode = editorGraph.GetNodes().OfType<StartNode>().FirstOrDefault();
            if (startNode != null)
            {
                var entryPoint = startNode.GetOutputPorts().FirstOrDefault()?.firstConnectedPort;
                if (entryPoint != null)
                {
                    runtimeGraph.EntryNodeID = nodeIdMap[entryPoint.GetNode()];
                }
            }

            foreach (var node in editorGraph.GetNodes())
            {
                if (node is StartNode || node is EndNode) continue;
                
                var runtimeNode = new RuntimeDialogueNode {DialogueNodeID = nodeIdMap[node]};
                if (node is DialogueNode dialogueNode)
                {
                    ProcessDialogueNode(dialogueNode, runtimeNode, nodeIdMap);
                }else if (node is ChoiceNode choiceNode)
                {
                    ProsessChoiceNode(choiceNode, runtimeNode, nodeIdMap);
                }
                
                runtimeGraph.AllNodes.Add(runtimeNode);
            }
            
            ctx.AddObjectToAsset("RuntimeData", runtimeGraph);
            ctx.SetMainObject(runtimeGraph);
        }

        private void ProcessDialogueNode(DialogueNode node, RuntimeDialogueNode runtimeNode,
            Dictionary<INode, string> nodeIdMap)
        {
            runtimeNode.SpeakerName = GetPortValue<string>(node.GetInputPortByName("Speaker"));
            runtimeNode.DialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));
            
            var nextNodePort = node.GetOutputPortByName("out")?.firstConnectedPort;
            if (nextNodePort != null)
                runtimeNode.NextDialogueNodeID = nodeIdMap[nextNodePort.GetNode()];
        }

        private void ProsessChoiceNode(ChoiceNode node, RuntimeDialogueNode runtimeNode,
            Dictionary<INode, string> nodeIdMap)
        {
            runtimeNode.SpeakerName = GetPortValue<string>(node.GetInputPortByName("Speaker"));
            runtimeNode.SpeakerName = GetPortValue<string>(node.GetInputPortByName("Dialogue"));
            
            var ChoiceOuputPort = node.GetOutputPorts().Where(p => p.name.StartsWith("Choice "));

            foreach (var outputPort in ChoiceOuputPort)
            {
                var index = outputPort.name.Substring("Choice ".Length);
                var textPort = node.GetInputPortByName($"Choice Text {index}");

                var ChoiceData = new ChoiceData
                {
                    ChoiceText = GetPortValue<string>(textPort),
                    DestinationNodeID = outputPort.firstConnectedPort != null
                        ? nodeIdMap[outputPort.firstConnectedPort.GetNode()]
                        : null
                };
                
                runtimeNode.Choices.Add(ChoiceData);
            }
        }
        
        private T GetPortValue<T>(IPort port)
        {
            if(port == null) return default(T);

            if (port.isConnected)
            {
                if (port.firstConnectedPort.GetNode() is IVariableNode variableNode)
                {
                    variableNode.variable.TryGetDefaultValue(out T value);
                    return value;
                }
            }
            
            port.TryGetValue(out T fallbackValue);
            return fallbackValue;
        }
    }
}