using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoMode.DiscordSDK
{
    public class DiscordInit
    {
        public static void Init()
        {
            BingoRichPresence.InitDiscord();
            BingoRichPresence.Hook();
        }
    }
}
