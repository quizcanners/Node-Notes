using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuizCannersUtilities;
using PlayerAndEditorGUI;
using System;

namespace NodeNotes_Visual
{

    [TaggedType(classTag)]
    [ExecuteInEditMode]
    public class EffectLightsMGMT : NodesStyleBase, IGotDisplayName {

        const string classTag = "nebula";

        public override string ClassTag => classTag;

        public List<ParticleSystem> systems = new List<ParticleSystem>();

        public BackPaintTextureSetter backgroundPainter;

        public float fadeOutSpeedup = 4;

        public bool isFadingOut = false;

        [NonSerialized]
        public float[] originalSimulationSpeed;

        public override void FadeAway()
        {
            
            if (backgroundPainter)
                backgroundPainter.enabled = false;

            if (originalSimulationSpeed == null || originalSimulationSpeed.Length < systems.Count)
            {
                originalSimulationSpeed = new float[systems.Count];
                for (int i = 0; i < systems.Count; i++)
                {
                    var ps = systems[i];
                    if (ps)
                        originalSimulationSpeed[i] = ps.main.simulationSpeed;
                }
            }

            for (int i = 0; i < systems.Count; i++)
            {
                var ps = systems[i];

                if (ps)
                {
                    var mn = ps.main;
                    mn.simulationSpeed = originalSimulationSpeed[i] * fadeOutSpeedup;

                    var em = ps.emission;

                    em.enabled = false;
                }
            }

            isFadingOut = true;

        }

        public string NameForDisplayPEGI => "Microcosmos";

        public override bool TryFadeIn()
        {

            backgroundPainter.enabled = true;

            for (int i = 0; i < systems.Count; i++)
            {
                var ps = systems[i];
                if (ps)
                {

                    if (originalSimulationSpeed != null && originalSimulationSpeed.Length == systems.Count)
                    {
                        var mn = ps.main;
                        mn.simulationSpeed = originalSimulationSpeed[i];
                    }

                    var em = ps.emission;

                    em.enabled = true;
                }
            }

            isFadingOut = false;

            return true;
        }

        void Update()
        {

            if (!isFadingOut && Camera.main)
            {

                var col = Camera.main.backgroundColor;

                Camera.main.backgroundColor = MyMath.Lerp_bySpeed(col, Color.black, 3);
            }

    
            for (int c = 0; c < 4; c++)
            {
                var ep = EffectLightPoint.all[c];

                if (ep) {
                    cols_Properties[c].GlobalValue = ep.col.ToVector4() * ep.multiplier;
                    pos_Properties[c].GlobalValue = ep.transform.position.ToVector4(1);
                }
            }
        }

        static List<ShaderProperty.VectorValue> cols_Properties;
        static List<ShaderProperty.VectorValue> pos_Properties;

        void OnEnable()
        {
            if (cols_Properties.IsNullOrEmpty())
            {
                cols_Properties = new List<ShaderProperty.VectorValue>();
                pos_Properties = new List<ShaderProperty.VectorValue>();
                for (int i = 0; i < 4; i++)
                {
                    cols_Properties.Add(new ShaderProperty.VectorValue("l" + i + "col"));
                    pos_Properties.Add(new ShaderProperty.VectorValue("l" + i + "pos"));
                }
            }
        }

     
    }
}