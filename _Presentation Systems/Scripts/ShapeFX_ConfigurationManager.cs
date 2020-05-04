using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes_Visual
{

    public class ShapeFX_ConfigurationManager : NodeNodesNeedEnableAbstract
    {
        public override string ClassTag => "ShapeFx_Cfg";

        public override bool Decode(string tg, string data)
        {
            return false;
        }

        public override CfgEncoder Encode()
        {
            return new CfgEncoder();
        }

        public override void ManagedOnEnable()
        {

        }
    }
}