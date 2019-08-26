using QuizCannersUtilities;
using PlayerAndEditorGUI;
using NodeNotes;
using PlaytimePainter;
using UnityEngine;

namespace NodeNotes_Visual {

    public class BackgroundsAttribute : AbstractWithTaggedTypes { public override TaggedTypesCfg TaggedTypes => BackgroundBase.all; }

    [Backgrounds]
    public abstract class BackgroundBase : ComponentCfg, INodeNotesVisualStyle, IManageFading, IPEGI, IGotClassTag
    {

        #region Tagged Types MGMT
        public abstract string ClassTag { get;  }
        public static TaggedTypesCfg all = new TaggedTypesCfg(typeof(BackgroundBase));
        public TaggedTypesCfg AllTypes => all;
        #endregion

        public static Camera MainCamera => NodesVisualLayer.MainCam;
        
        public abstract void FadeAway();

        public abstract bool TryFadeIn();

        public abstract void MakeVisible(Base_Node node);

        public abstract void MakeHidden(Base_Node node);

        public abstract void ManagedOnEnable();

        public abstract void OnLogicUpdate();
        
        public abstract void SetNode(Node node);

        public abstract void ManagedOnDisable();
        
        public virtual CfgEncoder EncodePerBookData() => new CfgEncoder();

    }

}