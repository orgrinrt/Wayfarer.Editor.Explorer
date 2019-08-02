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
        [Get("VBox")] private VBoxContainer _main;

        private Tree _tree;
        private ExplorerPlugin _plugin;
    
        public override void _Ready()
        {
            this.SetupWayfarer();
            
            try
            {
                _tree = _plugin.GetEditorTree();
                _tree.Name = "EditorHierarchy";
                _tree.AnchorBottom = 1;
                _tree.AnchorRight = 1;
                _tree.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
                _tree.SizeFlagsVertical = (int)SizeFlags.ExpandFill;
                _main.AddChild(_tree);
                Log.Wf.Editor("EditorTree added!", true);
                
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Couldn't add the ExplorerPlugin.GetEditorTree() tree to the dock", e, true);
            }
        }

        public override void _ExitTree()
        {
            Log.Wf.Editor("EditorExplorerDock is being queued free", true);
        }

        public void SetPlugin(ExplorerPlugin plugin)
        {
            _plugin = plugin;
        }
    }
}

#endif