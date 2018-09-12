using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using STD_Logic;

namespace NodeNotes
{
    public class BookMark : NodeBook_Base  {
    
        public int nodeIndex;
        public string values;
        public string bookName;

        public override string NameForPEGI { get => bookName; set => bookName = value; }

        #region Encode_Decode

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add_String("vals", values)
            .Add("ind", nodeIndex)
            .Add_String("n", bookName);
        
        public override bool Decode(string tag, string data) {
            switch (tag)
            {
                case "vals": values = data; break;
                case "ind": nodeIndex = data.ToInt(); break;
                case "n": bookName = data; break;
                default: return false;
            }
            return true;
        }

        #endregion

    }


}