using System.Collections;
using PlayerAndEditorGUI;
using QuizCannersUtilities;

namespace NodeNotes
{

    public class BookEntryPoint : ICfg, IPEGI, IGotName, IGotIndex, IPEGI_ListInspect {

        public string entryPointName = "Rename Me";

        public int nodeIndex = -1;

        public bool startPoint;

        public string NameForPEGI { get => entryPointName; set => entryPointName = value; }

        public int IndexForPEGI { get => nodeIndex; set => nodeIndex = value; }

        #region Encode/Decode

   

        public void Decode(string tg, CfgData data)
        {
            switch (tg)
            {
                case "s": startPoint = data.ToBool(); break;
                case "ind": nodeIndex = data.ToInt(); break;
                case "Name": entryPointName = data.ToString(); break;
            }
        }
        


        public CfgEncoder Encode() => new CfgEncoder()//this.EncodeUnrecognized()
            .Add_Bool("s", startPoint)
            .Add("ind", nodeIndex)
            .Add_String("Name", entryPointName);

        #endregion

        #region Inspector

        public virtual void ResetInspector()
        {
        }

        public bool Inspect() {

            bool changed = false;

            this.inspect_Name().changes(ref changed);

            if (Shortcuts.CurrentNode == null && "Start Here".ClickConfirm("stSt", "Are ou sure? This will let this Book as origin of this character."))
                Shortcuts.CurrentNode = NodeBook.inspected.allBaseNodes[nodeIndex].AsNode;
            
            pegi.nl();

            "{0} is a reference Key to this Entry Point. Target node can be changed at any point".F(entryPointName).writeHint();

            "Destination Node".select_iGotIndex_SameClass<Base_Node, Node>(100, ref nodeIndex, NodeBook.inspected.allBaseNodes.GetAllObjsNoOrder()).nl();
            
            "Can Be A Game Start".toggle(ref startPoint).nl();
            
            return changed;
        }

        public bool InspectInList(IList list, int ind, ref int edited)
        {
            var changed = this.inspect_Name();

            var b = NodeBook.inspected;
            
            if (startPoint && Shortcuts.CurrentNode == null 
                && icon.Play.ClickConfirm("EnPoSt",
                    "This will set book {0} as your Starting Point HUB. It's a big deal.".F(b)))
                Shortcuts.CurrentNode = b.allBaseNodes[nodeIndex] as Node;
            
            if (icon.Enter.Click("Inspect Entry Point"))
                edited = ind;

            return changed;
        }

        #endregion

    }
}
