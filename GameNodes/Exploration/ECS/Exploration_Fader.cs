using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;
using PlayerAndEditorGUI;
using SharedTools_Stuff;
using Unity.Mathematics;



namespace NodeNotes_Visual {

    [Serializable]
    public struct PhisicsArrayObject : IComponentData, IGotDisplayName, IPEGI_ListInspect, ISTD {

        public int index;

        public float testValue;

        public string NameForPEGIdisplay => "Exploration Lerp Data";

        #region Encode & Decode
        public void Decode(string data)  {
            
        }

        public bool Decode(string tag, string data)
        {
            throw new NotImplementedException();
        }

        public StdEncoder Encode()
        {
            throw new NotImplementedException();
        }
        #endregion

        public bool PEGI_inList(IList list, int ind, ref int edited)
        {
            "Yep, can inspect Entities".nl();

            return false;
        }
    }

    public class Exploration_Fader : ComponentDataWrapper<PhisicsArrayObject> { }


}