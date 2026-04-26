using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Items.Consumables
{
    /// <summary>
    /// Крайне редкий потребляемый предмет, восстанавливающий всё ХП
    /// и накладывающий дебафф на повторное использование
    /// </summary>
    public class Cherry : ModItem
    {
        public override void SetDefaults()
        {
            // Тип предмета - расходуемый
            Item.consumable = true;
            Item.maxStack = 99;

            // Размеры и редкость (фиолетовая - очень редкий)
            Item.width = 20;
            Item.height = 24;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.buyPrice(gold: 2);

            // Звук использования - приятный "хруст"
            Item.UseSound = SoundID.Item2;

            // Время использования - быстрое
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.EatFood;
        }

        public override bool CanUseItem(Player player)
        {
            // Проверяем, нет ли уже дебаффа на вишню
            return !player.HasBuff(ModContent.BuffType<Buffs.CherryCooldown>());
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                // Восстанавливаем всё ХП
                player.statLife = player.statLifeMax2;
                player.HealEffect(player.statLifeMax2);

                // Накладываем дебафф на 1 минуту (60 сек * 60 = 3600 тиков)
                player.AddBuff(ModContent.BuffType<Buffs.CherryCooldown>(), 3600);
            }

            return true;
        }
    }
}
