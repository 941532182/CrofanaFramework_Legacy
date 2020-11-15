using System;
using Crofana.IoC;

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
        static void Main(string[] args)
        {
            StandardCrofanaObjectFactory cof = new StandardCrofanaObjectFactory();

            var co = cof.GetObject<CrofanaObject1>();

            Console.WriteLine(co.CO2.x);
            Console.WriteLine(co.co3.x);
            Console.WriteLine(co.CO2.co1.x);
        }
    }
}
