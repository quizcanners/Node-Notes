using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NodeNotes;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Debug = UnityEngine.Debug;
using static QuizCannersUtilities.QcSharp;

namespace NodeNotes_Visual {

    [TaggedType(classTag)]
    public class TextBackgroundController : BackgroundBase, IPointerClickHandler
    {
        #region Ecode & Decode
        const string classTag = "textRead";
        public override string ClassTag => classTag;
        #endregion

        #region Text MGMT

        [SerializeField] private Texture floatingParticles;

        private TextConfiguration activeTexts = new TextConfiguration();

        ShaderProperty.TextureValue floatingParticlesProperty = new ShaderProperty.TextureValue("_FloatingParticles");
        LinkedLerp.ColorValue textColor = new LinkedLerp.ColorValue("TextColor");
        LinkedLerp.FloatValue textFade = new LinkedLerp.FloatValue("Text Lerp", 0, 2);


        private LinkedLerp.ShaderColorValueGlobal bgColUp = new LinkedLerp.ShaderColorValueGlobal("_BG_GRAD_COL_1");
        private LinkedLerp.ShaderColorValueGlobal bgColCnter = new LinkedLerp.ShaderColorValueGlobal("_BG_CENTER_COL");
        private LinkedLerp.ShaderColorValueGlobal bgColDown = new LinkedLerp.ShaderColorValueGlobal("_BG_GRAD_COL_2");
        

        private LerpData ld = new LerpData();

        bool dirty = false;

        public void Update()
        {

            bool needUpdate = false;
            
            if (isFadingAway) {
                if (pTextMeshPro.enabled) {

                    float tfd = textFade.CurrentValue;

                    tfd = LerpUtils.LerpBySpeed(tfd, tfd > 0.5f ? 1f : 0f, 2f);

                    needUpdate = true;
                    
                    if (tfd == 0 || tfd == 1) {
                        pTextMeshPro.text = "";
                        pTextMeshPro.enabled = false;
                    }

                    textFade.CurrentValue = tfd;

                }
            }
            else
            {

                bool gotAnotherText = targetText != null;

                if (gotAnotherText || dirty) {

                    ld.Reset();

                    bgColUp.targetValue = activeTexts.backgroundColorUp;
                    bgColCnter.targetValue = activeTexts.backgroundColorCenter;
                    bgColDown.targetValue = activeTexts.backgroundColorDown;
                    textColor.targetValue = activeTexts.textColor;
                    textFade.targetValue = gotAnotherText ? 1 : 0.5f;

                    textFade.Portion(ld);
                    bgColUp.Portion(ld);
                    bgColCnter.Portion(ld);
                    bgColDown.Portion(ld);
                    textColor.Portion(ld);

                    bgColUp.Lerp(ld);
                    bgColCnter.Lerp(ld);
                    bgColDown.Lerp(ld);
                    textColor.Lerp(ld);
                    textFade.Lerp(ld);
                    
                    needUpdate = true;

                    dirty = ld.Portion() < 1;

                    if (gotAnotherText && textFade.CurrentValue == 1) {
                        pTextMeshPro.text = targetText;
                        targetText = null;
                        textFade.CurrentValue = 0;
                    }

                } else if  (textFade.CurrentValue < 0.5f) {

                    textFade.CurrentValue = LerpUtils.LerpBySpeed(textFade.CurrentValue, 0.5f, 1f);
                    needUpdate = true;

                }
            }

            if (needUpdate) 
                pTextMeshPro.color = textColor.CurrentValue.Alpha(textFade.CurrentValue);
            

        }

        public void UpdateText()
        {

            var value = currentNode != null ? activeTexts.GetAllTextFor(currentNode) : "No Node";

            if (targetText != null)
            {
                if (value.Equals(targetText)) 
                    return; 
            } else if (value.Equals(pTextMeshPro.text))
                return;
        
        if (skipLerpForEditor) {
                targetText = null;
                skipLerpForEditor = false;
                pTextMeshPro.text = value;
            }
            else
                targetText = value;
        }

