using System.Collections;
using TMPro;
using UnityEngine;
using QuizCannersUtilities;
using PlayerAndEditorGUI;
using System;
using System.Collections.Generic;
using NodeNotes;
using QcTriggerLogic;
using UnityEditor.IMGUI.Controls;

namespace NodeNotes_Visual {


#pragma warning disable IDE0018 // Inline variable declaration

    [ExecuteInEditMode]
    public class NodeCircleController : ComponentCfg, IGotIndex, ILinkedLerping {

        private static NodesVisualLayer Mgmt => NodesVisualLayer.Instance;

        public Renderer circleRenderer;

        public MeshCollider circleCollider;

        public Base_Node source;
        
        private bool IsCurrent => source == Shortcuts.CurrentNode;

        static Node CurrentNode => Shortcuts.CurrentNode;

        #region Node Image

        private Texture _coverImage;
        [NonSerialized] public string imageUrl = "";
        private bool _hideLabel;
        private int _imageIndex = -1;
        private float _imageScaling = 1f;

        private enum ImageMode { Fit, Tile }

        private ImageMode _mode;

        void LoadCoverImage() {
            if (imageUrl.Length > 8)
                _imageIndex = Mgmt.textureDownloader.StartDownload(imageUrl);
        }

        #endregion

        #region TEXT

        public void UpdateName()
        {
            if (source != null)
            {

                if (fadePortion < 0.1f)
                {
                    newText = null;
                    _activeTextAlpha = 1;
                    ActiveText.text = source.name;
                    gameObject.name = source.name;
                    PassiveText.text = "";
                    UpdateView();
                }
                else
                    newText = source.name;
            } else Debug.LogError("No source on ", this);

            lerpsFinished = false;
        }

        public TextMeshPro textA;

        public TextMeshPro textB;

        private bool _activeTextIsA;

        private TextMeshPro ActiveText => _activeTextIsA ? textA : textB;

        private TextMeshPro PassiveText => _activeTextIsA ? textB : textA;

        [NonSerialized] public string newText;

        private float _activeTextAlpha;

        #endregion

        #region Inspector
        public Base_Node myLastLinkedNode;

        private int _indexInPool;
        public int IndexForPEGI { get { return _indexInPool;  } set { _indexInPool = value; } }
        
        public override bool InspectInList(IList list, int ind, ref int edited) {

            var changed = ActiveConfig.InspectInList(list, ind, ref edited);

            if (_coverImage)
                this.ClickHighlight(_coverImage);
            else
                this.ClickHighlight();

            return changed;
        }

        public override string NameForPEGI {
            get { return source.name; }

            set
            {
                source.name = value;

                UpdateName();
            }
        }

        readonly LoopLock _loopLock = new LoopLock();

