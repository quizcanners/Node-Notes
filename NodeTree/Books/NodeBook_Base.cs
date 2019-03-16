using QuizCannersUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerAndEditorGUI;

namespace NodeNotes {

    [DerivedList(typeof(NodeBook), typeof(NodeBook_OffLoaded))]
    public class NodeBook_Base : AbstractKeepUnrecognizedCfg, IGotDisplayName, IGotName {

        public const string BooksFolder = "Books";

        public virtual string NameForDisplayPEGI => NameForPEGI;

        public virtual string NameForPEGI { get => "ERROR, is a base class"; set { } }
        
    }
}