        public StringBuilder stringBuilder = new StringBuilder();
        private string targetText ="";

        public TextMeshProUGUI pTextMeshPro;
        #endregion

        public bool isFadingAway = false;

        public override bool TryFadeIn() {

            isFadingAway = false;

            floatingParticlesProperty.GlobalValue = floatingParticles;

            instance = this;
            pTextMeshPro.enabled = true;

            dirty = true;
            
            gameObject.SetActive(true);

            return true;
        }

        public override void FadeAway() {
            
            isFadingAway = true;

            targetText = null;

            Unlink();
        }

        public void OnPointerClick(PointerEventData eventData)
        {

            var c = pTextMeshPro.canvas;

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(pTextMeshPro, Input.mousePosition, c.renderMode == RenderMode.ScreenSpaceOverlay ? null : c.worldCamera);
            if (linkIndex != -1) {
                TMP_LinkInfo linkInfo = pTextMeshPro.textInfo.linkInfo[linkIndex];

                //Debug.Log("Clicked " + linkIndex);

                activeTexts.ProcessClick(linkIndex);

            }
        }

        public override void MakeVisible(Base_Node node) { }
        
        public override void MakeHidden(Base_Node node) { }

        public override void ManagedOnEnable() { }

        public override void ManagedOnDisable()
        {
            Unlink();
        }

        private void Unlink() {
            if (currentNode != null) {
                currentNode.Unlink(activeTexts.Encode());
                currentNode = null;
            }
        }

        public Node currentNode;

        public override void SetNode(Node node)
        {
            Unlink();
            
            currentNode = node;

            if (node!= null)
                activeTexts.Decode(node.LinkTo(activeTexts));

            UpdateText();

        }

        public override void OnLogicUpdate() => UpdateText();

        #region Inspector

        public static bool skipLerpForEditor = false;

        public static TextBackgroundController instance;

        private bool showDependencies = false;

        public override bool Inspect() {

            var changed = false;

            instance = this;
            
            if (currentNode != null)
                currentNode.Nested_Inspect().nl(ref changed);

            if ("Dependencies".foldout(ref showDependencies).nl()){

                if ("Floating Particles".edit(ref floatingParticles).nl(ref changed))
                    floatingParticlesProperty.GlobalValue = floatingParticles;
            }

            dirty |= changed;

            return changed;
        }
        #endregion
    }
    
    public class TextConfiguration : AbstractKeepUnrecognizedCfg, IPEGI
    {
        private Node Node => TextBackgroundController.instance.currentNode;

        public Color linksColor = Color.blue;

        public Color backgroundColorUp = Color.white;
        public Color backgroundColorCenter = Color.white;
        public Color backgroundColorDown = Color.white;

        public Color textColor = Color.black;

        public List<TextChunkBase> textChunks = new List<TextChunkBase>();

        #region Encode & Decode

        public override void Decode(string data)
        {
            textChunks.Clear();

            base.Decode(data);
        }

        public override bool Decode(string tg, string data)
        {
            switch (tg)
            {
                case "tch": data.Decode_List(out textChunks, TextChunkBase.all); break;
                case "lnkCol": linksColor = data.ToColor(); break;
                case "bgUp": backgroundColorUp = data.ToColor(); break;
                case "bgc": backgroundColorCenter = data.ToColor(); break;
                case "bgDwn": backgroundColorDown = data.ToColor(); break;
                case "tx": textColor = data.ToColor(); break;
                default: return false;
            }

            return true;
        }

        public override CfgEncoder Encode() => this.EncodeUnrecognized()
            .Add("tch", textChunks, TextChunkBase.all)
            .Add("lnkCol", linksColor)
            .Add("bgUp", backgroundColorUp)
            .Add("bgc", backgroundColorCenter)
            .Add("bgDwn", backgroundColorDown)

            .Add("tx", textColor);
        #endregion



        public static TextConfiguration inspected;

        public int linkIndex = 0;

        public void ProcessClick(int index) {

            var ch = sortedChunks[index];

            if (ch != null)
                ch.ProcessClick(index);
            else
                Debug.LogError("No text chunks found for link "+index);

        }

