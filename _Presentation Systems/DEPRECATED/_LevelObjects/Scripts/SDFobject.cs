
using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes_Visual
{
    
    public class SDFobject : MonoBehaviour, ICfg
    {
        public void Decode(string tg, CfgData data)
        {
            switch (tg)
            {
                case "pos": transform.localPosition = data.ToVector3(); break;
               
            }
        }

        public CfgEncoder Encode() => new CfgEncoder()
            .Add("pos", transform.localPosition);
    }
}