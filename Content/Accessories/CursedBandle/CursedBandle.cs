using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace LK_Ugrumiy_WP.Content.Accessories
{
	public class CursedBandle : ModItem
	{
		public override string Texture => "LK_Ugrumiy_WP/Content/Accessories/CursedBandle/CursedBandle";

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 30;
			Item.accessory = true;
			Item.rare = ItemRarityID.Red;
			Item.value = Item.sellPrice(gold: 10);
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPlayer = player.GetModPlayer<CursedBandlePlayer>();
			modPlayer.isCursed = true;
			modPlayer.cursedItemType = Item.type;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Bone, 30)
				.AddIngredient(ItemID.Cobweb, 50)
				.AddIngredient(ItemID.SoulofNight, 15)
				.AddTile(TileID.DemonAltar)
				.Register();
		}
	}
}