using QuizCannersUtilities;
using UnityEngine;

[ExecuteInEditMode]
public class LightParticlesMaskMGMT : MonoBehaviour {

    public Texture2D lightParticlesTexture;
    public Texture2D darkParticlesTexture;
    public Texture2D spiralMask;

    private readonly ShaderProperty.TextureValue _waterParticlesTextureGlobalLight = new ShaderProperty.TextureValue("_Global_Water_Particles_Mask_L");
    private readonly ShaderProperty.TextureValue _waterParticlesTextureGlobalDark = new ShaderProperty.TextureValue("_Global_Water_Particles_Mask_D");
    private readonly ShaderProperty.VectorValue _mousePosition = new ShaderProperty.VectorValue("_NodeNotes_MousePosition");
    private readonly ShaderProperty.VectorValue _mousePositionPrev = new ShaderProperty.VectorValue("_NodeNotes_MousePositionPrev");
    private readonly ShaderProperty.VectorValue _mousePressDerrived = new ShaderProperty.VectorValue("_NodeNotes_MouseDerrived");
    private readonly ShaderProperty.TextureValue _spiralMask = new ShaderProperty.TextureValue("_NodeNotes_SpiralMask");

    private float _mouseDownStrength;
    private float _mouseDownStrengthOneDirectional;
    private bool downClickFullyShown = true;
    private Vector2 _mouseDownPosition;

    void UpdatemousePosition()
    {
        _mousePositionPrev.GlobalValue = _mousePosition.GlobalValue;
        _mousePosition.GlobalValue = _mouseDownPosition.ToVector4(_mouseDownStrength, ((float)Screen.width) / Screen.height);
        _mousePressDerrived.GlobalValue = new Vector4(_mouseDownStrengthOneDirectional, 0 ,0 ,0 );
    }

    void Update() {

        _taravanaTime += Time.deltaTime * 0.1f;

        if (_taravanaTime > resetTimer * 3)
            _taravanaTime = 0;

        _shaderTime.SetGlobal((float)_taravanaTime);

        bool down = Input.GetMouseButton(0);

        if (down || _mouseDownStrength > 0){

            bool downThisFrame = Input.GetMouseButtonDown(0);

            if (downThisFrame) {
                _mouseDownStrength = 0;
                _mouseDownStrengthOneDirectional = 0;
                downClickFullyShown = false;
            }

            _mouseDownStrengthOneDirectional = LerpUtils.LerpBySpeed(_mouseDownStrengthOneDirectional,
                down ? 0 : 1
                , down ? 4f : (3f - _mouseDownStrengthOneDirectional * 3f));

            _mouseDownStrength = LerpUtils.LerpBySpeed(_mouseDownStrength, downClickFullyShown ?
                0 : (down ? 0.9f : 1f)
                , (down) ? 5 : (downClickFullyShown ? 0.75f : 2.5f));

            if (_mouseDownStrength > 0.99f)
                downClickFullyShown = true;

            if (down)
            {
                var newPosition = Input.mousePosition.XY() / new Vector2(Screen.width, Screen.height);
                _mouseDownPosition = newPosition;//LerpUtils.LerpBySpeed(_mouseDownPosition, newPosition, 4) ;
            }

            UpdatemousePosition();
        }
    }

    void OnEnable() {
        _waterParticlesTextureGlobalLight.GlobalValue = lightParticlesTexture;
        _waterParticlesTextureGlobalDark.GlobalValue = darkParticlesTexture;
        _spiralMask.GlobalValue = spiralMask;
        UpdatemousePosition();
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
    
    void OnApplicationPause(bool state)
    {
        _taravanaTime = 0;
    }

    #endregion
}