        //int showDependencies = false;
        public override bool Inspect() {

            var changed = false;
     
            if (_loopLock.Unlocked && source != null && source.inspectionLock.Unlocked) {
                using (_loopLock.Lock()) {
                    if (pegi.Try_Nested_Inspect(source).changes(ref changed)) {
                        if (name != source.name)
                            NameForPEGI = source.name;
                        
                        Shortcuts.visualLayer.OnLogicVersionChange();
                    }
                }

            } else {
                
                var onPlayScreen = pegi.paintingPlayAreaGui;

                if (source != null && source.parentNode == null && icon.Exit.Click("Exit story"))
                    Shortcuts.CurrentNode = null;

                if (source != null)
                {
                    var conditionPassed = source.Conditions_isEnabled();

                    var nd = source.AsNode;

                    if (nd != null)
                    {
                        if (IsCurrent && nd.parentNode != null && icon.StateMachine.Click("Exit this Node"))
                            Shortcuts.TryExitCurrentNode(); 
                        
                        if (!IsCurrent && icon.Enter.Click("Enter this Node"))
                            Shortcuts.CurrentNode = nd;
                    }

                    if ((conditionPassed ? icon.Active : icon.InActive).Click("Try Force Active conditions to {0}".F(!conditionPassed)) && !source.TryForceEnabledConditions(Values.global,!conditionPassed))
                        Debug.Log("No Conditions to force to {0}".F(!conditionPassed));

                    pegi.nl();

                    if (IsCurrent) 
                        source.name.write(PEGI_Styles.ListLabel);
                    else
                        source.name.write("Lerp parameter {0}".F(dominantParameter), conditionPassed ? PEGI_Styles.EnterLabel : PEGI_Styles.ExitLabel);
                        
                }

                pegi.nl();

                if (circleRenderer)
                {
                    if (source != null)
                    {
                        var node = source.AsNode;

                        if (node != null)
                        {

                            var bg = TaggedTypes.TryGetByTag(Mgmt.backgroundControllers, node.visualStyleTag);

                            if (bg != null)
                            {
                                if (pegi.Try_Nested_Inspect(bg).nl(ref changed))
                                    source.visualStyleConfigs[NodesVisualLayer.SelectedVisualLayer.ClassTag] =
                                        bg.Encode().ToString();
                            }
                        }
                    } else "No source node is currently linked.".writeHint();
                }

                if (source == null || (!source.InspectingTriggerItems)) {

                    var altVis = PossibleOverrideVisualConfig;
                    var act = ActiveConfig;

                    if (altVis != _nodeActiveDefaultVisuals) {
                        if ("Override visuals for {0}".F(altVis == _nodeInactiveVisuals ? "Disabled" : "Entered")
                            .toggleIcon(ref altVis.enabled).nl()) {
                            if (altVis.enabled)
                                altVis.Decode(act.Encode().ToString());
                        }
                    }

                    ActiveConfig.Nested_Inspect().changes(ref changed);
                }

                if (!onPlayScreen) {
                    pegi.nl();

                    var seeDependencies = "Dependencies".enter(ref inspectedItems, 3).nl();

                    if (!textA || seeDependencies)
                        "Text A".edit(ref textA).nl();

                    if (!textB || seeDependencies)
                        "Text B".edit(ref textB).nl();

                    if (!circleRenderer || seeDependencies)
                        "Mesh Renderer".edit(ref circleRenderer).nl();

                    if (!circleCollider || seeDependencies)
                        "Collider".edit(ref circleCollider).nl();
                }

                if (inspectedItems == -1) {

                    if (_imageIndex != -1) {
                        if (!pegi.paintingPlayAreaGui)
                            "Downloading {0} [1]".F(imageUrl, _imageIndex).write();
                    }  else  {
                        if ("Image".edit("Will not be saved", 40, ref _coverImage).nl())
                            SetImage();
                        
                        var shortUrl = imageUrl.SimplifyDirectory(); 

                        var reload = false;

                        var changedUrl = "Paste URL".edit(90, ref shortUrl).changes(ref changed);
                        if (changedUrl && (shortUrl.Length > 8 || shortUrl.Length == 0)) {
                                reload = true;
                                imageUrl = shortUrl;
                        }

                        reload |= (imageUrl.Length > 8 && icon.Refresh.Click().changes(ref changed));

                        if (reload)
                            LoadCoverImage();

                        pegi.nl();
                    
                        if (_coverImage) {

                            if ("Img Mode".editEnum(50, ref _mode).nl())
                                SetImage();

                        if (_mode == ImageMode.Tile)
                            "Image Scale".edit(70, ref _imageScaling, 1, 10).nl(ref changed);
                        else
                            "Hide Label".toggleIcon(ref _hideLabel).nl(ref changed);

                            if (!pegi.paintingPlayAreaGui)
                                _coverImage.write(200); pegi.nl();

                        }
                    }
                }
                
            }
            
            if (changed)
            {
                SetDirty();
                bgColor = ActiveConfig.targetColor;
                UpdateView();
            }

            return changed;
        }
        
        #endregion

        #region Visual Configuration

        private NodeVisualConfig _nodeActiveDefaultVisuals = new NodeVisualConfig();
        private NodeVisualConfig _nodeEnteredVisuals = new NodeVisualConfig();
        private NodeVisualConfig _nodeInactiveVisuals = new NodeVisualConfig();

