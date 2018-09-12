using STD_Logic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace NodeNotes
{
    public abstract class NodesVisualLayerAbstract : LogicMGMT {
        public abstract bool TrySetCurrentNode(Node node);
    }
}