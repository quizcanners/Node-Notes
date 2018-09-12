using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using SharedTools_Stuff;
using PlayerAndEditorGUI;
using System;
using NodeNotes;

namespace NodeNotes_Visual
{

    [ExecuteInEditMode]
    public class NodeCircleController : ComponentSTD, IPEGI, IGotName
    {
        static Nodes_PEGI Mgmt => Nodes_PEGI.NodeMGMT_inst;
        
        public Renderer circleRendy;

        public Base_Node source;

        public int background = 0;

        public string backgroundConfig ="";

        #region TEXT
        public TextMeshPro textA;

        public TextMeshPro textB;

        bool activeTextIsA = false;

        TextMeshPro ActiveText => activeTextIsA ? textA : textB;

        TextMeshPro PassiveText => activeTextIsA ? textB : textA;

        public string newText = null;

        float activeTextAlpha = 0;

        #endregion
#if PEGI

        public override string NameForPEGI {
            get {  return source.name; }

            set
            {
                source.name = value ;

                if (fadePortion < 0.1f)
                {
                    newText = null;
                    activeTextAlpha = 1;
                    ActiveText.text = value;
                    gameObject.name = value;
                    PassiveText.text = "";
                    UpdateShaders();
                }
                else
                    newText = value;
            }
        }
#endif
        [NonSerialized] public bool isFading;
        [NonSerialized] public bool assumedPosition;

        NodeVisualConfig exploredVisuals = new NodeVisualConfig();
        NodeVisualConfig subVisuals = new NodeVisualConfig();

        NodeVisualConfig ActiveConfig => this.source == Shortcuts.CurrentNode ? exploredVisuals : subVisuals;
        
        Color sh_currentColor;
        Vector4 sh_square = Vector4.zero;
        float sh_blur = 0;
        float sh_courners = 0.5f;
        float sh_selected = 0;
        float fadePortion = 0;

        void UpdateShaders()
        {

            if (textB && textA)
            {
                ActiveText.color = new Color(0, 0, 0, activeTextAlpha * fadePortion);
                PassiveText.color = new Color(0, 0, 0, (1 - activeTextAlpha) * fadePortion);
            }

            sh_currentColor.a = fadePortion;

            if (circleRendy)
            {
                if (circleRendy)
                {
                    if (Application.isPlaying)
                    {
                        circleRendy.material.SetColor("_Color", sh_currentColor);
                        circleRendy.material.SetVector("_Stretch", sh_square);
                        circleRendy.material.SetFloat("_Courners", sh_courners);
                        circleRendy.material.SetFloat("_Blur", sh_blur);
                        circleRendy.material.SetFloat("_Selected", sh_selected);
                    }
                    else
                    {
                        circleRendy.sharedMaterial.SetColor("_Color", sh_currentColor);
                        circleRendy.sharedMaterial.SetVector("_Stretch", sh_square);
                        circleRendy.sharedMaterial.SetFloat("_Courners", sh_courners);
                        circleRendy.sharedMaterial.SetFloat("_Blur", sh_blur);
                        circleRendy.sharedMaterial.SetFloat("_Selected", sh_selected);
                    }
                }
            }
        }
#if PEGI

