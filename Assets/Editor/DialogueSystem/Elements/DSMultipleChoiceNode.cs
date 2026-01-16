using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;



    public class DSMultipleChoiceNode : DSNode
    {
        private Button _addChoiceButton;
        private Button _changeNodeType;
        private TextField _statedNodeField;
        private List<Port> _choicePorts = new List<Port>();
        private TextField _fieldTranslateLabel;

        public override void Initialize(string nodeName, DSGraphView dsGraphView, Vector2 position)
        {
            base.Initialize(nodeName, dsGraphView, position);
        }

        private void ClearChoicePorts()
        {
            Saves.ChoicesInNode.Clear();

            foreach (var kv in Saves.ConditionsMap)
            {
                ClearConditions(kv.Value);
            }
            
            Saves.ConditionsMap.Clear();

            foreach (Port port in _choicePorts)
            {
                if (port.connected)
                {
                    graphView.DeleteElements(port.connections);
                }

                graphView.RemoveElement(port);
            }
            _choicePorts.Clear();
        }
        
        private void CreateSingleChoicePort(string text, bool canBeDeleted, bool haveConditions)
        {
            DSChoiceSaveData choiceData = new DSChoiceSaveData()
            {
                // CALLBACK ON CHANGE DROP DOWN VALUE TO KEY LOCAL
            };
            
            Saves.ChoicesInNode.Add(choiceData);
            Port choicePort = CreateChoicePort(choiceData);
            outputContainer.Add(choicePort);
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
                CreateSingleChoicePort("New choice", true, true);
                CreateSingleChoicePort("New choice", true, true);
            }
            else
                CreateSingleChoicePort("Continue", false, false);
            
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
                DSChoiceSaveData choiceData = new DSChoiceSaveData() {};
                
                Saves.ChoicesInNode.Add(choiceData);
                
                Port choicePort = CreateChoicePort(choiceData);
                
                outputContainer.Add(choicePort);
            });
            
            _changeNodeType = DSElementUtility.CreateButton("Switch node Type", () =>
            {
                SwitchNodeType();
            });
            
            mainContainer.Add(_changeNodeType);
            
            mainContainer.Add(_addChoiceButton);
            
            _changeNodeType.AddToClassList("ds-node__buttonSingle");
            _addChoiceButton.AddToClassList("ds-node__button");

            /* OUTPUT CONTAINER */

            foreach (DSChoiceSaveData choice in Saves.ChoicesInNode)
            {
                Port choicePort = CreateChoicePort(choice);
                
                // LOAD CONDITIONS //
                
                foreach (var conditions in Saves.ConditionsMap)
                {
                    if(conditions.Key.userData == choice)
                    {
                        foreach (var condElem in conditions.Value)
                        {
                            if (condElem == null) { return; } 
                            
                            AddConditionsBelowPort(conditions.Key, CreateConditions());
                        }
                    }
                }
                outputContainer.Add(choicePort);
            }
            
            if (Saves.ChoicesInNode.Count == 0)
            {
                DSChoiceSaveData choiceData = new DSChoiceSaveData() { };

                Saves.ChoicesInNode.Add(choiceData);
                Port choicePort = CreateChoicePort(choiceData);
                outputContainer.Add(choicePort);
            }
            
            SetNodeTypeLabel();
            
            RefreshExpandedState();
        }

        public Port CreateChoicePort(object userData)
        {
            Port choicePort = this.CreatePort();

            choicePort.userData = userData;

            DSChoiceSaveData choiceData = (DSChoiceSaveData) userData;
            if(Saves.isMultipleChoice)
            {
                DropdownField choiceDropdown = DSElementUtility.CreateDropdownArea("Choice KEY");
                FillCsvDropdown(choiceDropdown);
                choiceDropdown.RegisterValueChangedCallback(callback => 
                {
                    OnDropDownChoiceTranslate(choicePort);
                });
                
                choicePort.Add(choiceDropdown);
            }
            else
            {
                Label choiceLabel = new Label("Continue");
                choicePort.Add(choiceLabel);
            }


            if (Saves.isMultipleChoice)
            {
                if (Saves.ChoicesInNode.Count > 2)
                {
                    Button deleteChoiceButton = DSElementUtility.CreateButton("X", () =>
                    {
                        if (choicePort.connected)
                        {
                            graphView.DeleteElements(choicePort.connections);
                        }

                        if (Saves.ConditionsMap.TryGetValue(choicePort, out List<VisualElement> condElem))
                        {
                            ClearConditions(condElem);
                            Saves.ConditionsMap.Remove(choicePort);
                        }

                        Saves.ChoicesInNode.Remove(choiceData);
                        graphView.RemoveElement(choicePort);
                        
                    });
                    deleteChoiceButton.AddToClassList("ds-node__buttonDelete");
                    choicePort.Add(deleteChoiceButton);
                }
            }

            if (Saves.isMultipleChoice)
            {
                Button conditionsButton = DSElementUtility.CreateButton("Add Conditions", () => {AddConditionsBelowPort(choicePort, CreateConditions());});
                conditionsButton.AddToClassList("ds-node__button");
                choicePort.Add(conditionsButton);
                
                AddConditionsBelowPort(choicePort, CreateLabelChoiceTranslate(), false);
            }
            
            
            _choicePorts.Add(choicePort);

            return choicePort;
        }

        private void OnDropDownChoiceTranslate(Port choicePort)
        {
            if(Saves.ConditionsMap.TryGetValue(choicePort, out var condElem))
            {
                if (condElem[0] == null) { return;}
                choicePort.Remove(condElem[0]);
            }
        }

        private VisualElement CreateLabelChoiceTranslate()
        {
            _fieldTranslateLabel = DSElementUtility.CreateTextField("Translate: XXX", null);
            return _fieldTranslateLabel;
        }

        private VisualElement CreateConditions()
        {
            ObjectField conditionsField = new ObjectField()
            {
                objectType = typeof(ConditionsSC),
                allowSceneObjects = false
            };

            conditionsField.AddToClassList("ds-node__conditions-field");

            conditionsField.style.marginLeft = new StyleLength(16);
            conditionsField.style.alignSelf = Align.Stretch;

            return conditionsField;
        }

        public void AddConditionsBelowPort(Port port, VisualElement element, bool canBeDeleted = true)
        {
            if (port == null)
                return;

            VisualElement visualElementTarget = element;
            visualElementTarget.AddToClassList("ds-node__conditions-container");

            int idx = outputContainer.IndexOf(port);
            if (idx < 0)
            {
                outputContainer.Add(visualElementTarget);
            }
            else
            {
                outputContainer.Insert(idx + 1, visualElementTarget);
            }

            if (!Saves.ConditionsMap.ContainsKey(port))
            {
                Saves.ConditionsMap[port] = new List<VisualElement>();
            }
            
            Saves.ConditionsMap[port].Add(visualElementTarget);
            
            if (canBeDeleted)
            {
                Button butClearCondition = DSElementUtility.CreateButton("X", () => { this.RemoveCondition(visualElementTarget); });
            
                butClearCondition.AddToClassList("ds-node__buttonDeleteCondition");
                visualElementTarget.Add(butClearCondition);
            }
            
            RefreshExpandedState();
            MarkDirtyRepaint();
        }
        
        private void RemoveCondition(VisualElement condition)
        {
            if (condition != null && condition.parent != null)
            {
                outputContainer.Remove(condition);
            }
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

    }
