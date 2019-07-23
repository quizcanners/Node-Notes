using NodeNotes_Visual;
using PlaytimePainter;
using QuizCannersUtilities;
using TMPro;
using UnityEngine;

[ExecuteAlways] 
public class DialogueUI_SpeechBox : MonoBehaviour, ILinkedLerping {

    private DialogueUI Mgmt => DialogueUI.instance;

    private NodesVisualLayer VisualMgmt => NodesVisualLayer.Instance;

    public TextMeshProUGUI text = null;

    public RoundedGraphic graphic;

    public RectTransform rectTransform;
    
    public string Text { set { text.text = value; } }

    public int index;

    public void Click() => DialogueUI.instance.Click(index);
        
    public bool isFirst = false;

    public bool isLast = false;

    public bool deformingFinished = false;

    public void Update() {

        if (!deformingFinished) {

            float upper = graphic.GetCorner(true, false);
            float lover = graphic.GetCorner(false, false);

        }

    }

    LinkedLerp.FloatValue upperEdge = new LinkedLerp.FloatValue();
    LinkedLerp.FloatValue loverEdge = new LinkedLerp.FloatValue();

    public void Portion(LerpData ld) {

        upperEdge.targetValue = isFirst ? 0 : 1;
        loverEdge.targetValue = isLast ? 0 : 1;

        upperEdge.Portion(ld);
        loverEdge.Portion(ld);

    }

    public void Lerp(LerpData ld, bool canSkipLerp) {

        upperEdge.Lerp(ld);
        loverEdge.Lerp(ld);

        graphic.SetCorner(1, upperEdge.CurrentValue);
        graphic.SetCorner(2, upperEdge.CurrentValue);

        graphic.SetCorner(0, loverEdge.CurrentValue);
        graphic.SetCorner(3, loverEdge.CurrentValue);

    }
}
