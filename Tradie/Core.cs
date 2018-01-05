using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using PoeHUD.Plugins;
using PoeHUD.Poe;
using PoeHUD.Poe.Components;
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
            var ourItems = GetItemObjects(tradingItems.ourItems);
            var theirItems = GetItemObjects(tradingItems.theirItems);

            DisplayTradeCurrency(ourItems, new Vector2(800, 800));
            DisplayTradeCurrency(theirItems, new Vector2(200, 200));
        }

        private void DisplayTradeCurrency(IEnumerable<Item> items, Vector2 startLocation, bool ourItems = true)
        {
            var counter = 0;
            foreach (var item in items)
            {
                counter++;
                var width = 20;
                var height = 20;
                var fontSize = width;
                var x = startLocation.X;
                var y = ourItems ? startLocation.Y - (counter * height) : startLocation.Y + (counter * height);
                var rec = new RectangleF(x - width, y, width, height);

                var text = $"x{item.Amount}";
                if (!DrawImage(item.Path, rec))
                {
                    text += item.ItemName;
                }
                Graphics.DrawText(text, fontSize, new Vector2(x, y),
                    Color.White);
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

        private (List<NormalInventoryItem> ourItems, List<NormalInventoryItem> theirItems) GetItemsInTradingWindow(
            Element tradingWindow)
        {
            var ourItemsElement = tradingWindow.Children[0];
            var theirItemsElement = tradingWindow.Children[1];

            var ourItems = new List<NormalInventoryItem>();
            var theirItems = new List<NormalInventoryItem>();

            // We are skipping the first, since it's a Element ("Place items you want to trade here") that we don't need.
            // 
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

        private IEnumerable<Item> GetItemObjects(IEnumerable<NormalInventoryItem> normalInventoryItems)
        {
            var items = new List<Item>();

            foreach (var normalInventoryItem in normalInventoryItems)
            {
                var baseItemType = GameController.Files.BaseItemTypes.Translate(normalInventoryItem.Item.Path);
                var stack = normalInventoryItem.Item.GetComponent<Stack>();
                var amount = stack?.Info == null ? 1 : stack.Size;
                var name = baseItemType.BaseName;
                var found = false;

                foreach (var item in items)
                {
                    if (item.ItemName.Equals(name))
                    {
                        item.Amount += amount;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    var metaData = normalInventoryItem.Item.GetComponent<RenderItem>().ResourcePath;
                    if (metaData.Equals(""))
                    {
                        LogMessage($"Meta data of {name} is empty, skipping!", 5);
                        continue;
                    }

                    var path = GetImagePath(metaData);
                    items.Add(new Item(name, amount, path));
                }
            }

            return items;
        }

        private bool DrawImage(string path, RectangleF rec)
        {
            try
            {
                Graphics.DrawPluginImage(path, rec);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private string GetImagePath(string metadata)
        {
            metadata = metadata.Replace(".dds", ".png");
            var url = $"http://webcdn.pathofexile.com/image/{metadata}";
            var metadataPath = metadata.Replace('/', '\\');
            var fullPath = $"{PluginDirectory}\\images\\{metadataPath}";

            if (File.Exists(fullPath))
            {
                return fullPath;
            }

            var path = fullPath.Substring(0, fullPath.LastIndexOf('\\'));
            Directory.CreateDirectory(path);

            using (var client = new WebClient())
            {
                client.DownloadFile(new Uri(url), fullPath);
            }

            return fullPath;
        }
    }
}