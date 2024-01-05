using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

using Aarthificial.Reanimation.Nodes;
using jmayberry.ReanimatorHelper.Utilities;

namespace jmayberry.ReanimatorHelper.GraphNodes {
	public class SwitchGraphNode : BaseGraphNode {
		public ReanimatorNode[] nodes { get; set; }

		protected override void DrawHeader() {
            this.SetLabel("Switch");
            base.DrawHeader();
		}
	}
}
