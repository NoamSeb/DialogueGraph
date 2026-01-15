using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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

            DialogueType = DSDialogueType.MultipleChoice;

            DSChoiceSaveData choiceData = new DSChoiceSaveData()
            {
            };
            
            if (Saves.isMultipleChoice)
            {
                choiceData.Text = "New Choice";
            }
            else
            {
                choiceData.Text = "Continue";
            }

            
            Saves.ChoicesInNode.Add(choiceData);
        }

        private void ClearChoicePorts()
        {
            Saves.ChoicesInNode.Clear(); 
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
        
        private void CreateSingleChoicePort(string text, bool canBeDeleted)
        {
            DSChoiceSaveData choiceData = new DSChoiceSaveData()
            {
                Text = text,
            };

            Saves.ChoicesInNode.Add(choiceData);
            Port choicePort = CreateChoicePort(choiceData, canBeDeleted);
            outputContainer.Add(choicePort);
        }

        private void SetNodeType()
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


            if (Saves.isMultipleChoice)
            {
                CreateSingleChoicePort("New choice", true);
                CreateSingleChoicePort("New choice", true);
            }
            else
                CreateSingleChoicePort("Continue", false);
            
            SetNodeType();

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
                outputContainer.Add(choicePort);
            }
            
            SetNodeType();
            
            RefreshExpandedState();
            
        }

        private Port CreateChoicePort(object userData, bool canBeDeleted = true)
        {
            Port choicePort = this.CreatePort();

            choicePort.userData = userData;

            DSChoiceSaveData choiceData = (DSChoiceSaveData) userData;

            if (canBeDeleted)
            {
                Button deleteChoiceButton = DSElementUtility.CreateButton("X", () =>
                {
                    if (Saves.ChoicesInNode.Count == 1)
                    {
                        return;
                    }

                    if (choicePort.connected)
                    {
                        graphView.DeleteElements(choicePort.connections);
                    }

                    Saves.ChoicesInNode.Remove(choiceData);
                    graphView.RemoveElement(choicePort);
                
                });
                
                deleteChoiceButton.AddToClassList("ds-node__button");
                choicePort.Add(deleteChoiceButton);

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
    }
