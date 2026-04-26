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

			Main.NewText("The candy fills you with overwhelming energy!", 50, 255, 100);

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
			Main.NewText("The candy... was a firecracker?!", 255, 150, 50);

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
			player.KillMe(
				PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral($"{player.name} shouldn't have eaten that candy...")),
				999999,
				0
			);
		}

		/// <summary>
		/// Рандомные тупые анекдоты про конфеты.
		/// </summary>
		private static string GetRandomCandyJoke()
		{
			string[] jokes = new string[]
			{
				"Why did the candy go to school? Because it wanted to be a Smartie!",
				"What do you call a candy that sings? A wrapper!",
				"Why don't candies ever win arguments? They always get licked!",
				"What's a candy's favorite dance? The Tootsie Roll!",
				"Why was the candy so good at baseball? It was a real sucker for the game!",
				"What did one candy say to the other? 'We're in a sticky situation!'",
				"Why did the gummy bear go to the dentist? He lost his filling!",
				"How does candy greet each other? 'Hey there, sweet thing!'",
				"What's a ghost's favorite candy? Boo-ble gum!",
				"Why did the lollipop cross the road? Because it was stuck to the chicken!",
				"What candy is always late? Choco-LATE!",
				"What do you call a bear with no teeth? A gummy bear!",
				"Why did the M&M go to school? It wanted to be a Smartie!",
				"What's a candy's favorite type of music? Wrap music!",
				"Knock knock. Who's there? Candy. Candy who? Candy door open any slower?!",
				"My doctor told me to stop eating candy... that was the sweetest advice I never took.",
				"I told a candy joke once. It was pretty sweet, but the delivery was a bit hard to swallow.",
				"What did the candy say before it died? 'Life is sweet... too sweet...'",
				"Why did the jawbreaker file a police report? It got mugged by a mouth!",
				"I ate a candy and it killed me. At least I died doing what I loved.",
			};

			return jokes[Main.rand.Next(jokes.Length)];
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