using System;
using UnityEditor.Experimental.GraphView;
using Node = Unity.GraphToolkit.Editor.Node;
using Unity.GraphToolkit.Editor;
using UnityEngine;



    [Serializable]
    public class StartNode : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort("out").Build();
        }
    }

    [Serializable]
    public class EndNode : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("in").Build();
        }
    }

    [Serializable]
    public class DialogueNode : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("in").Build();
            context.AddOutputPort("out").Build();

            context.AddInputPort<string>("Speaker").Build();
            context.AddInputPort<string>("Dialogue").Build();
        }
    }
    

[Serializable]
public class ChoiceNode : Node
{
    private const string portCountName = "portCount";
    private const int k_MaxChoices = 8; // limite raisonnable
    int portCountyipi = 2;

    protected override void OnDefineOptions(IOptionDefinitionContext context)
    {
        context.AddOption<int>(portCountName)
               .WithDisplayName("Number of choices")
               .WithDefaultValue(2);
    }

    protected override void OnDefinePorts(Node.IPortDefinitionContext context)
    {
        context.AddInputPort("in").Build();
        context.AddInputPort<string>("Speaker").Build();
        context.AddInputPort<string>("Dialogue").Build();

        var portCountOpt = GetNodeOptionByName(portCountName);
        int portCount = 2; 
        if (portCountOpt != null)
            portCountOpt.TryGetValue(out portCount);

        portCount = Mathf.Clamp(portCount, 0, k_MaxChoices);
        for (int i = 0; i < portCount; i++)
        {
            var textOpt = GetNodeOptionByName($"choiceText_{i}");
            string displayName = null;
            if (textOpt != null)
                textOpt.TryGetValue(out displayName);
            
            context.AddInputPort<ConditionsSC>($"condtions_{i}")
                .WithDisplayName(string.IsNullOrEmpty(displayName) ? $"Choice {i}" : displayName)
                .Build();
            context.AddOutputPort($"choice_{i}")
                   .WithDisplayName(string.IsNullOrEmpty(displayName) ? $"Choice {i}" : displayName)
                   .Build();
        }
    }
}

    [Serializable]
    
    public class ConditionNode : Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("in").Build();
            context.AddOutputPort("true").Build();
            context.AddOutputPort("false").Build();
            
            context.AddInputPort<ConditionsSC>("Condition").Build();
        }
    }
