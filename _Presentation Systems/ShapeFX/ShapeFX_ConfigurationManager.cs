using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes_Visual
{

    public class ShapeFX_ConfigurationManager : PresentationSystemsAbstract
    {
        public override string ClassTag => "ShapeFx_Cfg";


        public override void ManagedOnEnable()
        {

        }

        #region Encode & Decode
        public override bool Decode(string tg, string data)
        {
            return false;
        }

        public override CfgEncoder Encode()
        {
            return new CfgEncoder();
        }

        #endregion

        #region Inspect
        public override bool Inspect()
        {

            return false;
        }

        public override string NameForDisplayPEGI() => "Shape FX";

        #endregion
    }
}