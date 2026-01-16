using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;



public class DSMultipleChoiceNode : DSNode
{
    private Button _addChoiceButton;
    private Button _changeNodeType;
    private TextField _statedNodeField;
    private List<Port> _choicePorts = new List<Port>();
    private Dictionary<Port, TextField> _choicePortsTextField = new Dictionary<Port, TextField>();
    

    private void ClearChoicePorts()
    {
        Saves.choicesInNode.Clear();

        foreach (var kv in Saves.ConditionsMapElement)
        {
            ClearConditions(kv.Value);
        }

        Saves.ConditionsMapElement.Clear();

        foreach (Port port in _choicePorts)
        {
            if (port.connected)
            {
                graphView.DeleteElements(port.connections);
            }

            _choicePortsTextField.TryGetValue(port, out TextField textField);
            if (textField != null)
            {
                textField.RemoveFromHierarchy();
            }

            graphView.RemoveElement(port);
        }

        _choicePorts.Clear();
    }


    private void SetNodeTypeLabel()
    {
        if (Saves.isMultipleChoice)
        {
            _changeNodeType.RemoveFromClassList("ds-node__buttonMultiple");
            _changeNodeType.AddToClassList("ds-node__buttonSingle");
            _statedNodeField.value = "Node Type: Multiple Choice";
            mainContainer.Add(_addChoiceButton);
        }
        else
        {
            _statedNodeField.value = "Node Type: Single Choice";
            _changeNodeType.RemoveFromClassList("ds-node__buttonSingle");
            _changeNodeType.AddToClassList("ds-node__buttonMultiple");
            if (mainContainer.Contains(_addChoiceButton))
            {
                mainContainer.Remove(_addChoiceButton);
            }
        }
    }

    private void SwitchNodeType()
    {
        ClearChoicePorts();

        Saves.isMultipleChoice = !Saves.isMultipleChoice;

        UpdateNodeOnSwitch();
    }

    private void UpdateNodeOnSwitch()
    {
        if (Saves.isMultipleChoice)
        {
            CreateSingleChoicePortNew("");
            CreateSingleChoicePortNew("");
        }
        else
            CreateSingleChoicePortNew("Continue");

        SetNodeTypeLabel();

        RefreshExpandedState();
    }

    public override void Draw()
    {
        base.Draw();

        // TOP LEVEL CONTAINERS

        _statedNodeField = DSElementUtility.CreateTextField("Node Type", null);

        titleContainer.Add(_statedNodeField);

        /* MAIN CONTAINER */

        _addChoiceButton = DSElementUtility.CreateButton("Add Choice", () =>
        {
            CreateSingleChoicePortNew("New Choice");
        });

        _changeNodeType = DSElementUtility.CreateButton("Switch node Type", () => { SwitchNodeType(); });

        mainContainer.Add(_changeNodeType);
        mainContainer.Add(_addChoiceButton);

        _changeNodeType.AddToClassList("ds-node__buttonSingle");
        _addChoiceButton.AddToClassList("ds-node__button");

        /* OUTPUT CONTAINER */

        foreach (DSChoiceSaveData choice in Saves.choicesInNode)
        {
            CreateSingleChoicePortForExisting(choice, choice.GetDropDownKeyChoice());
            if (choice.Conditions != null && choice.Conditions.Count > 0)
            {
                Port port = _choicePorts.LastOrDefault();
                if (port != null)
                {
                    foreach (var savedSc in choice.Conditions)
                    {
                        var obj = CreateConditions(port, savedSc);
                        AddConditionsBelowPort(port, obj, true);
                    }
                }
            }
        }

        if (Saves.choicesInNode.Count == 0)
        {
            CreateSingleChoicePortNew("");
        }
        SetNodeTypeLabel();

        RefreshExpandedState();
    }

    private void OnDropDownChoiceTranslate(Port choicePort, DropdownField dropdown, DSChoiceSaveData choiceData)
    {
        if (_choicePortsTextField.TryGetValue(choicePort, out TextField textField))
        {
            if (dropdown != null)
            {
                choiceData.SaveDropDownKeyChoice(dropdown.value);
                textField.value = FantasyDialogueTable.LocalManager.GetAllDialogueFromValue(dropdown.value);
            }
        }
    }

    private TextField CreateLabelChoiceTranslate()
    {
        var text = DSElementUtility.CreateTextField("Choose a key to translate", null);
        return text;
    }

    private void ClearConditions(List<VisualElement> conditions)
    {
        foreach (var condition in conditions)
        {
            if (condition != null && condition.parent != null)
            {
                outputContainer.Remove(condition);
            }
        }
    }
    
    private bool IsAncestor(VisualElement possibleAncestor, VisualElement element)
    {
        var p = element;
        while (p != null)
        {
            if (p == possibleAncestor) return true;
            p = p.parent;
        }
        return false;
    }
    
    private VisualElement FindContainerForPort(Port choicePort)
    {
        if (choicePort?.parent != null) return choicePort.parent;

        var candidates = new[] { outputContainer, inputContainer, extensionContainer, mainContainer };
        foreach (var cand in candidates)
        {
            if (cand == null) continue;
            foreach (var child in cand.Children())
            {
                if (child == choicePort || IsAncestor(child, choicePort))
                    return cand;
            }
        }

        return null;
    }
    
