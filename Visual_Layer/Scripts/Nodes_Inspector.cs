using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerAndEditorGUI;

#if UNITY_EDITOR  && PEGI
using UnityEditor;

namespace NodeNotes_Visual
{
    [CustomEditor(typeof(Nodes_PEGI))]
    public class Nodes_PEGIEditor : PEGI_Editor<Nodes_PEGI> {  }

    [CustomEditor(typeof(NodeCircleController))]
    public class NodeCircleControllerEditor : PEGI_Editor<NodeCircleController>  {  }
}
#endif
