using System;
using Godot;
using Godot.Collections;
using Wayfarer.ModuleSystem;
using Wayfarer.Utils.Debug;
using Array = Godot.Collections.Array;

#if TOOLS
namespace Wayfarer.Editor.Explorer
{
    [Tool]
#endif
    public class ExplorerHelp : Node
    {
        public override Array _GetPropertyList()
        {
            Array list = new Array();

            Dictionary header = new Dictionary
            {
                {"name", "Welcome"},
                {"type", Variant.Type.Nil},
                {"usage", PropertyUsageFlags.Category}
            };
            list.Add(header);

            Dictionary title = new Dictionary
            {
                {"name", "textbox"},
                {"type", Variant.Type.Nil},
                {"hint_string", "This is the Editor Explorer"}
            };
            list.Add(title);

            Dictionary usageDesc = new Dictionary
            {
                {"name", "usage/textbox"},
                {"type", Variant.Type.Nil},
                {
                    "hint_string", "You may freely browse any of the Editor's elements " +
                                   "and inspect them simply by selecting them on the tree. "
                }
            };
            list.Add(usageDesc);
        
            Dictionary usageDescNote = new Dictionary
            {
                {"name", "usage/note/textbox"},
                {"type", Variant.Type.Nil},
                {
                    "hint_string", "Please note, however, that the changes you make via the Explorer " +
                                   "are not persistent. You can use it for testing, but any changes " +
                                   "will be lost whenever the editor is closed. " +
                                   "For any permanent modifications, create a plugin."
                }
            };
            list.Add(usageDescNote);

            Dictionary updatingDesc = new Dictionary
            {
                {"name", "updating/textbox"},
                {"type", Variant.Type.Nil},
                {
                    "hint_string", "You can easily update the tree by clicking the update icon on the " +
                                   "top-left corner of the dock. Sometimes Godot changes its hierarchy, " +
                                   "so it is often worth the effort to update the tree if you are about to do something crucial. " +
                                   "You can also " +
                                   "enable automatic updating via the Wayfarer User Settings. (Note: has performance implications)"
                }
            };
            list.Add(updatingDesc);

            Dictionary highlighterDesc = new Dictionary
            {
                {"name", "highlighter/textbox"},
                {"type", Variant.Type.Nil},
                {
                    "hint_string", "By default, the highlighting option is enabled. " +
                                   "You can change this via the Wayfarer User Settings."
                }
            };
            list.Add(highlighterDesc);
        
            Dictionary highlighterState = new Dictionary
            {
                {"name", "highlighter/current_state"},
                {"type", Variant.Type.Bool}
            };
            list.Add(highlighterState);

            Dictionary popupDesc = new Dictionary
            {
                {"name", "popups/textbox"},
                {"type", Variant.Type.Nil},
                {
                    "hint_string", "You can preview any popup windows by clicking the contextually " +
                                   "appearing \"Popup\" button. Whenever you select a window that is hidden, " +
                                   "but popuppable, the dock will automatically show you the Popup button."
                }
            };
            list.Add(popupDesc);

            return list;
        }

        public override object _Get(string property)
        {
            try
            {
                if (property == "highlighter/current_state")
                {
                    Log.Wf.Print("RETURNING CURRENT_STATE (" + (bool)WayfarerProjectSettings.Get(ExplorerPlugin.SettingPathEnableHighlighter) + ")", true);
                    return WayfarerProjectSettings.Get(ExplorerPlugin.SettingPathEnableHighlighter);
                }
            }
            catch (Exception e)
            {
                Log.Wf.EditorError("Chouldn't load hightlighter/current_state from WayfarerProjectSettings", e, true);
            }
        
        
            return base._Get(property);
        }
    }
}
