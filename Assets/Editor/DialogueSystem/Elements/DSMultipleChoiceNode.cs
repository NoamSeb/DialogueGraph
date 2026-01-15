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
                Text = text,
            };

            Saves.ChoicesInNode.Add(choiceData);
            Port choicePort = CreateChoicePort(choiceData, canBeDeleted, haveConditions);
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
                DSChoiceSaveData choiceData = new DSChoiceSaveData()
                {
                    Text = "New Choice"
                };

                Saves.ChoicesInNode.Add(choiceData);
                Port choicePort = CreateChoicePort(choiceData , true, true);
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
                Port choicePort = CreateChoicePort(choice, Saves.isMultipleChoice, Saves.isMultipleChoice);
                
                // LOAD CONDITIONS //
                
                foreach (var conditions in Saves.ConditionsMap)
                {
                    if(conditions.Key.userData == choice.Text)
                    {
                        foreach (var condElem in conditions.Value)
                        {
                            if (condElem == null)
                            {
                                return;
                            }
                            
                            AddConditionsBelowPort(conditions.Key);
                        }
                    }
                }
                outputContainer.Add(choicePort);
            }
            
            if (Saves.ChoicesInNode.Count == 0)
            {
                Debug.Log("No choices found, creating default ones.");
                DSChoiceSaveData choiceData = new DSChoiceSaveData()
                {
                    Text = "Continue"
                };

                Saves.ChoicesInNode.Add(choiceData);
                Port choicePort = CreateChoicePort(choiceData , false, false);
                outputContainer.Add(choicePort);
            }
            
            SetNodeTypeLabel();
            
            RefreshExpandedState();
        }

        private Port CreateChoicePort(object userData, bool canBeDeleted, bool haveConditions)
        {
            Port choicePort = this.CreatePort();

            choicePort.userData = userData;

            DSChoiceSaveData choiceData = (DSChoiceSaveData) userData;

            if (canBeDeleted)
            {
                Button deleteChoiceButton = DSElementUtility.CreateButton("X", () =>
                {
                    if (Saves.ChoicesInNode.Count <= 2)
                    {
                        return;
                    }

                    if (choicePort.connected)
                    {
                        graphView.DeleteElements(choicePort.connections);
                    }

                    if (Saves.ConditionsMap.TryGetValue(choicePort, out List<VisualElement> condElem))
                    {
                        Debug.Log("Clearing conditions for deleted choice." + condElem.Count);
                        
                        ClearConditions(condElem);
                        Saves.ConditionsMap.Remove(choicePort);
                    }

                    Saves.ChoicesInNode.Remove(choiceData);
                    graphView.RemoveElement(choicePort);
                });
                
                deleteChoiceButton.AddToClassList("ds-node__buttonDelete");
                choicePort.Add(deleteChoiceButton);
            }

            if (haveConditions)
            {
                Button conditionsButton = DSElementUtility.CreateButton("Add Conditions", () =>
                { AddConditionsBelowPort(choicePort); });

    
                conditionsButton.AddToClassList("ds-node__button");
                choicePort.Add(conditionsButton);
            }

            TextField choiceTextField = DSElementUtility.CreateTextField(choiceData.Text, null, callback =>
            {
                choiceData.Text = callback.newValue;
            });

            choiceTextField.AddClasses
            (
                "ds-node__text-field",
                "ds-node__text-field__hidden",
                "ds-node__choice-text-field"
            );

            choicePort.Add(choiceTextField);
            
            _choicePorts.Add(choicePort);

            return choicePort;
        }

        private VisualElement CreateConditions()
        {
            ObjectField conditionsField = new ObjectField("Conditions Object")
            {
                objectType = typeof(ConditionsSC),
                allowSceneObjects = false
            };

            conditionsField.AddToClassList("ds-node__conditions-field");

            conditionsField.style.marginLeft = new StyleLength(16);
            conditionsField.style.alignSelf = Align.Stretch;

            return conditionsField;
        }
        
        private void AddConditionsBelowPort(Port port)
        {
            if (port == null)
                return;

            VisualElement conditions = CreateConditions();
            conditions.AddToClassList("ds-node__conditions-container");

            int idx = outputContainer.IndexOf(port);
            if (idx < 0)
            {
                outputContainer.Add(conditions);
            }
            else
            {
                outputContainer.Insert(idx + 1, conditions);
            }

            if (!Saves.ConditionsMap.ContainsKey(port))
            {
                Debug.Log("Adding first conditions element to the port.");
                Saves.ConditionsMap[port] = new List<VisualElement>();
            }
            else
            {
                Debug.Log("Adding another conditions element to the same port.");
            }
            Saves.ConditionsMap[port].Add(conditions);
            
            Button butClearCondition = DSElementUtility.CreateButton("X", () =>
                { this.ClearCondition(conditions); });
            
            butClearCondition.AddToClassList("ds-node__buttonDeleteCondition");
            conditions.Add(butClearCondition);

            RefreshExpandedState();
            MarkDirtyRepaint();
        }
        
        private void ClearCondition(VisualElement condition)
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
