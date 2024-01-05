using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using Aarthificial.Reanimation.Common;

using jmayberry.ReanimatorHelper.Utilities;
using System.Linq;

namespace jmayberry.ReanimatorHelper.GraphNodes {
	public class BaseGraphNode : Node {
		[SerializeField] protected ReadableControlDriver controlDriver = new ReadableControlDriver();
		[SerializeField] protected DriverDictionary drivers = new DriverDictionary();
		[SerializeField] protected List<Port> outputPorts = new List<Port>();

		public virtual void Initialize(Vector2 position) {
			SetPosition(new Rect(position, Vector2.zero));

			mainContainer.AddToClassList("ds-node__main-container");
			extensionContainer.AddToClassList("ds-node__extension-container");
		}

		public virtual void Draw() {
			DrawHeader();
			DrawPorts();
			DrawBody();
		}

		protected virtual void DrawHeader() {
			TextField nameField = GraphUtilities.CreateTextField(controlDriver.GetName());
			nameField.AddToClassList("ds-node__text-field");
			nameField.AddToClassList("ds-node__text-field__hidden");
			nameField.AddToClassList("ds-node__filename-text-field");
			titleContainer.Insert(1, nameField);
		}

		protected virtual void DrawPorts() {
            var portContainer = new VisualElement();
            portContainer.style.flexGrow = 1;
            portContainer.style.flexDirection = FlexDirection.Row;
            inputContainer.Insert(0, portContainer);

            portContainer.Add(this.CreatePort("", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi));

            var outputContainer = new VisualElement();
            outputContainer.style.flexGrow = 1;
            outputContainer.style.flexDirection = FlexDirection.Column;
			portContainer.Add(outputContainer);

			RebuildPortList(outputContainer);
        }

        protected virtual void RebuildPortList(VisualElement outputContainer) {
            outputContainer.Clear();

            var addButton = GraphUtilities.CreateButton(
                text: "Add",
                onClick: () => {
					outputPorts.Add(this.CreatePort("", Orientation.Horizontal, Direction.Output, Port.Capacity.Single));
                    RebuildPortList(outputContainer);
                }
            );

			foreach (var port in outputPorts) {
                var horizontalContainer = new VisualElement();
                horizontalContainer.style.flexDirection = FlexDirection.Row;
                outputContainer.Add(horizontalContainer);

                horizontalContainer.Add(GraphUtilities.CreateButton(
                    text: "-",
                    onClick: () => {
                        RemovePortConnections(port);
                        outputPorts.Remove(port);
                        RebuildPortList(outputContainer);
                    },
                    width: 25
                ));
                horizontalContainer.Add(port);
			}

            addButton.SetEnabled(!drivers.keys.Any(key => key == ""));
            outputContainer.Add(addButton);
        }

        protected virtual void DrawBody() {
			DrawControlDriver();
			DrawDriverDictionary();
		}

		protected virtual void DrawControlDriver() {
			Foldout controlDriverFoldout = new Foldout() { text = "Control Driver", value = false };
			extensionContainer.Add(controlDriverFoldout);

			Toggle autoIncrementToggle = new Toggle() { text = "Auto Increment", value = controlDriver.GetAutoIncrement() };
			controlDriverFoldout.Add(autoIncrementToggle);

			Toggle percentageBasedToggle = new Toggle() { text = "Percentage Based", value = controlDriver.GetPercentageBased() };
			controlDriverFoldout.Add(percentageBasedToggle);
		}

		protected virtual void DrawDriverDictionary() {
			if (drivers == null) {
				drivers = new DriverDictionary();
			}
			if (drivers.keys == null) {
				drivers.keys = new List<string>();
			}
			if (drivers.values == null) {
				drivers.values = new List<int>();
			}

			Foldout driversFoldout = new Foldout() { text = "Drivers", value = true };
			extensionContainer.Add(driversFoldout);

			RebuildDriverList(driversFoldout);
		}

		private void RebuildDriverList(Foldout driversFoldout) {
			driversFoldout.Clear();

            var addButton = GraphUtilities.CreateButton(
                text: "Add",
                onClick: () => {
                    drivers.keys.Add("");
                    drivers.values.Add(0);
                    RebuildDriverList(driversFoldout);
                }
            );

            for (int i = 0; i < drivers.keys.Count; i++) {
				int currentIndex = i;
				string currentDriverName = drivers.keys[currentIndex];

				var horizontalContainer = new VisualElement();
				horizontalContainer.style.flexDirection = FlexDirection.Row;

				horizontalContainer.Add(GraphUtilities.CreateButton(
					text: "-",
					onClick: () => {
						drivers.keys.RemoveAt(currentIndex);
						drivers.values.RemoveAt(currentIndex);
						RebuildDriverList(driversFoldout);
					},
					width: 25
				));

				var nameField = GraphUtilities.CreateTextField(
					value: drivers.keys[currentIndex],
					flexGrow: 1
				);
                nameField.RegisterValueChangedCallback(evt => {
                    if (!drivers.keys.Contains(evt.newValue) || evt.newValue == currentDriverName) {
                        drivers.keys[currentIndex] = evt.newValue;
                        addButton.SetEnabled(!drivers.keys.Any(key => key == ""));
                    }
                    else {
                        nameField.value = currentDriverName;
                    }
                });
                horizontalContainer.Add(nameField);

				drivers.values[currentIndex] = currentIndex;
                horizontalContainer.Add(GraphUtilities.CreateIntegerField(
					value: drivers.values[currentIndex],
					onValueChanged: evt => drivers.values[currentIndex] = evt.newValue,
					flexGrow: 1
				));

				driversFoldout.Add(horizontalContainer);
			}
			
			addButton.SetEnabled(!drivers.keys.Any(key => key == ""));
			driversFoldout.Add(addButton);
		}

		private void RemovePortConnections(Port port) {
			if (port == null) {
				return;
			}

			foreach (var connection in port.connections.ToList()) {
				port.Disconnect(connection);

				if (connection.input != null && (connection.input != port)) {
					connection.input.Disconnect(connection);
				}
				if (connection.output != null && (connection.output != port)) {
					connection.output.Disconnect(connection);
				}

				connection.RemoveFromHierarchy();
			}
		}
	}
}
