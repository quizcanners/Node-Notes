using System.Collections;
using System.Collections.Generic;
using NodeNotes;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NodeNotes_Visual {

    [TaggedType(classTag)]
    public class TextBackgroundController : BackgroundBase, IPointerClickHandler
    {
        const string classTag = "textRead";

        public TextMeshProUGUI pTextMeshPro;

        public override string ClassTag => classTag;

        public override bool TryFadeIn() {

            pTextMeshPro.enabled = true;
            return true;
        }

        public override void FadeAway() {

            pTextMeshPro.enabled = false;

        }

        public void OnPointerClick(PointerEventData eventData)
        {

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, Input.mousePosition, null);
            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];

                Debug.Log("Clicked " + linkIndex);
                
            }
        }

        public override void MakeVisible(Base_Node node) { }
        
        public override void MakeHidden(Base_Node node) { }

        public override void ManagedOnEnable() { }

        public override void OnLogicVersionChange() { }

        public override void ManagedOnDisable() { }

        public override void SetNode(Node node) { }

        public override void OnLogicUpdate() { }

        public override bool Inspect()
        {
            var changed = false;



            return changed;
        }
    }


 

  
    public class TextConfiguration : AbstractKeepUnrecognizedCfg, IPEGI {

        public List<TextChunkBase> textChunks = new List<TextChunkBase>();

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "tch": data.Decode_List(out textChunks, TextChunkBase.all); break;
                default: return false;
            }

            return true;
        }

        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add("tch", textChunks, TextChunkBase.all);


        private int enteredText = -1;
        public override bool Inspect()
        {
            var changed = false;

            "Texts".edit_List(ref textChunks, ref enteredText);

            return changed;
        }
        

        #region Text Chunks

        public class TextChunkAttribute : AbstractWithTaggedTypes { public override TaggedTypesCfg TaggedTypes => TextChunkBase.all; }

        [TextChunk]
        public abstract class TextChunkBase : AbstractCfg, IGotClassTag  {
            #region Tagged Types MGMT
            public abstract string ClassTag { get; }
            public static TaggedTypesCfg all = new TaggedTypesCfg(typeof(TextChunkBase));
            public TaggedTypesCfg AllTypes => all;
            #endregion
        }

        [TaggedType(tag)]
        public class JustText : TextChunkBase, IPEGI_ListInspect, IPEGI {
            public string text;

            #region Encode & Decode
            private const string tag = "jt";
            public override string ClassTag => tag;

            public override CfgEncoder Encode() => new CfgEncoder().Add_String("t", text);

            public override bool Decode(string tg, string data)
            {
                switch (tg)
                {
                    case "t": text = data; break;
                    default: return false;
                }

                return true;
            }
            #endregion

            #region Inspect
            public bool InspectInList(IList list, int ind, ref int edited) {

                pegi.edit(ref text);

                if (icon.Enter.Click())
                    edited = ind;

                return false;
            }

            public bool Inspect() => pegi.editBig(ref text);
            #endregion
        }
        #endregion
    }

}