using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;

namespace AutoFacDI
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Which version to run?");
          // Console.WriteLine("1. Singleton Func");
            Console.WriteLine("2. Transient");
            //Console.WriteLine("3. No DI");
            //Console.WriteLine("4. No Dispose");
            //Console.WriteLine("5. Transient Scoped");
            //Console.WriteLine("6. Transient Scoped Func");

            var result = Console.ReadLine();

            switch (result)
            {
                case "1":
                    //FuncTest();
                    break;
                case "2":
                    TransientTest();
                    break;

                case "3":
                    //    NoDITest();
                    break;

                case "4":
                    //NoDispose();
                    break;
                case "5":
                    //  TransientTestScoped();
                    break;
                case "6":
                    //    TransientTestFuncScoped();
                    break;

            }

        }

        private static void TransientTest()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<MyService1>().ExternallyOwned();
            builder.RegisterType<MyService2>().ExternallyOwned();

            var container = builder.Build();

           


            for (int i = 0; i < 1000000000; i++)
            {
                if (i % 100 == 0)
                {
                    Thread.Sleep(1000);
                }

                var myservice = container.Resolve<MyService1>();

                if (i % 10 == 0)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Console.WriteLine($"GC Called {i}");
                }
            }

            Console.Read();


        }
    }

    public class MyService2 : IDisposable
    {
        private static int _count = 0;
        private IEnumerable<string> _text;

        private bool disposed = false;

        public MyService2()
        {
            Interlocked.Increment(ref _count);
            Console.WriteLine($"MyService2 {_count}");
            _text = Enumerable.Range(0, 20000000).Select(x => "a");
        }

        public void Dispose()
        {
            Interlocked.Decrement(ref _count);

            Console.WriteLine("Disposed 1");
        }


        ~MyService2()
        {
            Interlocked.Decrement(ref _count);
            Console.WriteLine("Final");

        }
    }

    public class MyService1Func

    {
        private readonly MyService2 _service2;

        public MyService1Func(Func<MyService2> service2)
        {
            _service2 = service2.Invoke();
        }
    }

    public class MyService1

    {
        private readonly MyService2 _service2;

        public MyService1(MyService2 service2)
        {
            _service2 = service2;
        }
    }


    public class MyService3

    {
        private readonly MyService4 _service2;

        public MyService3(MyService4 service2)
        {
            _service2 = service2;
        }
    }

    public class MyService4
    {
        private static int _count = 0;
        private IEnumerable<string> _text;

        private bool disposed = false;

        public MyService4()
        {
            Interlocked.Increment(ref _count);
            Console.WriteLine($"MyService4 {_count}");
            _text = Enumerable.Range(0, 20000000).Select(x => "a");
        }




        ~MyService4()
        {
            Interlocked.Decrement(ref _count);
            Console.WriteLine("Final");

        }
    }
}
