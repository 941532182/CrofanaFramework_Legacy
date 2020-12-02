using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

using Crofana.Cache;
using Crofana.IoC;

public static class A
{
    public static B b = new B();
}

public class B
{
    public B() { Console.WriteLine("bbb"); }
}

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

        class A
        {
            public int x;
        }

        static void Main(string[] args)
        {
            string[] x = { "x", "y", "z" };
            x.Select(e => typeof(A).GetField(e)).ToList().ForEach(e => Console.WriteLine(e == null));
            /*var factory = new StandardCrofanaObjectFactory();
            var manager = factory.GetObject<StandardCrofanaEntityManager>();
            var serializer = factory.GetObject<CPKSerializer>();
            manager.Deserialize(serializer, System.IO.File.OpenRead("character.xlsx"));
            manager.Deserialize(serializer, System.IO.File.OpenRead("weapon.xlsx"));*/
        }
    }
}
