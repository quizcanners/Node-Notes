using System.Collections;
using System.Collections.Generic;
using PlaytimePainter;
using QuizCannersUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoundedButtonWithText : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public RoundedGraphic graphic;
    [SerializeField] protected Graphic highlight;

    public string Text {
        set { textMeshPro.text = value; }
    }

    public void Update() {
        
        if (highlight) {

            float a = highlight.color.a;

            if (LerpUtils.IsLerpingBySpeed(ref a, graphic.ClickPossible ? 1 : 0, 8))
                highlight.TrySetAlpha_DisableIfZero(a);
            
        }
 
    }
    



}
