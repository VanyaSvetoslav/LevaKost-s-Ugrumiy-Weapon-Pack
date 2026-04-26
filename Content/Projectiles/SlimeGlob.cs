using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LK_Ugrumiy_WP.Content.Projectiles
{
	/// <summary>
	/// Капля белой слизи: летит по баллистике, при столкновении с тайлом
	/// создаёт лужу (SlimePuddle), которая растекается и замедляет врагов.
	/// </summary>
	public class SlimeGlob : ModProjectile
	{
		// Используем спрайт снежка — белый, круглый, компактный
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SnowBallFriendly;

		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.tileCollide = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.alpha = 50;
		}

		public override void AI()
		{
			// Гравитация — капля падает
			Projectile.velocity.Y += 0.25f;

			// Вращение по направлению полёта
			Projectile.rotation += 0.1f;

			// Белый след слизи при полёте
			if (Main.rand.NextBool(2))
			{
				Dust dust = Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.TintableDust,
					Projectile.velocity.X * 0.1f,
					Projectile.velocity.Y * 0.1f,
					200,
					new Color(240, 240, 255),
					0.8f
				);
				dust.noGravity = true;
				dust.velocity *= 0.2f;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);

			// Белая слизь с лёгкой прозрачностью
			float opacity = (255 - Projectile.alpha) / 255f;
			Color whiteSlime = new Color(245, 245, 255) * opacity;

			Main.EntitySpriteDraw(
				texture,
				drawPos,
				null,
				whiteSlime,
				Projectile.rotation,
				origin,
				1f,
				SpriteEffects.None,
				0
			);

			return false;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			// При столкновении с поверхностью — создаём лужу
			Projectile.NewProjectile(
				Projectile.GetSource_FromThis(),
				Projectile.Center,
				Vector2.Zero,
				ModContent.ProjectileType<SlimePuddle>(),
				Projectile.damage / 3,
				0f,
				Projectile.owner
			);

			// Звук шлепка
			Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 0.5f, Pitch = 0.5f }, Projectile.position);

			// Частицы разбрызгивания — белые
			for (int i = 0; i < 8; i++)
			{
				Dust dust = Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.TintableDust,
					Main.rand.NextFloat(-2f, 2f),
					Main.rand.NextFloat(-2f, 0.5f),
					180,
					new Color(240, 240, 255),
					1.2f
				);
				dust.noGravity = false;
			}

			return true;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.Slow, 120);

			for (int i = 0; i < 10; i++)
			{
				Dust dust = Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.TintableDust,
					Main.rand.NextFloat(-2f, 2f),
					Main.rand.NextFloat(-2f, 2f),
					180,
					new Color(240, 240, 255),
					1f
				);
				dust.noGravity = true;
			}
		}
	}
}