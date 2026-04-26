using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Accessories
{
	/// <summary>
	/// ИИ с реальным Q-Learning обучением.
	/// Учится на своём опыте: получает награды за убийства,
	/// штрафы за полученный урон, и постепенно улучшает стратегию.
	/// </summary>
	public class CursedBandleAI
	{
		private readonly QLearningBrain brain = new QLearningBrain();
		private readonly Random rng = new Random();

		// Кулдауны
		private int jumpCooldown;
		private int attackCooldown;
		private int healCooldown;

		// Отслеживание изменений для наград
		private int prevPlayerHp;
		private int prevEnemyHp;
		private int prevEnemyWhoAmI = -1;
		private Vector2 prevPosition;
		private int stuckCounter;

		// Параметры
		private const float DetectRange = 700f;
		private const int LearnInterval = 6;  // Обучаемся каждые N тиков (не каждый)
		private int tickCounter;

		// Автосохранение
		private int saveTimer;
		private const int SaveInterval = 3600; // Каждые 60 секунд

		public void Reset()
		{
			jumpCooldown = 0;
			attackCooldown = 0;
			healCooldown = 0;
			prevEnemyWhoAmI = -1;
			stuckCounter = 0;
			brain.Reset();
		}

		/// <summary>Загрузить знания из файла при экипировке.</summary>
		public void LoadBrain()
		{
			string path = Path.Combine(ModLoader.ModPath, "CursedBandleBrain");
			Directory.CreateDirectory(path);
			brain.Load(path);
		}

		/// <summary>Сохранить знания при смерти / периодически.</summary>
		public void SaveBrain()
		{
			string path = Path.Combine(ModLoader.ModPath, "CursedBandleBrain");
			Directory.CreateDirectory(path);
			brain.Save(path);
		}

		public void OnDeath()
		{
			brain.OnDeath();
			SaveBrain();
		}

		/// <summary>
		/// Главный метод — вызывается каждый тик.
		/// </summary>
		public void Update(Player player)
		{
			var modPlayer = player.GetModPlayer<CursedBandlePlayer>();

			// Кулдауны
			if (jumpCooldown > 0) jumpCooldown--;
			if (attackCooldown > 0) attackCooldown--;
			if (healCooldown > 0) healCooldown--;
			tickCounter++;

			// Автосохранение
			saveTimer++;
			if (saveTimer >= SaveInterval)
			{
				saveTimer = 0;
				SaveBrain();
			}

			// Находим врагов
			NPC nearestEnemy = FindNearestEnemy(player);
			int enemyCount = CountEnemiesInRange(player, DetectRange);

			// Проверяем — был ли убит предыдущий враг
			bool enemyKilled = false;
			if (prevEnemyWhoAmI >= 0 && prevEnemyWhoAmI < Main.maxNPCs)
			{
				NPC prevEnemy = Main.npc[prevEnemyWhoAmI];
				if (!prevEnemy.active || prevEnemy.life <= 0)
					enemyKilled = true;
			}

			// Проверяем — получили ли мы урон
			bool wasHit = player.statLife < prevPlayerHp;

			// Проверяем — застряли ли
			bool isStuck = Vector2.Distance(player.Center, prevPosition) < 1f && player.velocity.Y == 0f;
			if (isStuck) stuckCounter++;
			else stuckCounter = 0;

			// === Q-Learning цикл (каждые N тиков) ===
			if (tickCounter % LearnInterval == 0)
			{
				// 1. Наблюдаем текущее состояние
				QLearningBrain.StateKey currentState = brain.Observe(player, nearestEnemy, enemyCount);

				// 2. Считаем награду за предыдущее действие
				float reward = brain.CalculateReward(
					player, nearestEnemy,
					prevPlayerHp,
					prevEnemyHp,
					enemyKilled,
					wasHit,
					stuckCounter > 30
				);

				// 3. Обучаемся на полученном опыте
				brain.Learn(currentState, reward);

				// 4. Выбираем следующее действие
				QLearningBrain.AIAction action = brain.ChooseAction(currentState);

				// 5. Выполняем действие
				ExecuteAction(player, action, nearestEnemy, modPlayer);

				// 6. Запоминаем для следующего шага
				brain.Remember(currentState, action);
			}
			else
			{
				// Между шагами обучения — продолжаем текущее действие
				ContinueCurrentAction(player, nearestEnemy, modPlayer);
			}

			// Обновляем отслеживание
			prevPlayerHp = player.statLife;
			if (nearestEnemy != null)
			{
				prevEnemyHp = nearestEnemy.life;
				prevEnemyWhoAmI = nearestEnemy.whoAmI;
			}
			else
			{
				prevEnemyHp = 0;
				prevEnemyWhoAmI = -1;
			}
			prevPosition = player.Center;

			// Целимся на врага (для подмены мыши)
			if (nearestEnemy != null)
				modPlayer.aiTargetPosition = nearestEnemy.Center;
			else
				modPlayer.aiTargetPosition = player.Center + new Vector2(player.direction * 200f, 0f);
		}

		// ============================================================
		// Выполнение действий
		// ============================================================

		private QLearningBrain.AIAction lastAction;

		private void ExecuteAction(Player player, QLearningBrain.AIAction action,
			NPC enemy, CursedBandlePlayer modPlayer)
		{
			lastAction = action;

			switch (action)
			{
				case QLearningBrain.AIAction.MoveLeft:
					player.controlLeft = true;
					player.ChangeDir(-1);
					break;

				case QLearningBrain.AIAction.MoveRight:
					player.controlRight = true;
					player.ChangeDir(1);
					break;

				case QLearningBrain.AIAction.Jump:
					TryJump(player);
					break;

				case QLearningBrain.AIAction.JumpLeft:
					player.controlLeft = true;
					player.ChangeDir(-1);
					TryJump(player);
					break;

				case QLearningBrain.AIAction.JumpRight:
					player.controlRight = true;
					player.ChangeDir(1);
					TryJump(player);
					break;

				case QLearningBrain.AIAction.Attack:
					if (enemy != null)
					{
						SelectBestWeapon(player, enemy);
						player.controlUseItem = true;
						float dx = enemy.Center.X - player.Center.X;
						player.ChangeDir(dx > 0 ? 1 : -1);
						attackCooldown = 8;
					}
					break;

				case QLearningBrain.AIAction.AttackApproach:
					if (enemy != null)
					{
						float dx2 = enemy.Center.X - player.Center.X;
						if (dx2 > 30f) player.controlRight = true;
						else if (dx2 < -30f) player.controlLeft = true;
						player.ChangeDir(dx2 > 0 ? 1 : -1);

						if (enemy.Center.Y < player.Center.Y - 60f || IsBlocked(player))
							TryJump(player);

						SelectBestWeapon(player, enemy);
						player.controlUseItem = true;
						attackCooldown = 6;
					}
					break;

				case QLearningBrain.AIAction.Flee:
					if (enemy != null)
					{
						float dx3 = enemy.Center.X - player.Center.X;
						if (dx3 > 0) player.controlLeft = true;
						else player.controlRight = true;
						TryJump(player);
					}
					else
					{
						// Нет врага — просто двигаемся
						if (rng.Next(2) == 0) player.controlLeft = true;
						else player.controlRight = true;
					}
					break;

				case QLearningBrain.AIAction.Heal:
					if (healCooldown <= 0)
					{
						player.controlQuickHeal = true;
						healCooldown = 60; // 1 секунда кулдаун
					}
					break;

				case QLearningBrain.AIAction.DodgeJump:
					// Уклонение: прыжок в случайную сторону
					TryJump(player);
					if (rng.Next(2) == 0) player.controlLeft = true;
					else player.controlRight = true;
					break;

				case QLearningBrain.AIAction.BuildBelow:
					TryBuild(player);
					break;

				case QLearningBrain.AIAction.Idle:
					// Ничего не делаем — "думаем"
					break;
			}
		}

		/// <summary>Продолжаем последнее действие между шагами обучения.</summary>
		private void ContinueCurrentAction(Player player, NPC enemy, CursedBandlePlayer modPlayer)
		{
			// Упрощённое продолжение текущего действия
			switch (lastAction)
			{
				case QLearningBrain.AIAction.MoveLeft:
					player.controlLeft = true;
					break;
				case QLearningBrain.AIAction.MoveRight:
					player.controlRight = true;
					break;
				case QLearningBrain.AIAction.JumpLeft:
					player.controlLeft = true;
					break;
				case QLearningBrain.AIAction.JumpRight:
					player.controlRight = true;
					break;
				case QLearningBrain.AIAction.Attack:
				case QLearningBrain.AIAction.AttackApproach:
					if (enemy != null && attackCooldown <= 0)
					{
						player.controlUseItem = true;
						float dx = enemy.Center.X - player.Center.X;
						player.ChangeDir(dx > 0 ? 1 : -1);
						if (lastAction == QLearningBrain.AIAction.AttackApproach)
						{
							if (dx > 30f) player.controlRight = true;
							else if (dx < -30f) player.controlLeft = true;
						}
					}
					break;
				case QLearningBrain.AIAction.Flee:
					if (enemy != null)
					{
						float dx2 = enemy.Center.X - player.Center.X;
						if (dx2 > 0) player.controlLeft = true;
						else player.controlRight = true;
					}
					break;
			}
		}

		// ============================================================
		// Утилиты
		// ============================================================

		/// <summary>Выбирает лучшее оружие с учётом дистанции до врага.</summary>
		private void SelectBestWeapon(Player player, NPC enemy)
		{
			float dist = Vector2.Distance(player.Center, enemy.Center);
			int bestSlot = 0;
			int bestScore = 0;

			for (int i = 0; i < 10; i++)
			{
				Item item = player.inventory[i];
				if (!item.active || item.damage <= 0 || item.accessory || item.createTile >= TileID.Dirt)
					continue;

				int score = item.damage;

				// Бонус за рэнж на дистанции
				if (dist > 200f && item.DamageType == DamageClass.Ranged)
					score += 50;
				if (dist > 200f && item.DamageType == DamageClass.Magic)
					score += 40;

				// Бонус за мили вблизи
				if (dist < 150f && item.DamageType == DamageClass.Melee)
					score += 30;

				if (score > bestScore)
				{
					bestScore = score;
					bestSlot = i;
				}
			}

			player.selectedItem = bestSlot;
		}

		private NPC FindNearestEnemy(Player player)
		{
			NPC closest = null;
			float closestDist = float.MaxValue;

			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (!npc.active || npc.friendly || npc.townNPC || npc.life <= 0)
					continue;

				float dist = Vector2.Distance(player.Center, npc.Center);
				if (dist < closestDist)
				{
					closestDist = dist;
					closest = npc;
				}
			}

			return closestDist < DetectRange ? closest : null;
		}

		private int CountEnemiesInRange(Player player, float range)
		{
			int count = 0;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && !npc.friendly && !npc.townNPC && npc.life > 0)
				{
					if (Vector2.Distance(player.Center, npc.Center) < range)
						count++;
				}
			}
			return count;
		}

		private bool IsBlocked(Player player)
		{
			return Math.Abs(player.velocity.X) < 0.5f && player.velocity.Y == 0f;
		}

		private void TryJump(Player player)
		{
			if (jumpCooldown <= 0)
			{
				player.controlJump = true;
				jumpCooldown = 12;
			}
		}

		private void TryBuild(Player player)
		{
			for (int i = 0; i < 50; i++)
			{
				Item item = player.inventory[i];
				if (item.active && item.createTile >= TileID.Dirt && item.stack > 0)
				{
					player.selectedItem = i;
					player.controlUseItem = true;
					player.controlDown = true;
					return;
				}
			}
		}
	}
}