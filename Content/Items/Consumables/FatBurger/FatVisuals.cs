using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Items.Consumables
{
	public class FatPlayerScale : ModPlayer
	{
		internal float scaleX = 1f;
		internal float scaleY = 1f;

		public override void FrameEffects()
		{
			var fp = Player.GetModPlayer<FatPlayer>();
			if (fp.FatLevel <= 0f) return;

			float ratio = fp.FatLevel / FatPlayer.MaxFat;
			int extraWidth = (int)(ratio * Player.defaultWidth * 0.3f);
			Player.width = Player.defaultWidth + extraWidth;
		}

		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
			var fp = drawInfo.drawPlayer.GetModPlayer<FatPlayer>();
			if (fp.FatLevel <= 5f) return;

			float ratio = fp.FatLevel / FatPlayer.MaxFat;

			float sx = 1f + ratio * 0.35f;
			float sy = 1f + ratio * 0.1f;

			drawInfo.Position.X -= (sx - 1f) * drawInfo.drawPlayer.width * 0.5f;
			drawInfo.Position.Y -= (sy - 1f) * drawInfo.drawPlayer.height;

			drawInfo.drawPlayer.GetModPlayer<FatPlayerScale>().scaleX = sx;
			drawInfo.drawPlayer.GetModPlayer<FatPlayerScale>().scaleY = sy;
		}
	}

	public class FatDrawLayer : PlayerDrawLayer
	{
		public override Position GetDefaultPosition()
		{
			// AfterParent(HeldItem) — один из последних слоёв отрисовки
			return new AfterParent(PlayerDrawLayers.HeldItem);
		}

		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
		{
			return drawInfo.drawPlayer.GetModPlayer<FatPlayer>().FatLevel > 5f;
		}

		protected override void Draw(ref PlayerDrawSet drawInfo)
		{
			var fp = drawInfo.drawPlayer.GetModPlayer<FatPlayer>();
			var fatScale = drawInfo.drawPlayer.GetModPlayer<FatPlayerScale>();

			if (fp.FatLevel <= 5f) return;

			float sX = fatScale.scaleX;
			float sY = fatScale.scaleY;

			Vector2 feetCenter = drawInfo.Position - Main.screenPosition
				+ new Vector2(drawInfo.drawPlayer.width / 2f, drawInfo.drawPlayer.height);

			for (int i = 0; i < drawInfo.DrawDataCache.Count; i++)
			{
				DrawData data = drawInfo.DrawDataCache[i];

				Vector2 offset = data.position - feetCenter;
				offset.X *= sX;
				offset.Y *= sY;
				data.position = feetCenter + offset;

				if (data.scale.X != 0f && data.scale.Y != 0f)
				{
					data.scale = new Vector2(data.scale.X * sX, data.scale.Y * sY);
				}

				drawInfo.DrawDataCache[i] = data;
			}

			if (fp.FatLevel > 40f)
			{
				float ratio = fp.FatLevel / FatPlayer.MaxFat;

				Vector2 bellyPos = feetCenter - new Vector2(0f, drawInfo.drawPlayer.height * 0.45f * sY);

				float bW = (6f + ratio * 12f) * sX;
				float bH = (3f + ratio * 8f) * sY;

				Color bellyColor = Color.Lerp(
					new Color(255, 230, 200),
					new Color(255, 200, 160),
					ratio
				) * (0.15f + ratio * 0.15f);

				Rectangle bellyRect = new Rectangle(
					(int)(bellyPos.X - bW / 2f),
					(int)(bellyPos.Y - bH / 2f),
					(int)bW,
					(int)bH
				);

				drawInfo.DrawDataCache.Add(new DrawData(
					TextureAssets.MagicPixel.Value,
					bellyRect,
					new Rectangle(0, 0, 1, 1),
					bellyColor,
					0f,
					Vector2.Zero,
					SpriteEffects.None,
					0
				));
			}
		}
	}

	public class FatEffects : ModPlayer
	{
		private bool wasOnGround;

		public override void PostUpdate()
		{
			var fp = Player.GetModPlayer<FatPlayer>();
			if (fp.FatLevel < 40f)
			{
				wasOnGround = Player.velocity.Y == 0f;
				return;
			}

			float ratio = fp.FatLevel / FatPlayer.MaxFat;

			bool onGround = Player.velocity.Y == 0f;
			if (onGround && !wasOnGround && fp.FatLevel >= 60f)
			{
				int dustCount = 3 + (int)(ratio * 10);
				for (int i = 0; i < dustCount; i++)
				{
					Vector2 dustPos = Player.Bottom + new Vector2(Main.rand.Next(-12, 12), -2);
					Dust.NewDust(dustPos, 4, 4,
						Terraria.ID.DustID.Smoke,
						Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(0.5f, 2f),
						150, default, 0.6f + ratio * 0.4f);
				}
			}
			wasOnGround = onGround;

			if (Math.Abs(Player.velocity.X) > 3f && onGround && Main.GameUpdateCount % 8 == 0 && fp.FatLevel >= 50f)
			{
				Dust.NewDust(Player.Bottom - new Vector2(4, 0), 8, 4,
					Terraria.ID.DustID.Smoke, 0f, -0.5f, 100, default, 0.5f);
			}
		}
	}
}