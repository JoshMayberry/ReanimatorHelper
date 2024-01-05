using System.Collections.Generic;
using jmayberry.ReanimatorHelper.GraphNodes;

namespace jmayberry.ReanimatorHelper.Data.Error {

	public class NodeErrorData {
		public ErrorData ErrorData { get; set; }
		public List<BaseGraphNode> Nodes { get; set; }

		public NodeErrorData()
		{
			ErrorData = new ErrorData();
			Nodes = new List<BaseGraphNode>();
		}
	}
}