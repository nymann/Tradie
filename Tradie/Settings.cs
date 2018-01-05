using PoeHUD.Hud.Settings;
using PoeHUD.Plugins;
using SharpDX;

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
            YourItemsAscending = true;
            YourItemTextColor = Color.LightBlue;
            YourItemsImageLeftOrRight = true;

            TheirItemStartingLocationX = new RangeNode<int>(966, 0, (int)BasePlugin.API.GameController.Window.GetWindowRectangle().Width);
            TheirItemStartingLocationY = new RangeNode<int>(863, 0, (int)BasePlugin.API.GameController.Window.GetWindowRectangle().Height);
            TheirItemsAscending = false;
            TheirItemTextColor = Color.Red;
            TheirItemsImageLeftOrRight = false;
        }
        public RangeNode<int> ImageSize { get; set; }
        public RangeNode<int> TextSize { get; set; }

        public RangeNode<int> YourItemStartingLocationX { get; set; }
        public RangeNode<int> YourItemStartingLocationY { get; set; }
        public ToggleNode YourItemsAscending { get; set; }
        public ColorNode YourItemTextColor { get; set; }
        public ToggleNode YourItemsImageLeftOrRight { get; set; }

        public RangeNode<int> TheirItemStartingLocationX { get; set; }
        public RangeNode<int> TheirItemStartingLocationY { get; set; }
        public ToggleNode TheirItemsAscending { get; set; }
        public ColorNode TheirItemTextColor { get; set; }
        public ToggleNode TheirItemsImageLeftOrRight { get; set; }
    }
}
