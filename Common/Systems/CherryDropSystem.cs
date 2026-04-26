using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using LK_Ugrumiy_WP.Content.Items.Consumables;

namespace LK_Ugrumiy_WP.Common.Systems
{
    public class CherryDropSystem : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (!Main.hardMode || !NPC.downedPlantBoss)
                return;

            if (npc.townNPC || npc.friendly || npc.boss)
                return;

            if (npc.SpawnedFromStatue)
                return;

            int chance = 2000;

            if (Main.expertMode)
                chance = 1500;
            if (Main.masterMode)
                chance = 1000;

            if (Main.rand.NextBool(chance))
            {
                Item.NewItem(npc.GetSource_Loot(), npc.position, npc.width, npc.height, ModContent.ItemType<Cherry>(), 1);
            }
        }
    }
}
