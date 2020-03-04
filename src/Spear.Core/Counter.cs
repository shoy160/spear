using System;
using System.Threading;

namespace Spear.Core
{
    public static class Counter
    {
        public static long SendData => _sendData;
        public static long ReceiveData => _receiveData;
        public static long Called => _called;

        private static long _sendData;
        private static long _receiveData;
        private static long _called;

        public static void Send(long length)
        {
            Interlocked.Add(ref _sendData, length);
        }

        public static void Received(long length)
        {
            Interlocked.Add(ref _receiveData, length);
        }

        public static void Call()
        {
            Interlocked.Increment(ref _called);
        }

        public static void Clear()
        {
            _sendData = 0;
            _receiveData = 0;
            _called = 0;
        }

        public static void Show()
        {
            Console.WriteLine("================= Counter =================");
            Console.WriteLine($"send data: {_sendData} bytes");
            Console.WriteLine($"receive data: {_receiveData} bytes");
            Console.WriteLine($"call count: {_called}");
            Console.WriteLine("===================================");
        }
    }
}
