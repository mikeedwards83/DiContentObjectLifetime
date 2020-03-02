using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace MSDI
{
    class Program
    {
        static void Main(string[] args)
        {



            Console.WriteLine("Which version to run?");
            Console.WriteLine("1. Singleton Func");
            Console.WriteLine("2. Transient");
            Console.WriteLine("3. No DI");
            Console.WriteLine("4. No Dispose");
            Console.WriteLine("5. Transient Scoped");
            Console.WriteLine("6. Transient Scoped Func");

            var result = Console.ReadLine();

            switch (result)
            {
                case "1":
                    FuncTest();
                    break;
                case "2":
                    TransientTest();
                    break;

                case "3":
                    NoDITest();
                    break;

                case "4":
                    NoDispose();
                    break;
                case "5":
                    TransientTestScoped();
                    break;
                case "6":
                    TransientTestFuncScoped();
                    break;

            }


        }



        public static ServiceProvider ServiceProvider { get; set; }


        private static T Get<T>()
        {
            return ServiceProvider.GetService<T>();


        }

        private static MyService2 CreateMyService2()
        {
            return new MyService2();
        }

        private static void FuncTest()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient(_ => CreateMyService2());
            serviceCollection.AddSingleton<Func<MyService2>>(_ => Get<MyService2>);
            serviceCollection.AddTransient(typeof(MyService1Func));

            ServiceProvider = serviceCollection.BuildServiceProvider();

            for (int i = 0; i < 1000000000; i++)
            {
                Thread.Sleep(1000);

                var myservice = ServiceProvider.GetService(typeof(MyService1Func));

                if (i % 10 == 0)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Console.WriteLine("GC Called");
                }
            }

            Console.Read();
        }

        private static void TransientTest()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient(typeof(MyService2));
            serviceCollection.AddTransient(typeof(MyService1));

            ServiceProvider = serviceCollection.BuildServiceProvider();

            for (int i = 0; i < 1000000000; i++)
            {
                if (i % 100 == 0)
                {
                    Thread.Sleep(1000);
                }

                var myservice = ServiceProvider.GetService(typeof(MyService1));

                if (i % 10 == 0)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Console.WriteLine($"GC Called {i}");
                }
            }

            Console.Read();

        }

        p


        private static void TransientTestFuncScoped()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient< MyService2>();
            serviceCollection.AddTransient<Func<MyService2>>(c => c.GetService<MyService2>);
            serviceCollection.AddScoped<MyService1>(c =>
            {
                var f = c.GetService<Func<MyService2>>();
                return new MyService1(f());
            });


            ServiceProvider = serviceCollection.BuildServiceProvider();

            for (int i = 0; i < 1000000000; i++)
            {
                using (var scope = ServiceProvider.CreateScope())
                {
                    Thread.Sleep(1000);

                    var myservice = scope.ServiceProvider.GetService(typeof(MyService1));

                    if (i % 10 == 0)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        Console.WriteLine("GC Called");

                    }
                }


            }

            Console.Read();


        }

        private static void TransientTestScoped()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient(typeof(MyService2));
            serviceCollection.AddScoped(typeof(MyService1));


            ServiceProvider = serviceCollection.BuildServiceProvider();

            for (int i = 0; i < 1000000000; i++)
            {
                using (var scope = ServiceProvider.CreateScope())
                {
                    Thread.Sleep(1000);

                    var myservice = scope.ServiceProvider.GetService(typeof(MyService1));
                    var myservice2 = scope.ServiceProvider.GetService(typeof(MyService2));

                    if (i % 10 == 0)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        Console.WriteLine("GC Called");

                    }
                }


            }

            Console.Read();


        }

        private static void NoDispose()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient(typeof(MyService3));
            serviceCollection.AddTransient(typeof(MyService4));

            ServiceProvider = serviceCollection.BuildServiceProvider();

            for (int i = 0; i < 1000000000; i++)
            {
                Thread.Sleep(1000);

                var myservice = ServiceProvider.GetService(typeof(MyService3));

                if (i % 10 == 0)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Console.WriteLine("GC Called");

                }
            }

            Console.Read();


        }

        private static void NoDITest()
        {
            for (int i = 0; i < 1000000000; i++)
            {
                Thread.Sleep(1000);

                var myservice = new MyService1(new MyService2());

                if (i % 10 == 0)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Console.WriteLine("GC Called");

                }
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



}