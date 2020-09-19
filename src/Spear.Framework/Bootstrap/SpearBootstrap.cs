using Autofac;
using Spear.Core.Dependency;
using Spear.Core.Reflection;
using System;
using System.Linq;
using ILifetimeScope = Autofac.ILifetimeScope;

namespace Spear.Framework
{
    /// <summary> 启动器 </summary>
    public class SpearBootstrap : AbstractBootstrap
    {
        private bool _init;

        public ContainerBuilder Builder { get; set; }

        public ILifetimeScope ContainerRoot { get; private set; }

        /// <summary> Ioc容器 </summary>
        private IContainer _container;

        /// <summary> Ioc构建事件 </summary>
        public event Action<ContainerBuilder> BuilderHandler;

        internal void ReBuild(Action<ContainerBuilder> builderAction)
        {
            var updater = new ContainerBuilder();
            builderAction.Invoke(updater);
            updater.Update(_container);
        }

        public IContainer CreateContainer()
        {
            _container = Builder.Build();
            ContainerRoot = _container;
            DatabaseInit();
            //ModulesInstaller();
            return _container;
        }

        public void CreateContainer(ILifetimeScope root)
        {
            ContainerRoot = root;
            DatabaseInit();
            //ModulesInstaller();
        }

        /// <summary> 初始化 </summary>
        public override void Initialize()
        {
            if (_init) return;
            _init = true;

            LoggerInit();
            CacheInit();
            IocRegisters();
            BuilderHandler?.Invoke(Builder);
        }

        /// <summary> Ioc注册 </summary>
        protected override void IocRegisters()
        {
            Builder = Builder ?? new ContainerBuilder();
            //注入程序集查找器
            var finder = new DefaultAssemblyFinder();
            Builder.RegisterInstance(finder).As<IAssemblyFinder>().SingleInstance();
            var assemblies = finder.FindAll().ToArray();
            Builder.RegisterAssemblyTypes(assemblies)
                .Where(type => typeof(IScopedDependency).IsAssignableFrom(type) && !type.IsAbstract)
                .AsSelf() //自身服务，用于没有接口的类
                .AsImplementedInterfaces() //接口服务
                .PropertiesAutowired()//属性注入
                .InstancePerLifetimeScope(); //保证生命周期基于请求

            Builder.RegisterAssemblyTypes(assemblies)
                .Where(type => typeof(IDependency).IsAssignableFrom(type) && !type.IsAbstract)
                .AsSelf() //自身服务，用于没有接口的类
                .AsImplementedInterfaces() //接口服务
                .PropertiesAutowired(); //属性注入

            Builder.RegisterAssemblyTypes(assemblies)
                .Where(type => typeof(ISingleDependency).IsAssignableFrom(type) && !type.IsAbstract)
                .AsSelf() //自身服务，用于没有接口的类
                .AsImplementedInterfaces() //接口服务
                .PropertiesAutowired()//属性注入
                .SingleInstance(); //保证单例注入

            IocManager = new IocManager(this);
            CurrentIocManager.SetIocManager(IocManager);
            Builder.RegisterInstance(IocManager).AsSelf().As<IIocManager>().SingleInstance();
        }

        protected override void CacheInit()
        {
            //CacheManager.SetProvider(CacheLevel.First, new RuntimeMemoryCacheProvider());
        }

        protected override void LoggerInit()
        {
            //LogManager.AddAdapter(new DefaultLoggerAdapter());
            //LogManager.AddAdapter(new Log4NetAdapter());
        }

        protected override void DatabaseInit()
        {
        }
    }
}
