using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuizCannersUtilities;

[ExecuteInEditMode]
public class ClickToPlaneFollower : MonoBehaviour {

    public Color targetColor;

    public List<Material> materials = new List<Material>();

    public TrailRenderer trail;

    Plane plane = new Plane(Vector3.up, Vector3.zero);

    ShaderProperty.VectorValue clockStrengthProperty = new ShaderProperty.VectorValue("_ClickStrength");

    ShaderProperty.ColorValue colorValueProperty = new ShaderProperty.ColorValue("_Color");
    
    float strength = 0;

	void Update () {

        if (Input.GetMouseButton(0))
        {

            strength = Mathf.Lerp(strength, 1, Time.deltaTime * 20);

            Vector3 hp;
            if (plane.MouseToPlane(out hp))
            {
                transform.position = hp;
            }

        }
        else
        {
            strength = Mathf.Lerp(strength, 0, Time.deltaTime * 5);
            if (trail)
                trail.Clear();
        }

        clockStrengthProperty.GlobalValue = new Vector4(strength, 0,0,0);

        targetColor.a = strength;

        colorValueProperty.lastValue = targetColor;

        foreach (var m in materials)
            colorValueProperty.SetOn(m); 


    }
}
