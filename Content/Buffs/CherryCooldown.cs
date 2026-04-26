using Terraria;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Buffs
{
    /// <summary>
    /// Дебафф после использования Вишни - запрещает повторное использование в течение 1 минуты
    /// </summary>
    public class CherryCooldown : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = false; // Сохраняется при выходе из мира
            Main.buffNoTimeDisplay[Type] = false; // Показываем таймер
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Дебафф просто висит и отсчитывает время
            // Никаких дополнительных эффектов не даёт
        }
    }
}
