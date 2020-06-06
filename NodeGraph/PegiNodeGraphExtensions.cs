#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace PlayerAndEditorGUI.NodeGraph
{
    public static class PegiNodeGraphExtensions 
    {
        public static void OpenNodeWindow(this IPEGI_Node pegiNode)
        {
            #if UNITY_EDITOR
                EditorWindow.GetWindow<PegiGraphWindow>(pegiNode.GetNameForInspector()).Show(pegiNode);
            #endif


        }

    }
}