        bool showDependencies = false;
        public override bool PEGI() {
            bool changed = false;

            bool onPlayScreen = pegi.paintingPlayAreaGUI;

            if (source != null)
            {
             if (source.Try_Nested_Inspect()) {
                    if (name != source.name)
                        NameForPEGI = source.name;

                    changed = true;

                    Nodes_PEGI.UpdateVisibility();
                }
            }

            if (source != null && source.parentNode == null && icon.Exit.Click("Exit story"))
                Shortcuts.CurrentNode = null;

            if (!onPlayScreen)
            "Lerp parameter {0}".F(dominantParameter).nl();

            if (circleRendy) {

                if (!onPlayScreen) {

                    if (newText != null)
                        "Changeing text to {0}".F(newText).nl();

                    if (isFading)
                        "Fading...{0}".F(fadePortion).nl();

                }

                if (source!= null && (source.GetType() == typeof(Node))) {

                    if ("Background ".select(ref background, Mgmt.backgroundControllers).nl())
                    {
                        changed = true;
                        Mgmt.SetBackground(background, backgroundConfig);
                    }

                    var bg = Mgmt.backgroundControllers.TryGet(background);
                    if (bg != null)
                    {
                        if (bg.Try_Nested_Inspect())
                        {
                            changed = true;
                            var std = bg as ISTD;
                            if (std != null)
                                backgroundConfig = std.Encode().ToString();
                        }
                    }    
                }

               if (source == null || (!source.InspectingTriggerStuff))
               if (ActiveConfig.Nested_Inspect()) {
                    assumedPosition = false;
                    sh_currentColor = ActiveConfig.targetColor;
                    UpdateShaders();
               }
            }

            

            if (!onPlayScreen) {

                pegi.nl();

                "Dependencies".foldout(ref showDependencies).nl();

                if (!textA || showDependencies)
                    "Text A".edit(ref textA);

                if (!textB || showDependencies)
                    "Text B".edit(ref textB);

                if (!circleRendy || showDependencies)
                    "Mesh Rendy".edit(ref circleRendy);
            }

            return changed;
        }
#endif
        public string dominantParameter;
        void Update() {

            bool needShaderUpdate = false;
            
            float portion = 1;

            var ac = ActiveConfig;

            if (Base_Node.editingNodes && (this == dragging) && !isFading)
            {
                if (!Input.GetMouseButton(0))
                    dragging = null;
                else  {
                    Vector3 pos;
                    if (upPlane.MouseToPlane(out pos)) {
                        transform.position = pos + dragOffset;
                        ac.targetLocalPosition = transform.localPosition;
                    }
                }

                assumedPosition = false;
            }

            if (!assumedPosition)  {

                float dist = (transform.localPosition - ac.targetLocalPosition).magnitude;

                sh_blur = Mathf.Lerp(sh_blur, Mathf.Clamp01(dist*5), Time.deltaTime * 10);

                if (50f.SpeedToMinPortion(dist, ref portion))
                    dominantParameter = "postiton";

                if (12f.SpeedToMinPortion(ac.targetColor.DistanceRGB(sh_currentColor), ref portion))
                    dominantParameter = "color";

                var BGtf = circleRendy.transform;

                var scale = BGtf.localScale;
                if (10f.SpeedToMinPortion((scale - ac.targetSize).magnitude , ref portion))
                    dominantParameter = "size";

                if (4f.SpeedToMinPortion(fadePortion - (isFading ? 0f : 1f), ref portion))
                    dominantParameter = "fade";

                if (8f.SpeedToMinPortion(1-activeTextAlpha, ref portion))
                    dominantParameter = "text Alpha";

                float targetCourners =  (this == dragging) ? 0 : (source == Shortcuts.CurrentNode) ? 0.4f : 0.9f;
            
                if (4f.SpeedToMinPortion(Mathf.Abs(targetCourners - sh_courners), ref portion))
                    dominantParameter = "courners";

                float targetSelected = (this == Mgmt.selectedNode ? 1f : 0f);
                if (4f.SpeedToMinPortion(Mathf.Abs(sh_selected - targetSelected), ref portion))
                    dominantParameter = "Selection Outline";

                float teleportPortion = ( fadePortion < 0.1f && !isFading) ? 1 : portion;

                transform.localPosition = Vector3.Lerp(transform.localPosition, ac.targetLocalPosition, teleportPortion);
                scale = Vector3.Lerp(scale, ac.targetSize, teleportPortion);
                BGtf.localScale = scale;
                sh_currentColor = Color.Lerp(sh_currentColor, ac.targetColor, teleportPortion);
                fadePortion = Mathf.Lerp(fadePortion, isFading ? 0 : 1, portion);
                sh_courners = Mathf.Lerp(sh_courners, targetCourners, teleportPortion);
                sh_selected = Mathf.Lerp(sh_selected, targetSelected, portion);

                needShaderUpdate = true;
                if (portion == 1)
                {
                    ActiveConfig.targetSize = circleRendy.transform.localScale;
                    ActiveConfig.targetLocalPosition = transform.localPosition;

                    assumedPosition = true;
                }
                
                if (scale.x > 0)
                    sh_square.x = scale.x > scale.y ? ((scale.x - scale.y) / scale.x) : 0;
                else sh_square.x = 0;
                if (scale.y > 0)
                    sh_square.y = scale.y > scale.x ? ((scale.y - scale.x) / scale.y) : 0;
                else sh_square.y = 0;
                
                var textSize = new Vector2(17 + scale.x * 3, 5f + Mathf.Max(0, (scale.y - 1f) * 3f));

                textA.rectTransform.sizeDelta = textSize;
                textB.rectTransform.sizeDelta = textSize;

            }
            else {

                var before = sh_blur;
                sh_blur =  Mathf.Lerp(sh_blur, 0, Time.deltaTime * 10);
                if (before != sh_blur)
                    needShaderUpdate = true;

                fadePortion = MyMath.Lerp(fadePortion, isFading ? 0 : 1, 2, out portion);
                if (isFading || fadePortion < 1)
                    needShaderUpdate = true;
            }



            if (newText != null || activeTextAlpha < 1)
            {
                if (newText == null)
                    activeTextAlpha = Mathf.Lerp(activeTextAlpha, 1, portion);
                else
                    activeTextAlpha = MyMath.Lerp(activeTextAlpha, 1, 4);

                if (activeTextAlpha == 1 && newText != null)  {
                    activeTextIsA = !activeTextIsA;
                    ActiveText.text = newText;
                    gameObject.name = newText;
                    activeTextAlpha = 0;
                    newText = null;
                }

                needShaderUpdate = true;
            }
            
            if (needShaderUpdate)
                UpdateShaders();

            if (fadePortion == 0 && isFading && Application.isPlaying)
                gameObject.SetActive(false);
        }

