using UnityEditor;
using UnityEngine.UIElements;

using Aarthificial.Reanimation.Cels;
using jmayberry.ReanimatorHelper.Utilities;

namespace jmayberry.ReanimatorHelper.GraphNodes {
	public class MirroredAnimationGraphNode : BaseGraphNode {
		public MirroredCel[] cels { get; set; }

        protected override void DrawHeader() {
            this.SetLabel("Mirror");
            base.DrawHeader();
        }

        protected override void RebuildPortList(VisualElement outputContainer) {
            // Do not allow output nodes
        }
    }
}