        NodeVisualConfig PossibleOverrideVisualConfig => 
            (source == null ? _nodeActiveDefaultVisuals : (source.Conditions_isEnabled()
               ? (IsCurrent ? _nodeEnteredVisuals : _nodeActiveDefaultVisuals)
                : _nodeInactiveVisuals));

        NodeVisualConfig ActiveConfig
        {
            get
            {
               var vc = PossibleOverrideVisualConfig;

                if (vc.IsDefault)
                    vc = _nodeActiveDefaultVisuals;

                return vc;
            }
        }
        
        #endregion
        
        #region Lerping

        private LinkedLerp.ColorValue _textColor = new LinkedLerp.ColorValue("Text Color");

        private Color bgColor;
        private Vector4 _shSquare = Vector4.zero;
        private float _shBlur;

        private LinkedLerp.MaterialFloat _shadeCorners;
        private LinkedLerp.MaterialFloat _shadeSelected;
        private LinkedLerp.MaterialFloat _textureFadeIn;
        private LinkedLerp.TransformLocalPosition _localPos;
        private LinkedLerp.TransformLocalScale _localScale;
        private LinkedLerp.RendererMaterialTextureTransition _texTransition;

        private bool includedInLerp;

        public void Portion(LerpData ld) {

            includedInLerp = (gameObject.activeSelf && !lerpsFinished && this != _dragging);

            if  (!includedInLerp) return;
            
            var ac = ActiveConfig;

            if (!isFading || !_fadingRelation)
                _localPos.targetValue = ac.targetLocalPosition;
            else if (!_fadingIntoParent)
                _localPos.targetValue = _fadingRelation.transform.localPosition
                                        + (transform.localPosition - _fadingRelation.transform.localPosition).normalized * 50;
            else
                _localPos.targetValue = _fadingRelation.transform.localPosition;
                
            _localPos.Portion(ld);

            _localScale.targetValue = isFading ? Vector3.one : ac.targetSize;

            _localScale.Portion(ld);

            _textColor.targetValue = ac.targetTextColor;

            _textColor.Portion(ld);

            if (12f.SpeedToMinPortion(ac.targetColor.DistanceRgb(bgColor), ld))
                dominantParameter = "color";

            if (4f.SpeedToMinPortion(fadePortion - (isFading ? 0f : 1f),  ld))
                dominantParameter = "fade";

            if (8f.SpeedToMinPortion(1 - _activeTextAlpha,  ld))
                dominantParameter = "text Alpha";

            _textureFadeIn.targetValue = _coverImage ? 1 : 0;
            _textureFadeIn.Portion(ld);

            _shadeCorners.targetValue = (this == _dragging) ? 0 : (source == Shortcuts.CurrentNode) ? 0.4f : 0.9f;
            _shadeCorners.Portion(ld);

            _shadeSelected.targetValue = (this == WhiteBackground.inst.selectedNode ? 1f : 0f);
            _shadeSelected.Portion(ld);

            _texTransition.Portion(ld);

        }

