using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Items.Weapons.VxeMouse
{
    public class VxeMouse : ModItem
    {
        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Magic;
            Item.damage = 28;
            Item.knockBack = 3f;
            Item.crit = 6;

            Item.mana = 14;
            Item.useAnimation = 24;
            Item.useTime = 24;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;
            Item.autoReuse = false;

            Item.shoot = ModContent.ProjectileType<Projectiles.VxeMouseProjectile>();
            Item.shootSpeed = 0f;

            Item.width = 32;
            Item.height = 32;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.LightPurple;
            Item.UseSound = SoundID.Item43;
        }

        public override bool CanUseItem(Player player)
        {
            // Check that the player is actually pointing at a valid spot.
            return player.CheckMana(Item.mana, false);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Despawn any existing mouse owned by this player so only one is active at a time.
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];
                if (other.active && other.owner == player.whoAmI && other.type == type)
                {
                    other.Kill();
                }
            }

            Vector2 spawnPos = player.MountedCenter;
            Vector2 toMouse = Main.MouseWorld - spawnPos;
            Vector2 startVel = toMouse.SafeNormalize(Vector2.UnitY) * 6f;

            Projectile.NewProjectile(source, spawnPos, startVel, type, damage, knockback, player.whoAmI);

            // Subtle cast feedback
            SoundEngine.PlaySound(SoundID.Item8, spawnPos);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wire, 5)
                .AddIngredient(ItemID.SoulofLight, 3)
                .AddIngredient(ItemID.CrystalShard, 6)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
