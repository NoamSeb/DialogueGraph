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
                if (node is StartNode or EndNode) continue;
                
                var runtimeNode = new RuntimeDialogueNode {DialogueNodeID = nodeIdMap[node]};
                if (node is DialogueNode dialogueNode)
                {
                    ProcessDialogueNode(dialogueNode, runtimeNode, nodeIdMap);
                }else if (node is ChoiceNode choiceNode)
                {
                    ProcessChoiceNode(choiceNode, runtimeNode, nodeIdMap);
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

        private void ProcessChoiceNode(ChoiceNode node, RuntimeDialogueNode runtimeNode,
            Dictionary<INode, string> nodeIdMap)
        {
            runtimeNode.SpeakerName = GetPortValue<string>(node.GetInputPortByName("Speaker"));
            runtimeNode.DialogueText = GetPortValue<string>(node.GetInputPortByName("Dialogue"));

            var choiceOutputPorts = node.GetOutputPorts().Where(p => p.name.StartsWith("choice_"));

            foreach (var outputPort in choiceOutputPorts)
            {
                var indexStr = outputPort.name.Substring("choice_".Length);
                if (!int.TryParse(indexStr, out int index))
                {
                    Debug.LogWarning($"Impossible de parser index pour le port {outputPort.name}");
                    continue;
                }

                // Récupère le texte via l'option correspondante
                var textOpt = node.GetNodeOptionByName($"choiceText_{index}");
                string choiceText = null;
                if (textOpt != null)
                    textOpt.TryGetValue(out choiceText);

                // Récupère le ConditionsSC via l'option correspondante
                var condOpt = node.GetNodeOptionByName($"choiceCond_{index}");
                ConditionsSC condition = null;
                if (condOpt != null)
                    condOpt.TryGetValue(out condition);

                var choiceData = new ChoiceData
                {
                    ChoiceText = choiceText,
                    // Optionnel : stocker la condition dans ChoiceData
                    Condition = condition,
                    DestinationNodeID = outputPort.firstConnectedPort != null
                        ? nodeIdMap[outputPort.firstConnectedPort.GetNode()]
                        : null
                };

                runtimeNode.Choices.Add(choiceData);
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