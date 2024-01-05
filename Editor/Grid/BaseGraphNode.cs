using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using Aarthificial.Reanimation.Common;

using jmayberry.ReanimatorHelper.Utilities;

namespace jmayberry.ReanimatorHelper.Editor {
	public class BaseGraphNode : Node {
		[SerializeField] protected ReadableControlDriver controlDriver = new ReadableControlDriver();
		[SerializeField] protected DriverDictionary drivers = new DriverDictionary();

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
            titleContainer.Insert(0, nameField);
		}

		protected virtual void DrawPorts() {
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

			for (int i = 0; i < drivers.keys.Count; i++) {
				int currentIndex = i;

				var horizontalContainer = new VisualElement();
				horizontalContainer.style.flexDirection = FlexDirection.Row;

				horizontalContainer.Add(GraphUtilities.CreateTextField(
                    value: drivers.keys[currentIndex],
                    onValueChanged: evt => drivers.keys[currentIndex] = evt.newValue,
                    flexGrow: 1
                ));

                horizontalContainer.Add(GraphUtilities.CreateIntegerField(
                    value: drivers.values[currentIndex],
                    onValueChanged: evt => drivers.values[currentIndex] = evt.newValue,
                    flexGrow: 1
                ));

                horizontalContainer.Add(GraphUtilities.CreateButton(
                    text:"-",
                    onClick: () => {
                        drivers.keys.RemoveAt(currentIndex);
                        drivers.values.RemoveAt(currentIndex);
                        RebuildDriverList(driversFoldout);
                    },
					width: 25
                ));

				driversFoldout.Add(horizontalContainer);
			}

            driversFoldout.Add(GraphUtilities.CreateButton(
                text: "Add",
                onClick: () => {
                    drivers.keys.Add("");
                    drivers.values.Add(0);
                    RebuildDriverList(driversFoldout);
                }
            ));
		}
	}
}
