using System.Collections.Generic;
using UnityEngine;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        const float ShopperCollisionDistance = 30;
        static readonly string[] SpillLines =
        {
            "Alamak! Air satu. On the floor, boss.",
            "Aduh. My drink just resigned.",
            "Eh, free swimming pool ah?",
            "Sorry, boss. That cup was haunted."
        };
        static readonly string[] SlipLines =
        {
            "Adoi! Who waxed the floor with sirap?",
            "Oi! My slippers got hydroplaning now.",
            "Licin lah! This aisle got water park?",
            "Wah, I nearly checked out permanently."
        };

        sealed class DroppedStock
        {
            public Vector2 origin, position;
            public SpriteRenderer view;
            public float start;
        }
        struct Splash
        {
            public Vector2 position;
            public float start;
        }

        readonly List<DroppedStock> droppedStock = new List<DroppedStock>();
        readonly List<Splash> splashes = new List<Splash>();

        void ResetShopMishaps()
        {
            foreach (var item in droppedStock) if (item.view) Destroy(item.view.gameObject);
            droppedStock.Clear();
            splashes.Clear();
        }

        static bool BlocksPlayerStep(Vector2 from, Vector2 to, Vector2 obstruction, float distance)
        {
            float after = (to - obstruction).sqrMagnitude;
            return after < distance * distance && after < (from - obstruction).sqrMagnitude;
        }

        bool PlayerStepClear(Vector2 target)
        {
            if (!Walkable(target)) return false;
            foreach (var customer in customers)
            {
                if (!BlocksPlayerStep(player, target, customer.position, ShopperCollisionDistance)) continue;
                if (customer.state == ShopperState.Picking && customer.timer > 1.4f && Time.time > customer.yieldUntil)
                {
                    Speak(customer, "Sekejap. Comparing price per curse.");
                    customer.yieldUntil = Time.time + 2;
                    customer.timer = 4;
                }
                return false;
            }
            return true;
        }

        bool StationInReach(ShopStation station)
        {
            if (!station || !station.isActiveAndEnabled) return false;
            var approach = station.approachPoint ? ShopLayout.ToGame(station.approachPoint.position) : station.Position;
            float radius = shop.stationInteractionRadius * 100;
            return (player - approach).sqrMagnitude <= radius * radius && StaticSegmentClear(player, approach);
        }

        void Speak(Shopper customer, string line)
        {
            customer.speech = line;
            customer.speechUntil = Time.time + 3.2f;
        }

        void AddSplash(Vector2 position)
        {
            if (splashes.Count == 8) splashes.RemoveAt(0);
            splashes.Add(new Splash { position = position, start = Time.time });
        }

        void TryDropShelfItem(Shopper customer)
        {
            if (customer.droppedItem || droppedStock.Count >= 3) return;
            var position = customer.position + new Vector2(0, 32);
            if (!StaticSegmentClear(customer.position, position)) position = customer.position;
            foreach (var item in droppedStock)
                if ((item.position - position).sqrMagnitude < 65 * 65) return;

            int index = (customer.id + customer.shelf) % ReleaseShopArt.ProductCount;
            var sprite = ReleaseShopArt.Product(index);
            if (!sprite) return;
            var view = new GameObject("Fallen " + ReleaseShopArt.ProductName(index));
            view.transform.SetParent(runtimeObjects, false);
            var renderer = view.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = (int)position.y + 1;
            renderer.transform.localScale = Vector3.one * .3f;
            var itemDrop = new DroppedStock { origin = customer.position, position = position, view = renderer, start = Time.time };
            droppedStock.Add(itemDrop);
            PresentDroppedStock(itemDrop);
            customer.droppedItem = true;
            chaos = Mathf.Min(100, chaos + 4);
            PlaySound("Clatter");
            Speak(customer, "Eh, barang jatuh! It jumped, I swear.");
        }

        int NearestDroppedItem()
        {
            int result = -1;
            float nearest = 55 * 55;
            for (int i = 0; i < droppedStock.Count; i++)
            {
                var position = droppedStock[i].position;
                float distance = (player - position).sqrMagnitude;
                if (distance >= nearest || !StaticSegmentClear(player, position)) continue;
                nearest = distance;
                result = i;
            }
            return result;
        }

        void PickUpDroppedItem(int index)
        {
            var item = droppedStock[index];
            droppedStock.RemoveAt(index);
            if (item.view) Destroy(item.view.gameObject);
            chaos = Mathf.Max(0, chaos - 4);
            PlaySound("Pickup");
            Toast("Fallen stock picked up");
        }

        void PresentDroppedStock(DroppedStock item)
        {
            float t = Mathf.Clamp01((Time.time - item.start) / .34f);
            var point = Vector2.Lerp(item.origin, item.position, t);
            point.y -= (1 - t) * 48 + Mathf.Sin(t * Mathf.PI) * 16;
            item.view.transform.position = ShopLayout.ToWorld(point);
            item.view.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(6, -24, t));
        }

        void DrawShopperFeedback()
        {
            if (Event.current.type != EventType.Repaint) return;
            foreach (var item in droppedStock)
            {
                var point = item.position;
                if ((player - point).sqrMagnitude < 240 * 240)
                    Text(new Rect(point.x - 43, point.y + 4, 86, 22), "PICK UP", 12, arcadeCream, FontStyle.Bold, TextAnchor.MiddleCenter);
            }

            for (int i = 0; i < splashes.Count; i++)
            {
                var splash = splashes[i];
                float t = (Time.time - splash.start) / .55f;
                if (t >= 1) continue;
                for (int j = 0; j < 7; j++)
                {
                    float angle = j * (Mathf.PI * 2 / 7);
                    Vector2 p = splash.position + new Vector2(Mathf.Cos(angle) * 40, Mathf.Sin(angle) * 15) * t;
                    p.y -= Mathf.Sin(Mathf.PI * t) * (15 + j * 2);
                    Panel(new Rect(p.x, p.y, 6 * (1 - t) + 2, 12 * (1 - t) + 2), new Color(.44f, .86f, 1, 1 - t));
                }
            }

            Shopper speaker = null;
            float closest = 310 * 310;
            foreach (var shopper in customers)
            {
                if (shopper.speechUntil <= Time.time) continue;
                float distance = (player - shopper.position).sqrMagnitude;
                if (distance >= closest) continue;
                closest = distance;
                speaker = shopper;
            }
            if (speaker == null) return;

            // Keep the text a fixed size on the HUD and pin the tail to the shopper when visible.
            Vector2 speechPosition = speaker.position - new Vector2(0, 95);
            Vector3 screen = shop.worldCamera.WorldToScreenPoint(ShopLayout.ToWorld(speechPosition));
            var anchor = interfaceCanvasMatrix.inverse.MultiplyPoint3x4(new Vector3(screen.x, Screen.height - screen.y));
            if (anchor.x < 24 || anchor.x > 1576 || anchor.y < 70 || anchor.y > 850) return;
            Matrix4x4 world = GUI.matrix;
            GUI.matrix = interfaceCanvasMatrix;
            float x = Mathf.Clamp(anchor.x - 182, 18, 1218);
            float y = Mathf.Clamp(anchor.y - 65, 98, 780);
            var box = new Rect(x, y, 364, 60);
            Panel(new Rect(anchor.x - 8, box.yMax - 6, 16, 15), arcadeNavy);
            Panel(box, arcadeNavy, true);
            Panel(new Rect(box.x + 3, box.y + 3, box.width - 6, box.height - 6), arcadeCream);
            Text(new Rect(x + 12, y + 3, box.width - 24, box.height - 6), speaker.speech, 21, arcadeNavy, FontStyle.Bold, TextAnchor.MiddleCenter);
            GUI.matrix = world;
        }
    }
}
