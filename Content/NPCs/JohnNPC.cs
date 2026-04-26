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
    /// Джон - редкий NPC, спавнящийся в пещерах (Cavern layer).
    /// Отсылка на Grizzled Jon из RDR2.
    /// Если игрок носит его шляпу при встрече - Джон станет враждебным и попытается её забрать.
    /// </summary>
    public class JohnNPC : ModNPC
    {
        // Placeholder sprite: заменить на Content/NPCs/JohnNPC.png
        public override string Texture => "LK_Ugrumiy_WP/Content/NPCs/JohnNPC";

        public override LocalizedText DisplayName => Language.GetOrRegister(
            "Mods.LK_Ugrumiy_WP.NPCs.JohnNPC.DisplayName",
            () => "John");

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
            // ~14% шанс выпадения шляпы (1 из 7)
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<JohnsHat>(), 7));
        }

        public override void PostAI()
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active && !player.dead && Vector2.Distance(NPC.position, player.position) < 400)
                {
                    var johnSystem = ModContent.GetInstance<JohnHatSystem>();
                    if (johnSystem.playerWearingHat[i])
                    {
                        NPC.friendly = false;
                        NPC.damage = 40;
                        NPC.aiStyle = NPCAIStyleID.Fighter;

                        if (NPC.localAI[0] == 0f)
                        {
                            string msg = Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.JohnNPC.AngryChat");
                            Main.NewText(msg, 255, 100, 100);
                            NPC.localAI[0] = 1f;
                        }
                        break;
                    }
                }
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // Спавн только в пещерах (Cavern layer)
            if (!spawnInfo.Player.ZoneRockLayerHeight || spawnInfo.Player.ZoneDungeon)
                return 0f;

            // Не во время событий
            if (spawnInfo.Player.ZoneTowerNebula || spawnInfo.Player.ZoneTowerVortex ||
                spawnInfo.Player.ZoneTowerSolar || spawnInfo.Player.ZoneTowerStardust ||
                Main.invasionType > 0)
                return 0f;

            // Только один Джон за раз
            if (NPC.AnyNPCs(Type))
                return 0f;

            // Очень редкий спавн: 0.1% от обычного шанса
            return 0.001f;
        }
    }
}
