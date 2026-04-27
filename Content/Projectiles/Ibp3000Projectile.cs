using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace LK_Ugrumiy_WP.Content.Projectiles
{
    /// <summary>
    /// IBP-3000 — метательный блок питания. Летит с гравитацией, оставляет за собой
    /// электрические искры, при попадании громко «зап»-ает током и брызжет молниями.
    /// </summary>
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

        // Лёгкое самонаведение: каждый тик чуть-чуть подталкиваем снаряд в сторону
        // ближайшего врага в радиусе HomingRange. Сила нарочно слабая, чтобы это
        // не превратилось в seeker-snake — просто корректирует траекторию, если
        // враг рядом, а основной импульс задаётся бросом.
        private const float HomingRange = 320f;
        private const float HomingStrength = 0.45f;

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

            // Слабее сопротивление воздуха и пониженная гравитация — раньше PSU
            // ощущался слишком кирпичным; теперь летит дальше и ровнее.
            Projectile.velocity.X *= 0.992f;
            Projectile.velocity.Y += 0.22f;

            ApplyLightHoming();

            // Электрический след: ярко-голубые искры + случайные пыли молний.
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Electric,
                    0f,
                    0f,
                    100,
                    default(Color),
                    1.0f
                );
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
            if (Main.rand.NextBool(4))
            {
                Dust spark = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.MartianSaucerSpark,
                    Main.rand.NextFloat(-1f, 1f),
                    Main.rand.NextFloat(-1f, 1f),
                    150,
                    default(Color),
                    0.9f
                );
                spark.noGravity = true;
            }

            // Сам PSU слегка светится синим.
            Lighting.AddLight(Projectile.Center, 0.2f, 0.35f, 0.7f);
        }

        private void ApplyLightHoming()
        {
            int targetIdx = -1;
            float bestDistSq = HomingRange * HomingRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(Projectile)) continue;
                float distSq = Vector2.DistanceSquared(Projectile.Center, npc.Center);
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    targetIdx = i;
                }
            }
            if (targetIdx < 0) return;

            Vector2 toTarget = Main.npc[targetIdx].Center - Projectile.Center;
            if (toTarget.LengthSquared() < 0.01f) return;
            toTarget.Normalize();
            // Nudge velocity toward target without dominating the throw arc.
            Projectile.velocity += toTarget * HomingStrength;
        }

        public override void OnKill(int timeLeft)
        {
            // Звук удара током
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item93, Projectile.position);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item122, Projectile.position);

            // Большой электрический выброс
            for (int i = 0; i < 35; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Electric,
                    Projectile.velocity.X * 0.2f + (Main.rand.NextFloat() - 0.5f) * 4f,
                    Projectile.velocity.Y * 0.2f + (Main.rand.NextFloat() - 0.5f) * 4f,
                    0,
                    default(Color),
                    1.6f
                );
                dust.noGravity = true;
                dust.velocity *= 2f;
            }
            // Цветные искры от салютной платы
            for (int i = 0; i < 20; i++)
            {
                Dust spark = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.MartianSaucerSpark,
                    Main.rand.NextFloat(-3f, 3f),
                    Main.rand.NextFloat(-3f, 3f),
                    0,
                    default(Color),
                    1.2f
                );
                spark.noGravity = true;
            }
            // Лёгкий дымок
            for (int i = 0; i < 10; i++)
            {
                Dust smoke = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.Smoke,
                    Main.rand.NextFloat(-2f, 2f),
                    Main.rand.NextFloat(-2f, 2f),
                    0,
                    default(Color),
                    1f
                );
                smoke.noGravity = false;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Электрический «зап» по врагу
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item93, target.Center);
            for (int i = 0; i < 18; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    target.position,
                    target.width,
                    target.height,
                    DustID.Electric,
                    Main.rand.NextFloat(-3f, 3f),
                    Main.rand.NextFloat(-3f, 3f),
                    100,
                    default(Color),
                    1.2f
                );
                dust.noGravity = true;
            }
        }
    }
}
