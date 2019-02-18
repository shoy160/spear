# spear
Spear轻量级微服务框架，高扩展性，目前已支持TCP、HTTP协议，采用Consul作为服务注册与发现组件，TCP协议采用DotNetty底层实现，HTTP协议采用ASP.NET CORE MVC实现。

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
