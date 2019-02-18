# spear
Spear轻量级微服务框架，高扩展性，目前已支持TCP、HTTP协议，采用Consul作为服务注册与发现组件，TCP协议采用DotNetty底层实现，HTTP协议采用ASP.NET CORE MVC实现。

### Contracts
``` c#
[ServiceRoute("test")]
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
    var m = root.GetSection("micro").Get<ServiceAddress>();
    address.Service = "192.168.1.xx";//服务注册地址
    address.Host = "localhost";  //主机地址
    address.Port = 5001; //端口地址
});
```

### Client
``` c#
var services = new ServiceCollection()
    .AddMicroClient(opt =>
    {
        opt.AddJsonCoder()
            .AddHttpProtocol()
            .AddTcpProtocol()
            .AddConsul("http://192.168.0.252:8500");
    });
var provider = services.BuildServiceProvider();
var proxy = provider.GetService<IProxyFactory>();
var service = proxy.Create<ITestContract>();
```
