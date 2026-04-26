using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace LK_Ugrumiy_WP.Content.Items.Consumables
{
	/// <summary>
	/// UI-элемент: шкала жира, отображаемая на экране.
	/// </summary>
	public class FatBarUIState : UIState
	{
		public override void Draw(SpriteBatch spriteBatch)
		{
			Player player = Main.LocalPlayer;
			var fp = player.GetModPlayer<FatPlayer>();

			// Не показываем, если жир = 0
			if (fp.FatLevel <= 0f)
				return;

			// Позиция бара (справа от мини-карты)
			float screenX = Main.screenWidth - 260f;
			float screenY = 80f;

			float barWidth = 200f;
			float barHeight = 20f;
			float fillRatio = fp.FatLevel / FatPlayer.MaxFat;

			// Фон бара (тёмный)
			Rectangle bgRect = new Rectangle((int)screenX, (int)screenY, (int)barWidth, (int)barHeight);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, bgRect, Color.Black * 0.7f);

			// Заполнение бара (цвет зависит от уровня)
			Color barColor = GetFatColor(fp.FatStage);
			Rectangle fillRect = new Rectangle(
				(int)screenX + 2,
				(int)screenY + 2,
				(int)((barWidth - 4) * fillRatio),
				(int)barHeight - 4
			);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, fillRect, barColor);

			// Рамка
			DrawBorder(spriteBatch, bgRect, Color.White * 0.5f);

			// Иконка бургера слева от бара
			string icon = GetFatIcon(fp.FatStage);
			Vector2 iconPos = new Vector2(screenX - 25f, screenY - 2f);
			Utils.DrawBorderString(spriteBatch, icon, iconPos, Color.White, 1f);

			// Текст: "Fat: 45/100"
			string text = $"Fat: {(int)fp.FatLevel}/{(int)FatPlayer.MaxFat}";
			Vector2 textPos = new Vector2(screenX + barWidth / 2f, screenY + barHeight + 4f);
			Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text);
			Utils.DrawBorderString(spriteBatch, text, textPos - new Vector2(textSize.X / 2f, 0f),
				GetFatColor(fp.FatStage), 0.8f);

			// Подпись стадии
			string stageName = GetStageName(fp.FatStage);
			Vector2 stagePos = new Vector2(screenX + barWidth / 2f, screenY - 18f);
			Vector2 stageSize = FontAssets.MouseText.Value.MeasureString(stageName);
			Utils.DrawBorderString(spriteBatch, stageName,
				stagePos - new Vector2(stageSize.X / 2f * 0.7f, 0f),
				Color.White * 0.9f, 0.7f);
		}

		private Color GetFatColor(int stage)
		{
			return stage switch
			{
				0 => new Color(150, 255, 150),   // Зелёный — нормально
				1 => new Color(255, 255, 100),   // Жёлтый — слегка
				2 => new Color(255, 180, 50),    // Оранжевый — полный
				3 => new Color(255, 100, 50),    // Красно-оранжевый
				_ => new Color(255, 50, 50),     // Красный — опасно
			};
		}

		private string GetFatIcon(int stage)
		{
			return stage switch
			{
				0 => "~",
				1 => "o",
				2 => "O",
				3 => "@",
				_ => "#",
			};
		}

		private string GetStageName(int stage)
		{
			return stage switch
			{
				0 => "Normal",
				1 => "Chubby",
				2 => "Fat",
				3 => "Obese",
				_ => "Extremely Obese",
			};
		}

		private void DrawBorder(SpriteBatch sb, Rectangle rect, Color color)
		{
			int t = 1;
			sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rect.X, rect.Y, rect.Width, t), color);
			sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rect.X, rect.Bottom - t, rect.Width, t), color);
			sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rect.X, rect.Y, t, rect.Height), color);
			sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rect.Right - t, rect.Y, t, rect.Height), color);
		}
	}

	/// <summary>
	/// Регистрирует UI-шкалу жира в игре.
	/// </summary>
	public class FatBarUISystem : ModSystem
	{
		private UserInterface fatBarInterface;
		private FatBarUIState fatBarState;

		public override void Load()
		{
			if (!Main.dedServ)
			{
				fatBarState = new FatBarUIState();
				fatBarInterface = new UserInterface();
				fatBarInterface.SetState(fatBarState);
			}
		}

		public override void UpdateUI(GameTime gameTime)
		{
			fatBarInterface?.Update(gameTime);
		}

		public override void ModifyInterfaceLayers(System.Collections.Generic.List<GameInterfaceLayer> layers)
		{
			int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
			if (resourceBarIndex != -1)
			{
				layers.Insert(resourceBarIndex + 1, new LegacyGameInterfaceLayer(
					"LK_Ugrumiy_WP: Fat Bar",
					delegate
					{
						fatBarInterface?.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI
				));
			}
		}
	}
}