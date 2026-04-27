using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using LK_Ugrumiy_WP.Common.Systems;
using LK_Ugrumiy_WP.Content.Items.Accessories;

namespace LK_Ugrumiy_WP.Content.NPCs
{
    /// <summary>
    /// Джон - редкий NPC, спавнящийся в пещерах. Спокойно стоит и комментирует.
    /// Если игрок носит его шляпу, превращается в мини-босса с собственной логикой
    /// преследования (без переключения AIType — раньше это ломалось из-за того,
    /// что ванильный Zombie/Fighter AI пишет в наши NPC.localAI[]/NPC.ai[]).
    /// </summary>
    public class JohnNPC : ModNPC
    {
        // ---- Mini-boss tuning ----
        private const int MiniBossLifeMax = 500;
        private const int MiniBossDamage = 55;
        private const int MiniBossDefense = 18;
        private const float MiniBossKnockBackResist = 0.35f;
        private const float MiniBossScale = 1.15f;
        private const float HatDetectionRange = 400f;

        // ---- Custom AI tuning ----
        // Manoeuvring numbers higher than vanilla Fighter (~3.5 max speed,
        // ~0.07 accel) so Джон-mini-boss ощущается резвее и страшнее.
        private const float Gravity = 0.42f;
        private const float MaxFallSpeed = 12f;
        private const float ChaseMaxSpeed = 6.0f;
        private const float ChaseAccel = 0.22f;
        private const float ChaseDeccel = 0.32f;
        private const float JumpStrength = -9.5f;
        private const float HighJumpStrength = -12.5f;

        public override string Texture => "LK_Ugrumiy_WP/Content/NPCs/JohnNPC";

        public override LocalizedText DisplayName => Language.GetOrRegister(
            "Mods.LK_Ugrumiy_WP.NPCs.JohnNPC.DisplayName",
            () => "John");

        // We drive everything off custom AI (aiStyle = -1) to avoid the vanilla
        // AI dispatch from clobbering our state. Stored in a private field
        // so vanilla code physically can't touch it. Synced for MP via
        // SendExtraAI/ReceiveExtraAI below.
        private bool _transformed;

        // Frame animation state (we control it ourselves because aiStyle = -1
        // means tModLoader doesn't auto-cycle frames anymore).
        private int _frameTimer;

        public override void SetStaticDefaults()
        {
            // JohnNPC.png is a 40x1400 sheet = 25 frames of 56 px. Using the
            // Guide's frame count (26) would make the game divide 1400 by 26
            // and the sprite would drift upward while walking because each
            // frame is pulled from a non-integer pixel offset.
            Main.npcFrameCount[Type] = 25;
        }

