using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Diffie_Hellman
{
    public class DiffieHellman
    {
        private Random random = new Random();

        private int _g, _p;
        private int _privateKey;
        private int _currentA, _currentB; // ?
        
        public DiffieHellman(int p, int g, int privateKey)  { _p = p; _g = g; _privateKey = privateKey; }

        //public DiffieHellman()
        //{
        //    _p = random.Next(Constants.MinValueP, Constants.MaxValueP);
        //    _g = random.Next(Constants.MinValueG, Constants.MaxValueG);
        //    _privateKey = random.Next(Constants.MinValuePrivateKey, Constants.MaxValuePrivateKey);
        //}

        public int GetFirstPublicKey() => getModPow(_g, _privateKey, _p);

        public int GetNextPublicKey(int publicKey) => getModPow(publicKey, _privateKey, _p);

        private int getModPow(int value, int e, int mod) => (int)BigInteger.ModPow(value, e, mod);

        public override string ToString() => $"g = {_g} p = {_p} privateKey = {_privateKey}";

    }
}
