using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace LK_Ugrumiy_WP.Content.Projectiles
{
    public class Ibp3000Projectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
        }

        public override void AI()
        {
            // Поворачиваем снаряд по направлению полёта
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Замедление по горизонтали (сопротивление воздуха)
            Projectile.velocity.X *= 0.98f;

            // Гравитация - снаряд постепенно падает вниз
            Projectile.velocity.Y += 0.4f;

            // Создаём след из искр при полёте
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Torch,
                    0,
                    0,
                    0,
                    default(Color),
                    0.8f
                );
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Звук взрыва при столкновении
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            // Создаем искры/частицы
            for (int i = 0; i < 30; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Torch,
                    Projectile.velocity.X * 0.2f + (Main.rand.NextFloat() - 0.5f) * 2f,
                    Projectile.velocity.Y * 0.2f + (Main.rand.NextFloat() - 0.5f) * 2f,
                    0,
                    default(Color),
                    1.5f
                );
                dust.noGravity = true;
                dust.velocity *= 2f;
            }

            // Дополнительные искры
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Smoke,
                    Projectile.velocity.X * 0.1f + (Main.rand.NextFloat() - 0.5f) * 3f,
                    Projectile.velocity.Y * 0.1f + (Main.rand.NextFloat() - 0.5f) * 3f,
                    0,
                    default(Color),
                    1f
                );
                dust.noGravity = false;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Добавляем немного искр при попадании во врага
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Torch,
                    Main.rand.NextFloat(-3f, 3f),
                    Main.rand.NextFloat(-3f, 3f),
                    0,
                    default(Color),
                    1f
                );
                dust.noGravity = true;
            }
        }
    }
}
