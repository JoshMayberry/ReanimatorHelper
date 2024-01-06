using UnityEditor;
using UnityEngine.UIElements;

using Aarthificial.Reanimation.Cels;
using Aarthificial.Reanimation.Nodes;
using jmayberry.ReanimatorHelper.Utilities;
using UnityEngine;

namespace jmayberry.ReanimatorHelper.GraphNodes {
	public class MirroredAnimationGraphNode : BaseGraphNode {
		public MirroredCel[] cels { get; set; }
		MirroredAnimationNode data;

		public void SetData(MirroredAnimationNode data) {
			this.data = data;

			this.controlDriver = ReadableNodeUtilities.GetControlDriver(data);
			this.drivers = ReadableNodeUtilities.GetDriverDictionary(data);
			this.cels = ReadableNodeUtilities.GetCels(data);
		}

		protected override void DrawHeader() {
			this.SetLabel("Mirror");
			base.DrawHeader();
		}

		protected override void RebuildPortList(VisualElement outputContainer) {
			// Do not allow output nodes
		}

		public override void SaveData(string folderPath, bool autosave = true) {
			if (data == null) {
				this.data = ScriptableObject.CreateInstance<MirroredAnimationNode>();
				AssetDatabase.CreateAsset(this.data, AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{ReadableNodeUtilities.GetName(this.controlDriver)}.asset"));
			}

			ReadableNodeUtilities.SetControlDriver(data, this.controlDriver);
			ReadableNodeUtilities.SetDriverDictionary(data, this.drivers);
			ReadableNodeUtilities.SetCels(data, this.cels);
			EditorUtility.SetDirty(data);

			if (autosave) {
				AssetDatabase.SaveAssets();
			}
		}
	}
}
