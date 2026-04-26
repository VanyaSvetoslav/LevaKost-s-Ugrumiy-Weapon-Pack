using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace LK_Ugrumiy_WP.Content.NPCs
{
	/// <summary>
	/// NPC-корова. Выглядит как корова, но в диалогах утверждает, что он бык.
	/// Даёт молоко по кнопке "Grab Milk".
	/// Приходит только если у игрока в инвентаре есть 1+ сена (Hay).
	/// </summary>
	[AutoloadHead]
	public class CowNPC : ModNPC
	{
		// Собственный спрайтшит коровы (25 фреймов)
		public override string Texture => "LK_Ugrumiy_WP/Content/NPCs/CowNPC";

		public override LocalizedText DisplayName => Language.GetOrRegister(
			"Mods.LK_Ugrumiy_WP.NPCs.CowNPC.DisplayName",
			() => "Cow");

		/// <summary>Кулдаун выдачи молока (в тиках).</summary>
		private int milkCooldown = 0;

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Guide];

			NPCID.Sets.ExtraFramesCount[Type] = 9;
			NPCID.Sets.AttackFrameCount[Type] = 4;
			NPCID.Sets.DangerDetectRange[Type] = 700;
			NPCID.Sets.AttackType[Type] = 0;
			NPCID.Sets.AttackTime[Type] = 90;
			NPCID.Sets.AttackAverageChance[Type] = 30;
			NPCID.Sets.HatOffsetY[Type] = 4;

			// Важно для корректной работы городского NPC
			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
			{
				Velocity = 1f
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}

		public override void SetDefaults()
		{
			NPC.width = 18;
			NPC.height = 40;
			NPC.damage = 10;
			NPC.defense = 15;
			NPC.lifeMax = 500;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.5f;
			NPC.friendly = true;
			NPC.townNPC = true;

			AnimationType = NPCID.Guide;
		}

		public override bool CanTownNPCSpawn(int numTownNPCs)
		{
			for (int i = 0; i < Main.maxPlayers; i++)
			{
				Player player = Main.player[i];
				if (player.active && !player.dead)
				{
					if (PlayerHasHay(player))
						return true;
				}
			}
			return false;
		}

		public override bool CheckConditions(int left, int right, int top, int bottom)
		{
			return true;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				new FlavorTextBestiaryInfoElement("Mods.LK_Ugrumiy_WP.Bestiary.CowNPC")
			});
		}

		private static bool PlayerHasHay(Player player)
		{
			for (int i = 0; i < player.inventory.Length; i++)
			{
				if (player.inventory[i].type == ItemID.Hay && player.inventory[i].stack >= 1)
					return true;
			}
			return false;
		}

		public override void PostAI()
		{
			if (milkCooldown > 0)
				milkCooldown--;
		}

		public override string GetChat()
		{
			WeightedRandom<string> chat = new WeightedRandom<string>();

			chat.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.CowNPC.Standard1"));
			chat.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.CowNPC.Standard2"));
			chat.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.CowNPC.Standard3"));
			chat.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.CowNPC.Standard4"));
			chat.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.CowNPC.Standard5"));
			chat.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.CowNPC.Standard6"));

			return chat;
		}

		public override void SetChatButtons(ref string button, ref string button2)
		{
			button = Language.GetTextValue("Mods.LK_Ugrumiy_WP.UI.GrabMilk");
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shop)
		{
			if (!firstButton)
				return;

			Player player = Main.LocalPlayer;

			if (milkCooldown > 0)
			{
				Main.npcChatText = Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.CowNPC.Cooldown");
				return;
			}

			int milkType = ModContent.ItemType<Items.Consumables.CowMilk>();
			player.QuickSpawnItem(NPC.GetSource_GiftOrReward(), milkType);

			milkCooldown = 300;

			WeightedRandom<string> reaction = new WeightedRandom<string>();
			reaction.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.CowNPC.Milk1"));
			reaction.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.CowNPC.Milk2"));
			reaction.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.CowNPC.Milk3"));
			reaction.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.CowNPC.Milk4"));

			Main.npcChatText = reaction;
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			int num = NPC.life > 0 ? 1 : 5;
			for (int k = 0; k < num; k++)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood);
			}
		}

		public override void TownNPCAttackStrength(ref int damage, ref float knockback)
		{
			damage = 25;
			knockback = 6f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
		{
			cooldown = 30;
			randExtraCooldown = 30;
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
		{
			projType = ProjectileID.Boulder;
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
		{
			multiplier = 10f;
			gravityCorrection = 2f;
			randomOffset = 2f;
		}

		public override List<string> SetNPCNameList()
		{
			return new List<string>
			{
				"Bull",
				"Bully",
				"Ferdinand",
				"Angus",
				"Hereford",
				"Chuck",
				"Beefcake",
				"Sir Moo",
				"El Toro",
				"Rodeo"
			};
		}
	}
}