using System.IO;
using PlayerAndEditorGUI;
using QuizCannersUtilities;

namespace NodeNotes {

    [DerivedList(typeof(NodeBook), typeof(NodeBook_OffLoaded))]
    public class NodeBook_Base : ICfg, IGotDisplayName, IGotName, IBookReference {

        public virtual NodeBook AsLoadedBook => null;

        public const string BooksRootFolder = "Books";

        public string authorName = "Author Name";
    
        public virtual string NameForDisplayPEGI()=> this.EditedByCurrentUser() ? NameForPEGI : "{0} by {1}".F(NameForPEGI, authorName);

        #region Encode & Decode

        public virtual CfgEncoder Encode() => new CfgEncoder()//this.EncodeUnrecognized()
            .Add_String("auth", authorName);

        public virtual bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "auth": authorName = data; break;
                default: return false;
            }
            return true;
        }

        public virtual void Decode(string data) => this.DecodeTagsFrom(data);

        #endregion

        public virtual void ResetInspector()
        {
        }

        protected int _inspectedItems = -1;

        public virtual string NameForPEGI { get => "ERROR, is a base class"; set { } }

        public string BookName {
            get { return NameForPEGI; }
            set { NameForPEGI = value; }
        }

        public string AuthorName {
            get { return authorName; }
            set { authorName = value; }
        }
    }
    
    public interface IBookReference
    {
        string BookName { get; set; }
        string AuthorName { get; set; }
    }

    public static class BookClassExtensions {

        public static bool EditedByCurrentUser<T>(this T reff) where T: IBookReference 
            => Shortcuts.users.current.isADeveloper && Shortcuts.users.current.Name.Equals(reff.AuthorName);

        public static string BookFolder<T>(this T reff) where T: IBookReference => Path.Combine(NodeBook_Base.BooksRootFolder, reff.AuthorName);


    }

}