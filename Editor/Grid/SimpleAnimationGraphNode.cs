using UnityEditor;
using UnityEngine.UIElements;

using Aarthificial.Reanimation.Cels;
using Aarthificial.Reanimation.Nodes;
using jmayberry.ReanimatorHelper.Utilities;
using UnityEngine;

namespace jmayberry.ReanimatorHelper.GraphNodes {
	public class SimpleAnimationGraphNode : BaseGraphNode {
		public SimpleCel[] cels { get; set; }
		internal SimpleAnimationNode data;

		public void SetData(SimpleAnimationNode data) {
			this.data = data;
			this.filename = this.data.name;

			this.controlDriver = ReadableNodeUtilities.GetControlDriver(data);
			this.drivers = ReadableNodeUtilities.GetDriverDictionary(data);
			this.cels = ReadableNodeUtilities.GetCels(data);
		}

		public override void SaveData_CreateAsset(string folderPath, bool autosave = true) {
			if (data == null) {
				this.data = ScriptableObject.CreateInstance<SimpleAnimationNode>();
				AssetDatabase.CreateAsset(this.data, AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{this.filename}.asset"));
			}

			if (autosave) {
				AssetDatabase.SaveAssets();
			}
		}

		public override void SaveData_UpdateAsset(string folderPath, bool autosave = true) {
			if (data == null) {
				Debug.LogError("data is empty");
				return;
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
