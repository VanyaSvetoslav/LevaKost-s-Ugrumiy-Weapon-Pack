using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Items.Consumables
{
	/// <summary>
	/// Бургер: при каждом съедении увеличивает уровень жира.
	/// Персонаж постепенно толстеет, получает бонусы и штрафы.
	/// </summary>
	public class FatBurger : ModItem
	{
		// Путь к собственной текстуре бургера
		public override string Texture => "LK_Ugrumiy_WP/Content/Items/Consumables/FatBurger/Burger_Zhirnosti";

		public override void SetDefaults()
		{
			Item.consumable = true;
			Item.maxStack = 99;
			Item.width = 26;
			Item.height = 26;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(silver: 50);
			Item.UseSound = SoundID.Item2;
			Item.useTime = 25;
			Item.useAnimation = 25;
			Item.useStyle = ItemUseStyleID.EatFood;
			Item.healLife = 30;
			Item.potion = false;
		}

		public override bool CanUseItem(Player player)
		{
			var fp = player.GetModPlayer<FatPlayer>();
			return fp.FatLevel < FatPlayer.MaxFat;
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				var fp = player.GetModPlayer<FatPlayer>();

				int fatGain = Main.rand.Next(5, 9);
				fp.AddFat(fatGain);

				for (int i = 0; i < 10; i++)
				{
					Dust.NewDust(player.position, player.width, player.height,
						DustID.YellowTorch, 0f, -2f, 150, default, 1.2f);
				}

				string key;
				Color color;
				if (fp.FatLevel >= 90) { key = "BurgerEat90"; color = new Color(255, 100, 100); }
				else if (fp.FatLevel >= 60) { key = "BurgerEat60"; color = new Color(255, 180, 50); }
				else if (fp.FatLevel >= 30) { key = "BurgerEat30"; color = new Color(255, 220, 100); }
				else { key = "BurgerEat0"; color = new Color(255, 255, 150); }

				Main.NewText(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Misc." + key), color.R, color.G, color.B);
			}

			return true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(3)
				.AddIngredient(ItemID.Hay, 10)
				.AddIngredient(ItemID.GoldCoin, 1)
				.AddTile(TileID.CookingPots)
				.Register();
		}
	}

	/// <summary>
	/// Диетическая таблетка: снижает жир.
	/// </summary>
	public class DietPill : ModItem
	{
		public override string Texture => "LK_Ugrumiy_WP/Content/Items/Consumables/FatBurger/Diet_Pill";

		public override void SetDefaults()
		{
			Item.consumable = true;
			Item.maxStack = 30;
			Item.width = 20;
			Item.height = 26;
			Item.rare = ItemRarityID.Lime;
			Item.value = Item.buyPrice(gold: 1);
			Item.UseSound = SoundID.Item3;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.DrinkLiquid;
		}

		public override bool CanUseItem(Player player)
		{
			var fp = player.GetModPlayer<FatPlayer>();
			return fp.FatLevel > 0;
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				var fp = player.GetModPlayer<FatPlayer>();
				fp.RemoveFat(15);
				Main.NewText(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Misc.DietPillUse"), 100, 255, 100);

				for (int i = 0; i < 15; i++)
				{
					Dust.NewDust(player.position, player.width, player.height,
						DustID.GreenTorch, 0f, -3f, 100, default, 1.5f);
				}
			}

			return true;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.BottledWater, 1)
				.AddIngredient(ItemID.Daybloom, 3)
				.AddIngredient(ItemID.Blinkroot, 2)
				.AddTile(TileID.Bottles)
				.Register();
		}
	}
}