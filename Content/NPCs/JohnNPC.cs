using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using LK_Ugrumiy_WP.Common.Systems;
using LK_Ugrumiy_WP.Content.Items.Accessories;

namespace LK_Ugrumiy_WP.Content.NPCs
{
    /// <summary>
    /// Джон - редкий NPC, спавнящийся в пещерах. Если игрок носит его шляпу при встрече,
    /// Джон превращается в мини-босса: HP взлетает до 500, он становится враждебным,
    /// преследует игрока в стиле зомби-Fighter AI и появляется отдельный босс-бар.
    /// </summary>
    public class JohnNPC : ModNPC
    {
        private const int MiniBossLifeMax = 500;

        public override string Texture => "LK_Ugrumiy_WP/Content/NPCs/JohnNPC";

        public override LocalizedText DisplayName => Language.GetOrRegister(
            "Mods.LK_Ugrumiy_WP.NPCs.JohnNPC.DisplayName",
            () => "John");

        // localAI[0] is reused as a "transformed" flag so the boss morph happens once.
        private bool Transformed
        {
            get => NPC.localAI[0] != 0f;
            set => NPC.localAI[0] = value ? 1f : 0f;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Guide];
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
            NPC.friendly = true;
            NPC.townNPC = false;

            AIType = NPCID.Guide;
            AnimationType = NPCID.Guide;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
                new FlavorTextBestiaryInfoElement("Mods.LK_Ugrumiy_WP.Bestiary.JohnNPC")
            ]);
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

        public override void PostAI()
        {
            if (Transformed)
            {
                // Once angered we keep his fighter behavior locked in and re-target periodically
                // so he keeps chasing even after the player runs out of his initial vision.
                if (NPC.target < 0 || NPC.target == 255 || !Main.player[NPC.target].active || Main.player[NPC.target].dead)
                {
                    NPC.TargetClosest(true);
                }
                return;
            }

            int targetIdx = FindHatWearerInRange(400f);
            if (targetIdx >= 0)
            {
                BecomeMiniBoss(targetIdx);
            }
        }

        private int FindHatWearerInRange(float range)
        {
            JohnHatSystem johnSystem = ModContent.GetInstance<JohnHatSystem>();
            int best = -1;
            float bestDistSq = range * range;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (!p.active || p.dead)
                {
                    continue;
                }
                if (!johnSystem.playerWearingHat[i])
                {
                    continue;
                }
                float distSq = Vector2.DistanceSquared(NPC.Center, p.Center);
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    best = i;
                }
            }
            return best;
        }

        private void BecomeMiniBoss(int targetPlayer)
        {
            Transformed = true;

            NPC.friendly = false;
            NPC.damage = 40;
            NPC.defense = 25;
            NPC.knockBackResist = 0.2f;
            NPC.lifeMax = MiniBossLifeMax;
            NPC.life = MiniBossLifeMax;

            // Switch to a vanilla zombie-style fighter AI so John actually chases the
            // player. AIType is what tModLoader uses to dispatch vanilla AI for our NPC,
            // so changing it here (along with NPC.aiStyle and resetting NPC.ai) is what
            // actually enables the chase behavior — flipping aiStyle alone wasn't enough.
            AIType = NPCID.Zombie;
            NPC.aiStyle = NPCAIStyleID.Fighter;
            for (int k = 0; k < NPC.ai.Length; k++)
            {
                NPC.ai[k] = 0f;
            }

            // Mini-boss flag so the vanilla boss bar appears.
            NPC.boss = true;
            NPC.scale = 1.15f;

            NPC.target = targetPlayer;
            NPC.TargetClosest(true);

            string angryMsg = Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.JohnNPC.AngryChat");
            Main.NewText(angryMsg, 255, 100, 100);
            string transformMsg = Language.GetTextValue("Mods.LK_Ugrumiy_WP.Misc.JohnTransform");
            Main.NewText(transformMsg, 255, 60, 60);

            NPC.netUpdate = true;
        }

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
