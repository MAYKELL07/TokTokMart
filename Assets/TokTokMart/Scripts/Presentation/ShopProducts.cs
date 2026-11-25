using UnityEngine;

namespace TokTokMart
{
    public static class ShopProducts
    {
        static readonly string[] Products =
        {
            "CanItems01", "CanItems02", "CanItems03", "CanItems04",
            "Items01", "Items02", "Items03", "Items04", "Items05", "Items06"
        };
        static readonly string[] Names =
        {
            "Kaviar", "Jeruk Paru", "Kaviar Hijau", "Jeruk Biru",
            "Kopi Kedysa", "Kopi Hijau", "Keropok", "Kopi Ungu", "Kopi Biru", "Keropok Hijau"
        };
        static readonly Sprite[] ProductSprites = new Sprite[Products.Length];
        public static int ProductCount => Products.Length;
        public static int ShelfProduct(int shelf, int slot) => (shelf * 3 + slot) % ProductCount;
        public static string ProductName(int index) => Names[index];
        public static Sprite Product(int index)
        {
            if (!ProductSprites[index]) ProductSprites[index] = Load(Products[index]);
            return ProductSprites[index];
        }

        static Sprite Load(string name) => Resources.Load<Sprite>("NovemberArt/ShopProps/" + name);

    }
}
