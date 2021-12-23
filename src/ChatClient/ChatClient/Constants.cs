using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClient
{
    public static class Constants
    {
        public const string Host = "172.20.10.4";
        public const int Port = 8888;

        #region DiffieHellman
        public static int MinValueP = 10000000; 
        public static int MaxValueP = 99999999;

        public static int MinValueG = 10000;
        public static int MaxValueG = 90000;

        public static int MinValuePrivateKey = 10000;
        public static int MaxValuePrivateKey = 90000;
        #endregion
    }
}
