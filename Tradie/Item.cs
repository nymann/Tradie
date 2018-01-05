namespace Tradie
{
    public class Item
    {
        public string ItemName { get; set; }
        public string FileName { get; set; }
        public int Amount { get; set; } = 0;

        public Item()
        {
            
        }

        public Item(string itemName, int amount)
        {
            ItemName = itemName;
            Amount = amount;
        }
    }
}
