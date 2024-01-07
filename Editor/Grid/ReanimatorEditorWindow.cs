using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

using Aarthificial.Reanimation.Nodes;
using jmayberry.ReanimatorHelper.Utilities;
using jmayberry.ReanimatorHelper.GraphNodes;

namespace jmayberry.ReanimatorHelper.Editor {
	// Use: [Unity Dialogue System](https://www.youtube.com/playlist?list=PL0yxB6cCkoWK38XT4stSztcLueJ_kTx5f)
	public class ReanimatorEditorWindow : EditorWindow {
		private readonly string defaultFolderName = "";
		private ReanimatorGraphView graphView;

		private static TextField folderNameTextField;
		private Button saveButton;
		private Button miniMapButton;

		[MenuItem("Window/Reanimator")]
		public static void ShowWindow() {
			GetWindow<ReanimatorEditorWindow>("Reanimator");
		}

		protected void OnEnable() {
			AddStyles();
			AddGraph();
			AddToolbar();
		}

		protected void AddStyles() {
			rootVisualElement.styleSheets.Add((StyleSheet)EditorGUIUtility.Load("Variables.uss"));
		}

		private void AddToolbar() {
			Toolbar toolbar = new Toolbar();
			rootVisualElement.Add(toolbar);
			toolbar.styleSheets.Add((StyleSheet)EditorGUIUtility.Load("ToolbarStyles.uss"));

			folderNameTextField = GraphUtilities.CreateTextField(defaultFolderName, "Folder Path:", callback => {
				folderNameTextField.value = callback.newValue;
			});

			saveButton = GraphUtilities.CreateButton("Save", Save);
			miniMapButton = GraphUtilities.CreateButton("Minimap", () => {
				graphView.ToggleMinimap();
				miniMapButton.ToggleInClassList("ds-toolbar__button__selected");
			});

			toolbar.Add(folderNameTextField);
			toolbar.Add(GraphUtilities.CreateButton("Set Path to Current Folder", () => {
				folderNameTextField.value = GraphUtilities.GetCurrentFolderPath();
			}));
			toolbar.Add(GraphUtilities.CreateButton("Load", Load));
			toolbar.Add(saveButton);
			toolbar.Add(miniMapButton);
			toolbar.Add(GraphUtilities.CreateButton("AutoSort", () => {
				var nodeList = new List<BaseGraphNode>();

				foreach (Node node in graphView.nodes) {
					if (node is BaseGraphNode baseGraphNode) {
						nodeList.Add(baseGraphNode);
					}
				}

				this.AutoSort(nodeList);
			}));
		}
		private void AddGraph() {
			graphView = new ReanimatorGraphView(this) {
				name = "Reanimator Graph"
			};
			rootVisualElement.Add(graphView);

			graphView.StretchToParentSize();
		}

		protected void Load() {
			graphView.DeleteElements(graphView.nodes.ToList());

			Dictionary<string, BaseGraphNode> nodeCatalog = Load_AddNodes();
			Load_AddConnections(nodeCatalog);

			// Let them render in first
			graphView.schedule.Execute(() => {
				AutoSort(nodeCatalog.Values.ToList());
			}).StartingIn(10);
		}

		private Dictionary<string, BaseGraphNode> Load_AddNodes() {
			string folderPath = folderNameTextField.value;

			Dictionary<string, BaseGraphNode> nodeCatalog = new Dictionary<string, BaseGraphNode>();
			foreach (var guid in AssetDatabase.FindAssets("t:SwitchNode", new[] { folderPath })) {
				SwitchNode switchNode = AssetDatabase.LoadAssetAtPath<SwitchNode>(AssetDatabase.GUIDToAssetPath(guid));
				SwitchGraphNode node = graphView.CreateNode(switchNode, Vector2.zero);
				nodeCatalog[ReadableNodeUtilities.GetGuid(node.controlDriver)] = node;
				graphView.AddElement(node);
			}

			var simpleAnimationGuids = AssetDatabase.FindAssets("t:SimpleAnimationNode", new[] { folderPath });
			foreach (var guid in simpleAnimationGuids) {
				SimpleAnimationNode simpleAnimation = AssetDatabase.LoadAssetAtPath<SimpleAnimationNode>(AssetDatabase.GUIDToAssetPath(guid));
				SimpleAnimationGraphNode node = graphView.CreateNode(simpleAnimation, Vector2.zero);
				nodeCatalog[ReadableNodeUtilities.GetGuid(node.controlDriver)] = node;
				graphView.AddElement(node);
			}

			var mirroredAnimationGuids = AssetDatabase.FindAssets("t:MirroredAnimationNode", new[] { folderPath });
			foreach (var guid in mirroredAnimationGuids) {
				MirroredAnimationNode mirroredAnimation = AssetDatabase.LoadAssetAtPath<MirroredAnimationNode>(AssetDatabase.GUIDToAssetPath(guid));
				MirroredAnimationGraphNode node = graphView.CreateNode(mirroredAnimation, Vector2.zero);
				nodeCatalog[ReadableNodeUtilities.GetGuid(node.controlDriver)] = node;
				graphView.AddElement(node);
			}

			return nodeCatalog;
		}