        public void Lerp(LerpData ld, bool canSkipLerp = false) {

            if (!includedInLerp) return;

            var ac = ActiveConfig;

            var needShaderUpdate = false;

            if (!lerpsFinished && (this != _dragging)) {

                needShaderUpdate = true;

                _shBlur = Mathf.Lerp(_shBlur, Mathf.Clamp01((transform.localPosition - _localPos.targetValue).magnitude * 5), Time.deltaTime * 10);

                fadePortion = Mathf.Lerp(fadePortion, isFading ? 0 : 1, ld.MinPortion);
                    
                var scale = _localScale.Value;

                if (scale.x > 0)
                    _shSquare.x = scale.x > scale.y ? ((scale.x - scale.y) / scale.x) : 0;
                else _shSquare.x = 0;
                if (scale.y > 0)
                    _shSquare.y = scale.y > scale.x ? ((scale.y - scale.x) / scale.y) : 0;
                else _shSquare.y = 0;

                var textSize = new Vector2(17 + scale.x * 3, 5f + Mathf.Max(0, (scale.y - 1f) * 3f));

                textA.rectTransform.sizeDelta = textSize;
                textB.rectTransform.sizeDelta = textSize;
            }

            if (!lerpsFinished && (this != _dragging) && ld.MinPortion == 1)
                lerpsFinished = true;
            
            if (newText != null || _activeTextAlpha < 1)
            {
                lerpsFinished = false;

                _activeTextAlpha = newText == null 
                    ? Mathf.Lerp(_activeTextAlpha, 1, ld.Portion()) 
                    : LerpUtils.LerpBySpeed(_activeTextAlpha, 1, 4);

                if (_activeTextAlpha == 1 && newText != null)
                {
                    _activeTextIsA = !_activeTextIsA;
                    ActiveText.text = newText;
                    gameObject.name = newText;
                    _activeTextAlpha = 0;
                    newText = null;
                }

                needShaderUpdate = true;
            }

            bool skipLerpPossible = (_canJumpToPosition && fadePortion < 0.1f && !isFading);

            bgColor = Color.Lerp(bgColor, ac.targetColor, ld.Portion(skipLerpPossible));
            _textColor.Lerp(ld, skipLerpPossible);
            _localPos.Lerp(ld);
            _localScale.Lerp(ld, skipLerpPossible);
            _shadeCorners.Lerp(ld, skipLerpPossible);
            _shadeSelected.Lerp(ld, skipLerpPossible);
            _textureFadeIn.Lerp(ld, skipLerpPossible);
            _texTransition.Lerp(ld, skipLerpPossible);

            if (needShaderUpdate)
                UpdateView();

          
            if (fadePortion == 0 && isFading && Application.isPlaying)
                WhiteBackground.inst.Deactivate(this);
        }
        
        [NonSerialized] public string dominantParameter;

        #endregion

        #region Controls & Updates

        public void SetStartPositionOn(NodeCircleController previous) {

            _canJumpToPosition = false;

            if (previous && previous != this)
            {
                if (source.parentNode == CurrentNode)
                {
                    var vis = CurrentNode.visualRepresentation as NodeCircleController;
                    if (vis)
                    transform.localPosition = vis.transform.localPosition;
                    circleRenderer.transform.localScale = Vector3.one;
                }
                else
                {
                    transform.localPosition = ActiveConfig.targetLocalPosition + (ActiveConfig.targetLocalPosition - previous.transform.localPosition).normalized * 10f;
                   
                    circleRenderer.transform.localScale = ActiveConfig.targetSize * 2f;
                }
            }
            else _canJumpToPosition = true;

            SetDirty();
        }

        private bool _canJumpToPosition;
        private bool _fadingIntoParent;
        private NodeCircleController _fadingRelation;

        public void SetFadeAwayRelation(NodeCircleController previous) {
            if (previous) {

                if (previous != this) {
                    _fadingIntoParent = (CurrentNode?.Contains(previous.myLastLinkedNode) ?? false)
                       && myLastLinkedNode.parentNode == previous.myLastLinkedNode;  

                    if (_fadingIntoParent)
                        _fadingRelation = previous;
                    else 
                        _fadingRelation = CurrentNode?.visualRepresentation as NodeCircleController;

                } else
                {
                    _fadingIntoParent = false;
                    _fadingRelation = CurrentNode?.visualRepresentation as NodeCircleController;
                }


            }
            else _fadingRelation = null;

            SetDirty();
        }

        [NonSerialized] public bool isFading;
        [NonSerialized] public bool lerpsFinished;

        public bool SetDirty() => lerpsFinished = false;

        float fadePortion;

        private void SetImage() {
            var mat = circleRenderer.MaterialWhatever();

            _texTransition.TargetTexture = _coverImage;

            if (_mode == ImageMode.Fit)
                mat.EnableKeyword("_CLAMP");
            else
                mat.DisableKeyword("_CLAMP");

            SetDirty();
        }

        private void UpdateCoverImage() {

            if (_imageIndex != -1 && Mgmt.textureDownloader.TryGetTexture(_imageIndex, out _coverImage)) {
                    SetImage();
                    _imageIndex = -1;
                    SetDirty();
                
            } 
        }

        private static Camera MainCamera => NodesVisualLayer.MainCam;
        
