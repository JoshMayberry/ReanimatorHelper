using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using Aarthificial.Reanimation.Cels;
using Aarthificial.Reanimation.Nodes;
using Aarthificial.Reanimation.Common;
using System.Collections.Generic;

namespace jmayberry.ReanimatorHelper.Editor {

	public class ReanimatorEditor : EditorWindow {
		protected float splitterPos = 250f;
		protected bool isResizing;
		protected Rect splitterRect;
		protected TriggerArea triggerArea;
		private ReanimatorGraphView reanimatorGraph;

		[MenuItem("Window/Reanimator")]
		public static void ShowWindow() {
			GetWindow<ReanimatorEditor>("Reanimator");
		}

		protected void OnEnable() {
			triggerArea = new TriggerArea();

			reanimatorGraph = new ReanimatorGraphView {
				name = "Reanimator Graph"
			};
		}

		protected void OnGUI() {
			EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));

			Rect graphRect = new Rect(splitterPos + 6, 0, position.width - 3, position.height);
			GUILayout.BeginArea(graphRect);
			DrawGraph(graphRect);
			GUILayout.EndArea();

			triggerArea.DrawArea(splitterPos);
			DrawSplitter();

			EditorGUILayout.EndHorizontal();
			ProcessEvents(Event.current);
			if (GUI.changed) Repaint();
		}

		protected void DrawSplitter() {
			splitterRect = new Rect(splitterPos, 0, 2, position.height);
			EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);
			GUI.DrawTexture(splitterRect, EditorGUIUtility.whiteTexture);
		}

		protected void DrawGraph(Rect rect) {
			if (reanimatorGraph.parent == null) {
				rootVisualElement.Add(reanimatorGraph);
			}

			reanimatorGraph.style.position = Position.Absolute;
			reanimatorGraph.style.left = rect.xMin;
			reanimatorGraph.style.top = rect.yMin;
			reanimatorGraph.style.width = rect.width;
			reanimatorGraph.style.height = rect.height;
		}

		protected void ProcessEvents(Event e) {
			switch (e.type) {
				case EventType.MouseDown:
					if (splitterRect.Contains(e.mousePosition)) {
						isResizing = true;
					}
					break;
				case EventType.MouseUp:
					isResizing = false;
					break;
				case EventType.MouseDrag:
					if (isResizing) {
						splitterPos = e.mousePosition.x;
						Repaint();
					}
					break;
			}
		}
	}

	public class TriggerArea {
		public void DrawArea(float width) {
			EditorGUILayout.BeginVertical(GUILayout.Width(width), GUILayout.ExpandHeight(true));
			GUILayout.Label("Triggers", EditorStyles.boldLabel);
			EditorGUILayout.EndVertical();
		}
	}

	public class ReanimatorGraphView : GraphView {
		public ReanimatorGraphView() {
			DrawBackground();
			AddManipulators();
        }

        private void DrawBackground() {
			GridBackground background = new GridBackground();
			background.StretchToParentSize();
			Insert(0, background);
		}

		private void AddManipulators() {
			this.SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
            this.AddManipulator(CreateNodeContextualMenu());
        }

        private IManipulator CreateNodeContextualMenu() {
			ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
				menuEvent => menuEvent.menu.AppendAction("Add Switch", actionEvent => AddElement(CreateSwitch(actionEvent.eventInfo.localMousePosition)))
			);
			return contextualMenuManipulator;
        }

		private BaseGraphNode CreateSwitch(Vector2 position) {
			var switchNode = new SwitchGraphNode();
			switchNode.Initialize(position);
            switchNode.Draw();
            return switchNode;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            var compatablePorts = new List<Port>();

			ports.ForEach(port => {
				if ((startPort == port) || (startPort.node == port.node) || (startPort.direction == port.direction)) {
					return;
				}

				compatablePorts.Add(port);
			});

            return compatablePorts;
        }
    }

	public class ReadableControlDriver: ControlDriver {
		public string GetName() {
			return this.name;
		}
		public void SetName(string value) {
			this.name = value;
		}
		public bool GetAutoIncrement() {
			return this.autoIncrement;
		}
		public void SetAutoIncrement(bool value) {
			this.autoIncrement = value;
		}
		public bool GetPercentageBased() {
			return this.percentageBased;
		}
		public void SetPercentageBased(bool value) {
			this.percentageBased = value;
		}
	}

	public class BaseGraphNode : Node {
		[SerializeField] protected ReadableControlDriver controlDriver = new ReadableControlDriver();
		[SerializeField] protected DriverDictionary drivers = new DriverDictionary();

		public virtual void Initialize(Vector2 position) {
			SetPosition(new Rect(position, Vector2.zero));
		}

		public virtual void Draw() {
			DrawHeader();
			DrawPorts();
			DrawBody();
		}

		protected virtual void DrawHeader() {
			TextField nameField = new TextField() { value = controlDriver.GetName() };
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

                // Create a horizontal container for each set of elements
                var horizontalContainer = new VisualElement();
                horizontalContainer.style.flexDirection = FlexDirection.Row;

                var keyField = new TextField() { value = drivers.keys[currentIndex] };
                keyField.RegisterValueChangedCallback(evt => drivers.keys[currentIndex] = evt.newValue);
                keyField.style.flexGrow = 1; // Allow the field to grow as needed
                horizontalContainer.Add(keyField);

                var valueField = new IntegerField() { value = drivers.values[currentIndex] };
                valueField.RegisterValueChangedCallback(evt => drivers.values[currentIndex] = evt.newValue);
                valueField.style.flexGrow = 1; // Allow the field to grow as needed
                horizontalContainer.Add(valueField);

                var removeButton = new Button() { text = "-" };
                removeButton.clicked += () => {
                    drivers.keys.RemoveAt(currentIndex);
                    drivers.values.RemoveAt(currentIndex);
                    RebuildDriverList(driversFoldout);
                };
                // Optionally, restrict the width of the remove button
                removeButton.style.width = 25;
                horizontalContainer.Add(removeButton);

                // Add the horizontal container to the foldout
                driversFoldout.Add(horizontalContainer);
            }

            var addButton = new Button() { text = "Add" };
            addButton.clicked += () => {
                drivers.keys.Add("New Key");
                drivers.values.Add(0);
                RebuildDriverList(driversFoldout);
            };
            driversFoldout.Add(addButton);
        }


    }

    public class SwitchGraphNode : BaseGraphNode {
		public ReanimatorNode[] nodes { get; set; }

		protected override void DrawPorts() {
			base.DrawPorts();

			Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
			inputPort.portName = "From";
			inputContainer.Insert(0, inputPort);

            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            outputPort.portName = "To";
			inputContainer.Insert(1, outputPort);
        }
    }

	public class SimpleAnimationGraphNode : BaseGraphNode {
		public SimpleCel[] cels { get; set; }
	}

	public class MirroredAnimationGraphNode : BaseGraphNode {
		public MirroredCel[] cels { get; set; }
	}
}