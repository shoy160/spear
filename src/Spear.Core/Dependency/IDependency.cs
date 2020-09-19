namespace Spear.Core.Dependency
{
    /// <summary> 依赖注入接口，表示该接口实现类自动注册到Ioc容器 </summary>
    public interface IDependency { }

    /// <summary> 生命周期基于请求的注入 </summary>
    public interface IScopedDependency { }

    /// <summary> 单例注入 </summary>
    public interface ISingleDependency { }
}