        private void UpdateView() {

            if (textB && textA) {

                var textFadePortion = (_hideLabel ? (1 - _textureFadeIn.Value) : 1f) * fadePortion;

                var col = _textColor.CurrentValue;

                ActiveText.color = col.Alpha(_activeTextAlpha * textFadePortion);
                PassiveText.color = col.Alpha((1 - _activeTextAlpha) * textFadePortion);
            }

            if (circleRenderer) {
                bgColor.a = fadePortion;
                
                var pos = MainCamera.WorldToScreenPoint(transform.position).ToVector2();
                pos.Scale(new Vector2(1f / Screen.width, 1f / Screen.height));

                var mat = circleRenderer.MaterialWhatever();

                mat.Set(_projPos, pos.ToVector4(_imageScaling));
                mat.Set(_color, bgColor);
                mat.Set(_stretch, _shSquare);
                mat.Set(_blur, _shBlur);

            }
        }

        private readonly ShaderProperty.ColorValue _color = new ShaderProperty.ColorValue("_Color");
        private readonly ShaderProperty.VectorValue _projPos = new ShaderProperty.VectorValue("_ProjTexPos");
        private readonly ShaderProperty.VectorValue _stretch = new ShaderProperty.VectorValue("_Stretch");
        private readonly ShaderProperty.FloatValue _blur = new ShaderProperty.FloatValue("_Blur");

        static NodeCircleController _dragging;
        Vector3 _dragOffset = Vector3.zero;
        private static readonly Plane UpPlane = new Plane(Vector3.up, Vector3.zero);
        public void TryDragAndDrop()
        {
            if (_dragging) return;

            if (Input.GetMouseButtonDown(0))
            {
                var gn = source.AsGameNode;

                if (gn != null)
                    Shortcuts.visualLayer.FromNodeToGame(gn);
                else
                    WhiteBackground.inst.SetSelected(this);

                Vector3 pos;
                if (UpPlane.MouseToPlane(out pos, MainCamera))
                {
                    _dragging = this;
                    _dragOffset = transform.position - pos;
                }
            }
        }
        
        private void Update() {

            UpdateCoverImage();

            if (this != _dragging) return;

            if (isFading || !Base_Node.editingNodes || !Input.GetMouseButton(0))
                _dragging = null;
            else
            {
                Vector3 pos;
                if (UpPlane.MouseToPlane(out pos, MainCamera))
                {
                    transform.localPosition = pos + _dragOffset;
                    ActiveConfig.targetLocalPosition = transform.localPosition;
                }
            }
            SetDirty();
        }
        
        public void OnMouseOver()
        {
            if (isFading) return;

            if (Base_Node.editingNodes)
                TryDragAndDrop();
            else
                source?.OnMouseOver();
        }

        #endregion
        
        #region Encode & Decode
        public override void Decode(string data) {

            if (this == _dragging)
                _dragging = null;

            SetDirty();

            imageUrl = "";
            _imageIndex -= 1;
            _coverImage = null;
            _hideLabel = false;

            _nodeEnteredVisuals = new NodeVisualConfig();
            _nodeActiveDefaultVisuals = new NodeVisualConfig
            {
                enabled = true
            };
            _nodeInactiveVisuals = new NodeVisualConfig();

            base.Decode(data);

            LoadCoverImage();
            
        }

        public override bool Decode(string tg, string data)   {
            switch (tg)   {
                case "expVis": data.DecodeInto(out _nodeEnteredVisuals); break;
                case "subVis": data.DecodeInto(out _nodeActiveDefaultVisuals); break;
                case "disVis": data.DecodeInto(out _nodeInactiveVisuals); break;
                case "bg_cfg": source.visualStyleConfigs[WhiteBackground.classTag] = data; break;
                case "bg_cfgs": data.Decode_Dictionary(out source.visualStyleConfigs); break;
                case "URL": imageUrl = data; break;
                case "imgScl": _imageScaling = data.ToFloat(); break;
                case "imgMd": _mode = (ImageMode)data.ToInt(); break;
                case "hidTxt": _hideLabel = data.ToBool(); break; 
                default: return false;
            }

            return true;
        }

