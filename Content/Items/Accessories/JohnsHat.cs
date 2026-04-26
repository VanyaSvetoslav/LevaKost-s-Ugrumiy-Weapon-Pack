using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using LK_Ugrumiy_WP.Common.Systems;

namespace LK_Ugrumiy_WP.Content.Items.Accessories
{
    /// <summary>
    /// Шапка Джона - редкий аксессуар, выпадающий из NPC "Джон" в пещерах.
    /// Отсылка на Grizzled Jon из RDR2.
    /// </summary>
    public class JohnsHat : ModItem
    {
        // Placeholder sprite: заменить на свой спрайт в Content/Items/Accessories/JohnsHat.png
        public override string Texture => "LK_Ugrumiy_WP/Content/Items/Accessories/JohnsHat";

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 5);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // Урон по гуманоидам
            player.GetDamage(DamageClass.Generic) += 0.1f;
            // Критический шанс
            player.GetCritChance(DamageClass.Generic) += 5;

            // Помечаем игрока как "носящего шляпу Джона"
            player.GetModPlayer<JohnsHatPlayer>().wearingJohnsHat = true;
        }
    }

    public class JohnsHatPlayer : ModPlayer
    {
        public bool wearingJohnsHat;

        public override void ResetEffects()
        {
            wearingJohnsHat = false;
        }

        public override void PostUpdateEquips()
        {
            var johnSystem = ModContent.GetInstance<JohnHatSystem>();
            johnSystem.playerWearingHat[Player.whoAmI] = wearingJohnsHat;
        }
    }
}
