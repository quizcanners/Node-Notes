using NodeNotes_Visual;
using PlaytimePainter;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class DialogueUI_SpeechBox : MonoBehaviour {

    private DialogueUI Mgmt => DialogueUI.instance;

    private NodesVisualLayer VisualMgmt => NodesVisualLayer.Instance;

    public TextMeshProUGUI text;
    public RoundedGraphic graphic;

    public State state = State.Option;

    public enum State { Option, PlayerText, OtherPersonsText }

    public void Update() {

        bool aboveTheLine = state != State.Option;




    }


}
