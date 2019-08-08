using System;

namespace Spear.Tests.Netty
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BaseTest test;
            var port = args.Length > 0 ? Convert.ToInt32(args[0]) : 5002;
            var type = args.Length > 1 ? args[1] : string.Empty;
            if (string.Equals("server", type, StringComparison.CurrentCultureIgnoreCase))
                test = new Server(port);
            else
                test = new Client(port);

            while (true)
            {
                var cmd = Console.ReadLine();
                switch (cmd)
                {
                    case "exit":
                        test.Dispose();
                        break;
                    default:
                        test.OnCommand(cmd);
                        break;
                }
            }
        }
    }
}
