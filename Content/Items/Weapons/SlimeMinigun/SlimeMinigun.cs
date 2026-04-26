using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace LK_Ugrumiy_WP.Content.Items.Weapons
{
	/// <summary>
	/// Слизневая мини-акула: автоматическое оружие с высокой скорострельностью.
	/// Стреляет каплями белой слизи, как Minishark — пулями.
	/// Не требует боеприпасов.
	/// </summary>
	public class SlimeMinigun : ModItem
	{
		public override string Texture => "LK_Ugrumiy_WP/Content/Items/Weapons/SlimeMinigun/SlimeMinigun";

		public override void SetDefaults()
		{
			Item.DamageType = DamageClass.Ranged;
			Item.damage = 12;
			Item.knockBack = 1f;
			Item.crit = 4;

			Item.shootSpeed = 14f;
			Item.useAnimation = 8;
			Item.useTime = 8;
			Item.useStyle = ItemUseStyleID.Shoot;

			Item.shoot = ModContent.ProjectileType<Projectiles.SlimeGlob>();
			Item.useAmmo = AmmoID.None;

			Item.width = 50;
			Item.height = 22;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(gold: 10);

			Item.UseSound = SoundID.Item17;

			Item.autoReuse = true;
			Item.noMelee = true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			float spreadAngle = MathHelper.ToRadians(6f);
			float angle = velocity.ToRotation() + Main.rand.NextFloat(-spreadAngle, spreadAngle);
			float speed = velocity.Length() * Main.rand.NextFloat(0.95f, 1.05f);
			Vector2 bulletVelocity = angle.ToRotationVector2() * speed;

			Projectile.NewProjectile(
				source,
				position,
				bulletVelocity,
				type,
				damage,
				knockback,
				player.whoAmI
			);

			return false;
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-8f, -2f);
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			Vector2 muzzleOffset = Vector2.Normalize(velocity) * 25f;
			if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
			{
				position += muzzleOffset;
			}
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Minishark, 1)
				.AddIngredient(ModContent.ItemType<Consumables.CowMilk>(), 5)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}