        static NodeCircleController dragging = null;
        Vector3 dragOffset = Vector3.zero;
        static Plane upPlane = new Plane(Vector3.up, Vector3.zero);
        public void TryDragAndDrop()
        {
            if (dragging == null && Input.GetMouseButtonDown(0)) {

                Nodes_PEGI.NodeMGMT_inst.SetSelected(this);

                Vector3 pos;
                if (upPlane.MouseToPlane(out pos))  {
                    dragging = this;
                    dragOffset = transform.position - pos;
                }
            }
        }

        public void OnMouseOver() {
            if (!isFading) {
                if (Base_Node.editingNodes)
                    TryDragAndDrop();
                else
                      if (source != null)
                    source.OnMouseOver();
            }
        }

        public override ISTD Decode(string data)   {
            if (this == dragging)
                dragging = null;
            assumedPosition = false;
            return base.Decode(data);
        }

        public override bool Decode(string tag, string data)   {
            switch (tag)   {
                case "expVis": data.DecodeInto(out exploredVisuals); break;
                case "subVis": data.DecodeInto(out subVisuals); break;
                case "bg": background = data.ToInt(); break;
                case "bg_cfg": backgroundConfig = data; break;
                default: return false;
            }

            return true;
        }

        public override StdEncoder Encode()
        {

            var cody = this.EncodeUnrecognized()
                .Add("expVis", exploredVisuals)
                .Add("subVis", subVisuals)
                .Add("bg", background)
                .Add_String("bg_cfg", backgroundConfig);
           
            return cody;
        }

        public void LinkTo(Base_Node node)
        {
            source = node;
            if (source.visualRepresentation != null)
                Debug.LogError("Visual representation is not null",this);
            source.visualRepresentation = this;
            source.previousVisualRepresentation = this;
            Decode(source.configForVisualRepresentation);
            NameForPEGI = source.name;
            isFading = false;
            gameObject.SetActive(true);
        }

        public void Unlink()
        {
            if (source != null) {
                source.configForVisualRepresentation = Encode().ToString();
                source.visualRepresentation = null;
                source = null;
            }
            else
                Debug.LogError("source is null ", this);

            isFading = true;
        }

        private void OnEnable()
        {
            if (Application.isEditor)
            {
             
                if (!circleRendy)
                    circleRendy = GetComponent<MeshRenderer>();
            }
        }

        void OnDisable() {
            if (!isFading)
                Unlink();
        }

    }

    public class NodeVisualConfig : AbstractKeepUnrecognized_STD, IPEGI {
        public Vector3 targetSize = Vector3.one;
        public Vector3 targetLocalPosition = Vector3.zero;
        public Color targetColor = Color.gray;
        
        public override bool Decode(string tag, string data)
        {
            switch (tag)  {
                case "sc": targetSize = data.ToVector3(); break;
                case "pos": targetLocalPosition = data.ToVector3(); break;
                case "col": targetColor = data.ToColor(); break;
                default: return false;
            }

            return true;
        }

        public override StdEncoder Encode()  {
            targetSize.z = Mathf.Max(targetSize.z, 1);

            var cody = this.EncodeUnrecognized()
            .Add("sc", targetSize)
            .Add("pos", targetLocalPosition)
            .Add("col", targetColor);

            return cody;
        }
        #if PEGI

        public override bool PEGI() {

            bool changed = false;

            float x = targetSize.x;
            if ("Width".edit(50, ref x, 1f, 5f).nl())  {
                changed = true;
                targetSize.x = x;
            }

            float y = targetSize.y;
            if ("Height".edit(50, ref y, 1f, 5f).nl()) {
                changed = true;
                targetSize.y = y;
            }

            if ("Color".edit(ref targetColor))
                changed = true;
            
            return changed;
        }
#endif
    }

}