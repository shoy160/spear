# spear
Spear轻量级微服务框架，高扩展性，目前已支持TCP、HTTP协议，采用Consul作为服务注册与发现组件，TCP协议采用DotNetty底层实现，HTTP协议采用ASP.NET CORE MVC实现。

| Package Name |  NuGet | Downloads | |
|--------------|  ------- |  ---- | -- |
| Spear.ProxyGenerator | [![nuget](https://img.shields.io/nuget/v/Spear.ProxyGenerator.svg?style=flat-square)](https://www.nuget.org/packages/Spear.ProxyGenerator) | [![stats](https://img.shields.io/nuget/dt/Spear.ProxyGenerator.svg?style=flat-square)](https://www.nuget.org/stats/packages/Spear.ProxyGenerator?groupby=Version) |
| Spear.Core | [![nuget](https://img.shields.io/nuget/v/Spear.Core.svg?style=flat-square)](https://www.nuget.org/packages/Spear.Core) | [![stats](https://img.shields.io/nuget/dt/Spear.Core.svg?style=flat-square)](https://www.nuget.org/stats/packages/Spear.Core?groupby=Version) | [Wiki](https://github.com/shoy160/spear/wiki) |
| Spear.Consul | [![nuget](https://img.shields.io/nuget/v/Spear.Consul.svg?style=flat-square)](https://www.nuget.org/packages/Spear.Consul) | [![stats](https://img.shields.io/nuget/dt/Spear.Consul.svg?style=flat-square)](https://www.nuget.org/stats/packages/Spear.Consul?groupby=Version) |
| Spear.Protocol.Http | [![nuget](https://img.shields.io/nuget/v/Spear.Protocol.Http.svg?style=flat-square)](https://www.nuget.org/packages/Spear.Protocol.Http) | [![stats](https://img.shields.io/nuget/dt/Spear.Protocol.Http.svg?style=flat-square)](https://www.nuget.org/stats/packages/Spear.Protocol.Http?groupby=Version) |
| Spear.Protocol.Tcp | [![nuget](https://img.shields.io/nuget/v/Spear.Protocol.Tcp.svg?style=flat-square)](https://www.nuget.org/packages/Spear.Protocol.Tcp) | [![stats](https://img.shields.io/nuget/dt/Spear.Protocol.Tcp.svg?style=flat-square)](https://www.nuget.org/stats/packages/Spear.Protocol.Tcp?groupby=Version) |

### Contracts
``` c#
[ServiceRoute("test")] //自定义路由键
public interface ITestContract : ISpearService
{
    Task Notice(string name);
    Task<string> Get(string name);
}
```
### Server
``` c#
var services = new ServiceCollection();
//服务协议
var protocol = ServiceProtocol.Tcp;
services.AddMicroService(builder =>
{
    builder
        .AddJsonCoder() //json编解码
        .AddConsul("http://127.0.0.1:8500"); //Consul服务注册与发现
    switch (protocol)
    {
        case ServiceProtocol.Tcp:
            //TCP协议,基于DotNetty
            builder.AddTcpProtocol();
            break;
        case ServiceProtocol.Http:
            //HTTP协议基于aspnetcore MVC
            builder.AddHttpProtocol();
            break;
    }
});

services.AddTransient<ITestContract, TestService>();

var provider = services.BuildServiceProvider();

provider.UseMicroService(address =>
{
    address.Service = "192.168.1.xx";//服务注册地址,需要保持与客户端的网络访问
    address.Host = "localhost";  //主机地址
    address.Port = 5001; //端口地址
});
```

### Client
``` c#
var services = new ServiceCollection()
    .AddMicroClient(opt =>
    {
        opt.AddJsonCoder() //json编解码
            .AddHttpProtocol()
            .AddTcpProtocol() //客户端允许支持多种协议
            .AddConsul("http://127.0.0.1:8500");
    });
var provider = services.BuildServiceProvider();
var proxy = provider.GetService<IProxyFactory>();
var service = proxy.Create<ITestContract>();
```

### BenchMark
![image](benchmark.png)