        public override void SetDefaults()
        {
            NPC.width = 18;
            NPC.height = 40;
            NPC.damage = 10;
            NPC.defense = 15;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(gold: 1);
            NPC.knockBackResist = 0.5f;
            // Stays peaceful in calm phase. Flipped to false in BecomeMiniBoss
            // so contact damage starts dealing once transformed.
            NPC.friendly = true;
            NPC.townNPC = false;
            NPC.npcSlots = 1f;
            NPC.aiStyle = -1; // Fully custom AI.

            // No AIType / AnimationType anymore — we run our own logic and our
            // own FindFrame, so vanilla NPC.ai[]/localAI[] arrays stay free for
            // our use without collision with vanilla door/jump timers.
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                new FlavorTextBestiaryInfoElement("Mods.LK_Ugrumiy_WP.Bestiary.JohnNPC")
            ]);
        }

        // ---- Chat (calm phase only) ---------------------------------------

        // John isn't a town NPC, but we still want the player to be able to
        // right-click and get a line of dialogue while he's calm. CanChat
        // defaults to NPC.townNPC; overriding it lets us opt in without
        // making him try to settle into a house.
        public override bool CanChat() => !_transformed;

        public override string GetChat()
        {
            WeightedRandom<string> chat = new WeightedRandom<string>();
            chat.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.JohnNPC.Standard1"));
            chat.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.JohnNPC.Standard2"));
            chat.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.JohnNPC.Standard3"));
            return chat;
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            // No shop / action button — just a single chat line with vanilla Quit.
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            int num = NPC.life > 0 ? 1 : 5;
            for (int k = 0; k < num; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<JohnsHat>(), 7));
        }

        public override void AI()
        {
            // Sample ground state BEFORE applying gravity. At the start of AI(),
            // a grounded NPC has velocity.Y == 0 (set by the previous frame's
            // tile collision pass); ApplyGravity() then adds Gravity to it,
            // which would clobber that signal and make every onGround check
            // below evaluate false (so John could never jump).
            bool onGround = NPC.velocity.Y == 0f;

            ApplyGravity();

            if (_transformed)
            {
                CombatAI(onGround);
            }
            else
            {
                CalmAI();
            }
        }

        private void ApplyGravity()
        {
            // Standard Terraria-ish gravity. tModLoader does NOT apply gravity
            // for us when aiStyle = -1, so we have to do it ourselves.
            NPC.velocity.Y += Gravity;
            if (NPC.velocity.Y > MaxFallSpeed)
            {
                NPC.velocity.Y = MaxFallSpeed;
            }
        }

        // ---- Calm phase ---------------------------------------------------

        private void CalmAI()
        {
            // Idle: damp horizontal velocity, look at the nearest player.
            NPC.velocity.X *= 0.85f;

            int gazeIdx = FindNearestActivePlayer(800f);
            if (gazeIdx >= 0)
            {
                NPC.direction = NPC.Center.X < Main.player[gazeIdx].Center.X ? 1 : -1;
                NPC.spriteDirection = NPC.direction;
            }

            int hatIdx = FindHatWearerInRange(HatDetectionRange);
            if (hatIdx >= 0)
            {
                BecomeMiniBoss(hatIdx);
            }
        }

        // ---- Combat phase -------------------------------------------------

        private void CombatAI(bool onGround)
        {
            // Re-target if our current target died/disconnected.
            if (NPC.target < 0 || NPC.target >= Main.maxPlayers
                || !Main.player[NPC.target].active
                || Main.player[NPC.target].dead)
            {
                NPC.TargetClosest(true);
                if (NPC.target < 0 || NPC.target >= Main.maxPlayers)
                {
                    return; // No one to chase, just sit still and gravity us down.
                }
            }

            Player target = Main.player[NPC.target];
            float dx = target.Center.X - NPC.Center.X;
            float dy = target.Center.Y - NPC.Center.Y;

            // Face the target.
            NPC.direction = dx >= 0f ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            // Horizontal pursuit: accelerate toward target, with snappy reversal.
            float wanted = NPC.direction * ChaseMaxSpeed;
            if (Math.Sign(NPC.velocity.X) != Math.Sign(wanted) && NPC.velocity.X != 0f)
            {
                // Quickly cancel opposite velocity for sharp turns.
                if (NPC.velocity.X > 0f) NPC.velocity.X = Math.Max(0f, NPC.velocity.X - ChaseDeccel);
                else NPC.velocity.X = Math.Min(0f, NPC.velocity.X + ChaseDeccel);
            }
            else
            {
                if (NPC.velocity.X < wanted) NPC.velocity.X = Math.Min(wanted, NPC.velocity.X + ChaseAccel);
                else if (NPC.velocity.X > wanted) NPC.velocity.X = Math.Max(wanted, NPC.velocity.X - ChaseAccel);
            }

            bool blockedAhead = Collision.SolidCollision(
                NPC.position + new Vector2(NPC.direction * 6f, 0f),
                NPC.width, NPC.height);

            // Jump over walls / up to elevated targets.
            if (onGround)
            {
                if (blockedAhead)
                {
                    // Standard hop.
                    NPC.velocity.Y = JumpStrength;
                    NPC.netUpdate = true;
                }
                else if (dy < -NPC.height * 1.5f && Math.Abs(dx) < 200f)
                {
                    // Target is significantly above us and roughly nearby — high jump.
                    NPC.velocity.Y = HighJumpStrength;
                    NPC.netUpdate = true;
                }
            }
        }

        // Allow falling through platforms when the player is below us (so John
        // doesn't get stuck on rope ledges chasing a player downward).
        public override bool? CanFallThroughPlatforms()
        {
            if (!_transformed) return false;
            if (NPC.target < 0 || NPC.target >= Main.maxPlayers) return false;
            Player target = Main.player[NPC.target];
            if (!target.active || target.dead) return false;
            return target.Top.Y > NPC.Bottom.Y;
        }

        // ---- Targeting helpers --------------------------------------------

        private int FindNearestActivePlayer(float range)
        {
            int best = -1;
            float bestDistSq = range * range;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (!p.active || p.dead) continue;
                float distSq = Vector2.DistanceSquared(NPC.Center, p.Center);
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    best = i;
                }
            }
            return best;
        }

        private int FindHatWearerInRange(float range)
        {
            JohnHatSystem johnSystem = ModContent.GetInstance<JohnHatSystem>();
            int best = -1;
            float bestDistSq = range * range;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (!p.active || p.dead) continue;
                if (!johnSystem.playerWearingHat[i]) continue;
                float distSq = Vector2.DistanceSquared(NPC.Center, p.Center);
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    best = i;
                }
            }
            return best;
        }

        // ---- Transformation -----------------------------------------------

        private void BecomeMiniBoss(int targetPlayer)
        {
            _transformed = true;

            NPC.friendly = false;
            NPC.damage = MiniBossDamage;
            NPC.defense = MiniBossDefense;
            NPC.knockBackResist = MiniBossKnockBackResist;
            NPC.lifeMax = MiniBossLifeMax;
            NPC.life = MiniBossLifeMax;

            // Mini-boss flag so vanilla brings up the boss bar; npcSlots taken
            // up so we don't get a swarm of zombies muddying the fight.
            NPC.boss = true;
            NPC.scale = MiniBossScale;
            NPC.npcSlots = 5f;

            NPC.target = targetPlayer;
            NPC.TargetClosest(true);

            string angryMsg = Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.JohnNPC.AngryChat");
            Main.NewText(angryMsg, 255, 100, 100);
            string transformMsg = Language.GetTextValue("Mods.LK_Ugrumiy_WP.Misc.JohnTransform");
            Main.NewText(transformMsg, 255, 60, 60);

            NPC.netUpdate = true;
        }

        // ---- Frame animation (custom because aiStyle = -1) ----------------

        public override void FindFrame(int frameHeight)
        {
            int frameCount = Main.npcFrameCount[Type];

            // Idle / mid-air pose: use frame 0 (Guide standing pose).
            bool moving = Math.Abs(NPC.velocity.X) > 0.1f && NPC.velocity.Y == 0f;

            if (!moving)
            {
                NPC.frame.Y = 0;
                _frameTimer = 0;
                return;
            }

            // Walking cycle through frames 1..frameCount-1.
            _frameTimer++;
            int ticksPerFrame = Math.Max(2, 8 - (int)Math.Abs(NPC.velocity.X));
            if (_frameTimer >= ticksPerFrame)
            {
                _frameTimer = 0;
                int nextFrame = NPC.frame.Y / frameHeight + 1;
                if (nextFrame < 1 || nextFrame >= frameCount) nextFrame = 1;
                NPC.frame.Y = nextFrame * frameHeight;
            }
        }

        // ---- Multiplayer sync ---------------------------------------------

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(_transformed);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            _transformed = reader.ReadBoolean();
            if (_transformed)
            {
                // Mirror BecomeMiniBoss server-side stat changes that aren't
                // covered by the vanilla NPC packet (friendly/boss/scale).
                NPC.friendly = false;
                NPC.boss = true;
                NPC.scale = MiniBossScale;
            }
        }

        // ---- Spawn rules --------------------------------------------------

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!spawnInfo.Player.ZoneRockLayerHeight || spawnInfo.Player.ZoneDungeon)
                return 0f;

            if (spawnInfo.Player.ZoneTowerNebula || spawnInfo.Player.ZoneTowerVortex ||
                spawnInfo.Player.ZoneTowerSolar || spawnInfo.Player.ZoneTowerStardust ||
                Main.invasionType > 0)
                return 0f;

            if (NPC.AnyNPCs(Type))
                return 0f;

            return 0.001f;
        }
    }
}
