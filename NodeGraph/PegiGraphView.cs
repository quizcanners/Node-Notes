using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityGraph = UnityEditor.Experimental.GraphView;

namespace PlayerAndEditorGUI.NodeGraph
{
    public class PegiGraphView : UnityGraph.GraphView
    {
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var ports = base.ports.ToList();

            return ports;
        }

        public PegiGraphView()
        {
            SetupZoom(UnityGraph.ContentZoomer.DefaultMinScale, UnityGraph.ContentZoomer.DefaultMaxScale);

			this.AddManipulator(new UnityGraph.ContentDragger());
			this.AddManipulator(new UnityGraph.SelectionDragger());
			this.AddManipulator(new UnityGraph.RectangleSelector());
           
			var grid = new UnityGraph.GridBackground();
			Insert(0, grid);
			grid.StretchToParentSize();
		}

        public PegiNode SetTarget(IPEGI_Node target)
        {
            var node = new PegiNode(target, this);
            AddElement(node);
            return node;
        }
    }
}