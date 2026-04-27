using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace LK_Ugrumiy_WP.Content.NPCs
{
	/// <summary>
	/// Механик-Мерриуэзер — городской NPC, заселяется после убийства Стены плоти.
	/// Начальный ассортимент копирует стандартного ванильного Механика; позже
	/// сюда будут добавлены кастомные вещи из мода.
	/// </summary>
	[AutoloadHead]
	public class MechanicMerriweatherNPC : ModNPC
	{
		public const string ShopName = "Shop";

		public override string Texture => "LK_Ugrumiy_WP/Content/NPCs/MechanicMerriweatherNPC";

		public override LocalizedText DisplayName => Language.GetOrRegister(
			"Mods.LK_Ugrumiy_WP.NPCs.MechanicMerriweatherNPC.DisplayName",
			() => "Mechanic-Merriweather");

		public override void SetStaticDefaults()
		{
			// Matches the Guide's frame layout (same sheet dimensions as CowNPC),
			// otherwise the sprite drifts vertically while walking because the
			// game divides texture height by the wrong frame count.
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Guide];

			NPCID.Sets.ExtraFramesCount[Type] = 9;
			NPCID.Sets.AttackFrameCount[Type] = 4;
			NPCID.Sets.DangerDetectRange[Type] = 700;
			NPCID.Sets.AttackType[Type] = 0;
			NPCID.Sets.AttackTime[Type] = 90;
			NPCID.Sets.AttackAverageChance[Type] = 30;
			NPCID.Sets.HatOffsetY[Type] = 4;

			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
			{
				Velocity = 1f
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

			// Счастье: базовые предпочтения, ориентированы на ванильного Механика.
			NPC.Happiness
				.SetBiomeAffection<SnowBiome>(AffectionLevel.Like)
				.SetBiomeAffection<HallowBiome>(AffectionLevel.Dislike)
				.SetNPCAffection(NPCID.GoblinTinkerer, AffectionLevel.Love)
				.SetNPCAffection(NPCID.Cyborg, AffectionLevel.Like)
				.SetNPCAffection(NPCID.ArmsDealer, AffectionLevel.Dislike)
				.SetNPCAffection(NPCID.Wizard, AffectionLevel.Hate);
		}

		public override void SetDefaults()
		{
			NPC.townNPC = true;
			NPC.friendly = true;
			NPC.width = 18;
			NPC.height = 40;
			NPC.aiStyle = NPCAIStyleID.Passive;
			NPC.damage = 10;
			NPC.defense = 15;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.5f;

			AnimationType = NPCID.Guide;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				new FlavorTextBestiaryInfoElement("Mods.LK_Ugrumiy_WP.Bestiary.MechanicMerriweatherNPC")
			});
		}

		/// <summary>
		/// Условие появления: убита Стена плоти (начало хардмода).
		/// </summary>
		public override bool CanTownNPCSpawn(int numTownNPCs)
		{
			return Main.hardMode;
		}

		public override List<string> SetNPCNameList()
		{
			return new List<string>
			{
				"Merriweather",
				"Мерриуэзер",
				"Gearheart",
				"Sparkwright",
				"Boltholomew",
				"Wrenchley",
				"Coggins",
				"Voltaire",
				"Circuitry",
				"Ampersand"
			};
		}

		public override string GetChat()
		{
			WeightedRandom<string> chat = new WeightedRandom<string>();
			chat.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.MechanicMerriweatherNPC.Standard1"));
			chat.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.MechanicMerriweatherNPC.Standard2"));
			chat.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.MechanicMerriweatherNPC.Standard3"));
			chat.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.MechanicMerriweatherNPC.Standard4"));
			chat.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.MechanicMerriweatherNPC.Standard5"));
			chat.Add(Language.GetTextValue("Mods.LK_Ugrumiy_WP.Dialogue.MechanicMerriweatherNPC.Rare"), 0.1);
			return chat;
		}

		public override void SetChatButtons(ref string button, ref string button2)
		{
			button = Language.GetTextValue("LegacyInterface.28"); // "Shop"
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shop)
		{
			if (firstButton)
			{
				shop = ShopName;
			}
		}

		/// <summary>
		/// Стандартный ассортимент «ванильного» Механика. Позже сюда будут
		/// добавлены кастомные предметы мода.
		/// </summary>
		public override void AddShops()
		{
			NPCShop npcShop = new NPCShop(Type, ShopName)
				.Add(new Item(ItemID.Wrench))
				.Add(new Item(ItemID.BlueWrench))
				.Add(new Item(ItemID.GreenWrench))
				.Add(new Item(ItemID.YellowWrench))
				.Add(new Item(ItemID.MulticolorWrench))
				.Add(new Item(ItemID.WireCutter))
				.Add(new Item(ItemID.Wire))
				.Add(new Item(ItemID.Actuator))
				.Add(new Item(ItemID.Lever))
				.Add(new Item(ItemID.Switch))
				.Add(new Item(ItemID.RedPressurePlate))
				.Add(new Item(ItemID.GreenPressurePlate))
				.Add(new Item(ItemID.GrayPressurePlate))
				.Add(new Item(ItemID.BrownPressurePlate))
				.Add(new Item(ItemID.BluePressurePlate))
				.Add(new Item(ItemID.YellowPressurePlate))
				.Add(new Item(ItemID.OrangePressurePlate))
				.Add(new Item(ItemID.ProjectilePressurePad))
				.Add(new Item(ItemID.MechanicalLens), Condition.DownedMechBossAny)
				.Add(new Item(ItemID.LaserRuler), Condition.DownedMechBossAny);

			npcShop.Register();
		}

		public override void TownNPCAttackStrength(ref int damage, ref float knockback)
		{
			damage = 20;
			knockback = 4f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
		{
			cooldown = 30;
			randExtraCooldown = 30;
		}

		public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
		{
			projType = ProjectileID.Bullet;
			attackDelay = 1;
		}

		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
		{
			multiplier = 12f;
			randomOffset = 2f;
		}
	}
}
