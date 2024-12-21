using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libra
{
    public interface ILogger
    {
        public void Msg(string mensagem, string final = "\n");
    }

    public class ConsoleLogger : ILogger
    {
        public void Msg(string mensagem, string final = "\n")
        {
            Console.Write(mensagem + final);
            Console.ResetColor();
        }
    }

}
