using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EffectLightPoint : MonoBehaviour {

    public static EffectLightPoint[] all = new EffectLightPoint[4];
    public int index = 0;
    public Color col;
    public float multiplier = 1;

	void OnEnable () {
        index = Mathf.Clamp(index, 0,4);
        all[index] = this;
	}

    private void OnDisable() => all[index] = this;
   
}
