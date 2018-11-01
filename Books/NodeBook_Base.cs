using SharedTools_Stuff;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NodeNotes {

    [DerrivedList(typeof(NodeBook), typeof(NodeBook_OffLoaded))]
    public class NodeBook_Base : AbstractKeepUnrecognized_STD, IGotDisplayName, IGotName {

        public const string BooksFolder = "Books";

        public virtual string NameForPEGIdisplay => NameForPEGI;

        public virtual string NameForPEGI { get => "ERROR, is a base class"; set { } }
        
    }
}