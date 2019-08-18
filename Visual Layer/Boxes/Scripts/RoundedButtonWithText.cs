using System.Collections;
using System.Collections.Generic;
using PlaytimePainter;
using TMPro;
using UnityEngine;

public class RoundedButtonWithText : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public RoundedGraphic graphic;

    public string Text {
        set { textMeshPro.text = value; }
    }


}
