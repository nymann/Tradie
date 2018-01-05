using PoeHUD.Controllers;
using PoeHUD.Hud.Settings;
using PoeHUD.Plugins;

namespace Tradie
{
    public class Settings : SettingsBase
    {
        public Settings()
        {
            Enable = false;
            ImageSize = new RangeNode<int>(32, 1, 78);
            TextSize = new RangeNode<int>(20, 1, 60);


            YourItemStartingLocationX = new RangeNode<int>(966, 0, (int)BasePlugin.API.GameController.Window.GetWindowRectangle().Width);
            YourItemStartingLocationY = new RangeNode<int>(863, 0, (int)BasePlugin.API.GameController.Window.GetWindowRectangle().Height);
            YourItemsImageLeftOrRight = true;

            TheirItemStartingLocationX = new RangeNode<int>(966, 0, (int)BasePlugin.API.GameController.Window.GetWindowRectangle().Width);
            TheirItemStartingLocationY = new RangeNode<int>(863, 0, (int)BasePlugin.API.GameController.Window.GetWindowRectangle().Height);
            TheirItemsImageLeftOrRight = false;
        }
        public RangeNode<int> ImageSize { get; set; }
        public RangeNode<int> TextSize { get; set; }

        public RangeNode<int> YourItemStartingLocationX { get; set; }
        public RangeNode<int> YourItemStartingLocationY { get; set; }
        public ToggleNode YourItemsImageLeftOrRight { get; set; }

        public RangeNode<int> TheirItemStartingLocationX { get; set; }
        public RangeNode<int> TheirItemStartingLocationY { get; set; }
        public ToggleNode TheirItemsImageLeftOrRight { get; set; }
    }
}
