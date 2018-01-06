using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using PoeHUD.Hud.Menu;
using PoeHUD.Plugins;
using PoeHUD.Poe;
using PoeHUD.Poe.Components;
using PoeHUD.Poe.Elements;
using SharpDX;
using SharpDX.Direct3D9;

namespace Tradie
{
    public class Core : BaseSettingsPlugin<Settings>
    {
        private readonly List<string> _whiteListedPaths = new List<string>
        {
            "Art/2DItems/Currency",
            "Art/2DItems/Maps"
        };

        public Core()
        {
            PluginName = "Tradie";
        }

        public override void InitialiseMenu(MenuItem mainMenu)
        {
            base.InitialiseMenu(mainMenu);
            var rootMenu = PluginSettingsRootMenu;
            MenuWrapper.AddMenu(rootMenu, "Image Size", Settings.ImageSize);
            MenuWrapper.AddMenu(rootMenu, "Text Size", Settings.TextSize);

            var background = MenuWrapper.AddMenu(rootMenu, "Background Box");
            MenuWrapper.AddMenu(background, "Color", Settings.BackgroundColor);
            MenuWrapper.AddMenu(background, "Transparency", Settings.BackgroundTransparency);

            var yourItems = MenuWrapper.AddMenu(rootMenu, "Your Trade Items");
            MenuWrapper.AddMenu(yourItems, "Ascending Order", Settings.YourItemsAscending);
            MenuWrapper.AddMenu(yourItems, "Currency Before Or After", Settings.YourItemsImageLeftOrRight, "On: <Currency> x<Amount>\nOff: <Amount>x <Currency>");
            MenuWrapper.AddMenu(yourItems, "Text Color", Settings.YourItemTextColor);
            var yourItemLocation = MenuWrapper.AddMenu(yourItems, "Starting Location");
            MenuWrapper.AddMenu(yourItemLocation, "X", Settings.YourItemStartingLocationX);
            MenuWrapper.AddMenu(yourItemLocation, "Y", Settings.YourItemStartingLocationY);

            var theirItems = MenuWrapper.AddMenu(rootMenu, "Their Trade Items");
            MenuWrapper.AddMenu(theirItems, "Ascending Order", Settings.TheirItemsAscending);
            MenuWrapper.AddMenu(theirItems, "Currency Before Or After", Settings.TheirItemsImageLeftOrRight, "On: <Currency> x<Amount>\nOff: <Amount>x <Currency>");
            MenuWrapper.AddMenu(theirItems, "Text Color", Settings.TheirItemTextColor);
            var theirItemLocation = MenuWrapper.AddMenu(theirItems, "Starting Location");
            MenuWrapper.AddMenu(theirItemLocation, "X", Settings.TheirItemStartingLocationX);
            MenuWrapper.AddMenu(theirItemLocation, "Y", Settings.TheirItemStartingLocationY);
        }

        public override void Render()
        {
            var tradingWindow = GetTradingWindow();
            if (tradingWindow == null || !tradingWindow.IsVisible)
                return;

            var tradingItems = GetItemsInTradingWindow(tradingWindow);
            var ourData = new ItemDisplay
            {
                Items = GetItemObjects(tradingItems.ourItems),
                X = Settings.YourItemStartingLocationX,
                Y = Settings.YourItemStartingLocationY,
                TextSize = Settings.TextSize,
                TextColor = Settings.YourItemTextColor,
                BackgroundColor = Settings.BackgroundColor,
                BackgroundTransparency = Settings.BackgroundTransparency,
                ImageSize = Settings.ImageSize,
                Spacing = 5,
                LeftAlignment = Settings.YourItemsImageLeftOrRight,
                Ascending = Settings.YourItemsAscending
            };

            var theirData = new ItemDisplay
            {
                Items = GetItemObjects(tradingItems.theirItems),
                X = Settings.TheirItemStartingLocationX,
                Y = Settings.TheirItemStartingLocationY,
                TextSize = Settings.TextSize,
                TextColor = Settings.TheirItemTextColor,
                BackgroundColor = Settings.BackgroundColor,
                BackgroundTransparency = Settings.BackgroundTransparency,
                ImageSize = Settings.ImageSize,
                Spacing = 5,
                LeftAlignment = Settings.TheirItemsImageLeftOrRight,
                Ascending = Settings.TheirItemsAscending
            };

            if (ourData.Items.Any())
                 DrawCurrency(ourData);

            if (theirData.Items.Any())
                DrawCurrency(theirData);
        }

        private void DrawCurrency(ItemDisplay data)
        {
            var counter = 0;
            var newColor = data.BackgroundColor;
            newColor.A = (byte)data.BackgroundTransparency;
            var maxCount = data.Items.Max(i => i.Amount);

            var background = new RectangleF(data.LeftAlignment ? data.X : data.X + data.ImageSize,
                data.Y,
                data.LeftAlignment
                    ? +data.ImageSize + data.Spacing +
                      Graphics.MeasureText(data.Spacing + "x " + maxCount, data.TextSize).Width
                    : -data.ImageSize - data.Spacing -
                      Graphics.MeasureText(data.Spacing + "x " + maxCount, data.TextSize).Width,
                data.Ascending
                    ? -data.ImageSize * data.Items.Count()
                    : data.ImageSize * data.Items.Count()
            );

            Graphics.DrawBox(background, newColor);
            foreach (var ourItem in data.Items)
            {
                counter++;
                var imageBox = new RectangleF(data.X,
                    data.Ascending
                        ? data.Y - counter * data.ImageSize
                        : data.Y - data.ImageSize + counter * data.ImageSize, data.ImageSize, data.ImageSize);


                DrawImage(ourItem.Path, imageBox);

                Graphics.DrawText(data.LeftAlignment ? $" x {ourItem.Amount}" : $"{ourItem.Amount} x ", data.TextSize,
                    new Vector2(data.LeftAlignment ? data.X + data.ImageSize + data.Spacing : data.X - data.Spacing * 2,
                        imageBox.Center.Y - data.TextSize / 2 - 3),
                    Settings.YourItemTextColor,
                    data.LeftAlignment ? FontDrawFlags.Left : FontDrawFlags.Right);
            }
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

        private bool IsWhitelisted(string metaData)
        {
            return _whiteListedPaths.Any(metaData.Contains);
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
                    if (item.ItemName.Equals(name))
                    {
                        item.Amount += amount;
                        found = true;
                        break;
                    }

                if (!found)
                {
                    var metaData = normalInventoryItem.Item.GetComponent<RenderItem>().ResourcePath;
                    if (metaData.Equals(""))
                    {
                        LogMessage($"Meta data of {name} is empty, skipping!", 5);
                        continue;
                    }

                    if (!IsWhitelisted(metaData))
                        continue;

                    var path = GetImagePath(metaData);
                    items.Add(new Item(name, amount, path));
                }
            }

            return items;
        }

        private string GetImagePath(string metadata)
        {
            metadata = metadata.Replace(".dds", ".png");
            var url = $"http://webcdn.pathofexile.com/image/{metadata}";
            var metadataPath = metadata.Replace('/', '\\');
            var fullPath = $"{PluginDirectory}\\images\\{metadataPath}";

            if (File.Exists(fullPath))
                return fullPath;

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