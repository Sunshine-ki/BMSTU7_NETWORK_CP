using NetworkServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;

namespace ChatClient
{
    public static class DiffieHellman
    {
        private static Random random = new Random();

        public static int Connect(NetworkStream stream) // todo: replace NetworkStream  to Stream
        {
            int g = random.Next(Constants.MinValueG, Constants.MaxValueG);
            // p is public prime number.
            int p = getRandomPrimeNum(Constants.MinValueP, Constants.MaxValueP); // Should be simple 
            // a is private key for Alice
            int a = random.Next(Constants.MinValuePrivateKey, Constants.MaxValuePrivateKey);

            // A is public key for Alice
            int A = (int)BigInteger.ModPow(g, a, p);
            var firstPublicData = $"{g} {p} {A}";
            WriteServices.SendString(stream, firstPublicData);

            var B = ReadServices.GetNumber(stream); // This is public key (for Bob)
            var privateKey = (int)BigInteger.ModPow(B, a, p);
            
            Console.WriteLine($"privateKey (connect) = {privateKey}");
            return privateKey;
        }

        public static int Wait(NetworkStream stream)
        {
            var tmp = ReadServices.GetMessage(stream);
            var publicData = tmp.Split(" ").Select(x => Convert.ToInt32(x)).ToList();
            
            int g = publicData[0];
            int p = publicData[1];
            int A = publicData[2];

            // b is secret key for Bob
            int b = random.Next(Constants.MinValuePrivateKey, Constants.MaxValuePrivateKey); 
            // B is public key for Bob
            int B = (int)BigInteger.ModPow(g, b, p);
            var PublicKey = B;
            var privateKey = (int)BigInteger.ModPow(A, b, p);
            WriteServices.SendNumber(stream, B);
            
            Console.WriteLine($"privateKey (wait)= {privateKey}");
            return privateKey;
        }

        private static int getRandomPrimeNum(int left, int right)
        {
            int num = random.Next(left, right);

            while (!MyMath.IsNumberPrime(num))
                num = random.Next(left, right);

            return num;
        }

     
    }
}
