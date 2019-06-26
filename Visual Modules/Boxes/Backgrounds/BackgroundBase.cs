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

        public virtual string ClassTag => CfgEncoder.NullTag;
        public static TaggedTypesCfg all = new TaggedTypesCfg(typeof(BackgroundBase));
        public TaggedTypesCfg AllTypes => all;

        #endregion

        public abstract void FadeAway();

        public abstract bool TryFadeIn();

        public abstract void MakeVisible(Base_Node node);

        public abstract void MakeHidden(Base_Node node);

        public abstract void ManagedOnEnable();

        public abstract void OnLogicVersionChange();

        public abstract void SetNode(Node node);

        public abstract void ManagedOnDisable();

        public abstract void OnLogicUpdate();


    }

}