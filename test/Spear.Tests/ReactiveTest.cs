using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography.X509Certificates;
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

        [TestMethod]
        public void GenerateCert()
        {
            const string path = "C:\\Users\\Administrator\\Desktop\\wechat\\apiclient_cert.p12";
            //var certFile = new X509Certificate2(path, "1377779702", X509KeyStorageFlags.MachineKeySet);
            //Assert.AreNotEqual(certFile, null);
            var buffer = File.ReadAllBytes(path);
            var base64 = Convert.ToBase64String(buffer);
            Console.WriteLine(base64);
            var cert = new X509Certificate2(buffer, "1377779702", X509KeyStorageFlags.MachineKeySet);
            Assert.AreNotEqual(cert, null);

        }

        [TestMethod]
        public void CertVerify()
        {
            const string base64 = "MIIKmgIBAzCCCmQGCSqGSIb3DQEHAaCCClUEggpRMIIKTTCCBM8GCSqGSIb3DQEHBqCCBMAwggS8AgEAMIIEtQYJKoZIhvcNAQcBMBwGCiqGSIb3DQEMAQYwDgQInQJUTSgDlNgCAggAgIIEiNqXcZQi7d08UxKgtC/fEIT2kqY0jX93ZgM2qN3FemYfOnAMO2rC8ZImmn0phVoBW9M4OEFE2vE1/Q9K0c/ek1M20/oWyB0wyQWdQZ46VRc+/7UfK7Pmad6bbb+LJINN2Ms4EVccv02U07ioPqRbAbE0bBjS03oYavrmmJJ24eOR/B6tXIJzCllpGCD+7f+ScE6KSL5oPq6tkf011vZoEkFWpwj4mSSFFQVyyLlTfhwAsYcxNHzQEWee4K+cVypokfW3HfFeTZeZ1BnnMpPnLGEjxcoDNYuJPubkfp/KDdEG90YR564NA0WUx/HnLekIwy0pLH00akrQrH+USmEVkx1Y8PmCmwOr/Mrq+4TsgowHIN5DN8oCoqtvYeP0+RLxP81chYpPs7+bRLFTIHaeKaRuYz1cJQI1borE4SHvQjGVa5SYXzDx396CHKF+osbSijPdosq5pDtFJo4C+3+468nDQcD+nog0jI7foLmEBBnuRw8+Yk3+JlVDKnnqq47jKAff1UTGoNgiYj0lc1tbCZ9eJx8IFWP/ZCGd8G8EK93lVkeJlajHbRmcXI8ttC2yygA1gzyQ03IjBIt85Oavo/QAQYsdAQ9rUvEEqsgFBd+UPZKmvjrMgMPfFciMhQI/rxphDz4UGFkrarOUosIyPxxwJzcmOcV62nL6gmqOopZN+v/8uPJzZYONTYi3pR+idOXB53vdZ5FNTqYqMknDYIbH7T7R8t2T+spsIk3cInyh2tylnKaosSom2HOdIiFI2HtZI2UXoe68+0n2DWN5WjCIRTEPug0r3HmnVYTpC4QgBoRnzZlTL1vs+FSdqGNjViN0gTKNdDnL4iyDWl/TP7hvH2e//+y4IH1bDLIvt5Y9jyQ3d9qnfFUqyQXZ4qZR/rMhd8fDsH+XDEvdUN5hLHi3BYffOWGUzQNr4wujhfYXBY1UyJL/on+s5xuOzDtG04tUDxfwYUosR8+b2zp90MUbU0Oi7vb/AOG18yHwFPfg54+dYNCfOQkojFH06xm/rA2KRA18e7viNw98pzn8smhxlphREXOtqX2VA0NNkCVucw5sWp2IgLsQmDQ49IgnXSaYG0b32A0v3IIuGFblRGKNXqDnpAvsEekSmJtwaW4iz4vFDe3UtgV4U4Rw/o2552glo/3Ry6T9V9u6yOJlTRZlheatWPY4f+vHMsMqPkZ6ULYwurMmYvDT1PpkPDtHwE5ezTQTlYAtY0UDgnvC0mIl4zX4VgGq8sX5AJ8lzAISTVMZ8kKDehAlgr1bR/CCpy8uG0bfu7zZBJBbpAGIerzNEnzdD0GkK8tXnKNYaVePSMKIzUW5G1WAWcpg8NtqqJTbdnbwtUhf0ApqwL7hsiwo1+t8lDJVDJhyX9BmFyP3FARF4NrMQVJW17f6h4onnf/A+YCyWteMcrbSMj9sbx20ubkgqPCl6PUJ5yQixrCtKTu5ANB5nuIfF+9BsqrR4WwEuS132FRc3u2fj+A6JAZBl/YQba1ZlJmk9VZw3kbTn/bUmdyc+I8qpB4NccXlcM42ctZPytclMIIFdgYJKoZIhvcNAQcBoIIFZwSCBWMwggVfMIIFWwYLKoZIhvcNAQwKAQKgggTuMIIE6jAcBgoqhkiG9w0BDAEDMA4ECJ1Ki0hPvXgHAgIIAASCBMhRwBlvuMpxYY23Ish1dyWwSXsmYdr8s8qbER+Pq4/ul6Lret3x35y2Ie3DhXDsrc1UlBWzX7/80RsdPG+TVbR2bOxi4l5flvuzQ92fGS8XEb5GMsrzC8ISkexlx0GjMWAylLpBQJrgpETc1mRB3R1ZJMS4QIhXSgKhX98jitHNQ/+ZBeMMCXPH4MmSrqk/BHD2b3VcWUr0s9kLUArNqleTFxEHlIgOz0iSPaqbDHzwcCfssIwB";
            var rawData = Convert.FromBase64String(base64);

            //var cert = new X509Certificate2(rawData, "1377779702", X509KeyStorageFlags.MachineKeySet);
            //Assert.AreNotEqual(cert, null);
        }
    }
}

