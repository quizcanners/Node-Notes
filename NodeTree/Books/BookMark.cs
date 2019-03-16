using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuizCannersUtilities;
using STD_Logic;
using PlayerAndEditorGUI;

namespace NodeNotes
{
    public class BookMark : AbstractKeepUnrecognizedCfg, IPEGI_ListInspect, IGotName {

        public string bookName;
        public int nodeIndex;
        public string values;
        public string gameNodesData;

        public string NameForPEGI { get => bookName; set => bookName = value; }

        #region Inspector
        #if PEGI
        public bool PEGI_inList(IList list, int ind, ref int edited) {
            "Node {0} in {1}".F(nodeIndex, bookName).write();   
            if (icon.Undo.Click("Return to the point (Will discard all the progress)")) 
                Shortcuts.user.ReturnToMark(this);
            
            return false;
        }
#endif
        #endregion

        #region Encode_Decode
        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add_String("vals", values)
            .Add("ind", nodeIndex)
            .Add_String("n", bookName)
            .Add_String("gnd", gameNodesData);
        
        public override bool Decode(string tg, string data) {
            switch (tg) {
                case "vals": values = data; break;
                case "ind": nodeIndex = data.ToInt(); break;
                case "n": bookName = data; break;
                case "gnd": gameNodesData = data; break;
                default: return false;
            }
            return true;
        }
        #endregion

    }
}