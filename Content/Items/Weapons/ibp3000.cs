using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Items.Weapons
{
    public class ibp3000 : ModItem
    {
        public override void SetDefaults()
        {
            // Тип предмета - метательный
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 69;
            Item.knockBack = 6f;
            Item.crit = 4;

            // Скорость броска (раньше 12f — ощущалось слишком тяжёлым)
            Item.shootSpeed = 19f;
            Item.useAnimation = 18;
            Item.useTime = 18;
            Item.useStyle = ItemUseStyleID.Swing;

            // Снаряд
            Item.shoot = ModContent.ProjectileType<Projectiles.Ibp3000Projectile>();

            // Расходный (метательный)
            Item.consumable = true;
            Item.maxStack = 999;

            // Размеры и редкость
            Item.width = 32;
            Item.height = 32;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.buyPrice(silver: 50);

            // Звук использования
            Item.UseSound = SoundID.Item1;

            // Авто-использование
            Item.autoReuse = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(10);
            recipe.AddIngredient(ItemID.IronBar, 5);
            recipe.AddIngredient(ItemID.FallenStar, 3);
            recipe.AddIngredient(ItemID.Glowstick, 2);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
