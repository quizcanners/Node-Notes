using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using System;
using STD_Logic;

namespace LinkedNotes
{
    public class Shortcuts : STD_ReferancesHolder, IKeepMySTD
    {
        [NonSerialized] public Values values = new Values();

        [NonSerialized] public List<NodeBook> books = new List<NodeBook>();

        [NonSerialized] public List<BookMark> bookMarks = new List<BookMark>();

        [HideInInspector]
        [SerializeField]
        string std_Data = "";
        public string Config_STD {
            get { return std_Data; }
            set { std_Data = value; } }

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("vals", values)
            .Add("books", books)
            .Add("marks", bookMarks);

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "vals": data.DecodeInto(out values); break;
                case "books": data.DecodeInto(out values); break;
                case "marks": data.DecodeInto(out bookMarks); break;
                default: return false;
            }
            return true;
        }
    }
}
