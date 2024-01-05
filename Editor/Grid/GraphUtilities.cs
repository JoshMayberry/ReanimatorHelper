using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

using jmayberry.ReanimatorHelper.GraphNodes;
using static Codice.Client.BaseCommands.WkStatus.Printers.StatusChangeInfo;

namespace jmayberry.ReanimatorHelper.Utilities {
	public static class GraphUtilities {
		public static Button CreateButton(string text, Action onClick = null, int width = 0) {
			Button button = new Button(onClick) {
				text = text
			};

			if (width > 0) {

				button.style.width = width;
			}

			return button;
		}

		public static Foldout CreateFoldout(string title, bool collapsed = false) {
			Foldout foldout = new Foldout() {
				text = title,
				value = !collapsed
			};

			return foldout;
		}

		public static Port CreatePort(this BaseGraphNode node, string portName = "", Orientation orientation = Orientation.Horizontal, Direction direction = Direction.Output, Port.Capacity capacity = Port.Capacity.Single) {
			Port port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));
			port.portName = portName;
			return port;
		}
		public static void SetLabel(this BaseGraphNode node, string label = "") {
			Label titleLabel = node.titleContainer.Q<Label>("title-label");
			if (titleLabel != null) {
				titleLabel.text = "Switch";
			} else {
				node.titleContainer.Add(new Label(label));
			}
		}

		public static TextField CreateTextField(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null, int flexGrow = -1) {
			TextField textField = new TextField() {
				value = value,
				label = label
			};

			if (flexGrow != -1) {
				textField.style.flexGrow = flexGrow;
			}

			if (onValueChanged != null) {
				textField.RegisterValueChangedCallback(onValueChanged);
			}

			return textField;
		}

		public static IntegerField CreateIntegerField(int value = 0, string label = null, EventCallback<ChangeEvent<int>> onValueChanged = null, int flexGrow = -1) {
			IntegerField textField = new IntegerField() {
				value = value,
				label = label
			};

			if (flexGrow != -1) {
				textField.style.flexGrow = flexGrow;
			}

			if (onValueChanged != null) {
				textField.RegisterValueChangedCallback(onValueChanged);
			}

			return textField;
		}

		public static TextField CreateTextArea(string value = null, string label = null, EventCallback<ChangeEvent<string>> onValueChanged = null) {
			TextField textArea = CreateTextField(value, label, onValueChanged);
			textArea.multiline = true;
			return textArea;
		}
	}
}
