using Unity.GraphToolkit.Editor;
using UnityEditor;
using System;

[Serializable]
[Graph(AssetExtension)]
public class DialogueGraph : Graph
{
    public const string AssetExtension = "pablograph";
    [MenuItem("Assets/Create/PabloGraphView")]
    public static void CreatePabloGraphView()
    {
        GraphDatabase.PromptInProjectBrowserToCreateNewAsset<DialogueGraph>();
    }
}

