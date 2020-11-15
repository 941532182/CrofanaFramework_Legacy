using System;
using System.Collections.Generic;
using Crofana.IoC;
using System.Runtime.Serialization;

namespace CrofanaTestClient
{
    class Client
    {
        class NotCrofanaObject { }

        [CrofanaObject]
        class CrofanaObject1
        {
            [Autowired]
            private CrofanaObject2 co2;
            [Autowired]
            public CrofanaObject3 co3 { get; set; }
            public CrofanaObject2 CO2 => co2;
            public int x = 999;
        }

        [CrofanaObject]
        class CrofanaObject2
        {
            public int x = 10;
            [Autowired]
            public CrofanaObject1 co1;
        }

        [CrofanaObjectConstructionListener]
        class CrofanaObject3
        {
            public int x = 50;
            private CrofanaObject3() { }
            [PostConstruct]
            private void PostConstruct(object obj)
            {
                Console.WriteLine($"POST CONSTRUCT: {obj.GetType().FullName}");
            }
        }
        [Serializable]
        public class Character
        {
            public int id;
            public string name;
            private int atk;
            public int def { get; }
            private int agi { get; set; }
        }
        static void Main(string[] args)
        {
            Console.WriteLine(typeof(int).IsGenericType);
        }
    }
}
