using PoeHUD.Hud.Menu;
using System;
using PoeHUD.Hud.Settings;

namespace Tradie
{
    internal class MenuWrapper
    {

        public static MenuItem AddMenu<T>(MenuItem parent, string menuName, RangeNode<T> setting, string toolTip = "") where T : struct
        {
            var item = new Picker<T>(menuName, setting);
            if (toolTip != "")
                item.TooltipText = toolTip;

            parent.AddChild(item);

            return item;
        }

        public static MenuItem AddMenu(MenuItem parent, string text, HotkeyNode node, string toolTip = "")
        {
            var item = new HotkeyButton(text, node);
            if (toolTip != "")
                item.TooltipText = toolTip;

            parent.AddChild(item);
            return item;
        }

        public static MenuItem AddMenu(MenuItem parent, string text, ButtonNode node, string toolTip = "")
        {
            var item = new ButtonButton(text, node);
            if (toolTip != "")
                item.TooltipText = toolTip;

            parent.AddChild(item);
            return item;
        }

        public static ListButton AddMenu(MenuItem parent, string text, ListNode node, string toolTip = "")
        {
            var item = new ListButton(text, node);
            if (toolTip != "")
                item.TooltipText = toolTip;

            parent.AddChild(item);
            node.SettingsListButton = item;
            return item;
        }

        public static MenuItem AddMenu(MenuItem parent, string text, ToggleNode node, string toolTip = "", string key = null, Func<MenuItem, bool> hide = null)
        {
            var item = new ToggleButton(parent, text, node, key, hide);
            if (toolTip != "")
                item.TooltipText = toolTip;

            parent.AddChild(item);
            return item;
        }

        public static MenuItem AddMenu(MenuItem parent, string text, string toolTip = "")
        {
            var item = new SimpleMenu(parent, text);
            if (toolTip != "")
                item.TooltipText = toolTip;

            parent.AddChild(item);
            return item;
        }

        public static void AddMenu(MenuItem parent, FileNode path, string toolTip = "")
        {
            var item = new FileButton(path);
            if (toolTip != "")
                item.TooltipText = toolTip;

            parent.AddChild(item);
        }

        public static MenuItem AddMenu(MenuItem parent, string text, ColorNode node, string toolTip = "")
        {
            var item = new ColorButton(text, node);
            if (toolTip != "")
                item.TooltipText = toolTip;

            parent.AddChild(item);
            return item;
        }
    }
}