    private VisualElement CreateConditions(Port choicePort, ConditionsSC initialValue = null)
    {
        ObjectField conditionsField = new ObjectField()
        {
            objectType = typeof(ConditionsSC),
            allowSceneObjects = false,
            value = initialValue
        };

        conditionsField.AddToClassList("ds-node__conditions-field");

        conditionsField.RegisterValueChangedCallback(evt =>
        {
            if (evt.previousValue == evt.newValue) return;
            AddConditionsScToObjectField(choicePort, (ConditionsSC)evt.newValue);
        });

        conditionsField.style.marginLeft = new StyleLength(16);
        conditionsField.style.alignSelf = Align.Stretch;

        return conditionsField;
    }
    
    private void ClearCondition(Port port, VisualElement condition)
    {
        if (condition == null) return;

        ConditionsSC scToRemove = null;
        var objField = condition as ObjectField ?? condition.Q<ObjectField>();
        if (objField != null)
        {
            scToRemove = objField.value as ConditionsSC;
        }

        if (condition.parent != null)
        {
            condition.parent.Remove(condition);
        }

        if (port != null && Saves.ConditionsMapElement != null && Saves.ConditionsMapElement.ContainsKey(port))
        {
            Saves.ConditionsMapElement[port].Remove(condition);
            if (Saves.ConditionsMapElement[port].Count == 0)
                Saves.ConditionsMapElement.Remove(port);
        }

        if (port != null)
        {
            int idx = _choicePorts.IndexOf(port);
            if (idx >= 0 && idx < Saves.choicesInNode.Count)
            {
                var choiceData = Saves.choicesInNode[idx];
                if (choiceData.Conditions != null && scToRemove != null)
                {
                    choiceData.Conditions.Remove(scToRemove);
                }
            }
        }

        RefreshExpandedState();
        MarkDirtyRepaint();
    }

    private void ClearConditions(List<VisualElement> conditions, Port port = null)
    {
        if (conditions == null) return;

        foreach (var condition in conditions.ToList())
        {
            if (condition == null) continue;

            if (condition.parent != null)
                condition.parent.Remove(condition);

            if (port != null && Saves.ConditionsMapElement != null && Saves.ConditionsMapElement.ContainsKey(port))
            {
                Saves.ConditionsMapElement[port].Remove(condition);
            }
            else if (Saves.ConditionsMapElement != null)
            {
                foreach (var kvp in Saves.ConditionsMapElement.ToList())
                {
                    if (kvp.Value.Contains(condition))
                    {
                        kvp.Value.Remove(condition);
                        if (kvp.Value.Count == 0)
                            Saves.ConditionsMapElement.Remove(kvp.Key);
                    }
                }
            }
        }

        RefreshExpandedState();
        MarkDirtyRepaint();
    }
    
