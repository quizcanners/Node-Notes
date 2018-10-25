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
    public class NodeCircleController : ComponentSTD, IPEGI, IGotName, IPEGI_ListInspect
    {
        static Nodes_PEGI Mgmt => Nodes_PEGI.NodeMGMT_inst;
        
        public Renderer circleRendy;

        public Base_Node source;

        public string background = "";

        public string backgroundConfig ="";

        public string imageURL = "";

        #region TEXT
        public TextMeshPro textA;

        public TextMeshPro textB;

        bool activeTextIsA = false;

        TextMeshPro ActiveText => activeTextIsA ? textA : textB;

        TextMeshPro PassiveText => activeTextIsA ? textB : textA;

        public string newText = null;

        float activeTextAlpha = 0;

        #endregion

        #region Inspector
#if PEGI

        public override bool PEGI_inList(IList list, int ind, ref int edited) {

            bool changed = ActiveConfig.PEGI_inList(list, ind, ref edited);

            this.clickHighlight();

            return changed;
        }


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

        bool showDependencies = false;
        public override bool Inspect()
        {

            bool changed = false;

            bool onPlayScreen = pegi.paintingPlayAreaGUI;

            if (source != null)
            {
                if (source.Try_Nested_Inspect())
                {
                    if (name != source.name)
                        NameForPEGI = source.name;

                    changed = true;

                    Nodes_PEGI.UpdateVisibility();
                }
            }

            if (source != null && source.parentNode == null && icon.Exit.Click("Exit story"))
                Shortcuts.CurrentNode = null;

            //if (!onPlayScreen && Application.isPlaying)
            //  ;
            if (source != null)
            {
                bool enabled = source.Conditions_isEnabled();

                var nd = source.AsNode;

                if (nd != null)
                {
                    if (isCurrent && icon.StateMachine.Click("Exit this Node"))
                        Shortcuts.CurrentNode = null;

                    if (!isCurrent && icon.Enter.Click("Enter this Node"))
                        Shortcuts.CurrentNode = nd;

                }


                if ((enabled ? icon.Active : icon.InActive).Click("Try Force Active fconditions to {0}".F(!enabled))
                    && !source.TryForceEnabledConditions(!enabled))
                {
                    Debug.Log("No Conditions to force to {0}".F(!enabled));
                }

                if (isCurrent)
                {
                    source.name.write( PEGI_Styles.ListLabel);

                    if (source != null) {
                        if (NodesStyleBase.all.selectTypeTag(ref background).nl())
                            changed = true;
                        Mgmt.SetBackground(this);
                    }
                } else
                    source.name.write("Lerp parameter {0}".F(dominantParameter), enabled ? PEGI_Styles.EnterLabel : PEGI_Styles.ExitLabel);


                pegi.nl();

                if (circleRendy)
                {
                    if (!onPlayScreen)
                    {
                        if (newText != null)
                            "Changeing text to {0}".F(newText).nl();

                        if (isFading)
                            "Fading...{0}".F(fadePortion).nl();
                    }

                 

                    var bg = Mgmt.backgroundControllers.TryGetByTag(background);
                    if (bg != null)
                    {
                        if (bg.Try_Nested_Inspect().nl())
                        {
                            changed = true;
                            var std = bg as ISTD;
                            if (std != null)
                                backgroundConfig = std.Encode().ToString();
                        }
                    }
                }

                if (source == null || (!source.InspectingTriggerStuff))
                    if (ActiveConfig.Nested_Inspect())
                    {
                        assumedPosition = false;
                        sh_currentColor = ActiveConfig.targetColor;
                        UpdateShaders();
                    }


                if (!onPlayScreen)
                {

                    pegi.nl();

                    "Dependencies".foldout(ref showDependencies).nl();

                    if (!textA || showDependencies)
                        "Text A".edit(ref textA).nl();

                    if (!textB || showDependencies)
                        "Text B".edit(ref textB).nl();

                    if (!circleRendy || showDependencies)
                        "Mesh Rendy".edit(ref circleRendy).nl();
                }
            }
                return changed;
        }
        
        #endif
        #endregion

        [NonSerialized] public bool isFading;
        [NonSerialized] public bool assumedPosition;

        NodeVisualConfig nodeEnteredVisuals = new NodeVisualConfig();
        NodeVisualConfig nodeActiveVisuals = new NodeVisualConfig();
        NodeVisualConfig nodeInactiveVisuals = new NodeVisualConfig();

        bool isCurrent => source == Shortcuts.CurrentNode;
        NodeVisualConfig ActiveConfig => 
            (source.Conditions_isEnabled() ? (isCurrent ? nodeEnteredVisuals : nodeActiveVisuals) : nodeInactiveVisuals);
        
        Color sh_currentColor;
        Vector4 sh_square = Vector4.zero;
        float sh_blur = 0;

        ShaderFloatValue shadeCourners = new ShaderFloatValue("_Courners", 0,4);
        ShaderFloatValue shadeSelected = new ShaderFloatValue("_Selected", 0, 4);

        float fadePortion = 0;

        void UpdateShaders() {
            if (textB && textA) {
                ActiveText.color = new Color(0, 0, 0, activeTextAlpha * fadePortion);
                PassiveText.color = new Color(0, 0, 0, (1 - activeTextAlpha) * fadePortion);
            }

            sh_currentColor.a = fadePortion;

          
                if (circleRendy)
                {
                
                    if (Application.isPlaying)
                    {
                        circleRendy.material.SetColor("_Color", sh_currentColor);
                        circleRendy.material.SetVector("_Stretch", sh_square);
                       // circleRendy.material.SetFloat("_Courners", sh_courners);
                        circleRendy.material.SetFloat("_Blur", sh_blur);
                        //circleRendy.material.SetFloat("_Selected", sh_selected);
                    }
                    else
                    {
                        circleRendy.sharedMaterial.SetColor("_Color", sh_currentColor);
                        circleRendy.sharedMaterial.SetVector("_Stretch", sh_square);
                      //  circleRendy.sharedMaterial.SetFloat("_Courners", sh_courners);
                        circleRendy.sharedMaterial.SetFloat("_Blur", sh_blur);
                        //circleRendy.sharedMaterial.SetFloat("_Selected", sh_selected);
                    }
                }
            
        }

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
                
                // if (4f.SpeedToMinPortion(Mathf.Abs(targetCourners - sh_courners), ref portion))
                //   dominantParameter = "courners";
                shadeCourners.targetValue = (this == dragging) ? 0 : (source == Shortcuts.CurrentNode) ? 0.4f : 0.9f;
                shadeCourners.Portion(ref portion, ref dominantParameter);

                shadeSelected.targetValue = (this == Mgmt.selectedNode ? 1f : 0f);
                shadeSelected.Portion(ref portion, ref dominantParameter);
                //if (4f.SpeedToMinPortion(Mathf.Abs(sh_selected - targetSelected), ref portion))
                //  dominantParameter = "Selection Outline";

                float teleportPortion = ( fadePortion < 0.1f && !isFading) ? 1 : portion;

                transform.localPosition = Vector3.Lerp(transform.localPosition, ac.targetLocalPosition, teleportPortion);
                scale = Vector3.Lerp(scale, ac.targetSize, teleportPortion);
                BGtf.localScale = scale;
                sh_currentColor = Color.Lerp(sh_currentColor, ac.targetColor, teleportPortion);
                fadePortion = Mathf.Lerp(fadePortion, isFading ? 0 : 1, portion);
                shadeCourners.Lerp(portion, circleRendy);
                //sh_courners = Mathf.Lerp(sh_courners, targetCourners, teleportPortion);
                //sh_selected = Mathf.Lerp(sh_selected, targetSelected, portion);
                shadeSelected.Lerp(portion, circleRendy);

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
                    activeTextAlpha = MyMath.Lerp_bySpeed(activeTextAlpha, 1, 4);

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


        #region Encode & Decode
        public override ISTD Decode(string data)   {
            if (this == dragging)
                dragging = null;
            assumedPosition = false;

            background = "";
            backgroundConfig = "";
            imageURL = "";

             nodeEnteredVisuals = new NodeVisualConfig();
             nodeActiveVisuals = new NodeVisualConfig();
             nodeInactiveVisuals = new NodeVisualConfig();

            return base.Decode(data);
        }

        public override bool Decode(string tag, string data)   {
            switch (tag)   {
                case "expVis": data.DecodeInto(out nodeEnteredVisuals); break;
                case "subVis": data.DecodeInto(out nodeActiveVisuals); break;
                case "disVis": data.DecodeInto(out nodeInactiveVisuals); break;
                case "bg": background = data; break;
                case "bg_cfg": backgroundConfig = data; break;
                case "URL": imageURL = data; break;
                default: return false;
            }

            return true;
        }

        public override StdEncoder Encode() {

            var cody = this.EncodeUnrecognized()
                .Add("subVis", nodeActiveVisuals)
                .Add("disVis", nodeInactiveVisuals)
                .Add_IfNotEmpty("bg", background)
                .Add_IfNotEmpty("bg_cfg", backgroundConfig)
                .Add_IfNotEmpty("URL", imageURL);

            if (source.AsNode != null)
                cody.Add("expVis", nodeEnteredVisuals); 
            return cody;
        }

        #endregion

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

    public class NodeVisualConfig : AbstractKeepUnrecognized_STD, IPEGI, IPEGI_ListInspect {
        public Vector3 targetSize = new Vector3(5,3,1);
        public Vector3 targetLocalPosition = Vector3.zero;
        public Color targetColor = Color.gray;

        #region Encode & Decode
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
            .Add("pos", targetLocalPosition);
            if (targetColor != Color.grey)
                cody.Add("col", targetColor);

            return cody;
        }
        #endregion

        #region Inspect
        #if PEGI

        public override bool Inspect() {

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

            if ("Color".edit(50, ref targetColor).nl())
                changed = true;
            
            return changed;
        }

        public bool PEGI_inList(IList list, int ind, ref int edited) {

            var changed = pegi.edit(ref targetColor);

            if (icon.Enter.Click())
                edited = ind;

            return changed;
        }
#endif
        #endregion
    }

}