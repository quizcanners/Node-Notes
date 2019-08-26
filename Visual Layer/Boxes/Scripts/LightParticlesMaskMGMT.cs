using QuizCannersUtilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightParticlesMaskMGMT : MonoBehaviour {

    public Texture2D lightParticlesTexture;
    public Texture2D darkParticlesTexture;

    private readonly ShaderProperty.TextureValue _waterParticlesTextureGlobalLight = new ShaderProperty.TextureValue("_Global_Water_Particles_Mask_L");
    private readonly ShaderProperty.TextureValue _waterParticlesTextureGlobalDark = new ShaderProperty.TextureValue("_Global_Water_Particles_Mask_D");


   

    void OnEnable() {
        _waterParticlesTextureGlobalLight.SetGlobal(lightParticlesTexture);
        _waterParticlesTextureGlobalDark.SetGlobal(darkParticlesTexture);
    }

    #region Custom Time for Shaders

    // Using Unity's _Time will yield large values if used for texture sampling and a resulting pixelation artifacts.
    // Using custom Time instead:

    private static double _taravanaTime;
    private readonly ShaderProperty.FloatValue _shaderTime = new ShaderProperty.FloatValue("_NodeNotes_Time");
    
    private const float resetTimer = 5000f;

    public static void OnViewChange() {
        if (_taravanaTime > resetTimer)
            _taravanaTime = 0;
    }
    
    void Update() {
        _taravanaTime += Time.deltaTime*0.1f;

        if (_taravanaTime > resetTimer * 3)
            _taravanaTime = 0;

        _shaderTime.SetGlobal((float)_taravanaTime);
        
    }

    void OnApplicationPause(bool state)
    {
        _taravanaTime = 0;
    }

    #endregion
}
