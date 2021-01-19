using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpProxy proxy = new TcpProxy();
            proxy.Start();

            Console.ReadKey();
        }
    }
}
