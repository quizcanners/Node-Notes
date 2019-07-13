using System.Collections;
using QuizCannersUtilities;
using PlayerAndEditorGUI;

namespace NodeNotes
{
    public class BookMark : AbstractKeepUnrecognizedCfg, IPEGI_ListInspect, IGotName, IGotDisplayName, IBookReference
    {

        //public string bookName;
        //public string authorName;

        public string BookName { get; set ; }
        public string AuthorName { get; set; }


        public int nodeIndex;
        public string values;
        public string gameNodesData;

        public string NameForPEGI { get => BookName; set => BookName = value; }

        public string NameForDisplayPEGI()=> "Node {0} in {1} by {2}".F(nodeIndex, BookName, AuthorName);


        #region Inspector

        public bool InspectInList(IList list, int ind, ref int edited) {
            NameForDisplayPEGI().write();   
            if (icon.Undo.Click("Return to the point (Will discard all the progress)")) 
                Shortcuts.user.ReturnToMark(this);
            
            return false;
        }

        #endregion

        #region Encode_Decode
        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add_String("vals", values)
            .Add("ind", nodeIndex)
            .Add_String("n", BookName)
            .Add_String("auth", AuthorName)
            .Add_String("gnd", gameNodesData);
        
        public override bool Decode(string tg, string data) {
            switch (tg) {
                case "vals": values = data; break;
                case "ind": nodeIndex = data.ToInt(); break;
                case "n": BookName = data; break;
                case "auth": AuthorName = data; break;
                case "gnd": gameNodesData = data; break;
                default: return false;
            }
            return true;
        }
        #endregion

    }
}