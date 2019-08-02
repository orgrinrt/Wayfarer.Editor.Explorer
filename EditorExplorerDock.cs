#if TOOLS

using System;
using Godot;
using Wayfarer.Core.Utils.Attributes;
using Wayfarer.Core.Utils.Debug;
using Wayfarer.Core.Utils.Helpers;

namespace Wayfarer.Editor.Explorer
{
    [Tool]
    public class EditorExplorerDock : Control
    {
        //[Get("VBox")] private VBoxContainer _main;

        //private Tree _tree;
    
        public override void _Ready()
        {
            //this.SetupWayfarer();
            /*
            try
            {
                _tree = _plugin.GetEditorTree();
                _tree = new Tree();
                _tree.Name = "EditorHierarchy";
                _main.AddChild(_tree);
                
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Couldn't add the ExplorerPlugin.GetEditorTree() tree to the dock", e, true);
            }*/
        }
/*
        public override void _ExitTree()
        {
            Log.Wf.Editor("EditorExplorerDock is being queued free", true);
        }*/
    }
}

#endif