		private void Load_AddConnections(Dictionary<string, BaseGraphNode> nodeCatalog) {
			foreach (BaseGraphNode baseNode in nodeCatalog.Values) {
				if (baseNode is SwitchGraphNode switchGraphNode) {
					bool addedPort = false;
					foreach (ReanimatorNode unknownNode in switchGraphNode.nodes) {
						BaseGraphNode inputNode;
						if (unknownNode is SwitchNode switchNode) {
							inputNode = nodeCatalog[ReadableNodeUtilities.GetGuid(switchNode)];
						}
						else if (unknownNode is SimpleAnimationNode simpleAnimationNode) {
							inputNode = nodeCatalog[ReadableNodeUtilities.GetGuid(simpleAnimationNode)];
						}
						else if (unknownNode is MirroredAnimationNode mirroredAnimationNode) {
							inputNode = nodeCatalog[ReadableNodeUtilities.GetGuid(mirroredAnimationNode)];
						}
						else {
							Debug.LogWarning($"Unknown node {unknownNode}");
							continue;
						}

						Port outputPort = switchGraphNode.CreatePort("", Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
						switchGraphNode.outputPorts.Add(outputPort);
						addedPort = true;

						Edge edge = new Edge() {
							input = inputNode.inputPort,
							output = outputPort,
						};
						inputNode.inputPort.Connect(edge);
						outputPort.Connect(edge);

						graphView.Add(edge);
					}

					if (addedPort) {
						switchGraphNode.RebuildOutputPorts();
					}
				}
			}
		}

		private void AutoSort(List<BaseGraphNode> nodeList) {
			var nodesByDepth = AutoSort_CalculateDepthLevels(nodeList);

			int yPadding = 30;
			int xPadding = 90;
			int initialYPos = 50;
			int initialXPos = 200;
			int widestNodeInPreviousDepth = 0;

			var sortedDepthLevels = nodesByDepth.Keys.ToList();
			sortedDepthLevels.Sort();

			var parentList = new List<BaseGraphNode>();
			foreach (var depthLevel in sortedDepthLevels) {
				int yPos = initialYPos;
				int widestNodeInThisDepth = 0;

				var nodesAtDepth = nodesByDepth[depthLevel];

				var connectionOrders = new Dictionary<BaseGraphNode, (int parentOrder, int portOrder)>();

				foreach (var node in nodesAtDepth) {
					connectionOrders[node] = AutoSort_GetConnectionOrder(node, parentList);
				}

				var sortedNodes = nodesAtDepth.OrderBy(n => connectionOrders[n].parentOrder)
											  .ThenBy(n => connectionOrders[n].portOrder)
											  .ToList();

				parentList = sortedNodes.ToList();
				foreach (var node in sortedNodes) {
					int nodeWidth = (int)node.layout.width;
					int xPos = initialXPos + widestNodeInPreviousDepth + nodeWidth / 2;

					node.SetPosition(new Rect(xPos, yPos, nodeWidth, node.layout.height));
					yPos += (int)node.layout.height + yPadding;

					widestNodeInThisDepth = Math.Max(widestNodeInThisDepth, nodeWidth);
				}

				widestNodeInPreviousDepth += widestNodeInThisDepth + xPadding;
			}
		}

		private (int parentOrder, int portOrder) AutoSort_GetConnectionOrder(BaseGraphNode node, List<BaseGraphNode> parentList) {
			if (node.inputPort.connected) {
				foreach (var edge in node.inputPort.connections) {
					if (edge.output.node is BaseGraphNode parentNode) {
						int parentOrder = parentList.IndexOf(parentNode);
						int portOrder = parentNode.outputPorts.IndexOf(edge.output);
						return (parentOrder, portOrder);
					}
				}
			}
			return (-1, -1);
		}

		private Dictionary<int, List<BaseGraphNode>> AutoSort_CalculateDepthLevels(List<BaseGraphNode> nodeList) {

			foreach (var node in nodeList) {
				node.userData = -1;
			}

			foreach (var node in nodeList) {
				if (!node.inputPort.connected) {
					AutoSort_TraverseAndSetDepth(node, 0);
				}
			}

			var nodesByDepth = new Dictionary<int, List<BaseGraphNode>>();
			foreach (var node in nodeList) {
				int currentDepth = (int)node.userData;

				if (!nodesByDepth.ContainsKey(currentDepth)) {
					nodesByDepth[currentDepth] = new List<BaseGraphNode>();
				}
				nodesByDepth[currentDepth].Add(node);
			}

			return nodesByDepth;
		}

		private void AutoSort_TraverseAndSetDepth(BaseGraphNode node, int currentDepth) {
			int recordedDepth = (int)node.userData;
			if (recordedDepth > currentDepth) {
				return; // For nodes connected to multiple outputs, push them out as far as needed
			}

			node.userData = currentDepth;
			foreach (var childNode in node.YieldChildNodes()) {
				AutoSort_TraverseAndSetDepth(childNode, currentDepth + 1);
			}
		}

		public void EnableSaving() {
			saveButton.SetEnabled(true);
		}

		public void DisableSaving() {
			saveButton.SetEnabled(false);
		}

		protected void Save() {
			string folderPath = folderNameTextField.value;
			if (!AssetDatabase.IsValidFolder($"{folderPath}/Animations/")) {
				AssetDatabase.CreateFolder(folderPath, "Animations");
			}
			if (!AssetDatabase.IsValidFolder($"{folderPath}/Switches/")) {
				AssetDatabase.CreateFolder(folderPath, "Switches");
			}

			foreach (Node node in graphView.nodes.ToList()) {
				if (node is SwitchGraphNode switchGraphNode) {
					switchGraphNode.SaveData($"{folderPath}/Switches", false);
					continue;
				}
				else if (node is SimpleAnimationGraphNode simpleAnimationGraphNode) {
					simpleAnimationGraphNode.SaveData($"{folderPath}/Animations", false);
					continue;
				}
				else if (node is MirroredAnimationGraphNode mirroredAnimationGraphNode) {
					mirroredAnimationGraphNode.SaveData($"{folderPath}/Switches", false);
					continue;
				}

				Debug.LogWarning($"Unknown node in node list {node}");
			}
			AssetDatabase.SaveAssets();
		}
	}
}
