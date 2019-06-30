using PlayerAndEditorGUI;

#if !NO_PEGI && UNITY_EDITOR
using UnityEditor;

namespace NodeNotes_Visual
{
    [CustomEditor(typeof(Nodes_PEGI))]
    public class Nodes_PEGIEditor : PEGI_Inspector_Mono<Nodes_PEGI> {  }

    [CustomEditor(typeof(NodeCircleController))]
    public class NodeCircleControllerEditor : PEGI_Inspector_Mono<NodeCircleController>  {  }
}
#endif
