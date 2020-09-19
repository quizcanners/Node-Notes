using System.Collections;
using PlayerAndEditorGUI;
using QuizCannersUtilities;

namespace NodeNotes
{
    public class BookMark : ICfg, IPEGI_ListInspect, IGotName, IGotDisplayName, IBookReference
    {

        //public string bookName;
        //public string authorName;

        public string BookName { get; set ; }
        public string AuthorName { get; set; }


        public int nodeIndex;
        public CfgData values;
        public CfgData gameNodesData;

        public string NameForPEGI { get => BookName; set => BookName = value; }

        public string NameForDisplayPEGI()=> "Node {0} in {1} by {2}".F(nodeIndex, BookName, AuthorName);


        #region Inspector

        public bool InspectInList(IList list, int ind, ref int edited) {
            NameForDisplayPEGI().write();   
            if (icon.Undo.Click("Return to the point (Will discard all the progress)"))
                Shortcuts.users.current.ReturnToBookMark(this);
            
            return false;
        }

        #endregion

        #region Encode_Decode
        public CfgEncoder Encode() => new CfgEncoder()//this.EncodeUnrecognized()
            .Add("vals", values)
            .Add("ind", nodeIndex)
            .Add_String("n", BookName)
            .Add_String("auth", AuthorName)
            .Add("gnd", gameNodesData);
        
        public void Decode(string tg, CfgData data) {
            switch (tg) {
                case "vals": values = data; break;
                case "ind": nodeIndex = data.ToInt(); break;
                case "n": BookName = data.ToString(); break;
                case "auth": AuthorName = data.ToString(); break;
                case "gnd": gameNodesData = data; break;
            }
        }
        

        #endregion

    }
}