using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Expedition;
using Menu;
using Menu.Remix;
using UnityEngine;

namespace BingoMode
{
    public class BingoPage : PositionedMenuObject
    {
        public ExpeditionMenu expMenu;

        public BingoPage(Menu.Menu menu, MenuObject owner, Vector2 pos) : base(menu, owner, pos)
        {
            expMenu = menu as ExpeditionMenu;


        }
    }
}
