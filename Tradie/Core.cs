using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoeHUD.Plugins;
using PoeHUD.Poe;
using PoeHUD.Poe.Elements;
using SharpDX;

namespace Tradie
{
    public class Core : BaseSettingsPlugin<Settings>
    {
        public Core()
        {
            PluginName = "Tradie";
        }

        public override void Render()
        {

            var tradingWindow = GetTradingWindow();
            if (tradingWindow == null || !tradingWindow.IsVisible)
            {
                return;
            }

            var tradingItems = GetItemsInTradingWindow(tradingWindow);

            foreach (var theirItem in tradingItems.theirItems)
            {
                Graphics.DrawFrame(theirItem.GetClientRect(), 2, Color.Red);
            }

            foreach (var ourItem in tradingItems.ourItems)
            {
                Graphics.DrawFrame(ourItem.GetClientRect(), 2, Color.Blue);
            }
        }

        private Element GetTradingWindow()
        {
            try
            {
                return GameController.Game.IngameState.UIRoot
                    .Children[1]
                    .Children[48]
                    .Children[3]
                    .Children[1]
                    .Children[0]
                    .Children[0];
            }
            catch
            {
                return null;
            }
        }

        private (List<NormalInventoryItem> ourItems, List<NormalInventoryItem> theirItems) GetItemsInTradingWindow(Element tradingWindow)
        {
            var ourItemsElement = tradingWindow.Children[0];
            var theirItemsElement = tradingWindow.Children[1];

            var ourItems = new List<NormalInventoryItem>();
            var theirItems = new List<NormalInventoryItem>();

            // We are skipping the first, since it's a invisible uiElement that we don't need.
            foreach (var ourElement in ourItemsElement.Children.Skip(1))
            {
                var normalInventoryItem = ourElement.AsObject<NormalInventoryItem>();
                if (normalInventoryItem == null)
                {
                    LogMessage("OurItem was null!", 5);
                    throw new Exception("OurItem was null!");
                }

                ourItems.Add(normalInventoryItem);
            }

            foreach (var theirElement in theirItemsElement.Children)
            {
                var normalInventoryItem = theirElement.AsObject<NormalInventoryItem>();
                if (normalInventoryItem == null)
                {
                    LogMessage("OurItem was null!", 5);
                    throw new Exception("OurItem was null!");
                }

                theirItems.Add(normalInventoryItem);
            }

            return (ourItems, theirItems);
        }
    }
}
