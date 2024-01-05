using UnityEditor;
using UnityEngine.UIElements;

using Aarthificial.Reanimation.Cels;
using jmayberry.ReanimatorHelper.Utilities;

namespace jmayberry.ReanimatorHelper.GraphNodes {
	public class SimpleAnimationGraphNode : BaseGraphNode {
		public SimpleCel[] cels { get; set; }

		protected override void DrawHeader() {
			this.SetLabel("Simple");
			base.DrawHeader();
		}

		protected override void RebuildPortList(VisualElement outputContainer) {
			// Do not allow output nodes
		}
	}
}
