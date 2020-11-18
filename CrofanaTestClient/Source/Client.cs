using System;
using System.Collections.Generic;
using Crofana.IoC;
using System.Runtime.Serialization;

using Crofana.Cache;
using Crofana.IoC;

namespace CrofanaTestClient
{
    [CrofanaEntity]
    public class Character
    {
        [PrimaryKey]
        public ulong Id;
        public Weapon Weapon;
    }

    [CrofanaEntity]
    public class Weapon
    {
        [PrimaryKey]
        public ulong Id;
        public string Name;
        public int ATK;
        public Character Owner;
    }

    class Client
    {

        static void Main(string[] args)
        {
            var factory = new StandardCrofanaObjectFactory();
            var manager = factory.GetObject<StandardCrofanaEntityManager>();
            var serializer = factory.GetObject<CPKSerializer>();
            manager.Deserialize(serializer, System.IO.File.OpenRead("character.xlsx"));
            manager.Deserialize(serializer, System.IO.File.OpenRead("weapon.xlsx"));
        }
    }
}
