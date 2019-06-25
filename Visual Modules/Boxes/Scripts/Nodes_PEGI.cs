using System;
using System.Collections.Generic;
using UnityEngine;
using QuizCannersUtilities;
using PlayerAndEditorGUI;
using UnityEngine.UI;
using TMPro;
using NodeNotes;
using PlaytimePainter;


namespace NodeNotes_Visual
{


#pragma warning disable IDE0018 // Inline variable declaration

    [ExecuteInEditMode]
    public class Nodes_PEGI : NodesVisualLayerAbstract {

        public static Nodes_PEGI Instance => inst as Nodes_PEGI;

        public QcUtils.TextureDownloadManager textureDownloader = new QcUtils.TextureDownloadManager();
        
        #region Node MGMT
        
        public override void Show(Base_Node node) => selectedController.MakeVisible(node);
        
        public override void Hide(Base_Node node) => selectedController.MakeHidden(node);

       // private readonly LoopLock _loopLock = new LoopLock();

        public override Node CurrentNode {

            get {  return Shortcuts.CurrentNode; }

            set
            {
                SetBackground(value);
                selectedController.CurrentNode = value;
            }
        }

        public override void OnLogicVersionChange()
        {
            var cn = Shortcuts.CurrentNode;

            SetBackground(cn);

            selectedController.UpdateCurrentNodeGroupVisibilityAround(cn);

        }

        #endregion

        #region BG

        public List<BackgroundBase> backgroundControllers = new List<BackgroundBase>();

        public static BackgroundBase selectedController;

        public static void SetBackground(Node source)
        {

            var tag = source.visualStyleTag;

            var bgc = Instance.backgroundControllers;

            if (tag.Length == 0 && bgc.Count > 0)
                tag = bgc[0].ClassTag;

            string data = "";

            source.visualStyleConfigs.TryGetValue(tag, out data);

            foreach (var bc in bgc)
                if (bc != null)
                {
                    if (bc.ClassTag == tag)
                    {
                        selectedController = bc;
                        bc.TryFadeIn();
                        data.TryDecodeInto(bc);

                    }
                    else
                        bc.FadeAway();
                }
        }

        #endregion

        #region Inspector
#if !NO_PEGI
        pegi.WindowPositionData_PEGI_GUI window = new pegi.WindowPositionData_PEGI_GUI();

        protected override void ResetInspector()
        {
            shortcuts?.ResetInspector();
            base.ResetInspector();
        }

        protected override void InspectionTabs()
        {
            var cn = Shortcuts.CurrentNode;

            if (cn != null) {
                icon.Active.toggle("{0} -> [{1}] Current: {2} - {3}"
                    .F(Shortcuts.user.startingPoint, Shortcuts.user.bookMarks.Count, cn.root.GetNameForInspector(), cn.GetNameForInspector())
                    , ref inspectedItems, 2);
            }
            else icon.InActive.write("No Active Node");

            icon.Book.toggle("Node Books", ref inspectedItems, 4);

            base.InspectionTabs();
        }

        public override bool Inspect() {

            if (gameNode != null) {

                if (icon.Save.Click("Exit Game Node & Save")) {
                    FromGameToNode();
                    return true;
                }

                "GN: {0}".F(gameNode.GetNameForInspector()).write();

                if (icon.Close.Click("Exit Game Node in Fail").nl())
                    FromGameToNode(true);
                else return gameNode.Nested_Inspect();
            }

            selectedController.Nested_Inspect();
            
            bool changed = base.Inspect();

            var cn = Shortcuts.CurrentNode;

            if (cn!= null && inspectedItems ==2)
                cn.Nested_Inspect(ref changed);

            pegi.nl();

                if (!shortcuts)
                    "Shortcuts".edit(ref shortcuts).nl(ref changed);
                else
                   if (inspectedItems == 4)
                    shortcuts.Nested_Inspect().changes(ref changed);
                else
                    pegi.nl();

            if (icon.Create.enter("Dependencies", ref inspectedItems, 5)) {
                pegi.nl();

                "Backgrounds".edit_Property(() => backgroundControllers, this).nl(ref changed);
    

              
            }

            pegi.nl();

            icon.Alpha.enter_Inspect("Textures", textureDownloader, ref inspectedItems, 6).nl_ifNotEntered(ref changed);

            if (inspectedItems == -1 && "Encode / Decode Test".Click(ref changed)) {
                OnDisable();
                OnEnable();
            }

            return changed;
        }

        public void OnGUI() {
            if (Application.isPlaying && !Base_Node.editingNodes)
                return;
              //  window.Render(selectedNode);
           // else 
                window.Render(this);
        }

#endif
        #endregion

        protected override void DerivedUpdate() {

            base.DerivedUpdate();

            if (Input.GetKey(KeyCode.Escape)) {
                OnDisable();
                Application.Quit();
                Debug.Log("Quit click");
            }
        }

        protected override void OnDisable() {

            base.OnDisable();

            foreach (var bg in backgroundControllers)
                bg.ManagedOnDisable();

            textureDownloader.Dispose();

        }

        public override void OnEnable()
        {
            Shortcuts.visualLayer = this;

            base.OnEnable();
            
            shortcuts.LoadAll();

          
            foreach (var bg in backgroundControllers)
                bg.ManagedOnEnable();

        }

        public override bool InspectBackgroundTag(Node node) {

            var changed = false;

            if (BackgroundBase.all.selectTypeTag(ref node.visualStyleTag).nl(ref changed))
                SetBackground(node);

            return changed;
        }
    }
}
