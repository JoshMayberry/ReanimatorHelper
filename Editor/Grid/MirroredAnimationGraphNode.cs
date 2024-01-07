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
			this.filename = this.data.name;

			this.controlDriver = ReadableNodeUtilities.GetControlDriver(data);
			this.drivers = ReadableNodeUtilities.GetDriverDictionary(data);
			this.cels = ReadableNodeUtilities.GetCels(data);
		}

		protected override void AddHeader() {
			this.SetLabel("Mirror");
			base.AddHeader();
		}

		public override void SaveData(string folderPath, bool autosave = true) {
			if (data == null) {
				this.data = ScriptableObject.CreateInstance<MirroredAnimationNode>();
				AssetDatabase.CreateAsset(this.data, AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{this.filename}.asset"));
			}

			this.data.name = this.filename;
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
