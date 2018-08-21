using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;

[ExecuteInEditMode]
public class BackPaintTextureSetter : MonoBehaviour {

    //public Renderer rendy;

    public Texture backPaintTexture;

	// Use this for initialization
	void OnEnable () {
        Shader.SetGlobalTexture("_Nebula_BG", backPaintTexture);

	}
	
	// Update is called once per frame
	void Update () {
        
        Shader.SetGlobalVector("_Nebula_Pos" ,transform.position.ToVector4(transform.localScale.x));

	}
}
