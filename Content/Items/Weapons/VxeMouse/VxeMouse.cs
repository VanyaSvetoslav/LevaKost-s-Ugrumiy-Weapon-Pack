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

            // Channeled magic spell: mana is paid once on the initial cast (see
            // ModifyManaCost below). While the mouse stays alive, no extra mana is spent.
            Item.mana = 14;
            Item.useAnimation = 18;
            Item.useTime = 18;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.channel = true;
            Item.autoReuse = true;
            Item.noMelee = true;

            Item.shoot = ModContent.ProjectileType<Projectiles.VxeMouseProjectile>();
            Item.shootSpeed = 0f;

            Item.width = 32;
            Item.height = 32;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.LightPurple;
            Item.UseSound = SoundID.Item43;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // While channeling, this hook fires repeatedly. Only spawn a mouse if one isn't already in the air.
            if (PlayerHasActiveMouse(player, type))
            {
                return false;
            }

            Vector2 spawnPos = player.MountedCenter;
            Vector2 toMouse = Main.MouseWorld - spawnPos;
            Vector2 startVel = toMouse.SafeNormalize(Vector2.UnitY) * 8f;

            Projectile.NewProjectile(source, spawnPos, startVel, type, damage, knockback, player.whoAmI);
            SoundEngine.PlaySound(SoundID.Item8, spawnPos);
            return false;
        }

        public override void ModifyManaCost(Player player, ref float reduce, ref float mult)
        {
            // Mana is paid once when the spell starts. While the mouse is already
            // alive (channeling continues), subsequent re-fires cost nothing.
            int projType = ModContent.ProjectileType<Projectiles.VxeMouseProjectile>();
            if (PlayerHasActiveMouse(player, projType))
            {
                mult = 0f;
                reduce = 0f;
            }
        }

        private static bool PlayerHasActiveMouse(Player player, int projType)
        {
            // ExampleLastPrism-style check: vanilla maintains a per-player projectile
            // counter, so we don't have to scan the whole projectile array every cast.
            return player.ownedProjectileCounts[projType] > 0;
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
