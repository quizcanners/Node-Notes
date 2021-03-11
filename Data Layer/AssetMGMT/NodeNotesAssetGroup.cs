using NodeNotes_Visual;
using QuizCannersUtilities;
using System;
using System.Collections.Generic;
using PlayerAndEditorGUI;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections;
using NodeNotes.RayTracing;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NodeNotes
{
    [CreateAssetMenu(fileName = "Unity Asset Group", menuName = "Node Nodes/Unity Asset Group", order = 0)]
    public class NodeNotesAssetGroup : ScriptableObject, IPEGI
    {

        [Serializable] public class TaggedAudioClips : EnumeratedAssetListsBase<AudioClip> { }




        private int _inspectedAssetList = -1;
        public bool Inspect()
        {
            pegi.toggleDefaultInspector(this).nl();

            audioClips.enter_Inspect(ref _inspectedAssetList, 0).nl();


            return false;
        }




    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NodeNotesAssetGroup))]
    public class NodeNotesAssetGroupsDrawer : PEGI_Inspector_SO<NodeNotesAssetGroup> { }
#endif
}