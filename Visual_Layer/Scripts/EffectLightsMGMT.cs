using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;
using System;

[ExecuteInEditMode]
public class EffectLightsMGMT : MonoBehaviour, IManageFading {

    public List<ParticleSystem> systems = new List<ParticleSystem>();

    public float fadeOutSpeedup = 4;

    [NonSerialized]
    public float[] originalSimulationSpeed;

    public void FadeAway() {

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
    }

    public bool TryFadeIn()  {
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
        return true;
    }
    
	void Update () {
        for (int c = 0; c < 4; c++)
        {
            var ep = EffectLightPoint.all[c];

            if (ep)  {
                Shader.SetGlobalVector("l" + c + "col", ep.col.ToVector4()*ep.multiplier);
                Shader.SetGlobalVector("l" + c + "pos", ep.transform.position.ToVector4(1));
            } 
        }
    }
}
