using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using STD_Logic;

namespace LinkedNotes
{
    public class BookMark : AbstractKeepUnrecognized_STD, IKeepMySTD  {

        string bookIndex;
        int nodeIndex;
        string stdData;

        public string Config_STD {
            get { return stdData; }
            set { stdData = value; }
        }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("vals",Values.global);
        
        public override bool Decode(string tag, string data) {
            switch (tag)
            {
                case "vals": data.DecodeInto(out Values.global); break;
                default: return false;
            }
            return true;
        }

    }
}