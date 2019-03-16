using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuizCannersUtilities;
using PlayerAndEditorGUI;

namespace NodeNotes_Visual {

    public class NodeStylesAttribute : AbstractWithTaggedTypes { public override TaggedTypesCfg TaggedTypes => NodesStyleBase.all; }

    [NodeStyles]
    public abstract class NodesStyleBase : ComponentCfg, IManageFading, IPEGI, IGotClassTag {

        #region Tagged Types MGMT
        public virtual string ClassTag => CfgEncoder.NullTag;
        public static TaggedTypesCfg all = new TaggedTypesCfg(typeof(NodesStyleBase));
        public TaggedTypesCfg AllTypes => all;
        #endregion

        public Color fallbackColor = Color.black;

        public abstract void FadeAway();

        public abstract bool TryFadeIn();
    }
}