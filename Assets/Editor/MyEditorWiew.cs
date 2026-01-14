using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


public class MyEditorWiew : EditorWindow
{
    [MenuItem("Window/EditorWiew")]
    public static void Open()
    {
        GetWindow<MyEditorWiew>("EditorWiew");
    }

    private void CreateGUI()
    {
        AddGraphView();
        AddStylization();
    }
    
    private void AddGraphView()
    {
        MyGraphWiew graphView = new MyGraphWiew();
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }
    
    private void AddStylization()
    {
        StyleSheet styleSheet = (StyleSheet) EditorGUIUtility.Load("Styles/DSVariables.uss");
        rootVisualElement.styleSheets.Add(styleSheet);
    }
}
