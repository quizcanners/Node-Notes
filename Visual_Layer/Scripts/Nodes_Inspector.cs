using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerAndEditorGUI;

#if UNITY_EDITOR  && PEGI
using UnityEditor;

namespace NodeNotes_Visual
{
    [CustomEditor(typeof(Nodes_PEGI))]
    public class PixelArtMeshGeneratorEditor : Editor
    {
        public override void OnInspectorGUI() => ((Nodes_PEGI)target).Inspect(serializedObject);
    }

    [CustomEditor(typeof(NodeCircleController))]
    public class NodeCircleControllerEditor : Editor
    {
        public override void OnInspectorGUI() => ((NodeCircleController)target).Inspect(serializedObject);
    }
}
#endif
