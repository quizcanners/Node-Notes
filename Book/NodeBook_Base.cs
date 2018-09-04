﻿using SharedTools_Stuff;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LinkedNotes
{

    [DerrivedList(typeof(NodeBook), typeof(NodeBook_OffLoaded))]
    public class NodeBook_Base : AbstractKeepUnrecognized_STD, IGotDisplayName, IGotIndex, IGotName {

        public string name;

        public virtual string NameForPEGIdisplay() => name;

        public string NameForPEGI { get => name; set => name = value; }

        int indexInList = 0; // May be different per player

        public int IndexForPEGI {
            get { return indexInList; }
            set { indexInList = value; }
        }

     
    }
}