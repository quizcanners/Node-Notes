﻿using System;
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
    public class NodesVisualLayer : NodesVisualLayerAbstract {

        public static NodesVisualLayer Instance => inst as NodesVisualLayer;

        [SerializeField] private Camera _mainCam;

        public static Camera MainCam {
            get
            {
                if (!Instance._mainCam)
                    Instance._mainCam = Camera.main;
                return Instance._mainCam;
            }
        }

        public QcUtils.TextureDownloadManager textureDownloader = new QcUtils.TextureDownloadManager();
        
        #region Node MGMT
        
        public override void Show(Base_Node node) => SelectedVisualLayer.MakeVisible(node);
        
        public override void Hide(Base_Node node) => SelectedVisualLayer.MakeHidden(node);

        public override Node CurrentNode {

            get {  return Shortcuts.CurrentNode; }

            set
            {
                SetBackground(value);

                if (Application.isPlaying)
                    value?.SetInspected();

                SelectedVisualLayer.SetNode(value);
            }
        }

        public override void OnLogicVersionChange()
        {
            var cn = Shortcuts.CurrentNode;

            SetBackground(cn);

            SelectedVisualLayer.OnLogicUpdate();

        }

        #endregion

        #region BG MGMT

        public List<BackgroundBase> backgroundControllers = new List<BackgroundBase>();

        public static BackgroundBase _selectedController;

        public static BackgroundBase SelectedVisualLayer {
            get {
                if (!_selectedController)
                    SetBackground(null);

                return _selectedController;
            }

            set { _selectedController = value; }
        }

        public static void SetBackground(Node source)
        {

            var tag = source?.visualStyleTag;

            var bgc = Instance.backgroundControllers;

            if (tag.IsNullOrEmpty() && bgc.Count > 0)
                tag = bgc[0].ClassTag;

            string data = "";

            if (source!= null)
                source.visualStyleConfigs.TryGetValue(tag, out data);

            foreach (var bc in bgc)
                if (bc && bc.ClassTag != tag)
                        bc.FadeAway();
                
            foreach (var bc in bgc)
                if (bc && bc.ClassTag == tag) {
                    _selectedController = bc;
                    bc.TryFadeIn();
                    data.TryDecodeInto(bc);
                    break;
                }

                
        }

        #endregion

        #region Inspector

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
                else
                    return gameNode.Nested_Inspect();
            }
            
            bool changed = base.Inspect();
            
            if (inspectedItems == 2)
                SelectedVisualLayer.Nested_Inspect().changes(ref changed);

            var cn = Shortcuts.CurrentNode;
            
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

                "Playtime UI".toggleIcon(ref Shortcuts.showPlaytimeUI).nl();

                "Backgrounds".edit_Property(() => backgroundControllers, this).nl(ref changed);
            }

            pegi.nl();

            icon.Alpha.enter_Inspect("Textures", textureDownloader, ref inspectedItems, 6).nl_ifNotEntered(ref changed);
            
            if (inspectedItems == -1 && "Encode / Decode Test".Click(ref changed)) {
                OnDisable();
                OnEnable();
            }

            if (changed)
                SelectedVisualLayer.OnLogicUpdate();

            return changed;
        }

        public void OnGUI() {

            if (!Shortcuts.showPlaytimeUI || (Application.isPlaying && !Base_Node.editingNodes))
                return;
            
            window.Render(this);
        }
        
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