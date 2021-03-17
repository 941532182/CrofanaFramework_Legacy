using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crofana.Network;

namespace Login
{
    public partial class CLoginController
    {
        [RequestMapping]
        private static Boolean Response(CLogin msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("登录成功！");
            Console.ResetColor();
            return true;
        }
    }
}
