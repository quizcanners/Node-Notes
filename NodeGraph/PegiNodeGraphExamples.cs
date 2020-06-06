using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace PlayerAndEditorGUI.NodeGraph
{
    public class PegiNodeGraphExamples : MonoBehaviour, IPEGI, IPEGI_Node
    {

        private TestNode node0 = new TestNode("Node A");
        private TestNode node1 = new TestNode("Node B");

        private PegiPort<float> portA = new PegiPort<float>(Direction.Input, "Port A");
        private PegiPort<float> portB = new PegiPort<float>(Direction.Output, "Port B");

        public IPEGI_GraphManager GraphManager { get; set; }

        public List<Action> GetActions()
        {
            throw new NotImplementedException();
        }

        public List<IPEGI_Node> GetNodes()
        {
            return new List<IPEGI_Node>()
            {
                node0,
                node1,
            };
        }

        public List<PegiPort> GetPorts()
        {
            return new List<PegiPort>()
            {
                portA,
                portB,
            };
        }


        private int _inspectedStuff = -1;

        public bool Inspect()
        {
            if ("Open Graph Editor".Click().nl())
               this.OpenNodeWindow();

            node0.enter_Inspect(ref _inspectedStuff, 0).nl(); //.Nested_Inspect();
            node1.enter_Inspect(ref _inspectedStuff, 1).nl(); //.Nested_Inspect();

            portA.enter_Inspect(ref _inspectedStuff, 2).nl(); //.Nested_Inspect();
            portB.enter_Inspect(ref _inspectedStuff, 3).nl(); //.Nested_Inspect();

            return false;
        }





        public class TestNode : IPEGI_Node, IPEGI, IGotDisplayName
        {
            private string _name;

            private PegiPort<float> portA = new PegiPort<float>(Direction.Input, "Port A");
            private PegiPort<float> portB = new PegiPort<float>(Direction.Output, "Port B");

            public IPEGI_GraphManager GraphManager { get; set; }

            public TestNode(string name)
            {
                _name = name;
            }

            public List<Action> GetActions()
            {
                throw new NotImplementedException();
            }

            public List<IPEGI_Node> GetNodes()
            {
                return new List<IPEGI_Node>();
            }

            public List<PegiPort> GetPorts()
            {
                return new List<PegiPort>()
                {
                    portA,
                    portB,
                };
            }

            public bool Inspect()
            {
                pegi.nl();

                portA.Nested_Inspect();
                portB.Nested_Inspect();

                return false;
            }

            public string NameForDisplayPEGI() => _name;

            

        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PegiNodeGraphExamples))]
    public class PixelPerfectShaderDrawer : PEGI_Inspector_Mono<PegiNodeGraphExamples> { }
#endif
}