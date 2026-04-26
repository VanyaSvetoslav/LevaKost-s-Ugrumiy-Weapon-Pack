using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace LK_Ugrumiy_WP.Content.Accessories
{
	public class CursedBandlePlayer : ModPlayer
	{
		public bool isCursed;
		public int cursedItemType;
		public Vector2 aiTargetPosition;

		private readonly CursedBandleAI ai = new CursedBandleAI();
		private int cursedSlotIndex = -1;
		private Item cursedItemBackup;
		private bool brainLoaded;

		public override void ResetEffects()
		{
			isCursed = false;
		}

		public override void SetControls()
		{
			if (!isCursed)
				return;

			// Загружаем мозг при первом включении
			if (!brainLoaded)
			{
				ai.LoadBrain();
				brainLoaded = true;
			}

			// 1. Обнуляем ввод игрока
			Player.controlLeft = false;
			Player.controlRight = false;
			Player.controlUp = false;
			Player.controlDown = false;
			Player.controlJump = false;
			Player.controlUseItem = false;
			Player.controlUseTile = false;
			Player.controlThrow = false;
			Player.controlSmart = false;
			Player.controlMount = false;
			Player.controlHook = false;
			Player.controlTorch = false;
			Player.controlQuickHeal = false;
			Player.controlQuickMana = false;
			Player.controlInv = false;

			// 2. Блокируем интерфейс
			if (Player.chest != -1)
				Player.chest = -1;
			Main.playerInventory = false;

			// 3. ИИ принимает решения
			ai.Update(Player);

			// 4. Подмена мыши на цель
			if (aiTargetPosition != Vector2.Zero)
			{
				Vector2 screenPos = aiTargetPosition - Main.screenPosition;
				Main.mouseX = (int)screenPos.X;
				Main.mouseY = (int)screenPos.Y;
			}
		}

		public override void PreUpdate()
		{
			if (cursedSlotIndex < 0 || cursedItemType == 0)
				return;

			if (Player.armor[cursedSlotIndex].type != cursedItemType && cursedItemBackup != null)
			{
				for (int i = 0; i < Player.inventory.Length; i++)
				{
					if (Player.inventory[i].type == cursedItemType)
					{
						Player.armor[cursedSlotIndex] = Player.inventory[i].Clone();
						Player.inventory[i].TurnToAir();
						return;
					}
				}

				if (Main.mouseItem.type == cursedItemType)
				{
					Player.armor[cursedSlotIndex] = Main.mouseItem.Clone();
					Main.mouseItem.TurnToAir();
					return;
				}

				Player.armor[cursedSlotIndex] = cursedItemBackup.Clone();
			}
		}

		public override void PostUpdate()
		{
			if (!isCursed)
			{
				cursedSlotIndex = -1;
				cursedItemBackup = null;
				return;
			}

			for (int i = 3; i < Player.armor.Length; i++)
			{
				if (Player.armor[i].type == cursedItemType)
				{
					cursedSlotIndex = i;
					cursedItemBackup = Player.armor[i].Clone();
					break;
				}
			}
		}

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
		{
			// ИИ получает штраф за смерть и сохраняет знания
			ai.OnDeath();

			if (cursedSlotIndex >= 0 && cursedSlotIndex < Player.armor.Length
				&& Player.armor[cursedSlotIndex].type == cursedItemType)
			{
				Player.QuickSpawnItem(Player.GetSource_Death(), Player.armor[cursedSlotIndex], Player.armor[cursedSlotIndex].stack);
				Player.armor[cursedSlotIndex].TurnToAir();
			}

			isCursed = false;
			cursedSlotIndex = -1;
			cursedItemBackup = null;
			brainLoaded = false;
			ai.Reset();
		}
	}
}