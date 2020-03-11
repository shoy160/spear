using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Newtonsoft.Json;
using Spear.Core.Micro.Services;

namespace Spear.Tests
{
    [TestClass]
    public class ReactiveTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            //响应式编程
            var list = Enumerable.Range(1, 100).ToObservable(NewThreadScheduler.Default);
            var subject = new Subject<int>();
            subject.Subscribe((temperature) => Console.WriteLine($"当前温度：{temperature}"));//订阅subject
            subject.Subscribe((temperature) => Console.WriteLine($"嘟嘟嘟，当前水温：{temperature}"));//订阅subject
            list.Subscribe(subject);
            //list.Wait();

            var observable = Observable.Return("shay");
            observable.Subscribe(Console.WriteLine);
        }

        [TestMethod]
        public void ServiceRandomTest()
        {
            var services = new List<ServiceAddress>
            {
                new ServiceAddress {Weight = 3},
                new ServiceAddress {Weight = 2},
                new ServiceAddress {Weight = 1}
            };
            var dict = new Dictionary<double, int>();
            for (var i = 0; i < 200; i++)
            {
                var service = services.Random();
                if (dict.ContainsKey(service.Weight))
                    dict[service.Weight]++;
                else
                    dict[service.Weight] = 1;
            }

            Console.WriteLine(JsonConvert.SerializeObject(dict));
        }
    }
}

