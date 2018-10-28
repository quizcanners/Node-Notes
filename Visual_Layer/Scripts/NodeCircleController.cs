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

        public string backgroundConfig = "";

        bool IsCurrent => source == Shortcuts.CurrentNode;

        #region Node Image
        Texture coverImage = null;
        public string imageURL = "";
        int imageIndex = -1;
        float imageScaling = 1f;
        enum imageMode { fit, tile }
        imageMode mode;

        void LoadCoverImage() {
            if (imageURL.Length > 8)
                imageIndex = Mgmt.textureDownloader.StartDownload(imageURL);
        }

        #endregion

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

            if (coverImage)
                this.clickHighlight(coverImage);
            else
                this.clickHighlight();

            return changed;
        }

        public override string NameForPEGI {
            get { return source.name; }

            set
            {
                source.name = value;

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

        LoopLock loopLock = new LoopLock();

        //int showDependencies = false;
        public override bool Inspect() {

            bool changed = false;

            if (loopLock.Unlocked) {
                using (loopLock.Lock()) {
                    if (source.Try_Nested_Inspect()) {
                        if (name != source.name)
                            NameForPEGI = source.name;

                        changed = true;
                        Shortcuts.visualLayer.UpdateVisibility();
                    }
                }
            } else {

                bool onPlayScreen = pegi.paintingPlayAreaGUI;

                if (source != null && source.parentNode == null && icon.Exit.Click("Exit story"))
                    Shortcuts.CurrentNode = null;

                if (source != null) {
                    bool enabled = source.Conditions_isEnabled();

                    var nd = source.AsNode;

                    if (nd != null) {
                        if (IsCurrent && nd.parentNode != null && icon.StateMachine.Click("Exit this Node"))
                            Shortcuts.CurrentNode = nd.parentNode;
                           
                        

                        if (!IsCurrent && icon.Enter.Click("Enter this Node"))
                            Shortcuts.CurrentNode = nd;
                    }

                    if ((enabled ? icon.Active : icon.InActive).Click("Try Force Active fconditions to {0}".F(!enabled)) && !source.TryForceEnabledConditions(!enabled))
                        Debug.Log("No Conditions to force to {0}".F(!enabled));

                    if (IsCurrent)
                    {
                        source.name.write(PEGI_Styles.ListLabel);

                        if (source != null)
                        {
                            if (NodesStyleBase.all.selectTypeTag(ref background).nl())
                                changed = true;
                            Mgmt.SetBackground(this);
                        }
                    }
                    else
                        source.name.write("Lerp parameter {0}".F(dominantParameter), enabled ? PEGI_Styles.EnterLabel : PEGI_Styles.ExitLabel);


                    pegi.nl();

                    if (circleRendy)
                    {
                        if (!onPlayScreen) {
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

                    if (source == null || (!source.InspectingTriggerStuff)) {

                        var altVis = PossibleOverrideVisualConfig;
                        var act = ActiveConfig;

                        if (altVis != nodeActive_Default_Visuals) {
                            if ("Override visuals for {0}".F(altVis == nodeInactiveVisuals ? "Disabled" : "Entered")
                                .toggleIcon(ref altVis.enabled, true).nl()) {
                                if (altVis.enabled)
                                    altVis.Decode(act.Encode().ToString());
                            }

                        }

                        changed |= ActiveConfig.Nested_Inspect();



                    }

                    if (!onPlayScreen)
                    {

                        pegi.nl();

                        bool seeDeps = "Dependencies".enter(ref inspectedStuff, 3).nl();

                        if (!textA || seeDeps)
                            "Text A".edit(ref textA).nl();

                        if (!textB || seeDeps)
                            "Text B".edit(ref textB).nl();

                        if (!circleRendy || seeDeps)
                            "Mesh Rendy".edit(ref circleRendy).nl();
                    }

                    if (inspectedStuff == -1) {

                        if (imageIndex != -1) {
                            if (!pegi.paintingPlayAreaGUI)
                                "Downloading {0} [1]".F(imageURL, imageIndex).write();
                        }
                        else
                        {

                            if ("Image".edit("Will not be saved", 40, ref coverImage).nl())
                                SetImage();

                            var shortURL = imageURL;
                            var ind = imageURL.LastIndexOf("/");
                            if (ind > 0)
                                shortURL = imageURL.Substring(ind);

                            bool reload = false;

                            bool changedURL = "Paste URL".edit(90, ref shortURL);
                            if (changedURL)
                            {
                                if (shortURL.Length > 8 || shortURL.Length == 0)
                                {
                                    reload = true;
                                    imageURL = shortURL;
                                }

                                changed = true;
                            }

                            reload |= (imageURL.Length > 8 && icon.Refresh.Click());

                            if (reload)
                                LoadCoverImage();
                            pegi.nl();


                            if (coverImage != null) {

                                if ("Img Mode".editEnum(50, ref mode).nl())
                                    SetImage();

                                if (mode == imageMode.tile)
                                    changed |= "Image Scale".edit(70, ref imageScaling, 1, 10).nl();

                                if (!pegi.paintingPlayAreaGUI)
                                    pegi.write(coverImage, 200); pegi.nl();

                            }
                        }
                    }

                }
            }


            if (changed)
            {
                SetDirty();
                sh_currentColor = ActiveConfig.targetColor;
                UpdateShaders();
            }

            return changed;
        }

#endif
        #endregion

        #region Visual Configuration


   
        NodeVisualConfig nodeActive_Default_Visuals = new NodeVisualConfig();
        NodeVisualConfig nodeEnteredVisuals = new NodeVisualConfig();
        NodeVisualConfig nodeInactiveVisuals = new NodeVisualConfig();

        NodeVisualConfig PossibleOverrideVisualConfig => (source == null ? nodeActive_Default_Visuals : (source.Conditions_isEnabled()
               ? (IsCurrent ? nodeEnteredVisuals : nodeActive_Default_Visuals)
                : nodeInactiveVisuals));

        NodeVisualConfig ActiveConfig
        {
            get
            {
               var vc = PossibleOverrideVisualConfig;

                if (vc.IsDefault)
                    vc = nodeActive_Default_Visuals;

                return vc;
            }
        }
        
        Color sh_currentColor;
        Vector4 sh_square = Vector4.zero;
        float sh_blur = 0;

        ShaderFloatValue shadeCourners = new ShaderFloatValue("_Courners", 0,4);
        ShaderFloatValue shadeSelected = new ShaderFloatValue("_Selected", 0, 4);
        ShaderFloatValue textureFadeIn = new ShaderFloatValue("_TextureFadeIn", 0, 10);
        #endregion

        #region Controls & Updates
        [NonSerialized] public bool isFading;
        [NonSerialized] public bool lerpsFinished;

        public bool SetDirty() => lerpsFinished = false;

        float fadePortion = 0;

        void SetImage() {
            circleRendy.MaterialWhaever().mainTexture = coverImage;
            SetDirty();
            if (mode == imageMode.fit)
                circleRendy.MaterialWhaever().EnableKeyword("_CLAMP");
            else
                circleRendy.MaterialWhaever().DisableKeyword("_CLAMP");
        }

        void UpdateCoverImage()
        {
            if (imageIndex != -1) {
                if (Mgmt.textureDownloader.TryGetTexture(imageIndex, out coverImage)) {
                    SetImage();
                    imageIndex = -1;
                    SetDirty();
                }
            }
        }

        void UpdateShaders() {
            if (textB && textA) {
                ActiveText.color = new Color(0, 0, 0, activeTextAlpha * fadePortion);
                PassiveText.color = new Color(0, 0, 0, (1 - activeTextAlpha) * fadePortion);
            }

            if (circleRendy) {
                sh_currentColor.a = fadePortion;

                var pos = Camera.main.WorldToScreenPoint(transform.position).ToVector2();
                pos.Scale(new Vector2(1f / Screen.width, 1f / Screen.height));

                circleRendy.MaterialWhaever().SetVector("_ProjTexPos", pos.ToVector4(imageScaling));

                circleRendy.MaterialWhaever().SetColor("_Color", sh_currentColor);
                circleRendy.MaterialWhaever().SetVector("_Stretch", sh_square);
                circleRendy.MaterialWhaever().SetFloat("_Blur", sh_blur);

            }
            
        }

        public string dominantParameter;
        void Update() {

            bool needShaderUpdate = false;
            
            float portion = 1;

            var ac = ActiveConfig;

            UpdateCoverImage();

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

                SetDirty();
            }

            if (!lerpsFinished)  {

                float dist = (transform.localPosition - ac.targetLocalPosition).magnitude;

                sh_blur = Mathf.Lerp(sh_blur, Mathf.Clamp01(dist*5), Time.deltaTime * 10);

                if (50f.SpeedToMinPortion(dist, ref portion))
                    dominantParameter = "postiton";

                if (12f.SpeedToMinPortion(ac.targetColor.DistanceRGB(sh_currentColor), ref portion))
                    dominantParameter = "color";

                var BGtf = circleRendy.transform;

                var scale = BGtf.localScale;
                if (40f.SpeedToMinPortion((scale - ac.targetSize).magnitude , ref portion))
                    dominantParameter = "size";

                if (4f.SpeedToMinPortion(fadePortion - (isFading ? 0f : 1f), ref portion))
                    dominantParameter = "fade";

                if (8f.SpeedToMinPortion(1-activeTextAlpha, ref portion))
                    dominantParameter = "text Alpha";

                textureFadeIn.targetValue = coverImage ? 1 : 0;
                textureFadeIn.Portion(ref portion, ref dominantParameter);

                shadeCourners.targetValue = (this == dragging) ? 0 : (source == Shortcuts.CurrentNode) ? 0.4f : 0.9f;
                shadeCourners.Portion(ref portion, ref dominantParameter);

                shadeSelected.targetValue = (this == Mgmt.selectedNode ? 1f : 0f);
                shadeSelected.Portion(ref portion, ref dominantParameter);

                float teleportPortion = ( fadePortion < 0.1f && !isFading) ? 1 : portion;

                transform.localPosition = Vector3.Lerp(transform.localPosition, ac.targetLocalPosition, teleportPortion);
                scale = Vector3.Lerp(scale, ac.targetSize, teleportPortion);
                BGtf.localScale = scale;
                sh_currentColor = Color.Lerp(sh_currentColor, ac.targetColor, teleportPortion);
                fadePortion = Mathf.Lerp(fadePortion, isFading ? 0 : 1, portion);

                shadeCourners.Lerp(portion, circleRendy);
                shadeSelected.Lerp(portion, circleRendy);
                textureFadeIn.Lerp(portion, circleRendy);

                needShaderUpdate = true;
                if (portion == 1)
                {
                    ActiveConfig.targetSize = circleRendy.transform.localScale;
                    ActiveConfig.targetLocalPosition = transform.localPosition;

                    lerpsFinished = true;
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

                if (source.AsGameNode != null)
                    Shortcuts.visualLayer.FromNodeToGame(source.AsGameNode);
                else 
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

        #endregion
        
        #region Encode & Decode
        public override ISTD Decode(string data)   {
            if (this == dragging)
                dragging = null;

            SetDirty();

            background = "";
            backgroundConfig = "";
            imageURL = "";
            imageIndex -= 1;
            coverImage = null;

            nodeEnteredVisuals = new NodeVisualConfig();
            nodeActive_Default_Visuals = new NodeVisualConfig();
            nodeActive_Default_Visuals.enabled = true;
            nodeInactiveVisuals = new NodeVisualConfig();

            base.Decode(data);

            LoadCoverImage();

            return this;
        }

        public override bool Decode(string tag, string data)   {
            switch (tag)   {
                case "expVis": data.DecodeInto(out nodeEnteredVisuals); break;
                case "subVis": data.DecodeInto(out nodeActive_Default_Visuals); break;
                case "disVis": data.DecodeInto(out nodeInactiveVisuals); break;
                case "bg": background = data; break;
                case "bg_cfg": backgroundConfig = data; break;
                case "URL": imageURL = data; break;
                case "imgScl": imageScaling = data.ToFloat(); break;
                default: return false;
            }

            return true;
        }

        public override StdEncoder Encode() {

            var cody = this.EncodeUnrecognized()
                .Add("subVis", nodeActive_Default_Visuals)
                .Add_IfNotDefault("disVis", nodeInactiveVisuals)
                .Add_IfNotEmpty("bg", background)
                .Add_IfNotEmpty("bg_cfg", backgroundConfig)
                .Add_IfNotEmpty("URL", imageURL);

            if (imageURL.Length > 0)
                cody.Add("imgScl", imageScaling);

            if (source.AsNode != null)
                cody.Add_IfNotDefault("expVis", nodeEnteredVisuals); 
            return cody;
        }

        #endregion

        #region MGMT
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
        #endregion
    }

    public class NodeVisualConfig : AbstractKeepUnrecognized_STD, IPEGI, IPEGI_ListInspect, ICanBeDefault_STD {
        public Vector3 targetSize = new Vector3(5,3,1);
        public Vector3 targetLocalPosition = Vector3.zero;
        public Color targetColor = Color.gray;
        public bool enabled = false;

        #region Encode & Decode
        public override bool IsDefault => !enabled;

        public override ISTD Decode(string data) {
            enabled = true;
            return base.Decode(data);
        }

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
            if ("Width".edit(50, ref x, 1f, 15f).nl())  {
                changed = true;
                targetSize.x = x;
            }

            float y = targetSize.y;
            if ("Height".edit(50, ref y, 1f, 15f).nl()) {
                changed = true;
                targetSize.y = y;
            }

            if ("Color".edit(50, ref targetColor).nl())
                changed = true;
            
            return changed;
        }

        public bool PEGI_inList(IList list, int ind, ref int edited) {

            var changed = "col".edit(40, ref targetColor);

            if (icon.Enter.Click())
                edited = ind;

            return changed;
        }
#endif
        #endregion
    }

}