    private void AddConditionsScToObjectField(Port port, ConditionsSC conditionSc)
    {
        if (port == null || conditionSc == null)
        {
            Debug.Log("Port or ConditionsSC is null, cannot add to ConditionsMapSc.");
            return;
        }

        int idx = _choicePorts.IndexOf(port);
        if (idx < 0 || idx >= Saves.choicesInNode.Count)
        {
            Debug.LogWarning("[AddConditionsScToObjectField] Impossible de retrouver l'index du choix pour ce port.");
            return;
        }

        DSChoiceSaveData choiceData = Saves.choicesInNode[idx];

        if (choiceData.Conditions == null)
            choiceData.Conditions = new List<ConditionsSC>();

        choiceData.Conditions.Add(conditionSc);

        Debug.Log($"Added ConditionsSC to choice index {idx}. Total conditions for this choice: {choiceData.Conditions.Count}");
    }
    
private (Port, DropdownField) CreateSingleChoicePortForExisting(DSChoiceSaveData choiceData, string dropDownKey = "")
{
    Port choicePort = this.CreatePort();
    choicePort.userData = choiceData;

    DropdownField choiceDropdown = null;
    if (Saves.isMultipleChoice)
    {
        choiceDropdown = DSElementUtility.CreateDropdownArea("Choice KEY");
        FillCsvDropdown(choiceDropdown);
        choiceDropdown.RegisterValueChangedCallback(callback => { OnDropDownChoiceTranslate(choicePort, choiceDropdown, choiceData); });

        if (!string.IsNullOrEmpty(dropDownKey))
            choiceDropdown.value = dropDownKey;

        choicePort.Add(choiceDropdown);
    }
    else
    {
        Label choiceLabel = new Label("Continue");
        choicePort.Add(choiceLabel);
    }

    if (Saves.isMultipleChoice && Saves.choicesInNode.Count > 2)
    {
        Button deleteChoiceButton = DSElementUtility.CreateButton("X", () =>
        {
            if (choicePort.connected)
            {
                graphView.DeleteElements(choicePort.connections);
            }

            if (Saves.ConditionsMapElement != null && Saves.ConditionsMapElement.TryGetValue(choicePort, out List<VisualElement> condElem))
            {
                ClearConditions(condElem);
                Saves.ConditionsMapElement.Remove(choicePort);
            }

            int idx = _choicePorts.IndexOf(choicePort);
            if (idx >= 0 && idx < Saves.choicesInNode.Count)
            {
                Saves.choicesInNode.RemoveAt(idx);
            }

            _choicePorts.Remove(choicePort);
            _choicePortsTextField.Remove(choicePort);

            graphView.RemoveElement(choicePort);
        });
        
        deleteChoiceButton.AddToClassList("ds-node__buttonDelete");
        choicePort.Add(deleteChoiceButton);
    }

    if (Saves.isMultipleChoice)
    {
        Button conditionsButton = DSElementUtility.CreateButton("Add Conditions", () => { AddConditionsBelowPort(choicePort, CreateConditions(choicePort)); });
        conditionsButton.AddToClassList("ds-node__button");
        choicePort.Add(conditionsButton);
    }

    outputContainer.Add(choicePort);

    _choicePorts.Add(choicePort);

    if (Saves.isMultipleChoice)
    {
        var label = CreateLabelChoiceTranslate();
        label.AddToClassList("ds-node__label-translate");
        AddConditionsBelowPort(choicePort, label, false);
        _choicePortsTextField[choicePort] = label;

        if (!string.IsNullOrEmpty(dropDownKey))
        {
            OnDropDownChoiceTranslate(choicePort, choiceDropdown, choiceData);
        }
    }

    choicePort.MarkDirtyRepaint();
    RefreshExpandedState();
    MarkDirtyRepaint();

    return (choicePort, choiceDropdown);
}


private void CreateSingleChoicePortNew(string dropDownKey = "")
{
    DSChoiceSaveData newChoice = new DSChoiceSaveData();
    newChoice.SaveDropDownKeyChoice(dropDownKey);

    Saves.choicesInNode.Add(newChoice);

    CreateSingleChoicePortForExisting(newChoice, dropDownKey);
}

private int FindChildIndexContainingPort(VisualElement container, Port choicePort)
{
    var children = container.Children().ToList();
    for (int i = 0; i < children.Count; i++)
    {
        if (children[i] == choicePort || IsAncestor(children[i], choicePort))
            return i;
    }
    return -1;
}

private int FindLabelIndexForPort(VisualElement container, int portIndex)
{
    var children = container.Children().ToList();
    for (int i = portIndex + 1; i < children.Count; i++)
    {
        var child = children[i];
        if (FindPortInElement(child) != null)
            return -1;

        if (child.ClassListContains("ds-node__label-translate"))
            return i;
    }
    return -1;
}

private Port FindPortInElement(VisualElement elem)
{
    if (elem == null) return null;
    if (elem is Port directPort) return directPort;

    foreach (var child in elem.Children())
    {
        var found = FindPortInElement(child);
        if (found != null) return found;
    }
    return null;
}

void AddConditionsBelowPort(Port choicePort, VisualElement elementToAdd, bool canBeDeleted = true)
{
    if (choicePort == null || elementToAdd == null) return;

    VisualElement container = FindContainerForPort(choicePort);
    if (container == null)
    {
        var fallback = extensionContainer ?? mainContainer ?? (VisualElement)this;
        fallback.Add(elementToAdd);
        elementToAdd.MarkDirtyRepaint();
        return;
    }

    int portIndex = FindChildIndexContainingPort(container, choicePort);
    if (portIndex < 0)
    {
        container.Add(elementToAdd);
        elementToAdd.MarkDirtyRepaint();
        return;
    }

    int labelIndexForThisPort = FindLabelIndexForPort(container, portIndex);

    int insertIndex;
    if (labelIndexForThisPort >= 0)
    {
        insertIndex = labelIndexForThisPort + 1;
    }
    else
    {
        insertIndex = portIndex + 1;
    }

    container.Insert(insertIndex, elementToAdd);

    bool isLabel = elementToAdd.ClassListContains("ds-node__label-translate");
    if (!isLabel)
    {
        elementToAdd.AddToClassList("ds-node__conditions-container");

        if (Saves.ConditionsMapElement == null)
            Saves.ConditionsMapElement = new Dictionary<Port, List<VisualElement>>();

        if (!Saves.ConditionsMapElement.ContainsKey(choicePort))
            Saves.ConditionsMapElement[choicePort] = new List<VisualElement>();

        Saves.ConditionsMapElement[choicePort].Add(elementToAdd);

        if (canBeDeleted)
        {
            Button butClearCondition = DSElementUtility.CreateButton("X", () =>
            {
                ClearCondition(choicePort, elementToAdd);
            });
            butClearCondition.AddToClassList("ds-node__buttonDeleteCondition");
            elementToAdd.Add(butClearCondition);
        }
    }
    else
    {
        if (!_choicePortsTextField.ContainsKey(choicePort))
            _choicePortsTextField.Add(choicePort, (TextField)elementToAdd);
        else
            _choicePortsTextField[choicePort] = (TextField)elementToAdd;
    }

    elementToAdd.MarkDirtyRepaint();
    RefreshExpandedState();
    MarkDirtyRepaint();
}

}

