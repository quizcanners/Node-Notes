using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerAndEditorGUI;

#if UNITY_EDITOR  && PEGI
using UnityEditor;

namespace LinkedNotes
{
    [CustomEditor(typeof(Nodes_PEGI))]
    public class PixelArtMeshGeneratorEditor : Editor
    {
        public override void OnInspectorGUI() => ((Nodes_PEGI)target).Inspect(serializedObject);
    }
}
#endif
