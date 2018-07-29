﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using System;
using STD_Logic;
using PlayerAndEditorGUI;

namespace LinkedNotes
{
    [CreateAssetMenu(fileName = "Story Shortcuts", menuName ="Story Nodes/Shortcuts", order = 0)]
    public class Shortcuts : STD_ReferancesHolder, IKeepMySTD
    {

        [HideInInspector]
        [SerializeField]
        string std_Data = "";
        public string Config_STD {
            get { return std_Data; }
            set { std_Data = value; }
        }

        public override bool PEGI()
        {
            bool changed = false;

          
           changed |= base.PEGI().nl();


            if (!showDebug)
            {
               

            }

            return changed;
        }

        [NonSerialized] public List<NodeBook> books = new List<NodeBook>();

        [NonSerialized] public List<BookMark> bookMarks = new List<BookMark>();

        public override StdEncoder Encode() => this.EncodeUnrecognized()
            .Add("vals", Values.global, this)
            .Add("trigs", TriggerGroup.all)
            .Add("books", books, this)
            .Add("marks", bookMarks, this);

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "vals": data.DecodeInto(out Values.global, this); break;
                case "trigs": data.DecodeInto(out TriggerGroup.all); break;
                case "books": data.DecodeInto(out books, this); break;
                case "marks": data.DecodeInto(out bookMarks, this); break;
                default: return false;
            }
            return true;
        }
    }
}
