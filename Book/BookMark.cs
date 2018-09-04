using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using STD_Logic;

namespace LinkedNotes
{
    public class BookMark : NodeBook_Base  {
    
        public int nodeIndex;
        public string bookData;

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("vals",Values.global)
            .Add("ind", nodeIndex)
            .Add_String("bd", bookData)
            .Add_String("n", name);
        
        public override bool Decode(string tag, string data) {
            switch (tag)
            {
                case "vals": data.DecodeInto(out Values.global); break;
                case "ind": nodeIndex = data.ToInt(); break;
                case "bd": bookData = data; break;
                case "n": name = data; break;
                default: return false;
            }
            return true;
        }

    }


}