        public override CfgEncoder Encode() {

            var cody = this.EncodeUnrecognized()
                .Add("subVis", _nodeActiveDefaultVisuals)
                .Add_IfNotDefault("disVis", _nodeInactiveVisuals)
                .Add_IfNotEmpty("URL", imageUrl);

            if (imageUrl.Length > 0) {
                cody.Add("imgMd", (int)_mode);
                if (_mode == ImageMode.Tile) 
                    cody.Add("imgScl", _imageScaling);
                        else
                    cody.Add_IfTrue("hidTxt", _hideLabel);
            }

            if (source.AsNode != null)
                cody.Add_IfNotDefault("expVis", _nodeEnteredVisuals); 

            return cody;
        }

        #endregion

        #region MGMT
        public void LinkTo(Base_Node node) {

            myLastLinkedNode = node;
            source = node;

            Decode(source.LinkTo(this));

            NameForPEGI = source.name;
            isFading = false;
            gameObject.SetActive(true);
            if (circleCollider)
                circleCollider.enabled = true;
        }

        public void Unlink()
        {
            if (source != null) {
                source.Unlink(Encode());
                source = null;
            }
       
            isFading = true;

            if (circleCollider)
                circleCollider.enabled = false;
        }

        private void OnEnable() {

            if (Application.isEditor) {
                if (!circleRenderer)
                    circleRenderer = GetComponent<MeshRenderer>();
            }

            _shadeCorners = new LinkedLerp.MaterialFloat("_Courners", 0, 4, circleRenderer);
            _shadeSelected = new LinkedLerp.MaterialFloat("_Selected", 0, 4, circleRenderer);
            _textureFadeIn = new LinkedLerp.MaterialFloat("_TextureFadeIn", 0, 10, circleRenderer);
            _localPos = new LinkedLerp.TransformLocalPosition(transform, 50);
            _localScale = new LinkedLerp.TransformLocalScale(circleRenderer.transform, 40);
            _texTransition = new LinkedLerp.RendererMaterialTextureTransition(circleRenderer, 1);

        }

        #endregion
    }

    public class NodeVisualConfig : AbstractKeepUnrecognizedCfg, IPEGI, IPEGI_ListInspect, ICanBeDefaultCfg {
        public Vector3 targetSize = new Vector3(5,3,1);
        public Vector3 targetLocalPosition = Vector3.zero;
        public Color targetColor = Color.gray;
        public Color targetTextColor = Color.black;
        public bool enabled;

        #region Encode & Decode
        public override bool IsDefault => !enabled;

        public override void Decode(string data) {
            enabled = true;
            base.Decode(data);
        }

        public override bool Decode(string tg, string data)
        {
            switch (tg)  {
                case "sc": targetSize = data.ToVector3(); break;
                case "pos": targetLocalPosition = data.ToVector3(); break;
                case "col": targetColor = data.ToColor(); break;
                case "tCol": targetTextColor = data.ToColor(); break;
                default: return false;
            }

            return true;
        }

        public override CfgEncoder Encode()  {
            targetSize.z = Mathf.Max(targetSize.z, 1);

            var cody = this.EncodeUnrecognized()
                .Add("sc", targetSize)
                .Add("pos", targetLocalPosition)
                .Add("tCol", targetTextColor);
            if (targetColor != Color.grey)
                cody.Add("col", targetColor);

            return cody;
        }
        #endregion

        #region Inspect
        public override bool Inspect() {

            var changed = false;

            var x = targetSize.x;
            if ("Width".edit(50, ref x, 1f, 35f).nl(ref changed))  
                targetSize.x = x;
            
            var y = targetSize.y;
            if ("Height".edit(50, ref y, 1f, 35f).nl(ref changed)) 
                targetSize.y = y;
            
            "BG Color".edit(60, ref targetColor).nl(ref changed);

            "TXT Color".edit(60, ref targetTextColor).nl(ref changed);

            return changed;
        }

        public bool InspectInList(IList list, int ind, ref int edited) {

            var changed = "col".edit(40, ref targetColor);

            if (icon.Enter.Click())
                edited = ind;

            return changed;
        }
        #endregion
    }

}