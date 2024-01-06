using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using Aarthificial.Reanimation.Nodes;
using Aarthificial.Reanimation.Common;
using jmayberry.ReanimatorHelper.Editor;
using jmayberry.ReanimatorHelper.Utilities;

namespace jmayberry.ReanimatorHelper.GraphNodes {
	public abstract class BaseGraphNode : Node {
		[SerializeField] internal protected ControlDriver controlDriver = new ControlDriver();
		[SerializeField] internal protected DriverDictionary drivers = new DriverDictionary();
		[SerializeField] internal protected List<Port> outputPorts = new List<Port>();
		[SerializeField] internal protected Port inputPort;
		[SerializeField] protected StyleColor defaultBackgroundColor;
		[SerializeField] protected ReanimatorGraphView graphView;

		public virtual void Initialize(ReanimatorGraphView graphView, Vector2 position) {
			this.graphView = graphView;
			SetPosition(new Rect(position, Vector2.zero));

			mainContainer.AddToClassList("ds-node__main-container");
			extensionContainer.AddToClassList("ds-node__extension-container");

			defaultBackgroundColor = mainContainer.style.backgroundColor;

			this.AddStyles();
			this.AddHeader();
			this.AddPorts();
			this.AddBody();

			this.RefreshExpandedState();
		}

		public abstract void SaveData(string folderPath, bool autoSave);

		private void AddStyles() {
			styleSheets.Add((StyleSheet)EditorGUIUtility.Load("NodeStyles.uss"));
		}

		protected virtual void AddHeader() {
			TextField nameField = GraphUtilities.CreateTextField(ReadableNodeUtilities.GetName(controlDriver)); // TODO: Make this the file name and put this in the control driver section
			nameField.AddToClassList("ds-node__text-field");
			nameField.AddToClassList("ds-node__text-field__hidden");
			nameField.AddToClassList("ds-node__filename-text-field");
			titleContainer.Insert(1, nameField);
		}

		protected virtual void AddPorts() {
			var portContainer = new VisualElement();
			portContainer.style.flexGrow = 1;
			portContainer.style.flexDirection = FlexDirection.Row;
			inputContainer.Insert(0, portContainer);

			AddInputPorts(portContainer);
			AddOutputPorts(portContainer);
		}

		protected virtual void AddInputPorts(VisualElement portContainer) {
			this.inputPort = this.CreatePort("", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
			portContainer.Add(this.inputPort);
		}

		protected virtual void AddOutputPorts(VisualElement portContainer) { }

		protected virtual void AddBody() {
			DrawControlDriver();
			DrawDriverDictionary();
		}

		protected virtual void DrawControlDriver() {
			Foldout controlDriverFoldout = new Foldout() { text = "Control Driver", value = false };
			extensionContainer.Add(controlDriverFoldout);

			Toggle autoIncrementToggle = new Toggle() { text = "Auto Increment", value = ReadableNodeUtilities.GetAutoIncrement(controlDriver) };
			controlDriverFoldout.Add(autoIncrementToggle);
			autoIncrementToggle.RegisterValueChangedCallback(evt => {
				ReadableNodeUtilities.SetAutoIncrement(controlDriver, evt.newValue);
			});

			Toggle percentageBasedToggle = new Toggle() { text = "Percentage Based", value = ReadableNodeUtilities.GetPercentageBased(controlDriver) };
			controlDriverFoldout.Add(percentageBasedToggle);
			percentageBasedToggle.RegisterValueChangedCallback(evt => {
				ReadableNodeUtilities.SetAutoIncrement(controlDriver, evt.newValue);
			});
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
				string currentName = drivers.keys[currentIndex];

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
					string newKey = evt.newValue;
					drivers.keys[currentIndex] = newKey;
					addButton.SetEnabled(!drivers.keys.Any(key => key == ""));
					UpdateErrorDisplay(driversFoldout);
				});
				horizontalContainer.Add(nameField);

				horizontalContainer.Add(GraphUtilities.CreateIntegerField(
					value: drivers.values[currentIndex],
					onValueChanged: evt => drivers.values[currentIndex] = evt.newValue,
					flexGrow: 1
				));

				driversFoldout.Add(horizontalContainer);
			}
			
			addButton.SetEnabled(!drivers.keys.Any(key => key == ""));
			UpdateErrorDisplay(driversFoldout);
			driversFoldout.Add(addButton);
		}

		private void UpdateErrorDisplay(Foldout driversFoldout) {
			string guid = ReadableNodeUtilities.GetGuid(controlDriver);

			graphView.NodeClearError(guid);

			List<VisualElement> containerList = driversFoldout.Children().ToList();
			HashSet<string> existingDriverKeys = new HashSet<string>();
			for (int i = 0; i < this.drivers.keys.Count; i++) {
				string driverKey = this.drivers.keys[i];
				VisualElement horizontalContainer = containerList[i];

				bool hasError = (driverKey != "") && existingDriverKeys.Contains(driverKey);
				if (hasError) {
					graphView.NodeHasError(guid);
				}

				horizontalContainer.EnableInClassList("ds-node__has-error--duplicate-driver", hasError);
				existingDriverKeys.Add(driverKey);
			}
		}

		protected void RemovePortConnections(Port port) {
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

		public Port CreatePort(string portName = "", Orientation orientation = Orientation.Horizontal, Direction direction = Direction.Output, Port.Capacity capacity = Port.Capacity.Single) {
			Port port = this.InstantiatePort(orientation, direction, capacity, typeof(bool));
			port.portName = portName;
			return port;
		}

		public void SetLabel(string label = "") {
			Label titleLabel = this.titleContainer.Q<Label>("title-label");
			if (titleLabel != null) {
				titleLabel.text = label;
			}
			else {
				this.titleContainer.Add(new Label(label));
			}
		}

		public IEnumerable<BaseGraphNode> YieldChildNodes() {
			foreach (var port in this.outputPorts) {
				foreach (var edge in port.connections) {
					if (edge.input.node is BaseGraphNode childNode) {
						yield return childNode;
					}
				}
			}
		}
	}
}
