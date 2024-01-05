using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

using jmayberry.ReanimatorHelper.GraphNodes;
using jmayberry.ReanimatorHelper.Utilities;
using System;

namespace jmayberry.ReanimatorHelper.Editor {
	public class ReanimatorGraphView : GraphView {

        private ReanimatorEditorWindow editorWindow;
        private ReanimatorSearchWindow searchWindow;
        public ReanimatorGraphView(ReanimatorEditorWindow editorWindow) {
            this.editorWindow = editorWindow;

            DrawBackground();
			AddSearchWindow();
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
			this.AddManipulator(CreateNodeContextualMenu("Switch"));
			this.AddManipulator(CreateNodeContextualMenu("SimpleAnimation"));
			this.AddManipulator(CreateNodeContextualMenu("MirroredAnimation"));
        }

        private void AddSearchWindow() {
            if (searchWindow == null) {
                searchWindow = ScriptableObject.CreateInstance<ReanimatorSearchWindow>();
            }

            searchWindow.Initialize(this);

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        private IManipulator CreateNodeContextualMenu(string nodeName) {
			ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
				menuEvent => menuEvent.menu.AppendAction($"Add {nodeName}", actionEvent => AddElement(CreateNode(nodeName, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
			);
			return contextualMenuManipulator;
		}

		internal BaseGraphNode CreateNode(string nodeName, Vector2 position) {
            Type nodeType = Type.GetType($"jmayberry.ReanimatorHelper.GraphNodes.{nodeName}GraphNode");
            BaseGraphNode node = (BaseGraphNode)Activator.CreateInstance(nodeType);
            node.Initialize(position);
			node.Draw();
			return node;
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

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false) {
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow) {
                worldMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, mousePosition - editorWindow.position.position);
            }

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);

            return localMousePosition;
        }
    }
}
