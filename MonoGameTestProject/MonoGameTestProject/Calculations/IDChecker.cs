using MonoGameTestProject.Entities.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTestProject.Calculations
{
    class IDChecker
    {

        public static bool isWall(ItemID.ID id)
        {
            if ((int)id >= 5000 && (int)id < 9000)
                return true;
            else
                return false;
        }

        public static bool isBlock(ItemID.ID id)
        {
            if ((int)id >= 5000)
                return false;
            else if ((int)id == 0)
                return false;
            else return true;
        }
    }
}
