using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

using Aarthificial.Reanimation.Nodes;
using jmayberry.ReanimatorHelper.Utilities;
using UnityEngine;

namespace jmayberry.ReanimatorHelper.GraphNodes {
	public class SwitchGraphNode : BaseGraphNode {
		public ReanimatorNode[] nodes { get; set; }
		SwitchNode data;

		public void SetData(SwitchNode data) {
			this.data = data;

			this.controlDriver = ReadableNodeUtilities.GetControlDriver(data);
			this.drivers = ReadableNodeUtilities.GetDriverDictionary(data);
			this.nodes = ReadableNodeUtilities.GetNodes(data);
		}

		protected override void DrawHeader() {
			this.SetLabel("Switch");
			base.DrawHeader();
		}

		public override void SaveData(string folderPath, bool autosave=true) {
			if (data == null) {
				this.data = ScriptableObject.CreateInstance<SwitchNode>();
				AssetDatabase.CreateAsset(this.data, AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{ReadableNodeUtilities.GetName(this.controlDriver)}.asset"));
			}

			ReadableNodeUtilities.SetControlDriver(data, this.controlDriver);
			ReadableNodeUtilities.SetDriverDictionary(data, this.drivers);
			ReadableNodeUtilities.SetNodes(data, this.nodes);
			EditorUtility.SetDirty(data);

			if (autosave) {
				AssetDatabase.SaveAssets();
			}
		}
	}
}
