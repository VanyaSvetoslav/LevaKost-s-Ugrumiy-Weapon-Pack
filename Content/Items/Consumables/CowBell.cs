using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Items.Consumables
{
	/// <summary>
	/// Колокольчик для призыва коровы. Для тестирования.
	/// </summary>
	public class CowBell : ModItem
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.Bell;

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 20;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item35;
			Item.consumable = false;
			Item.maxStack = 1;
		}

		public override bool? UseItem(Player player)
		{
			if (player.whoAmI == Main.myPlayer)
			{
				int npcType = ModContent.NPCType<NPCs.CowNPC>();

				// Спавним корову рядом с игроком
				NPC.NewNPC(
					player.GetSource_ItemUse(Item),
					(int)player.Center.X + 100,
					(int)player.Center.Y,
					npcType
				);

				Main.NewText("A cow appeared!", 100, 255, 100);
			}
			return true;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Hay, 1)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}