#if TOOLS

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using Wayfarer.Core.Systems;
using Wayfarer.Core.Utils.Coroutine;
using Wayfarer.Core.Utils.Debug;
using Wayfarer.Core.Utils.Helpers;


namespace Wayfarer.Editor.Explorer
{
    [Tool]
    public class ExplorerPlugin : WayfarerModule
    {
        public EditorInterface EditorInterface => GetEditorInterface();

        private EditorExplorerDock _dock;
        private float _dockDelay = 0.05f;
        
        public override void _EnterTree()
        {
            EnablePlugin();
        }

        public override void _Ready()
        {
            try
            {
                Log.Wf.Editor("ExplorerPlugin Ready, starting editor adding process!", true);
                CallDeferred(nameof(StartProcessToAddContainers));
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Couldn't add custom controls in ExplorerPlugin (_EnterTree)", e, true);
            }

            DisablePlugin();
        }

        public override void _ExitTree()
        {
            RemoveCustomContainers();
            
            try
            {
                RemoveOldExplorer(GetSameControlAsScene());
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Couldn't remove the OLD EditorExplorerDock via TabContainer parent, trying the recursive find...", e, true);

                try
                {
                    RemoveOldExplorer();
                }
                catch (Exception e2)
                {
                    Log.Wf.EditorError("... that didn't work either, failing hard and crashing", e2, true);
                }
            }
        }

        private void StartProcessToAddContainers()
        {
            try
            {
                RemoveOldExplorer(GetSameControlAsScene());
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Couldn't remove the OLD EditorExplorerDock via TabContainer parent, trying the recursive find...", e, true);

                try
                {
                    RemoveOldExplorer();
                }
                catch (Exception e2)
                {
                    Log.Wf.EditorError("... that didn't work either, failing hard and crashing", e2, true);
                }
            }
            
            try
            {
                Node baseControl = EditorInterface.GetBaseControl();
                Godot.Collections.Array children = baseControl.GetChildren();
                Godot.Collections.Array iterators = new Godot.Collections.Array();
                Iterator iterator;

                foreach (Node child in children)
                {
                    if (child is Iterator i)
                    {
                        iterators.Add(i);
                    }
                }
                
                if (iterators.Count > 1)
                {
                    Log.Wf.EditorError("There were MULTIPLE ITERATORS, this shouldn't happen (" + iterators.Count + ")", true);
                    iterator = (Iterator) iterators.Last();

                    for (int i = 0; i < iterators.Count - 1; i++)
                    {
                        Iterator it = (Iterator) iterators[i];
                        try
                        {
                            it.QueueFree();
                        }
                        catch (Exception e)
                        {
                            Log.Wf.EditorError("Couldn't QueueFree the extra iterators...?", e, true);
                        }
                    }
                }
                else
                {
                    iterator = (Iterator) iterators[0];
                }

                if (iterator == null)
                {
                    iterator = baseControl.GetNodeOfType<Iterator>();
                }

                iterator.Name = "EditorIterator";
                
                try
                {
                    if (iterator?.EditorCoroutine != null)
                    {
                        iterator.EditorCoroutine.Run(WaitAndThenAddContainers());
                    }
                    else
                    {
                        Log.Wf.EditorError("The EditorIterator was null!?", true);
                    }
                    
                    Log.Wf.Editor("COROUTINE AMOUNT = " + iterator?.EditorCoroutine?.Count);
                }
                catch (Exception e)
                {
                    Log.Wf.EditorError("Couldn't start an EditorCoroutine... is the EditorCoroutine instantiated yet?", e, true);
                }
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Couldn't find reference to the Iterator... could this be because it's not instantiated yet?", e, true);
            }
        }

        private IEnumerator WaitAndThenAddContainers()
        {
            Log.Wf.Editor("Starting WaitAndThenAddContainers() in ExplorerPlugin");
            bool first = true;
            while (true)
            {
                if (!first)
                {
                    Log.Wf.Editor("Adding the EditorExplorer containers!");
                    AddCustomContainers();
                    break;
                }

                first = false;
                yield return _dockDelay;
            }
        }

        private void AddCustomContainers()
        {
            try
            {
                PackedScene dockScene = GD.Load<PackedScene>("res://Addons/Wayfarer.Editor.Explorer/Assets/Scenes/EditorExplorerDock.tscn");
                _dock = (EditorExplorerDock) dockScene.Instance();
                _dock.SetPlugin(this);
                Control tab = GetSameControlAsScene();
                if (tab != null)
                {
                    tab.AddChild(_dock);
                }
                else
                {
                    Log.Wf.EditorError("Couldn't find the TabContainer that has Scene .... ????", true);
                }

                _dock.Name = "Editor";
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Couldn't add custom controls in ExplorerPlugin, resetting may work", e, true);
            }
        }

        private void RemoveCustomContainers()
        {
            // this proved to be futile and causing crashes - no fire-proof way to retain/get the reference to _dock
            // which is why we use the RemoveOldExplorer() method for this purpose here
        }

        private Control GetExplorerDockParent()
        {
            return null;
        }
        
        private Control GetSameControlAsScene()
        {
            try
            {
                Node[] editorNodes = EditorInterface.GetBaseControl().GetChildrenRecursive();

                foreach (Node node in editorNodes)
                {
                    if (node is TabContainer tabContainer)
                    {
                        foreach (Control child in tabContainer.GetChildren())
                        {
                            if (child.Name == "Scene")
                            {
                                return tabContainer;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Couldn't GetSameControlAsScene (ExplorerPlugin)", e, true);
            }
            
            return null;
        }

        private void RemoveOldExplorer()
        {
            try
            {
                Node[] editorNodes = EditorInterface.GetBaseControl().GetChildrenRecursive();

                foreach (Node node in editorNodes)
                {
                    if (node is EditorExplorerDock)
                    {
                        try
                        {
                            node.QueueFree();
                            Log.Wf.Editor("Removed old EditorExplorerDock (QueueFree)", true);
                        }
                        catch (Exception e)
                        {
                            Log.Wf.EditorError("Tried to QueueFree() EditorExplorerDock, but couldn't", e, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Couldn't Remove old EditorExplorer", e, true);
            }
            
        }

        private void RemoveOldExplorer(Control parent)
        {
            try
            {
                foreach (Node node in parent.GetChildren())
                {
                    if (node is EditorExplorerDock)
                    {
                        try
                        {
                            node.QueueFree();
                            Log.Wf.Editor("Removed old EditorExplorerDock (QueueFree) (with defined parent)", true);
                        }
                        catch (Exception e)
                        {
                            Log.Wf.EditorError("Tried to QueueFree() EditorExplorerDock, but couldn't (with defined parent)", e, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Couldn't Remove old EditorExplorer (with defined parent)", e, true);
            }
        }
    }
}

#endif