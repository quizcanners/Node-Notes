using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QuizCannersUtilities;

using UnityEngine.UIElements;

namespace PlayerAndEditorGUI.NodeGraph
{

#if UNITY_EDITOR
    using UnityEditor.Experimental.GraphView;

    public abstract class PegiPort
    {    
        protected Port.Capacity _capacity;
        protected Direction _direction;
        protected Orientation _orientation;
        protected Port _port;
        protected abstract Type ValueType { get; }
        public IPEGI_Node source;

        public virtual Port InstantiateOn(Node node)
        {
            _port = node.InstantiatePort(_orientation, _direction, _capacity, ValueType);

            return _port;
        }

        public PegiPort(Direction direction, Port.Capacity cacity = Port.Capacity.Multi, Orientation orientation = Orientation.Horizontal)
        {
            _orientation = orientation;
            _capacity = cacity;
            _direction = direction;
        }

    }

    public class PegiPortParentConnection : PegiPort
    {
        protected override Type ValueType => typeof(IPEGI_Node);
        
        public override Port InstantiateOn(Node node)
        {
            base.InstantiateOn(node);

            _port.portName = "parent";

            return _port;
        }

        public PegiPortParentConnection() : base(Direction.Input, Port.Capacity.Single, Orientation.Vertical){}
    }

    public class PegiPortChildrenConnection : PegiPort
    {

        protected override Type ValueType => typeof(IPEGI_Node);

        public override Port InstantiateOn(Node node)
        {
            base.InstantiateOn(node);

            _port.portName = "child";//_source.GetNameForInspector();

            return _port;
        }

        public PegiPortChildrenConnection() : base(Direction.Output, Port.Capacity.Multi, Orientation.Vertical){ }
    }

    public class PegiPort<T> : PegiPort, IPEGI, IPEGI_ListInspect
    {
        protected string _name;

        protected override Type ValueType => typeof(T);

        public IEnumerator<Node> GetInputs()
        {
            foreach (var edge in _port.connections)
            {
                var port = edge.input;
                if (port != null)
                {
                    yield return port.node;
                }
            }
        }

        public PegiPort(Direction direction, string portName = "", Port.Capacity cacity = Port.Capacity.Multi,  Orientation orientation = Orientation.Horizontal) : base(direction, cacity, orientation)
        {
            if (portName.IsNullOrEmpty())
                portName = typeof(T).ToString();

            _name = portName;
        }

        public override Port InstantiateOn(Node node)
        {
            base.InstantiateOn(node);
            
            _port.title = _name + "_Title";
            _port.name = _name + "_Name";
            _port.portName = _name;

            _port.AddManipulator(new EdgeManipulator());


            return _port;
        }

        #region Inspector
        public bool Inspect()
        {
            _name.nl(PEGI_Styles.ListLabel);
            "Connections: {0}".F(_port.connections.Count()).nl();

            return false;
        }

        public bool InspectInList(IList list, int ind, ref int edited) {
            typeof(T).ToString().edit(ref _name);

            if (icon.Enter.Click())
                edited = ind;

            return false;
        }
        #endregion
    }

#endif
}