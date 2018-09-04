using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using STD_Logic;

namespace LinkedNotes
{
    public class BookMark : NodeBook_Base  {
    
        public int nodeIndex;
        public string values;


        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add_String("vals", values)
            .Add("ind", nodeIndex)
            .Add_String("n", name);
        
        public override bool Decode(string tag, string data) {
            switch (tag)
            {
                case "vals": values = data; break;
                case "ind": nodeIndex = data.ToInt(); break;
                case "n": name = data; break;
                default: return false;
            }
            return true;
        }

    }


}