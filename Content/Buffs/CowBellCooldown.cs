using Terraria;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Content.Buffs
{
    /// <summary>
    /// Cooldown debuff after using CowBell. Prevents spamming the cow summon.
    /// </summary>
    public class CowBellCooldown : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Pure timer; no extra effects.
        }
    }
}
