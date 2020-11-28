using PlayerAndEditorGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NodeNotes.Minimalistic
{
    public class NodeNotesMinimalInspector : MonoBehaviour, IPEGI
    {
        public NodeNotesMinimalData data;

        public bool Inspect()
        {
            var changed = pegi.toggleDefaultInspector(this);

            pegi.nl();

            data.Nested_Inspect();

            return changed;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NodeNotesMinimalInspector))]
    public class NodeNotesMinimalInspectorDrawer : PEGI_Inspector_Mono<NodeNotesMinimalInspector> { }
#endif
}