using SwCore;
using System;
using System.Threading.Tasks;

namespace SwTest
{
    [SwComponent]
    class A1
    {
        public void Run()
        {
            Console.WriteLine("void A1.Run()");
        }
    }
    [SwComponent]
    class A2
    {
        public Task Run(int i, long l, short s)
        {
            Console.WriteLine($"Task A2.Run(int i:{i}, long l:{l}, short s:{s})");
            return Task.CompletedTask;
        }
    }

    [SwComponent]
    class B1
    {
        public string Run()
        {
            Console.WriteLine("string B1.Run()");
            return "test";
        }
    }
    [SwComponent]
    class B2
    {
        public Task<int> Run()
        {
            Console.WriteLine("Task<int> B2.Run()");
            return Task.FromResult(43);
        }
    }

    [SwComponent]
    class C1
    {
        public ValueTuple<long, short> Run(string input)
        {
            Console.WriteLine($"ValueTuple<long, short> C1.Run(string input:{input})");
            return (10, 1);
        }
    }
    [SwComponent]
    class C2
    {
        public Task Run(byte i1, double i2)
        {
            Console.WriteLine($"Task C2.Run(byte i1:{i1}, double i2:{i2})");
            return Task.CompletedTask;
        }

        //         public Task<ValueTuple<byte>> Run()
        //         {
        //             Console.WriteLine($"Task<ValueTuple<byte>> C2.Run()");
        //             return Task.FromResult(ValueTuple.Create<byte>(0));
        //         }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var builder = new SwContainerBuilder<(byte,double)>();
            builder.AddAssembly(typeof(Program).Assembly);
            var container = builder.Build();

            container.Run((1,2)).Wait();
        }
    }
}
