using NodeNotes_Visual;
using PlaytimePainter;
using TMPro;
using UnityEngine;

[ExecuteAlways] 
public class DialogueUI_SpeechBox : MonoBehaviour {

    private DialogueUI Mgmt => DialogueUI.instance;

    private NodesVisualLayer VisualMgmt => NodesVisualLayer.Instance;

    public TextMeshProUGUI text = null;

    public RoundedGraphic graphic;

    public RectTransform rectTransform;

    public State state = State.Option;

    public enum State { Option, PlayerText, OtherPersonsText }

    public string Text { set { text.text = value; } }

    public void Update() {

        bool aboveTheLine = state != State.Option;




    }


}
