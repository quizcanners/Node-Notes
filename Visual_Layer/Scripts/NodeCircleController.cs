using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using SharedTools_Stuff;
using PlayerAndEditorGUI;
using System;

namespace LinkedNotes {

    [ExecuteInEditMode]
    public class NodeCircleController : ComponentSTD, IPEGI
    {

        const float movementSpeed = 0.5f;
        const float scalingSpeed = 0.5f;

        public TextMeshPro textA;

        public TextMeshPro textB;

        bool activeTextIsA = false;

        TextMeshPro ActiveText => activeTextIsA ? textA : textB;

        TextMeshPro PassiveText => activeTextIsA ? textB : textA;

        float activeTextAlpha = 0;

        public Renderer circleRendy;

        public Color currentColor;

        public Base_Node source;

        public string newText = null;

        public float courners = 0.5f;

        public void SetNewText(string txt)
        {
            if (fadePortion < 0.1f)
            {
                newText = null;
                activeTextAlpha = 1;
                ActiveText.text = txt;
                gameObject.name = txt;
                PassiveText.text = "";
                UpdateShaders();
            }
            else
                newText = txt;
            
        }

        [NonSerialized] public bool isFading;
        [NonSerialized] public float fadePortion = 0;
        [NonSerialized] public bool assumedPosition;

        Vector3 targetSize;
        Vector3 targetPosition;
        Color targetColor;
        Vector4 square = Vector4.zero;
        

        bool showDependencies = false;
        public override bool PEGI() {
            bool changed = false;


            if (circleRendy)
            {
                if (newText != null)
                    "Changeing text to {0}".F(newText).nl();

                if (isFading)
                    "Fading...{0}".F(fadePortion).nl();

                if (source != null)
                    changed |= "Name ".edit(ref source.name).nl();

                float x = targetSize.x;
                if ("Width".edit(50, ref x, 1f, 5f).nl())
                {
                    assumedPosition = false;
                    targetSize.x = x;
                }

                float y = targetSize.y;
                if ("Height".edit(50, ref y, 1f, 5f).nl())
                {
                    assumedPosition = false;
                    targetSize.y = y;
                }

                if ("Color".edit(ref targetColor))
                {
                    assumedPosition = false;
                    changed = true;
                    currentColor = targetColor;
                    UpdateShaders();
                }

                if (isFading && icon.Play.Click().nl())
                    isFading = false;
                if (!isFading && icon.Pause.Click().nl())
                    isFading = true;
            }
                
            "Dependencies".foldout(ref showDependencies).nl();

            if (!textA || showDependencies)
                "Text A".edit(ref textA);
         
          if (!textB || showDependencies)
                "Text B".edit(ref textB);
            
          if (!circleRendy || showDependencies)
                "Mesh Rendy".edit(ref circleRendy);
            
                return changed;
        }
        
        void UpdateShaders() {

            if (textB && textA)
            {
                ActiveText.color = new Color(0, 0, 0, activeTextAlpha * fadePortion);
                PassiveText.color = new Color(0, 0, 0, (1 - activeTextAlpha) * fadePortion);
            }

            currentColor.a = fadePortion;

            if (circleRendy)
            {
                if (circleRendy)
                {
                    if (Application.isPlaying)
                    {
                        circleRendy.material.SetColor("_Color", currentColor);
                        circleRendy.material.SetVector("_Stretch", square);
                        circleRendy.material.SetFloat("_Courners", courners);
                    }
                    else
                    {
                        circleRendy.sharedMaterial.SetColor("_Color", currentColor);
                        circleRendy.sharedMaterial.SetVector("_Stretch", square);
                        circleRendy.sharedMaterial.SetFloat("_Courners", courners);
                    }
                }
            }
        }

        void Update() {

            bool needShaderUpdate = false;
            
            float portion = 1;

            if (!assumedPosition)  {
                
                10f.SpeedToMinPortion((transform.localPosition - targetPosition).magnitude, ref portion);

                9f.SpeedToMinPortion(targetColor.DistanceRGB(currentColor), ref portion);

                var BGtf = circleRendy.transform;

                var scale = BGtf.localScale;
                8f.SpeedToMinPortion((scale - targetSize).magnitude , ref portion);

                2f.SpeedToMinPortion(fadePortion - (isFading ? 0f : 1f), ref portion);

                4f.SpeedToMinPortion(1-activeTextAlpha, ref portion);

                float targetCourners = source == Nodes_PEGI.CurrentNode ? 0 : 0.9f;
            
                4f.SpeedToMinPortion(Mathf.Abs(targetCourners - courners), ref portion);

                transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, portion);
                scale = Vector3.Lerp(scale, targetSize, portion);
                BGtf.localScale = scale;
                currentColor = Color.Lerp(currentColor, targetColor, portion);
                fadePortion = Mathf.Lerp(fadePortion, isFading ? 0 : 1, portion);
                courners = Mathf.Lerp(courners, targetCourners, portion);

                needShaderUpdate = true;
                if (portion == 1)
                    assumedPosition = true;
                
                if (scale.x > 0)
                    square.x = scale.x > scale.y ? ((scale.x - scale.y) / scale.x) : 0;
                else square.x = 0;
                if (scale.y > 0)
                    square.y = scale.y > scale.x ? ((scale.y - scale.x) / scale.y) : 0;
                else square.y = 0;
                
                var textSize = new Vector2(17 + scale.x * 3, 5f + Mathf.Max(0, (scale.y - 1f) * 3f));

                textA.rectTransform.sizeDelta = textSize;
                textB.rectTransform.sizeDelta = textSize;

            }
            else {
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

                if (activeTextAlpha == 1 && newText != null)
                {
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

        public void OnMouseOver() {

            if (source != null)
                source.OnMouseOver();
        }

        public override ISTD Decode(string data)
        {
            assumedPosition = false;
            return base.Decode(data);
        }

        public override bool Decode(string tag, string data)
        {
            switch (tag)
            {
                case "s": targetSize = data.ToFloat() * Vector3.one; break;
                case "sc": targetSize = data.ToVector3(); break;
                case "pos": targetPosition = data.ToVector3(); break;
                case "col": targetColor = data.ToColor(); break;
                default: return false;
            }

            return true;
        }

        public override StdEncoder Encode()
        {
            var cody = this.EncodeUnrecognized()
            .Add("sc", assumedPosition ? circleRendy.transform.localScale : targetSize)
            .Add("pos", assumedPosition ? transform.localPosition : targetPosition)
            .Add("col", targetColor);

            return cody;
        }

        public void LinkTo(Base_Node node)
        {
            source = node;
            if (source.visualRepresentation != null)
                Debug.LogError("Visual representation is not null",this);
            source.visualRepresentation = this;
            Decode(source.configForVisualRepresentation);
            SetNewText(source.name);
            isFading = false;
            gameObject.SetActive(true);
        }

        public void Unlink()
        {
            if (source != null)
            {
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
}