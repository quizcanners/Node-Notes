
using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes_Visual
{
    
    public class SDFobject : MonoBehaviour, ICfg
    {
        public void Decode(string data) => new CfgDecoder(data).DecodeTagsFor(this);

        public bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "pos": transform.localPosition = data.ToVector3(); break;
                default: return false;
            }

            return true;
            
        }

        public CfgEncoder Encode() => new CfgEncoder()
            .Add("pos", transform.localPosition);
    }
}