using System.Collections;
using System.Collections.Generic;
using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes_Visual
{

    public class AmbientSoundsMixerMgmt : NodeNodesNeedEnableAbstract
    {
        public static AmbientSoundsMixerMgmt instance;

        #region Encode & Decode
        public override string ClassTag => "Ambnt";

        string test;

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "tst": test = data; break; 
                default: return true;
            }

            return false;
        }

        public override CfgEncoder Encode() => new CfgEncoder().Add_String("tst", test);
        #endregion

        public override void ManagedOnEnable()
        {
            instance = this;
        }

        public void OnEnable() => ManagedOnEnable();
        
        public bool Inspect()
        {
            var changed = false;


            return changed;
        }

    }
}
