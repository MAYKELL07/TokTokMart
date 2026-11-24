using UnityEngine;

namespace GokilKisah
{
    static class ReleaseShopArt
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

        public static void Apply(ShopLayout shop, ShelfInventory[] shelves)
        {
            for (int i = 0; i < shelves.Length; i++)
            {
                var shelf = shelves[i];
                if (!shelf || shelf.stockSlots == null) continue;
                var variants = new Sprite[shelf.stockSlots.Length];
                for (int j = 0; j < variants.Length; j++)
                {
                    if (!shelf.stockSlots[j]) continue;
                    var sprite = Product(ShelfProduct(i, j));
                    variants[j] = sprite;
                    Fit(shelf.stockSlots[j], sprite, shelf.stockedSprite, false);
                }
                shelf.UseStockVariants(variants);
            }

            var register = shop.register.transform.Find("Register")?.GetComponent<SpriteRenderer>();
            if (register)
            {
                Fit(register, Load("CashRegister01"));
                AddAdjacent(register, "Coin jar", Load("CoinJar01"), Vector2.down, .30f);
            }
            if (shop.counterCan) Fit(shop.counterCan, Load("IblisCan01"));
            var fixture = shop.transform.Find("Reference shop layout/Fridge 4")?.GetComponent<SpriteRenderer>();
            if (fixture) Fit(fixture, Load("DrinksDispenser01"), keepFoot: true);
        }

        static void Fit(SpriteRenderer renderer, Sprite sprite, Sprite fallback = null, bool keepFoot = false)
        {
            if (!sprite) return;
            var original = renderer.sprite ? renderer.sprite : fallback;
            Vector3 originalSize = original ? original.bounds.size : sprite.bounds.size;
            Vector3 size = sprite.bounds.size;
            float fit = Mathf.Min(originalSize.x / size.x, originalSize.y / size.y);
            float bottom = renderer.bounds.min.y;
            var scale = renderer.transform.localScale;
            renderer.sprite = sprite;
            renderer.transform.localScale = new Vector3(scale.x * fit, scale.y * fit, scale.z);
            if (keepFoot)
                renderer.transform.position += Vector3.up * (bottom - renderer.bounds.min.y);
        }

        static void AddAdjacent(SpriteRenderer anchor, string name, Sprite sprite, Vector2 direction, float size)
        {
            if (!sprite) return;
            var root = anchor.transform.parent;
            var previous = root.Find(name);
            if (previous) return;
            var obj = new GameObject(name);
            obj.transform.SetParent(root, false);
            obj.transform.position = anchor.bounds.center + (Vector3)(direction * (anchor.bounds.extents.y + .11f));
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerID = anchor.sortingLayerID;
            renderer.sortingOrder = anchor.sortingOrder + 1;
            float scale = size / Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
            var parentScale = root.lossyScale;
            obj.transform.localScale = new Vector3(scale / parentScale.x, scale / parentScale.y, 1);
        }
    }
}
