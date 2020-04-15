using NodeNotes;
using PlayerAndEditorGUI;
using QuizCannersUtilities;
using UnityEngine;

namespace NodeNotes_Visual {

    public abstract class PresentationMode : ComponentCfg, IManageFading, IPEGI, IGotClassTag
    {
        #region Tagged Types MGMT
        public abstract string ClassTag { get;  }
        public static TaggedTypesCfg all = new TaggedTypesCfg(typeof(PresentationMode));
        #endregion

        public static Camera MainCamera => NodesVisualLayer.MainCam;
        
        public abstract void FadeAway();

        public abstract bool TryFadeIn();

        public abstract void MakeVisible(Base_Node node);

        public abstract void MakeHidden(Base_Node node);

        public abstract void ManagedOnInitialize();

        public abstract void ManagedOnDeInitialize();
        
        public abstract void OnLogicUpdate();
        
        public abstract void SetNode(Node node);
        
        public virtual CfgEncoder EncodePerBookData() => new CfgEncoder();

    }

}