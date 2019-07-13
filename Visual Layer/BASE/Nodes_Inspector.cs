using PlayerAndEditorGUI;

#if UNITY_EDITOR
using UnityEditor;

namespace NodeNotes_Visual
{
    [CustomEditor(typeof(NodesVisualLayer))]
    public class Nodes_PEGIEditor : PEGI_Inspector_Mono<NodesVisualLayer> {  }

    [CustomEditor(typeof(NodeCircleController))]
    public class NodeCircleControllerEditor : PEGI_Inspector_Mono<NodeCircleController>  {  }
}
#endif
