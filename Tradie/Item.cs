using System.Linq;

namespace Tradie
{
    public class Item
    {
        public string ItemName { get; set; }
        public int Amount { get; set; }
        public string Path { get; set; }

        public Item()
        {
            
        }

        public Item(string itemName, int amount, string path)
        {
            ItemName = itemName;
            Amount = amount;
            Path = path.Split('\\').Last();
        }
    }
}
