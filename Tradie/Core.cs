using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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
            var yourItems = MenuWrapper.AddMenu(rootMenu, "Your Trade Items");
            MenuWrapper.AddMenu(yourItems, "Text Color", Settings.YourItemTextColor);
            MenuWrapper.AddMenu(yourItems, "Currency Before Or After", Settings.YourItemsImageLeftOrRight, "On: <Currency> x<Amount>\nOff: <Amount>x <Currency>");
            var yourItemLocation = MenuWrapper.AddMenu(yourItems, "Starting Location");
            MenuWrapper.AddMenu(yourItemLocation, "X", Settings.YourItemStartingLocationX);
            MenuWrapper.AddMenu(yourItemLocation, "Y", Settings.YourItemStartingLocationY);

            var theirItems = MenuWrapper.AddMenu(rootMenu, "Their Trade Items");
            MenuWrapper.AddMenu(theirItems, "Text Color", Settings.TheirItemTextColor);
            MenuWrapper.AddMenu(theirItems, "Currency Before Or After", Settings.TheirItemsImageLeftOrRight, "On: <Currency> x<Amount>\nOff: <Amount>x <Currency>");
            var theirItemLocation = MenuWrapper.AddMenu(theirItems, "Starting Location");
            MenuWrapper.AddMenu(theirItemLocation, "X", Settings.TheirItemStartingLocationX);
            MenuWrapper.AddMenu(theirItemLocation, "Y", Settings.TheirItemStartingLocationY);
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
            DrawOurCurrencyList(ourItems, Settings.YourItemsImageLeftOrRight);
            DraTheirCurrencyList(theirItems, Settings.TheirItemsImageLeftOrRight);
        }

        private void DrawOurCurrencyList(IEnumerable<Item> ourItems, bool LeftOrRight)
        {
            var textSize = Settings.TextSize;
            var imageSize = Settings.ImageSize;
            const int extraBit = 5;
            var counter = 0;

            var newColor = Color.Black;
            newColor.A = 230;

            var highestCurrencyCount = ourItems.Select(ourItem => ourItem.Amount).Concat(new[] {0}).Max();
            if (LeftOrRight)
            {
                var background = new RectangleF(Settings.YourItemStartingLocationX,
                    Settings.YourItemStartingLocationY,
                    +imageSize + extraBit +
                    Graphics.MeasureText(extraBit + "x " + highestCurrencyCount, textSize).Width,
                    -imageSize * ourItems.Count());

                Graphics.DrawBox(background, newColor);
                foreach (var ourItem in ourItems)
                {
                    counter++;
                    var imageBox = new RectangleF(Settings.YourItemStartingLocationX,
                        Settings.YourItemStartingLocationY - counter * imageSize, imageSize, imageSize);


                    DrawImage(ourItem.Path, imageBox);

                    Graphics.DrawText($" x {ourItem.Amount}", textSize,
                        new Vector2(Settings.YourItemStartingLocationX + imageSize + extraBit,
                            imageBox.Center.Y - textSize / 2 - 3),
                        Settings.YourItemTextColor);
                }
            }
            else
            {
                var background = new RectangleF(Settings.YourItemStartingLocationX + imageSize,
                    Settings.YourItemStartingLocationY,
                    -imageSize - extraBit -
                    Graphics.MeasureText(extraBit + "x " + highestCurrencyCount, textSize).Width,
                    -imageSize * ourItems.Count());

                Graphics.DrawBox(background, newColor);
                foreach (var ourItem in ourItems)
                {
                    counter++;
                    var imageBox = new RectangleF(Settings.YourItemStartingLocationX,
                        Settings.YourItemStartingLocationY - counter * imageSize, imageSize, imageSize);


                    DrawImage(ourItem.Path, imageBox);

                    Graphics.DrawText($"{ourItem.Amount} x ", textSize,
                        new Vector2(Settings.YourItemStartingLocationX - extraBit * 2,
                            imageBox.Center.Y - textSize / 2 - 3),
                        Settings.YourItemTextColor,
                        FontDrawFlags.Right);
                }
            }
        }

        private void DraTheirCurrencyList(IEnumerable<Item> theirItems, bool LeftOrRight)
        {
            var textSize = Settings.TextSize;
            var imageSize = Settings.ImageSize;
            const int extraBit = 5;
            var counter = 0;

            var newColor = Color.Black;
            newColor.A = 230;

            var highestCurrencyCount = theirItems.Select(ourItem => ourItem.Amount).Concat(new[] {0}).Max();
            if (LeftOrRight)
            {
                var background = new RectangleF(Settings.TheirItemStartingLocationX,
                    Settings.TheirItemStartingLocationY,
                    +imageSize + extraBit +
                    Graphics.MeasureText(extraBit + "x " + highestCurrencyCount, textSize).Width,
                    imageSize * theirItems.Count());

                Graphics.DrawBox(background, newColor);
                foreach (var ourItem in theirItems)
                {
                    counter++;
                    var imageBox = new RectangleF(Settings.TheirItemStartingLocationX,
                        (Settings.TheirItemStartingLocationY - imageSize) + (counter * imageSize), imageSize, imageSize);


                    DrawImage(ourItem.Path, imageBox);

                    Graphics.DrawText($" x {ourItem.Amount}", textSize,
                        new Vector2(Settings.TheirItemStartingLocationX + imageSize + extraBit,
                            imageBox.Center.Y - textSize / 2 - 3),
                        Settings.TheirItemTextColor);
                }
            }
            else
            {
                var background = new RectangleF(Settings.TheirItemStartingLocationX + imageSize,
                    Settings.TheirItemStartingLocationY,
                    -imageSize - extraBit -
                    Graphics.MeasureText(extraBit + "x " + highestCurrencyCount, textSize).Width,
                    imageSize * theirItems.Count());

                Graphics.DrawBox(background, newColor);
                foreach (var ourItem in theirItems)
                {
                    counter++;
                    var imageBox = new RectangleF(Settings.TheirItemStartingLocationX,
                        (Settings.TheirItemStartingLocationY - imageSize) + (counter * imageSize), imageSize, imageSize);


                    DrawImage(ourItem.Path, imageBox);

                    Graphics.DrawText($"{ourItem.Amount} x ", textSize,
                        new Vector2(Settings.TheirItemStartingLocationX - extraBit * 2,
                            imageBox.Center.Y - textSize / 2 - 3),
                        Settings.TheirItemTextColor,
                        FontDrawFlags.Right);
                }
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
