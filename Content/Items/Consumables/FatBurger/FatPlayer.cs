using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace LK_Ugrumiy_WP.Content.Items.Consumables
{
	public class FatPlayer : ModPlayer
	{
		public float FatLevel;
		public const float MaxFat = 100f;

		public int FatStage => FatLevel switch
		{
			< 10f => 0,
			< 30f => 1,
			< 55f => 2,
			< 80f => 3,
			_ => 4
		};

		private const float DeathFatLoss = 25f;
		private float burnAccumulator;

		public void AddFat(float amount)
		{
			FatLevel = Math.Min(MaxFat, FatLevel + amount);
			burnAccumulator = 0f;
		}

		public void RemoveFat(float amount)
		{
			FatLevel = Math.Max(0f, FatLevel - amount);
		}

		public void ResetFat()
		{
			FatLevel = 0f;
			burnAccumulator = 0f;
		}

		public override void ResetEffects()
		{
			ApplyFatEffects();
		}

		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
		{
			if (FatLevel > 0)
			{
				FatLevel = Math.Max(0f, FatLevel - DeathFatLoss);
				Main.NewText($"You lost some fat! (-{(int)DeathFatLoss})", 200, 200, 100);
			}
		}

		public override void PostUpdate()
		{
			if (FatLevel <= 0) return;

			float burnRate = 0f;

			// Базовое сжигание: всегда тикает (очень медленно)
			burnRate += 0.001f; // ~0.06/сек, ~3.6/мин

			// Бег: быстро сжигает
			if (Math.Abs(Player.velocity.X) > 2f && Player.velocity.Y == 0f)
			{
				burnRate += 0.015f; // ~0.48/сек, ~28.8/мин
			}

			// Спринт (быстрый бег)
			if (Math.Abs(Player.velocity.X) > 5f)
			{
				burnRate += 0.012f; // ещё быстрее
			}

			// Прыжки
			if (Player.jump > 0 || Player.velocity.Y < -2f)
			{
				burnRate += 0.005f;
			}

			// Бой (если игрок атакует)
			if (Player.itemAnimation > 0)
			{
				burnRate += 0.004f;
			}

			// Плавание
			if (Player.wet && !Player.lavaWet)
			{
				burnRate += 0.01f;
			}

			// Накапливаем и применяем
			burnAccumulator += burnRate;

			if (burnAccumulator >= 1f)
			{
				float toLose = (float)Math.Floor(burnAccumulator);
				FatLevel = Math.Max(0f, FatLevel - toLose);
				burnAccumulator -= toLose;

				// Уведомление при переходе на стадию ниже
				if (FatLevel > 0 && (int)(FatLevel + toLose) / 10 != (int)FatLevel / 10)
				{
					Main.NewText($"Burning fat! ({(int)FatLevel}/{(int)MaxFat})", 150, 255, 150);
				}
			}
		}

		private void ApplyFatEffects()
		{
			if (FatLevel <= 0) return;

			float ratio = FatLevel / MaxFat;

			Player.moveSpeed *= Math.Max(0.2f, 1f - (ratio * 0.4f));
			Player.jumpSpeedBoost -= ratio * 2f;

			if (FatLevel > 50f)
				Player.GetAttackSpeed(DamageClass.Generic) *= Math.Max(0.5f, 1f - ratio * 0.15f);

			Player.statLifeMax2 += (int)(ratio * 60f);
			Player.statDefense += (int)(ratio * 10f);
			Player.noKnockback = FatLevel >= 80f;

			if (FatLevel >= 90f)
				Player.lifeRegen = Math.Max(0, Player.lifeRegen - 4);
		}

		public override void SaveData(TagCompound tag)
		{
			tag["fatLevel"] = FatLevel;
			tag["burnAccumulator"] = burnAccumulator;
		}

		public override void LoadData(TagCompound tag)
		{
			FatLevel = tag.GetFloat("fatLevel");
			burnAccumulator = tag.GetFloat("burnAccumulator");
		}
	}
}