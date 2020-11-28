using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerAndEditorGUI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NodeNotes.Minimalistic
{
    [CreateAssetMenu(fileName = "Minimalistic Data", menuName = "Node Nodes Minimalistic/Data", order = 0)]
    public class NodeNotesMinimalData : ScriptableObject, IPEGI
    {
        public static NodeStoryBooks books = new NodeStoryBooks();
        public const string ProjectName = "Node-Notes-Minimal";

        #region Inspector
        private int _inspectedBook;
        public bool Inspect()
        {
            var changes = false;

            books.Nested_Inspect(ref changes);

            return changes;
        }
        #endregion
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NodeNotesMinimalData))]
    public class NodeNotesMinimalDataDrawer : PEGI_Inspector_SO<NodeNotesMinimalData> { }
#endif
}