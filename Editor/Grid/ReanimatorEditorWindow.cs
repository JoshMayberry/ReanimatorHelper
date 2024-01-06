using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using jmayberry.ReanimatorHelper.Utilities;
using UnityEditor.Experimental.GraphView;
using Aarthificial.Reanimation.Nodes;
using jmayberry.ReanimatorHelper.GraphNodes;
using System.Collections.Generic;

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
			
			string folderPath = folderNameTextField.value;
			var switchNodeGuids = AssetDatabase.FindAssets("t:SwitchNode", new[] { folderPath });

			List<BaseGraphNode> createdNodes = new List<BaseGraphNode>();
			foreach (var guid in switchNodeGuids) {
				SwitchNode switchNode = AssetDatabase.LoadAssetAtPath<SwitchNode>(AssetDatabase.GUIDToAssetPath(guid));
				var node = graphView.CreateNode(switchNode, Vector2.zero);
				graphView.AddElement(node);
				createdNodes.Add(node);
			}

			var simpleAnimationGuids = AssetDatabase.FindAssets("t:SimpleAnimationNode", new[] { folderPath });
			foreach (var guid in simpleAnimationGuids) {
				SimpleAnimationNode simpleAnimation = AssetDatabase.LoadAssetAtPath<SimpleAnimationNode>(AssetDatabase.GUIDToAssetPath(guid));
				var node = graphView.CreateNode(simpleAnimation, Vector2.zero);
				graphView.AddElement(node);
				createdNodes.Add(node);
			}

			var mirroredAnimationGuids = AssetDatabase.FindAssets("t:MirroredAnimationNode", new[] { folderPath });
			foreach (var guid in mirroredAnimationGuids) {
				MirroredAnimationNode mirroredAnimation = AssetDatabase.LoadAssetAtPath<MirroredAnimationNode>(AssetDatabase.GUIDToAssetPath(guid));
				var node = graphView.CreateNode(mirroredAnimation, Vector2.zero);
				graphView.AddElement(node);
				createdNodes.Add(node);
			}

			int xPadding = 30;
			int yPosition = 50;
			int initialXPos = 200;
			graphView.schedule.Execute(() => {
				int xPos = initialXPos;
				foreach (var node in createdNodes) {
					node.SetPosition(new Rect(xPos, yPosition, node.layout.width, node.layout.height));
					xPos += (int)node.layout.width + xPadding;
				}
			}).StartingIn(10);
		}

		public void EnableSaving() {
			saveButton.SetEnabled(true);
		}

		public void DisableSaving() {
			saveButton.SetEnabled(false);
		}

		protected void Save() {
			string folderPath = folderNameTextField.value;
			foreach (Node node in graphView.nodes.ToList()) {
				if (node is not BaseGraphNode baseGraphNode) {
					Debug.LogWarning($"Unknown node in node list {node}");
					continue;
				}

				baseGraphNode.SaveData(folderPath, false);
			}
			AssetDatabase.SaveAssets();
		}
	}
}
