using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClient
{
    public static class MyMath
    {
        public static bool IsNumberPrime(int num)
        {
            if (num < 2) return false;
            
            for (int i = 2; i < num; i++)
                if (num % i == 0)
                    return false;
            return true;
        }
    }
}
