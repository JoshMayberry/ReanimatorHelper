using UnityEditor.Experimental.GraphView;

using Aarthificial.Reanimation.Nodes;
using jmayberry.ReanimatorHelper.Utilities;

namespace jmayberry.ReanimatorHelper.Editor {
	public class SwitchGraphNode : BaseGraphNode {
		public ReanimatorNode[] nodes { get; set; }

		protected override void DrawPorts() {
			base.DrawPorts();

			inputContainer.Add(this.CreatePort("From", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi));
            inputContainer.Add(this.CreatePort("To", Orientation.Horizontal, Direction.Output, Port.Capacity.Single));
		}
	}
}
