using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using System;

namespace LinkedNotes
{

    //Have different classes to hold different forms of a book (Encoded, Decoded, Link, FileAdress)

    [DerrivedList(typeof(NodeBook), typeof(BookMark))]
    public class NodeBook : Node {
        
        public CountlessSTD<Node> allBookNodes = new CountlessSTD<Node>();

        

        public void Init () => Init(this);
        
        public override ISTD Decode(string data) => data.DecodeTagsFor(this);



    }
}