        public static Countless<TextChunkBase> sortedChunks = new Countless<TextChunkBase>();

        public string GetAllTextFor(Node node) {

            inspected = this;

            linkIndex = 0;

            sortedChunks.Clear();

            var sb = TextBackgroundController.instance.stringBuilder;

            sb.Clear();

            var lineOpen = false;

            foreach (var c in textChunks) {

                if (lineOpen && c.preNewLine)
                    sb.Append(Environment.NewLine);
                

                sb.Append(c.GetTextFor(node));

                if (c.postNewLine) {
                    sb.Append(Environment.NewLine);
                    lineOpen = false;
                }
                else lineOpen = true;

            }
            
            return sb.ToString();
        }

        private int enteredText = -1;
        public override bool Inspect() {

            inspected = this;

            var changed = false;
            
            pegi.nl();

            if (enteredText == -1)
            {
                "Text Color".edit(ref textColor).nl(ref changed);
                "Link color".edit(ref linksColor).nl(ref changed);
                "Background Up".edit(ref backgroundColorUp).nl(ref changed);
                "Background Center".edit(ref backgroundColorCenter).nl(ref changed);
                "Background Down".edit(ref backgroundColorDown).nl(ref changed); 
            }
            else {

                if ("…".Click("Copy Ellipsis to Clipboard"))
                    GUIUtility.systemCopyBuffer = "…";

                if ("²".Click("Copy Square to clipboard"))
                    GUIUtility.systemCopyBuffer = "²";

                if ("ƒ".Click("Copy Function to clipboard"))
                    GUIUtility.systemCopyBuffer = "ƒ";

                if ("×".Click("Copy Times to clipboard"))
                    GUIUtility.systemCopyBuffer = "×";
                
                if ("™".Click("Copy Trademark to clipboard"))
                    GUIUtility.systemCopyBuffer = "™";
                
                if ("©".Click("Copy © to clipboard"))
                    GUIUtility.systemCopyBuffer = "©";

                if ("®".Click("Copy ® to clipboard"))
                    GUIUtility.systemCopyBuffer = "®";

                pegi.nl();
            }

            "Texts".edit_List(ref textChunks, ref enteredText).nl(ref changed);

            if (changed)
                TextBackgroundController.skipLerpForEditor = true;



            return changed;
        }
        

        #region Text Chunks

        public class TextChunkAttribute : AbstractWithTaggedTypes { public override TaggedTypesCfg TaggedTypes => TextChunkBase.all; }

        [TextChunk]
        public abstract class TextChunkBase : AbstractKeepUnrecognizedCfg, IGotClassTag, IGotDisplayName  {
            #region Tagged Types MGMT
            public abstract string ClassTag { get; }
            public static TaggedTypesCfg all = new TaggedTypesCfg(typeof(TextChunkBase));
            public TaggedTypesCfg AllTypes => all;
            #endregion

 
            protected int GetLinkIndex() {

                var linkId = inspected.linkIndex;

                sortedChunks[linkId] = this;

                inspected.linkIndex++;

                return linkId;
            }

            public bool preNewLine = false;

            public bool postNewLine = false;

            public override CfgEncoder Encode() => new CfgEncoder()
                .Add_IfTrue("pre", preNewLine)
                .Add_IfTrue("post", postNewLine);

            public override bool Decode(string tag, string data) {

                switch (tag) {
                    case "pre": preNewLine = data.ToBool(); break;
                    case "post": postNewLine = data.ToBool(); break;
                    default: return false;
                }

                return true;
            }
            
            public virtual void ProcessClick(int index) { }

            public abstract string GetTextFor(Node node);

            public abstract string NameForDisplayPEGI();
        }

        [TaggedType(tag, "Just Text")]
        public class JustText : TextChunkBase, IPEGI_ListInspect, IPEGI {
            public string text;

            #region Encode & Decode
            private const string tag = "jt";
            public override string ClassTag => tag;

            public override CfgEncoder Encode() => new CfgEncoder()
                .Add("b", base.Encode)
                .Add_String("t", text);

