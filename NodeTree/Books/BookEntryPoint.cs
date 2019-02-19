using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuizCannersUtilities;
using PlayerAndEditorGUI;
using System;
using STD_Logic;

namespace NodeNotes
{

    public class BookEntryPoint : AbstractKeepUnrecognizedStd, IPEGI, IGotName {

        public string entryPointName = "Rename Me";

        public int nodeIndex = -1;

        public bool startPoint;

        public string NameForPEGI { get => entryPointName; set => entryPointName = value; }

        #region Encode/Decode

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "s": startPoint = data.ToBool(); break;
                case "ind": nodeIndex = data.ToInt(); break;
                case "Name": entryPointName = data; break;
                default: return false;
            }
            return true;
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add_Bool("s", startPoint)
            .Add("ind", nodeIndex)
            .Add_String("Name", entryPointName);

        #endregion

        #region Inspector

        #if PEGI
        public override bool Inspect()
        {
            bool changed = false;

            "{0} is a key. Don't change it after using".writeHint();

            //"Tag should not change after it was used by other book to link to this one".writeOneTimeHint("KeepTags");

            "On Node".select_iGotIndex_SameClass<Base_Node, Node>(60, ref nodeIndex, NodeBook.inspected.allBaseNodes.GetAllObjsNoOrder()).nl();
            
            "Can Be A Game Start".toggle(ref startPoint).nl();

            return changed;
        }
#endif
        #endregion

    }
}
