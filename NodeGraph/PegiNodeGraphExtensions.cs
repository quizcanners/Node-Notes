#if UNITY_EDITOR
    using UnityEditor;

namespace PlayerAndEditorGUI.NodeGraph
{
    public static class PegiNodeGraphExtensions 
    {
        public static void OpenNodeWindow(this IPEGI_Node pegiNode)
        {
                EditorWindow.GetWindow<PegiGraphWindow>(pegiNode.GetNameForInspector()).Show(pegiNode);
        }
    }
}
#endif
