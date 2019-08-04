#if TOOLS

using System;
using System.Collections;
using Godot;
using Wayfarer.Core.Systems;
using Wayfarer.Utils.Attributes;
using Wayfarer.Utils.Debug;
using Wayfarer.Utils.Helpers;

namespace Wayfarer.Editor.Explorer
{
    [Tool]
    public class EditorExplorerDock : Control
    {
        [Get("VBox")] private VBoxContainer _main;
        [Get("Highlighter")] private ColorRect _highlighter;
        [Get("VBox/Top/ClearSelection")] private Button _clearSelectionButton;
        [Get("VBox/Top/Popup")] private Button _popupButton;

        private Tree _tree;
        private ExplorerPlugin _plugin;
    
        public override void _Ready()
        {
            this.SetupWayfarer();
            
            try
            {
                _tree = GetEditorTree();
                _tree.Name = "EditorHierarchy";
                _tree.AnchorBottom = 1;
                _tree.AnchorRight = 1;
                _tree.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
                _tree.SizeFlagsVertical = (int)SizeFlags.ExpandFill;
                _tree.SelectMode = Tree.SelectModeEnum.Row;
                _tree.Connect("item_selected", this, nameof(InspectSelection));
                _main.AddChild(_tree);
                Log.Wf.Editor("EditorTree added!", true);

                if (_clearSelectionButton != null)
                {
                    _clearSelectionButton.Connect("button_up", this, nameof(ClearSelection));
                }
                else
                {
                    Log.Wf.EditorError("ClearSelection button was NULL", true);
                }
                if (_popupButton != null)
                {
                    _popupButton.Connect("button_up", this, nameof(PopupInspected));
                    _popupButton.Hide();
                }
                else
                {
                    Log.Wf.EditorError("ClearSelection button was NULL", true);
                }

            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Couldn't add the ExplorerPlugin.GetEditorTree() tree to the dock", e, true);
            }
        }

        public override void _ExitTree()
        {
            Log.Wf.Editor("EditorExplorerDock is being queued free", true);
            try
            {
                Iterator iterator = _plugin.EditorInterface.GetBaseControl().GetNodeOfType<Iterator>();
                iterator.EditorCoroutine.Stop(CheckForChanges());
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Tried to stop EditorCoroutine CheckForChanges() in EditorExplorerDock, but couldn't", true);
                Log.Wf.Simple("THIS IS OK, NO PROBLEM HERE, JUST A SANITY CHECK", true);
            }
        }

        private IEnumerator CheckForChanges()
        {
            while (true)
            {
                
                if (_tree.GetSelected() == _tree.GetRoot())
                {
                    _popupButton.Hide();
                    break;
                }

                yield return 1f;
            }
        }

        private void InspectSelection()
        {
            try
            {
                Node instance = (Node) _tree.GetSelected().GetMeta("instance");
                _plugin.EditorInterface.InspectObject(instance);

                if (instance is Popup popup)
                {
                    _popupButton.Show();
                }
                else if (_popupButton.Visible)
                {
                    _popupButton.Hide();
                }

                bool highlight = true;

                if (highlight)
                {
                    HighlightSelection(instance);
                }
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Couldn't get the instance meta from treeItem", e, true);
            }
            
            _plugin.EditorInterface.InspectObject(_tree.GetSelected());
        }

        private void PopupInspected()
        {
            try
            {
                Node instance = (Node) _tree.GetSelected().GetMeta("instance");
                _plugin.EditorInterface.InspectObject(instance);

                if (instance is Popup popup)
                {
                    popup.Popup_();
                    _popupButton.Hide();
                    _popupButton.SetMeta("curr", popup);
                    popup.Connect("popup_hide", this, nameof(OnPopupHide));
                }
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Tried to Pop up an Editor Popup, but couldn't", e, true);
            }

            try
            {
                Iterator iterator = _plugin.EditorInterface.GetBaseControl().GetNodeOfType<Iterator>();
                iterator.EditorCoroutine.Run(CheckForChanges());
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Tried to start EditorCoroutine CheckForChanges() in EditorExplorerDock, but couldn't", e, true);
            }
        }

        private void OnPopupHide()
        {
            try
            {
                Popup popup = (Popup) _popupButton.GetMeta("curr");
            
                popup.Disconnect("popup_hide", this, nameof(OnPopupHide));
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Tried to disconnect the earlier Popup from the OnPopupHide() method, but couldn't", e, true);
            }

            try
            {
                Node instance = (Node) _tree.GetSelected().GetMeta("instance");

                if (instance is Popup popup2)
                {
                    CallDeferred(nameof(ShowPopupButton));
                }
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Tried to show the PopupButton in case the new selection was a Popup, but couldn't", e, true);
            }
            
            try
            {
                Iterator iterator = _plugin.EditorInterface.GetBaseControl().GetNodeOfType<Iterator>();
                iterator.EditorCoroutine.Stop(CheckForChanges());
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Tried to stop EditorCoroutine CheckForChanges() in EditorExplorerDock, but couldn't", e, true);
            }
        }

        private void ShowPopupButton()
        {
            _popupButton.Show();
        }

        private void HighlightSelection(Node selection)
        {
            try
            {
                if (selection == null)
                {
                    _highlighter.Hide();
                }
                else if (selection is Control control)
                {
                    Rect2 rect = control.GetGlobalRect();

                    _highlighter.RectGlobalPosition = rect.Position;
                    _highlighter.RectSize = rect.Size;
                    _highlighter.Show();
                }
                else
                {
                    _highlighter.Hide();
                }
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Couldn't get the instance meta from treeItem", e, true);
            }
        }
        
        public Tree GetEditorTree()
        {
            try
            {
                Control root = _plugin.EditorInterface.GetBaseControl();
                return GetEditorTree(root);
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Couldn't get the base control of the editor", e, true);
            }

            return null;
        }

        public Tree GetEditorTree(Control rootNode)
        {
            Tree tree = new Tree();
            tree.HideRoot = true;
            int nameColumn = 0;

            try
            {
                TreeItem rootItem = tree.CreateItem();
                rootItem.SetText(nameColumn, "DummyRoot");
                
                TreeItem editorRoot = tree.CreateItem(rootItem);
                editorRoot.SetText(nameColumn, rootNode.Name);
                editorRoot.SetMeta("instance", rootNode);
                
                PopulateTreeItemsFromNodeChildrenRecursive(tree, rootNode, editorRoot, nameColumn);
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Couldn't generate the tree from the root node (" + rootNode.Name + ")", e, true);
            }

            return tree;
        }

        private void PopulateTreeItemsFromNodeChildrenRecursive(Tree tree, Node rootNode, TreeItem rootItem, int nameColumn)
        {
            try
            {
                if (rootNode.GetChildCount() > 0)
                {
                    foreach (Node child in rootNode.GetChildren())
                    {
                        TreeItem item = tree.CreateItem(rootItem);
                        item.SetText(nameColumn, child.Name + "(" + child.GetClass() + ")");
                        item.SetMeta("instance", child);
                        item.Collapsed = true;
                
                        PopulateTreeItemsFromNodeChildrenRecursive(tree, child, item, nameColumn);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Weird stuff man in " + nameof(PopulateTreeItemsFromNodeChildrenRecursive), e, true);
            }
        }

        private void ClearSelection()
        {
            Log.Wf.Editor("Clearing Selection", true);
            _tree.GetRoot().Select(0);
            _highlighter.Hide();
        }

        public void SetPlugin(ExplorerPlugin plugin)
        {
            _plugin = plugin;
        }
    }
}

#endif