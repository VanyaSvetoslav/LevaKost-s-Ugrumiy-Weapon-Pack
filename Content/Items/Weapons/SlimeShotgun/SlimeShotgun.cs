using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace LK_Ugrumiy_WP.Content.Items.Weapons
{
	/// <summary>
	/// Дробовик, стреляющий веером из капель склизкой белой жидкости.
	/// При попадании в поверхность жидкость растекается лужей, замедляя врагов.
	/// </summary>
	public class SlimeShotgun : ModItem
	{
		public override string Texture => "LK_Ugrumiy_WP/Content/Items/Weapons/SlimeShotgun/SlimeShotgun";

		public override void SetDefaults()
		{
			// Тип — огнестрельное оружие
			Item.DamageType = DamageClass.Ranged;
			Item.damage = 28;
			Item.knockBack = 4f;
			Item.crit = 4;

			// Скорость и анимация (как у дробовика Terraria)
			Item.shootSpeed = 10f;
			Item.useAnimation = 36;
			Item.useTime = 36;
			Item.useStyle = ItemUseStyleID.Shoot;

			// Снаряд — капля слизи
			Item.shoot = ModContent.ProjectileType<Projectiles.SlimeGlob>();
			Item.useAmmo = AmmoID.None; // Не требует патронов

			// Размеры и редкость
			Item.width = 44;
			Item.height = 18;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(gold: 5);

			// Звук выстрела
			Item.UseSound = SoundID.Item36; // звук дробовика

			Item.autoReuse = false;
			Item.noMelee = true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			int pelletCount = 5; // количество «капель» в залпе
			float spreadAngle = MathHelper.ToRadians(18f); // общий разброс

			for (int i = 0; i < pelletCount; i++)
			{
				// Случайное отклонение для каждой капли
				float angle = velocity.ToRotation() + Main.rand.NextFloat(-spreadAngle, spreadAngle);
				float speed = velocity.Length() * Main.rand.NextFloat(0.85f, 1.15f);
				Vector2 pelletVelocity = angle.ToRotationVector2() * speed;

				Projectile.NewProjectile(
					source,
					position,
					pelletVelocity,
					ModContent.ProjectileType<Projectiles.SlimeGlob>(),
					damage,
					knockback,
					player.whoAmI
				);
			}

			return false; // мы сами создали снаряды
		}

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-6f, 0f);
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Boomstick, 1)
				.AddIngredient(ModContent.ItemType<Consumables.CowMilk>(), 3)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}