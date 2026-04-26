using Terraria;
using Terraria.ModLoader;

namespace LK_Ugrumiy_WP.Common.Systems
{
    /// <summary>
    /// Система отслеживания игроков, носящих шляпу Джона.
    /// Нужно чтобы Джон мог определить кто носит его шляпу.
    /// </summary>
    public class JohnHatSystem : ModSystem
    {
        // Отслеживает кто из игроков носит шляпу
        public bool[] playerWearingHat = new bool[Main.maxPlayers];

        public override void ClearWorld()
        {
            // Сброс при выходе из мира
            for (int i = 0; i < playerWearingHat.Length; i++)
            {
                playerWearingHat[i] = false;
            }
        }
    }
}
