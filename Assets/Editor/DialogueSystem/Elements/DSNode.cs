using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;


    public class DSNode : Node
    {
        public string ID { get; set; }
        public string DialogueName { get; set; }
        public Espeaker Speaker { get; set; }
        public DSNodeSaveData Saves { get; set; }
        public string Text { get; set; }
        public DSDialogueType DialogueType { get; set; }
        public DSGroup Group { get; set; }

        protected DSGraphView graphView;
        
        private Color defaultBackgroundColor;

        private TextField _fieldLabel;

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());

            base.BuildContextualMenu(evt);
        }

        public virtual void Initialize(string nodeName, DSGraphView dsGraphView, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();
            
            DialogueName = nodeName;
            
           // DialogueName = DSIOUtility.CheckNameWithOthers(nodeName);

           var txt = DSIOUtility.CheckNameWithOthers(nodeName);
            
            
            Saves = new DSNodeSaveData();
            Saves.ChoicesInNode = new List<DSChoiceSaveData>();
            Text = "Dialogue text.";
            SetPosition(new Rect(position, Vector2.zero));

            graphView = dsGraphView;
            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        public virtual void Draw()
        {
            /* TITLE CONTAINER */

            TextField dialogueNameTextField = DSElementUtility.CreateTextField(DialogueName, null, callback =>
            {
                TextField target = (TextField) callback.target;

                target.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(target.value))
                {
                    if (!string.IsNullOrEmpty(DialogueName))
                    {
                        ++graphView.NameErrorsAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(DialogueName))
                    {
                        --graphView.NameErrorsAmount;
                    }
                }

                if (Group == null)
                {
                    graphView.RemoveUngroupedNode(this);

                    DialogueName = target.value;

                    graphView.AddUngroupedNode(this);

                    return;
                }

                DSGroup currentGroup = Group;

                graphView.RemoveGroupedNode(this, Group);

                DialogueName = target.value;

                graphView.AddGroupedNode(this, currentGroup);
            });

            var xx = DSElementUtility.CreateDropdownField();

            dialogueNameTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__text-field__hidden",
                "ds-node__filename-text-field"
            );

            titleContainer.Insert(0, dialogueNameTextField);
            
            // DRAW ENUMERATOR ESPEAKER 
            
            EnumField speakerEnumField = new EnumField("", Speaker);
            speakerEnumField.RegisterValueChangedCallback(callback => SetSpeaker((Espeaker) callback.newValue));
            
            titleContainer.Add(speakerEnumField);

            /* INPUT CONTAINER */

            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);

            inputContainer.Add(inputPort);

            /* EXTENSION CONTAINER */

            VisualElement customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("ds-node__custom-data-container");

            Foldout textFoldout = DSElementUtility.CreateFoldout("Dialogue Text");
            
            var dp = DSElementUtility.CreateDropdownArea("Dialogue Key", "Choose an option");
            
            FillCsvDropdown(dp);
            dp.RegisterValueChangedCallback(callback => { OnDropdownEvent(dp);});

            TextField textTextField = DSElementUtility.CreateTextArea(Text, null, callback => Text = callback.newValue);

            textTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__quote-text-field"
            );

            textFoldout.Add(textTextField);
            _fieldLabel = DSElementUtility.CreateTextField("XXX");
            
            textFoldout.Add(dp);
            textFoldout.Add(_fieldLabel);

            customDataContainer.Add(textFoldout);

            extensionContainer.Add(customDataContainer);
        }

        public void DisconnectAllPorts()
        {
            DisconnectInputPorts();
            DisconnectOutputPorts();
        }

        private void OnDropdownEvent(DropdownField dropdownField)
        {
            _fieldLabel.value = $"FR : {FantasyDialogueTable.Find_idLng(dropdownField.value).FR}";
        }

        public void FillCsvDropdown(DropdownField  dropdownField)
        {
            List<string> keys = FantasyDialogueTable.FindAll_Keys();
            foreach (string key in keys)
            {
                dropdownField.choices.Add(key);
            }
        }

        private void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }

        private void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }

        private void DisconnectPorts(VisualElement container)
        {
            if (container == null)
            {
                return;
            }
            foreach (var visualElement in container.Children())
            {
                var port = (Port)visualElement;
                if (port == null)
                {
                    continue;
                }
                if (!port.connected)
                {
                    continue;
                }

                graphView.DeleteElements(port.connections);
            }
        }

        public bool IsStartingNode()
        {
            Port inputPort = (Port) inputContainer.Children().First();

            return !inputPort.connected;
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }
        
        public void SetSpeaker(Espeaker speaker)
        {
            Speaker = speaker;
        }
    }
