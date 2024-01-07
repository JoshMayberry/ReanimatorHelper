using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

using Aarthificial.Reanimation.Nodes;
using jmayberry.ReanimatorHelper.Utilities;
using UnityEngine;
using System.Linq;

namespace jmayberry.ReanimatorHelper.GraphNodes {
	public class SwitchGraphNode : BaseGraphNode {
		VisualElement outputNodeContainer;
		public ReanimatorNode[] nodes { get; set; }
		SwitchNode data;

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

		public override void SaveData(string folderPath, bool autosave=true) {
			if (data == null) {
				this.data = ScriptableObject.CreateInstance<SwitchNode>();
				AssetDatabase.CreateAsset(this.data, AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{this.filename}.asset"));
			}

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
