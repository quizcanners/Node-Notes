using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControllerBase : MonoBehaviour {


    public virtual void Initialize()
    {
        gameObject.SetActive(false);

    }
   
}
