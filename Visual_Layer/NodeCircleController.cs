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

        public void SetNewText(string txt)
        {
            if (fadePortion < 0.1f)
            {
                newText = null;
                activeTextAlpha = 1;
                ActiveText.text = txt;
                PassiveText.text = "";
                UpdateShaders();
            }
            else
                newText = txt;
            
        }

        [NonSerialized] public bool isFading;
        [NonSerialized] public float fadePortion = 0;
        [NonSerialized] public bool assumedPosition;

        float targetSize;
        Vector3 targetPosition;
        Color targetColor;
    
        public override bool PEGI() {
            bool changed = false;

            if (textA == null)
                "Text A".edit(ref textA);
            else
            if (textB == null)
                "Text B".edit(ref textB);
            else 
            if (!circleRendy)
                "Mesh Rendy".edit(ref circleRendy);
            else
            {

                if (newText != null)
                    "Changeing text to {0}".F(newText).nl();

                if (isFading)
                    "Fading...{0}".F(fadePortion).nl();

                if (source != null)
                    changed |= "Name ".edit(ref source.name).nl();

                if ("Color".edit(ref targetColor).nl())
                {
                    changed = true;
                    currentColor = targetColor;
                    UpdateShaders();
                }
            }
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
                 if (Application.isPlaying)
                    circleRendy.material.SetColor("_Color", currentColor); 
                 else
                    circleRendy.sharedMaterial.SetColor("_Color", currentColor);
            }
        }

        void Update()
        {

            bool needShaderUpdate = false;

          

            float portion = 1;

            if (!assumedPosition)  {

                portion = Mathf.Min(5f.SpeedToPortion((transform.localPosition - targetPosition).magnitude), portion);

                portion = Mathf.Min(3f.SpeedToPortion(targetColor.DistanceRGB(currentColor)), portion);

                var scale = transform.localScale.x;
                portion = Mathf.Min(2f.SpeedToPortion(scale -  targetSize), portion);

                portion = Mathf.Min(1f.SpeedToPortion(fadePortion - (isFading ? 0f : 1f)), portion);

                portion = Mathf.Min(4f.SpeedToPortion(1-activeTextAlpha), portion);

                transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, portion);
                scale = Mathf.Lerp(scale, targetSize, portion);
                currentColor = Color.Lerp(currentColor, targetColor, portion);
                fadePortion = Mathf.Lerp(fadePortion, isFading ? 0 : 1, portion);

                needShaderUpdate = true;
                if (portion == 1)
                    assumedPosition = true;
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
                    activeTextAlpha = 0;
                    newText = null;
                }

                needShaderUpdate = true;
            }
            
            if (needShaderUpdate)
                UpdateShaders();

            if (fadePortion == 0 && isFading)
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
                case "s": targetSize = data.ToFloat(); break;
                case "pos": targetPosition = data.ToVector3(); break;
                case "col": targetColor = data.ToColor(); break;
                default: return false;
            }

            return true;
        }

        public override StdEncoder Encode()
        {
            var cody = this.EncodeUnrecognized()
            .Add("s", assumedPosition ? transform.localScale.x : targetSize)
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