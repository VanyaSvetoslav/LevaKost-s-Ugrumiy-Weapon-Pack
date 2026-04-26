using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Projectiles
{
    /// <summary>
    /// A magical "VXE R1 SE+" gaming mouse that flies after the player's cursor with a
    /// wobbly animation. It deals magic damage on contact, cannot pass through tiles,
    /// and shatters if it slams into a wall or floor at high speed.
    /// </summary>
    public class VxeMouseProjectile : ModProjectile
    {
        // Hard-impact threshold (pre-collision speed in px/tick).
        private const float BreakSpeed = 11f;
        // Soft-impact bounce/slide damping.
        private const float BounceDamping = 0.35f;
        // Spring/damper coefficients for cursor follow.
        private const float SpringStrength = 0.45f;
        private const float Damping = 0.86f;
        private const float MaxSpeed = 16f;
        // How close to the cursor before easing off.
        private const float DeadZone = 6f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 18;
        }

        // Animation / wobble timer (always advances locally, no need to sync).
        private ref float WobbleTimer => ref Projectile.localAI[0];

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (owner == null || !owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            WobbleTimer += 1f;

            // Only the owning client drives motion toward the cursor; other clients
            // interpolate via vanilla projectile sync.
            if (Projectile.owner == Main.myPlayer)
            {
                Vector2 target = Main.MouseWorld;
                Vector2 toTarget = target - Projectile.Center;
                float dist = toTarget.Length();

                if (dist > DeadZone)
                {
                    Vector2 dir = toTarget / dist;
                    // Spring-style acceleration: stronger when far, eases when near.
                    float pull = MathHelper.Clamp(dist / 60f, 0.2f, 1.5f) * SpringStrength;
                    Projectile.velocity += dir * pull;
                }

                Projectile.velocity *= Damping;

                if (Projectile.velocity.Length() > MaxSpeed)
                {
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxSpeed;
                }

                // Periodically broadcast position so other clients see it follow.
                if (Projectile.timeLeft % 6 == 0)
                {
                    Projectile.netUpdate = true;
                }
            }

            // Visual rotation: face direction of motion plus a wobble.
            float baseRot = Projectile.velocity.X * 0.05f;
            float wobble = (float)Math.Sin(WobbleTimer * 0.22f) * 0.18f;
            Projectile.rotation = baseRot + wobble;

            // Subtle cyan ambient sparkle.
            if (Main.rand.NextBool(8))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RainbowMk2, 0f, 0f, 150, new Color(0, 220, 255), 0.7f);
                d.noGravity = true;
                d.velocity *= 0.2f;
            }

            // Light source so it reads as magical.
            Lighting.AddLight(Projectile.Center, 0.0f, 0.45f, 0.55f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            float speed = oldVelocity.Length();
            if (speed >= BreakSpeed)
            {
                Shatter();
                return false;
            }

            // Soft bounce: invert the axis that hit and damp it.
            if (oldVelocity.X != Projectile.velocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X * BounceDamping;
            }
            if (oldVelocity.Y != Projectile.velocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y * BounceDamping;
            }

            // A tiny bonk poof.
            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, oldVelocity.X * -0.2f, oldVelocity.Y * -0.2f, 120, default, 0.8f);
                d.noGravity = true;
            }

            return false;
        }

        private void Shatter()
        {
            SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
            SoundEngine.PlaySound(SoundID.Item107, Projectile.position);

            for (int i = 0; i < 26; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RainbowMk2,
                    Main.rand.NextFloat(-4f, 4f),
                    Main.rand.NextFloat(-4f, 4f),
                    100, new Color(0, 220, 255), 1.2f);
                d.noGravity = true;
            }
            for (int i = 0; i < 14; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke,
                    Main.rand.NextFloat(-2f, 2f),
                    Main.rand.NextFloat(-2f, 2f),
                    150, default, 1.1f);
                d.noGravity = false;
            }

            Projectile.Kill();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Slight recoil so the mouse looks like it bonks off enemies.
            Vector2 push = (Projectile.Center - target.Center).SafeNormalize(Vector2.UnitY) * 4f;
            Projectile.velocity = (Projectile.velocity * 0.4f) + push;

            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RainbowMk2,
                    Main.rand.NextFloat(-3f, 3f),
                    Main.rand.NextFloat(-3f, 3f),
                    120, new Color(0, 220, 255), 1f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> texAsset = TextureAssets.Projectile[Projectile.type];
            Texture2D tex = texAsset.Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Squish/stretch wobble: x and y oscillate out of phase.
            float t = WobbleTimer;
            float sx = 1f + (float)Math.Sin(t * 0.22f) * 0.14f;
            float sy = 1f + (float)Math.Cos(t * 0.22f) * 0.14f;
            Vector2 scale = new Vector2(sx, sy) * Projectile.scale;

            // Cyan afterimage trail.
            for (int i = 1; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
            {
                Vector2 oldPos = Projectile.oldPos[i];
                if (oldPos == Vector2.Zero)
                {
                    continue;
                }
                Vector2 trailDraw = oldPos + Projectile.Size / 2f - Main.screenPosition;
                float fade = 1f - (i / (float)ProjectileID.Sets.TrailCacheLength[Projectile.type]);
                Color trailColor = new Color(0, 220, 255) * (fade * 0.45f);
                Main.EntitySpriteDraw(tex, trailDraw, null, trailColor, Projectile.rotation, origin, scale * fade, SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(tex, drawPos, null, lightColor, Projectile.rotation, origin, scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
