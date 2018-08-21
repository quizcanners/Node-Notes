using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTools_Stuff;

[ExecuteInEditMode]
public class ClickToPlaneFollower : MonoBehaviour {

   // List<Material> painters = new List<Material>();

    Plane plane = new Plane(Vector3.up, Vector3.zero);

	// Use this for initialization
	void Start () {
		
	}

    float strength = 0;
	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButton(0))
        {

            strength = Mathf.Lerp(strength, 1, Time.deltaTime * 10);

            Vector3 hp;
            if (plane.MouseToPlane(out hp))
            {
                transform.position = hp;
            }

        }
        else
            strength = Mathf.Lerp(strength, 0, Time.deltaTime);

        Shader.SetGlobalVector("_ClickStrength", new Vector4(strength, 0,0,0));

	}
}
