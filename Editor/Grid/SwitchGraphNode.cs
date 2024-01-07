using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using Aarthificial.Reanimation.Nodes;
using jmayberry.ReanimatorHelper.Utilities;

namespace jmayberry.ReanimatorHelper.GraphNodes {
	public class SwitchGraphNode : BaseGraphNode {
		VisualElement outputNodeContainer;
		public ReanimatorNode[] nodes { get; set; }
		internal SwitchNode data;

		public void SetData(SwitchNode data) {
			this.data = data;
			this.filename = this.data.name;

			this.controlDriver = ReadableNodeUtilities.GetControlDriver(data);
			this.drivers = ReadableNodeUtilities.GetDriverDictionary(data);
			this.nodes = ReadableNodeUtilities.GetNodes(data);
		}

		protected override void AddHeader() {
			this.SetLabel("Switch");
			base.AddHeader();
		}

		public override void SaveData_CreateAsset(string folderPath, bool autosave = true) {
			if (data == null) {
				this.data = ScriptableObject.CreateInstance<SwitchNode>();
				AssetDatabase.CreateAsset(this.data, AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{this.filename}.asset"));
			}

			if (autosave) {
				AssetDatabase.SaveAssets();
			}
		}

		public override void SaveData_UpdateAsset(string folderPath, bool autosave=true) {
			if (data == null) {
				Debug.LogError("data is empty");
				return;
			}

			this.nodes = this.outputPorts
				.SelectMany(port => port.connections)
				.Select(connection => connection.input.node)
				.OfType<BaseGraphNode>() // Filter nodes of type BaseGraphNode
				.Select(node => {
					switch (node) {
						case SwitchGraphNode switchNode:
							return switchNode.data as ReanimatorNode;
							
						case SimpleAnimationGraphNode simpleAnimationNode:
							return simpleAnimationNode.data as ReanimatorNode;
							
						case MirroredAnimationGraphNode mirroredAnimationNode:
							return mirroredAnimationNode.data as ReanimatorNode;
							
						default:
							Debug.Log($"Unknown data type: {node.GetType().Name}");
							return null;
					}
				})
				.Where(data => data != null)
				.ToArray();

			this.data.name = this.filename;
			ReadableNodeUtilities.SetControlDriver(data, this.controlDriver);
			ReadableNodeUtilities.SetDriverDictionary(data, this.drivers);
			ReadableNodeUtilities.SetNodes(data, this.nodes);
			EditorUtility.SetDirty(data);

			if (autosave) {
				AssetDatabase.SaveAssets();
			}
		}

		protected override void AddOutputPorts(VisualElement portContainer) {
			outputNodeContainer = new VisualElement();
			outputNodeContainer.style.flexGrow = 1;
			outputNodeContainer.style.flexDirection = FlexDirection.Column;
			portContainer.Add(outputNodeContainer);

			RebuildOutputPorts();
		}

		public virtual void RebuildOutputPorts() {
			outputNodeContainer.Clear();

			var addButton = GraphUtilities.CreateButton(
				text: "Add",
				onClick: () => {
					outputPorts.Add(this.CreatePort("", Orientation.Horizontal, Direction.Output, Port.Capacity.Single));
					RebuildOutputPorts();
				}
			);

			foreach (var port in outputPorts) {
				var horizontalContainer = new VisualElement();
				horizontalContainer.style.flexDirection = FlexDirection.Row;
				outputNodeContainer.Add(horizontalContainer);

				horizontalContainer.Add(GraphUtilities.CreateButton(
					text: "-",
					onClick: () => {
						RemovePortConnections(port);
						outputPorts.Remove(port);
						RebuildOutputPorts();
					},
					width: 25
				));
				horizontalContainer.Add(port);
			}

			addButton.SetEnabled(!drivers.keys.Any(key => key == ""));
			outputNodeContainer.Add(addButton);
		}
	}
}
