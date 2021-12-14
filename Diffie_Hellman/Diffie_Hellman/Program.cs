using System;

namespace Diffie_Hellman
{
    
    class Program
    {
        public static void Process()
        {
            int p = 8917217, g = 67543243;
            var alice = new DiffieHellman(p, g, 123456);
            var bob = new DiffieHellman(p, g, 654321);

            var A = alice.GetFirstPublicKey();
            var B = bob.GetFirstPublicKey();

            Console.WriteLine($"Alice: {alice}");
            Console.WriteLine($"Bob: {bob}");

            Console.WriteLine($"A = {A} B = {B}");

            var tmp = bob.GetNextPublicKey(A);
            A = alice.GetNextPublicKey(B);
            B = tmp;

            Console.WriteLine($"Result: A = {A} B = {B}");
        }

        static void Main(string[] args)
        {
            Process(); // 679 15617
        }
    }
}
