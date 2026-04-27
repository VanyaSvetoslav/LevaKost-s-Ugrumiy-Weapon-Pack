using System;
using Terraria;
using Terraria.ID;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Items.Consumables
{
	/// <summary>
	/// Конфета-рулетка: выглядит безобидно, но таит в себе сюрпризы.
	/// 55% — хил 67000 HP, 30% — взрыв, 15% — мгновенная смерть + анекдот.
	/// </summary>
	public class SpookyCandy : ModItem
	{
		// Путь к собственному спрайту в подпапке
		public override string Texture => "LK_Ugrumiy_WP/Content/Items/Consumables/SpookyCandy/SpookyCandy";

		public override void SetDefaults()
		{
			Item.consumable = true;
			Item.maxStack = 99;
			Item.width = 20;
			Item.height = 24;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(silver: 5);
			Item.UseSound = SoundID.Item2;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.EatFood;
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI != Main.myPlayer)
				return true;

			int roll = Main.rand.Next(100); // 0–99

			if (roll < 55)
			{
				// === 55%: Мега-хил ===
				DoMegaHeal(player);
			}
			else if (roll < 85)
			{
				// === 30%: Взрыв ===
				DoExplosion(player);
			}
			else
			{
				// === 15%: Мгновенная смерть + анекдот ===
				DoInstantDeath(player);
			}

			return true;
		}

		private void DoMegaHeal(Player player)
		{
			int healAmount = 67000;
			player.statLife = Math.Min(player.statLife + healAmount, player.statLifeMax2);
			player.HealEffect(healAmount);

			Main.NewText(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Misc.MegaHeal"), 50, 255, 100);

			// Зелёные искры
			for (int i = 0; i < 30; i++)
			{
				Dust.NewDust(player.position, player.width, player.height,
					DustID.GreenTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-5f, -1f),
					100, default, 1.5f);
			}
		}

		private void DoExplosion(Player player)
		{
			Main.NewText(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Misc.CandyExplosion"), 255, 150, 50);

			// Визуальный взрыв
			for (int i = 0; i < 50; i++)
			{
				Dust.NewDust(player.position, player.width, player.height,
					DustID.Smoke, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f),
					200, default, 2f);
			}
			for (int i = 0; i < 30; i++)
			{
				Dust.NewDust(player.position, player.width, player.height,
					DustID.Torch, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f),
					100, default, 1.8f);
			}

			// Настоящий снаряд-взрыв (урон игроку)
			Projectile.NewProjectile(
				player.GetSource_ItemUse(player.HeldItem),
				player.Center,
				Microsoft.Xna.Framework.Vector2.Zero,
				ProjectileID.Explosives,
				67000, // урон
				10f,
				player.whoAmI
			);
		}

		private void DoInstantDeath(Player player)
		{
			// Сначала анекдот, потом смерть
			string joke = GetRandomCandyJoke();
			Main.NewText(joke, 255, 80, 200);

			// Убиваем игрока
			string deathReason = Language.GetTextValue("Mods.LK_Ugrumiy_WP.Misc.CandyDeathReason", player.name);
			player.KillMe(
				PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral(deathReason)),
				999999,
				0
			);
		}

		/// <summary>
		/// Рандомные тупые анекдоты про конфеты.
		/// </summary>
		private static string GetRandomCandyJoke()
		{
			// Read jokes from localization (Mods.LK_Ugrumiy_WP.CandyJokes.Joke{N}).
			// Ranges automatically — adding more Joke{N} keys to the .hjson is enough.
			var keys = new System.Collections.Generic.List<string>();
			for (int i = 1; i <= 100; i++)
			{
				string key = $"Mods.LK_Ugrumiy_WP.CandyJokes.Joke{i}";
				if (!Language.Exists(key))
				{
					break;
				}
				keys.Add(key);
			}

			if (keys.Count == 0)
			{
				return string.Empty;
			}
			return Language.GetTextValue(keys[Main.rand.Next(keys.Count)]);
		}

		public override void AddRecipes()
		{
			CreateRecipe(5)
				.AddIngredient(ItemID.CandyCane, 3)
				.AddIngredient(ItemID.Gel, 10)
				.AddTile(TileID.CookingPots)
				.Register();

			// Альтернативный рецепт на Хэллоуин
			CreateRecipe(10)
				.AddIngredient(ItemID.GoodieBag, 1)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}