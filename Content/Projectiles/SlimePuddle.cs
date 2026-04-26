using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LK_Ugrumiy_WP.Content.Projectiles
{
	/// <summary>
	/// Лужа белой слизи: появляется при столкновении SlimeGlob с тайлом.
	/// Растекается в ширину, замедляет врагов, стоящих на ней, затем исчезает.
	/// </summary>
	public class SlimePuddle : ModProjectile
	{
		// Используем спрайт снежка — белый, нейтральный
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SnowBallFriendly;

		/// <summary>Текущая ширина лужи (растёт со временем).</summary>
		private float puddleWidth = 8f;

		/// <summary>Максимальная ширина, до которой лужа растекается.</summary>
		private const float MaxPuddleWidth = 80f;

		/// <summary>Скорость растекания в пикселях за тик.</summary>
		private const float SpreadSpeed = 1.5f;

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 300;
			Projectile.alpha = 60;

			Projectile.velocity = Vector2.Zero;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
		}

		public override void AI()
		{
			if (puddleWidth < MaxPuddleWidth)
			{
				puddleWidth += SpreadSpeed;
				if (puddleWidth > MaxPuddleWidth)
					puddleWidth = MaxPuddleWidth;
			}

			Projectile.width = (int)puddleWidth;
			Projectile.height = 8;

			Projectile.position.X = Projectile.Center.X - puddleWidth / 2f;

			if (!IsOnSolidGround())
			{
				Projectile.position.Y += 2f;
			}

			if (Main.rand.NextBool(6))
			{
				Vector2 dustPos = new Vector2(
					Projectile.position.X + Main.rand.NextFloat(puddleWidth),
					Projectile.position.Y
				);

				Dust dust = Dust.NewDustDirect(
					dustPos,
					4, 4,
					DustID.TintableDust,
					0f,
					Main.rand.NextFloat(-0.5f, -0.1f),
					200,
					new Color(240, 240, 255),
					0.6f
				);
				dust.noGravity = true;
				dust.velocity *= 0.3f;
			}

			if (Projectile.timeLeft < 60)
			{
				Projectile.alpha = (int)MathHelper.Lerp(60, 255, 1f - Projectile.timeLeft / 60f);
			}
		}

		private bool IsOnSolidGround()
		{
			int tileX = (int)(Projectile.Center.X / 16f);
			int tileY = (int)((Projectile.position.Y + Projectile.height + 2) / 16f);

			if (tileX < 0 || tileX >= Main.maxTilesX || tileY < 0 || tileY >= Main.maxTilesY)
				return false;

			Tile tile = Main.tile[tileX, tileY];
			return tile.HasTile && Main.tileSolid[tile.TileType];
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.Slow, 90);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Rectangle puddleRect = new Rectangle(
				(int)(Projectile.Center.X - puddleWidth / 2f),
				(int)Projectile.position.Y - 4,
				(int)puddleWidth,
				12
			);
			return puddleRect.Intersects(targetHitbox);
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(puddleWidth);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			puddleWidth = reader.ReadSingle();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;

			float scaleX = puddleWidth / texture.Width;
			float scaleY = 0.3f;

			float opacity = (255 - Projectile.alpha) / 255f;
			Color whiteSlime = new Color(245, 245, 255) * opacity;

			Main.EntitySpriteDraw(
				texture,
				drawPos,
				null,
				whiteSlime,
				0f,
				new Vector2(texture.Width / 2f, texture.Height / 2f),
				new Vector2(scaleX, scaleY),
				SpriteEffects.None,
				0
			);

			return false;
		}
	}
}