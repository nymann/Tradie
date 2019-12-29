using ExileCore;
using ExileCore.PoEMemory;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.Shared.Enums;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Tradie
{
    public partial class Core : BaseSettingsPlugin<Settings>
    {
        private readonly List<string> _whiteListedPaths = new List<string>
        {
                "Art/2DItems/Currency"

                //TODO, maps are loading strange and causing too many draw issues
                //"Art/2DItems/Maps"
        };

        private List<string> _initializedImageList = new List<string>();

        public override void Render()
        {
            var Area = GameController.Game.IngameState.Data.CurrentArea;
            if (!Area.IsTown && !Area.RawName.Contains("Hideout")) return;
            ShowNpcTradeItems();
            ShowPlayerTradeItems();
        }

        private void ShowNpcTradeItems()
        {
            var npcTradingWindow = GetNpcTadeWindow();
            if (npcTradingWindow == null || !npcTradingWindow.IsVisible)
                return;
            var tradingItems = GetItemsInTradingWindow(npcTradingWindow);
            var ourData = new ItemDisplay
            {
                Items = GetItemObjects(tradingItems.ourItems).OrderBy(item => item.Path),
                X = Settings.YourItemStartingLocationX,
                Y = Settings.YourItemStartingLocationY,
                TextSize = Settings.TextSize,
                TextColor = Settings.YourItemTextColor,
                BackgroundColor = Settings.YourItemBackgroundColor,
                BackgroundTransparency = Settings.YourItemBackgroundColor.Value.A,
                ImageSize = Settings.ImageSize,
                Spacing = Settings.Spacing,
                LeftAlignment = Settings.YourItemsImageLeftOrRight,
                Ascending = Settings.YourItemsAscending
            };
            var theirData = new ItemDisplay
            {
                Items = GetItemObjects(tradingItems.theirItems).OrderBy(item => item.Path),
                X = Settings.TheirItemStartingLocationX,
                Y = Settings.TheirItemStartingLocationY,
                TextSize = Settings.TextSize,
                TextColor = Settings.TheirItemTextColor,
                BackgroundColor = Settings.TheirItemBackgroundColor,
                BackgroundTransparency = Settings.TheirItemBackgroundColor.Value.A,
                ImageSize = Settings.ImageSize,
                Spacing = Settings.Spacing,
                LeftAlignment = Settings.TheirItemsImageLeftOrRight,
                Ascending = Settings.TheirItemsAscending
            };
            if (ourData.Items.Any())
                DrawCurrency(ourData);
            if (theirData.Items.Any())
                DrawCurrency(theirData);
        }

        private void ShowPlayerTradeItems()
        {
            var tradingWindow = GetTradingWindow();
            if (tradingWindow == null || !tradingWindow.IsVisible)
                return;
            var tradingItems = GetItemsInTradingWindow(tradingWindow);
            var ourData = new ItemDisplay
            {
                Items = GetItemObjects(tradingItems.ourItems).OrderBy(item => item.Path),
                X = Settings.YourItemStartingLocationX,
                Y = Settings.YourItemStartingLocationY,
                TextSize = Settings.TextSize,
                TextColor = Settings.YourItemTextColor,
                BackgroundColor = Settings.YourItemBackgroundColor,
                BackgroundTransparency = Settings.YourItemBackgroundColor.Value.A,
                ImageSize = Settings.ImageSize,
                Spacing = Settings.Spacing,
                LeftAlignment = Settings.YourItemsImageLeftOrRight,
                Ascending = Settings.YourItemsAscending
            };
            var theirData = new ItemDisplay
            {
                Items = GetItemObjects(tradingItems.theirItems).OrderBy(item => item.Path),
                X = Settings.TheirItemStartingLocationX,
                Y = Settings.TheirItemStartingLocationY,
                TextSize = Settings.TextSize,
                TextColor = Settings.TheirItemTextColor,
                BackgroundColor = Settings.TheirItemBackgroundColor,
                BackgroundTransparency = Settings.TheirItemBackgroundColor.Value.A,
                ImageSize = Settings.ImageSize,
                Spacing = Settings.Spacing,
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
            const string symbol = "-";
            var counter = 0;
            var newColor = data.BackgroundColor;
            newColor.A = (byte) data.BackgroundTransparency;
            var maxCount = data.Items.Max(i => i.Amount);

            var background = new RectangleF(data.LeftAlignment ? data.X : data.X + data.ImageSize, data.Y,
                data.LeftAlignment
                    ? +data.ImageSize + data.Spacing + 3 +
                      Graphics.MeasureText(symbol + " " + maxCount, data.TextSize).X
                    : -data.ImageSize - data.Spacing - 3 -
                      Graphics.MeasureText(symbol + " " + maxCount, data.TextSize).X,
                data.Ascending ? -data.ImageSize * data.Items.Count() : data.ImageSize * data.Items.Count());

            Graphics.DrawBox(background, newColor);

            foreach (var ourItem in data.Items)
            {
                counter++;
                var imageBox = new RectangleF(data.X,
                    data.Ascending
                        ? data.Y - counter * data.ImageSize
                        : data.Y - data.ImageSize + counter * data.ImageSize,
                    data.ImageSize, data.ImageSize);

                DrawImage(ourItem.Path, imageBox);

                Graphics.DrawText(data.LeftAlignment ? $"{symbol} {ourItem.Amount}" : $"{ourItem.Amount} {symbol}",
                    new Vector2(data.LeftAlignment ? data.X + data.ImageSize + data.Spacing : data.X - data.Spacing,
                        imageBox.Center.Y - data.TextSize / 2 - 3), data.TextColor,
                    data.LeftAlignment ? FontAlign.Left : FontAlign.Right);

            }
        }

        private bool DrawImage(string path, RectangleF rec)
        {
            try
            {
                Graphics.DrawImage(path, rec);
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
                return GameController.Game.IngameState.UIRoot.Children[1].Children[82].Children[3].Children[1].Children[0].Children[0];
            }
            catch
            {
                return null;
            }
        }

        private Element GetNpcTadeWindow()
        {
            try
            {
                return GameController.Game.IngameState.UIRoot.Children[1].Children[81].Children[3];
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

            // We are skipping the first, since it's a Element ("Place items you want to trade here") that we don't need.
            // 
            // skipping the first item as its a strange object added after 3.3 Incursion
            foreach (var ourElement in ourItemsElement.Children.Skip(2))
            {
                var normalInventoryItem = ourElement.AsObject<NormalInventoryItem>();
                if (normalInventoryItem == null)
                {
                    LogMessage("Tradie: OurItem was null!", 5);
                    throw new Exception("Tradie: OurItem was null!");
                }

                ourItems.Add(normalInventoryItem);
            }

            // skipping the first item as its a strange object added after 3.3 Incursion
            foreach (var theirElement in theirItemsElement.Children.Skip(1))
            {
                var normalInventoryItem = theirElement.AsObject<NormalInventoryItem>();
                if (normalInventoryItem == null)
                {
                    LogMessage("Tradie: OurItem was null!", 5);
                    throw new Exception("Tradie: OurItem was null!");
                }

                theirItems.Add(normalInventoryItem);
            }

            return (ourItems, theirItems);
        }

        private bool IsWhitelisted(string metaData) => _whiteListedPaths.Any(metaData.Contains);

        private IEnumerable<Item> GetItemObjects(IEnumerable<NormalInventoryItem> normalInventoryItems)
        {
            var items = new List<Item>();
            foreach (var normalInventoryItem in normalInventoryItems)
                try
                {
                    if (normalInventoryItem.Item == null) continue;
                    var metaData = normalInventoryItem.Item.GetComponent<RenderItem>().ResourcePath;
                    if (metaData.Equals("")) continue;
                    if (!IsWhitelisted(metaData))
                        continue;
                    var stack = normalInventoryItem.Item.GetComponent<Stack>();
                    var amount = stack?.Info == null ? 1 : stack.Size;
                    var name = GetImagePath(metaData, normalInventoryItem);
                    var found = false;
                    foreach (var item in items)
                        if (item.ItemName.Equals(name))
                        {
                            item.Amount += amount;
                            found = true;
                            break;
                        }

                    if (found) continue;
                    var path = GetImagePath(metaData, normalInventoryItem);
                    items.Add(new Item(name, amount, path));
                }
                catch
                {
                    LogError("Tradie: Sometime went wrong in GetItemObjects() for a brief moment", 5);
                }

            return items;
        }

        private string GetImagePath(string metadata, NormalInventoryItem invItem)
        {
            metadata = metadata.Replace(".dds", ".png");
            var url = $"http://webcdn.pathofexile.com/image/{metadata}";
            var metadataPath = metadata.Replace('/', '\\');
            var fullPath = $"{DirectoryFullName}\\images\\{metadataPath}";

            /////////////////////////// Yucky Map bits ///////////////////////////////
            if (invItem.Item.HasComponent<Map>())
            {
                var isShapedMap = 0;
                var mapTier = invItem.Item.GetComponent<Map>().Tier;
                foreach (var itemList in invItem.Item.GetComponent<Mods>().ItemMods)
                {
                    if (!itemList.RawName.Contains("MapShaped")) continue;
                    isShapedMap = 1;
                    mapTier += 5;
                }

                url = $"http://webcdn.pathofexile.com/image/{metadata}?mn=1&mr={isShapedMap}&mt={mapTier}";
                fullPath = $"{DirectoryFullName}\\images\\{metadataPath.Replace(".png", "")}_{mapTier}_{isShapedMap}.png";
            }
            //

            if (File.Exists(fullPath))
            {
                if (_initializedImageList.All(x => x != fullPath))
                {
                    Graphics.InitImage($"{fullPath}", false);
                    _initializedImageList.Add(fullPath);
                }

                return fullPath;
            }

            var path = fullPath.Substring(0, fullPath.LastIndexOf('\\'));
            Directory.CreateDirectory(path);
            using (var client = new WebClient())
            {
                client.DownloadFile(new Uri(url), fullPath);
            }

            if (_initializedImageList.All(x => x != fullPath))
            {
                Graphics.InitImage($"{fullPath}", false);
                _initializedImageList.Add(fullPath);
            }

            return fullPath;
        }
    }
}