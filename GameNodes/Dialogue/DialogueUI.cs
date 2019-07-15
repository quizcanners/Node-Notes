using System;
using UnityEngine;

[ExecuteInEditMode]
public class DialogueUI : MonoBehaviour
{

    public static DialogueUI instance;

    public float separator = 0.4f;


    void Awake() {
        instance = this;
    }

}
