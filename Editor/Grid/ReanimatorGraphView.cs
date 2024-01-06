using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

using jmayberry.ReanimatorHelper.Utilities;
using jmayberry.ReanimatorHelper.GraphNodes;
using System.Text.RegularExpressions;
using Aarthificial.Reanimation.Nodes;

namespace jmayberry.ReanimatorHelper.Editor {
	public class ReanimatorGraphView : GraphView {

		private ReanimatorEditorWindow editorWindow;
		private ReanimatorSearchWindow searchWindow;
		private MiniMap miniMap;
		internal HashSet<string> ErrorNodes = new HashSet<string>();

		public ReanimatorGraphView(ReanimatorEditorWindow editorWindow) {
			this.editorWindow = editorWindow;

			AddStyles();
			DrawBackground();
			AddSearchWindow();
			AddMiniMap();
			AddManipulators();
		}

		private void AddStyles() {
			styleSheets.Add((StyleSheet)EditorGUIUtility.Load("GraphViewStyles.uss"));
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
		private void AddMiniMap() {
			miniMap = new MiniMap() {
				anchored = true,
				visible = false,
			};
			Add(miniMap);

			miniMap.SetPosition(new Rect(15, 50, 200, 180));

			StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
			StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));
			miniMap.style.backgroundColor = backgroundColor;
			miniMap.style.borderTopColor = borderColor;
			miniMap.style.borderRightColor = borderColor;
			miniMap.style.borderBottomColor = borderColor;
			miniMap.style.borderLeftColor = borderColor;
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
			node.Initialize(this, position);
			node.Draw();

			return node;
		}

		internal SwitchGraphNode CreateNode(SwitchNode data, Vector2 position) {
			SwitchGraphNode node = new SwitchGraphNode();
			node.SetData(data);
			node.Initialize(this, position);
			node.Draw();

			return node;
		}

		internal SimpleAnimationGraphNode CreateNode(SimpleAnimationNode data, Vector2 position) {
			SimpleAnimationGraphNode node = new SimpleAnimationGraphNode();
			node.SetData(data);
			node.Initialize(this, position);
			node.Draw();

			return node;
		}

		internal MirroredAnimationGraphNode CreateNode(MirroredAnimationNode data, Vector2 position) {
			MirroredAnimationGraphNode node = new MirroredAnimationGraphNode();
			node.SetData(data);
			node.Initialize(this, position);
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

		public void ToggleMinimap() {
			miniMap.visible = !miniMap.visible;
		}

		public void NodeClearError(string guid) {
			ErrorNodes.Remove(guid);

			if (ErrorNodes.Count <= 0) {
				editorWindow.EnableSaving();
			}
		}

		public void NodeHasError(string guid) {
			ErrorNodes.Add(guid);
			editorWindow.DisableSaving();
		}
	}
}
