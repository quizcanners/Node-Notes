using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityGraph = UnityEditor.Experimental.GraphView;

namespace PlayerAndEditorGUI.NodeGraph
{
    public class PegiNode : Node, IPEGI, IPEGI_GraphManager
    {
        private IPEGI_Node _source;

        private Port toChildrenPort;

        public PegiNode(IPEGI_Node source, GraphView graph, PegiNode parent = null)
        {
            _source = source;

            _source.GraphManager = this;

            name = _source.GetNameForInspector();

            title = name;

            if (parent!= null)
            {
                Add(new PegiPortParentConnection()).ConnectTo(parent.toChildrenPort);
            }

            foreach (var port in source.GetPorts())
                Add(port);

            foreach (var childSource in source.GetNodes())
            {
                if (toChildrenPort == null)
                {
                    toChildrenPort = Add(new PegiPortChildrenConnection());
                }
                
                graph.AddElement(new PegiNode(source: childSource, graph: graph, this));
            }

            Button contentsButton = new Button(() => { Debug.Log("Clicked!"); });
            contentsButton.text = contentsButton.name = "Test Button";
            inputContainer.Add(contentsButton);

            RefreshExpandedState();
            RefreshPorts();
        }

        public Port Add<T>(T port) where T: PegiPort
        {
            port.source = _source;
            var inst = port.InstantiateOn(this);
            inputContainer.Add(inst);
            return inst;
        }

        public bool Inspect()
        {


            return false;
        }
    }

    public interface IPEGI_Node
    {
        List<IPEGI_Node> GetNodes();
        List<PegiPort> GetPorts();
        List<Action> GetActions();

        IPEGI_GraphManager GraphManager { set; }
    }

    public interface IPEGI_GraphManager : IPEGI
    {


    }


}