            public override bool Decode(string tg, string data)
            {
                switch (tg)
                {
                    case "b": data.Decode_Base(base.Decode, this); break;
                    case "t": text = data; break;
                    default: return false;
                }

                return true;
            }
            #endregion

            #region Inspect
            public bool InspectInList(IList list, int ind, ref int edited) {

                var changed = false;

                pegi.toggle(ref preNewLine).changes(ref changed);

                pegi.edit(ref text).changes(ref changed);
                
                pegi.toggle(ref postNewLine).changes(ref changed);

                if (icon.Enter.Click())
                    edited = ind;

                return changed;
            }

            public override bool Inspect() => pegi.editBig(ref text);

            public override string GetTextFor(Node node)
            {
                return text;
            }

            public override string NameForDisplayPEGI() => text.ToElipsisString(30);
            #endregion
        }
        
        [TaggedType(tag, "Exit")]
        public class ExitText : TextChunkBase, IPEGI_ListInspect, IPEGI
        {
            public string text;

            public override void ProcessClick(int index) {
                if (index == linkId)
                    Shortcuts.TryExitCurrentNode();
            }

            private int linkId;

            public override string GetTextFor(Node node) {
                
                linkId = GetLinkIndex();

                return HtmlTagWrap("b", HtmlTagWrap("link", HtmlTagWrap(text, inspected.linksColor)));

            }

            #region Encode & Decode
            private const string tag = "ext";
            public override string ClassTag => tag;

            public override CfgEncoder Encode() => new CfgEncoder()
                .Add("b", base.Encode)
                .Add_String("t", text);

            public override bool Decode(string tg, string data) {
                switch (tg){
                    case "b": data.Decode_Base(base.Decode, this); break;
                    case "t": text = data; break;
                    default: return false;
                }

                return true;
            }
            #endregion

            #region Inspect
            public bool InspectInList(IList list, int ind, ref int edited) {

                var changed = false;

                pegi.edit(ref text).changes(ref changed);

                if (icon.Enter.Click())
                    edited = ind;

                return changed;
            }

            public override bool Inspect() => pegi.editBig(ref text);
            
            public override string NameForDisplayPEGI() => "Exit: " + text.ToElipsisString(30);

            #endregion
        }

        [TaggedType(tag, "Node")]
        public class NodeText : TextChunkBase, IPEGI_ListInspect, IPEGI
        {
            public string text;

            public int childNodeIndex;

            public override void ProcessClick(int index) {
                    var n = Shortcuts.CurrentNode.coreNodes.GetByIGotIndex(childNodeIndex);
                    if (n!= null)
                        n.ExecuteInteraction();
                
            }

            private int linkNo = 0;

            public override string GetTextFor(Node node)
            {
                linkNo = GetLinkIndex();

                return HtmlTagWrap("b",HtmlTagWrap("link", HtmlTagWrap(text, inspected.linksColor)));

            }

            #region Encode & Decode
            private const string tag = "node";
            public override string ClassTag => tag;

            public override CfgEncoder Encode() => new CfgEncoder()
                .Add("b", base.Encode)
                .Add_String("t", text)
                .Add("i", childNodeIndex);

            public override bool Decode(string tg, string data)
            {
                switch (tg)
                {
                    case "b": data.Decode_Base(base.Decode, this); break;
                    case "t": text = data; break;
                    case "i": childNodeIndex = data.ToInt(); break;
                    default: return false;
                }

                return true;
            }
            #endregion

            #region Inspect

            public bool InspectInList(IList list, int ind, ref int edited) {

                var changed = false;

                pegi.edit(ref text).changes(ref changed);

                pegi.select_iGotIndex(ref childNodeIndex, Shortcuts.CurrentNode.coreNodes);

                if (icon.Enter.Click())
                    edited = ind;

                return changed;
            }

            public override bool Inspect() => pegi.editBig(ref text);

            public override string NameForDisplayPEGI() => "Node: " + text.ToElipsisString(30);

            #endregion
        }

        #endregion
    }

}