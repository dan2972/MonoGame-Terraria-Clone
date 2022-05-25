using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameTestProject.Entities.Blocks
{
    class ItemID
    {

        public enum ID:int
        { 
            air_block = 0,
            grass_block = 1,
            dirt_block = 2,
            stone_block = 3,
            dirt_wall = 5000,
            stone_wall = 5001,
            fire_wand = 9000,
            pistol_basic = 9001,
            stone_hammer = 9002,
            NULL = 99999
        }

    }
}
