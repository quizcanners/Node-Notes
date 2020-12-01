using PlayerAndEditorGUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuizCannersUtilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NodeNotes.Minimalistic
{
    public class NodeNotesMinimalInspector : NodesVisualLayerAbstract, IPEGI
    {

        public override void Decode(CfgData data)
        {
        }

        public override CfgEncoder EncodePerBookData() => new CfgEncoder();

        /*public override void Hide(Base_Node node)
        {
        }*/

        public override void HideAllBackgrounds()
        {
        }

        public override bool Inspect()
        {
            var changed = base.Inspect();  



            return changed;
        }

        public override void OnBeforeNodeSet(Node node)
        {

        }

        public override void OnLogicVersionChange()
        {

        }

     /*   public override void Show(Base_Node node)
        {

        }*/
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(NodeNotesMinimalInspector))]
    public class NodeNotesMinimalInspectorDrawer : PEGI_Inspector_Mono<NodeNotesMinimalInspector> { }
#endif
}