using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DOL.GS;

namespace DOL.GS.Scripts
{
    class MimicNPCInventory : GameLivingInventory
    {
        MimicNPC m_Mimic;

        public MimicNPCInventory(MimicNPC mimic)
        {
            if (mimic != null)
            {
                m_Mimic = mimic;
            }
        }
    }
}