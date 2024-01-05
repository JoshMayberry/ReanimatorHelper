using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

using jmayberry.ReanimatorHelper.Utilities;

namespace jmayberry.ReanimatorHelper.Editor {
	public class ReanimatorGraphView : GraphView {
		public ReanimatorGraphView() {
			DrawBackground();
			AddStyles();
			AddManipulators();
		}

		private void DrawBackground() {
			GridBackground background = new GridBackground();
			background.StretchToParentSize();
			Insert(0, background);
		}

		private void AddStyles() {
			styleSheets.Add((StyleSheet)EditorGUIUtility.Load("Variables.uss"));
			styleSheets.Add((StyleSheet)EditorGUIUtility.Load("NodeStyles.uss"));
			styleSheets.Add((StyleSheet)EditorGUIUtility.Load("ToolbarStyles.uss"));
			styleSheets.Add((StyleSheet)EditorGUIUtility.Load("GraphViewStyles.uss"));
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
}
