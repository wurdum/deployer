using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Deployer.Service.Contract;
using Deployer.Service.Contract.Contexts;
using Deployer.Service.Contract.Data;
using Deployer.Service.Contract.Exceptions;
using Deployer.Service.Contract.Helpers;
using Deployer.Service.Contract.Settings;
using Deployer.Service.Core.Exceptions;
using Deployer.Service.Core.Helpers;
using Deployer.Service.Core.Helpers.Deplyers;
using Deployer.Service.Core.Helpers.Validators;
using Deployer.Service.Core.Settings;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace Deployer.Service.Core
{
    public static class ServiceContainer
    {
        public static void Configure(IUnityContainer container) {
            container.RegisterType<IDeployService, DeployService>(new HierarchicalLifetimeManager());
            container.RegisterType<IDeployServiceFactory, DeployServiceFactory>(new HierarchicalLifetimeManager());
            container.RegisterType<IServiceLogger, ServiceLogger>(new HierarchicalLifetimeManager());
            container.RegisterType<IPackageManager, PackageManager>(new HierarchicalLifetimeManager());
            container
                .RegisterType<IValidator, DirectoryValidator>("dir")
                .RegisterType<IValidator, IIS7Validator>("iis")
                .RegisterType<IValidator, DatabaseValidator>("db");
            container
                .RegisterType<IDeployer, DirectoryDeployer>("dir")
                .RegisterType<IDeployer, IIS7Deployer>("iis")
                .RegisterType<IDeployer, DatabaseDeployer>("db");

            container.RegisterType<Func<IPackageManager>>(new InjectionFactory(c => new Func<IPackageManager>(() => c.Resolve<IPackageManager>())));
            container.RegisterType<Func<string, DeployContext, DeployMode, IPackageManager, IDeploySettings>>(new InjectionFactory(
                c => new Func<string, DeployContext, DeployMode, IPackageManager, IDeploySettings>((sf, dc, pm, um) => new DeploySettings(sf, dc, pm, um))));
            container.RegisterType<Func<IDeployEvents>>(new InjectionFactory(
                c => new Func<IDeployEvents>(() => OperationContext.Current.GetCallbackChannel<IDeployEvents>())));

            container.RegisterType<Func<IDeploySettings, IEnumerable<IValidator>>>(
                new InjectionFactory(c => new Func<IDeploySettings, IEnumerable<IValidator>>(
                    ds => {
                        var ctorParam = new ParameterOverride("appSettings", ds);
                        return ds.DeployMode == DeployMode.Install ?
                            new List<IValidator> { c.Resolve<IValidator>("dir", ctorParam), c.Resolve<IValidator>("iis", ctorParam), c.Resolve<IValidator>("db", ctorParam) } :
                            new List<IValidator> { c.Resolve<IValidator>("dir", ctorParam), c.Resolve<IValidator>("iis", ctorParam) };
                    })));
            container.RegisterType<Func<IDeploySettings, IEnumerable<IDeployer>>>(
                new InjectionFactory(c => new Func<IDeploySettings, IEnumerable<IDeployer>>(
                    ds => {
                        var ctorParam = new ParameterOverride("appSettings", ds);
                        return ds.DeployMode == DeployMode.Install ?
                            new List<IDeployer> { c.Resolve<IDeployer>("dir", ctorParam), c.Resolve<IDeployer>("iis", ctorParam), c.Resolve<IDeployer>("db", ctorParam) } :
                            new List<IDeployer> { c.Resolve<IDeployer>("dir", ctorParam) };
                    })));

            container.RegisterType<Func<IDeploySettings, string, string, IDeploySession>>(new InjectionFactory(
                c => new Func<IDeploySettings, string, string, IDeploySession>((ds, s, u) => new DeploySession(ds, s, u))));
            container.RegisterType<Func<IDeployEvents, IDeploySettings, IServiceLogger, IDeploySession, IDeployExceptionHandler>>(new InjectionFactory(
                c => new Func<IDeployEvents, IDeploySettings, IServiceLogger, IDeploySession, IDeployExceptionHandler>(
                    (de, dss, sl, dsn) => new DeployExceptionHandler(de, dss, sl, dsn))));

            container.AddNewExtension<Interception>();
            container.Configure<Interception>()
                .SetInterceptorFor<IValidator>("dir", new TransparentProxyInterceptor())
                .SetInterceptorFor<IValidator>("iis", new TransparentProxyInterceptor())
                .SetInterceptorFor<IValidator>("db", new TransparentProxyInterceptor())
                .SetInterceptorFor<IDeployer>("dir", new TransparentProxyInterceptor())
                .SetInterceptorFor<IDeployer>("iis", new TransparentProxyInterceptor())
                .SetInterceptorFor<IDeployer>("db", new TransparentProxyInterceptor())
                .SetInterceptorFor<IPackageManager>(new TransparentProxyInterceptor());


            var methodsToTrack = new List<string>();
            methodsToTrack.AddRange(GetMethods<IDeployer>());
            methodsToTrack.AddRange(GetMethods<IValidator>());
            methodsToTrack.AddRange(GetMethods<IPackageManager>());

            container.Configure<Interception>()
                .AddPolicy("ServiceMethodsInterceptionPolicy")
                .AddMatchingRule<MemberNameMatchingRule>(new InjectionConstructor(new InjectionParameter(methodsToTrack.ToArray())))
                .AddCallHandler(new LoggerInterceptor(container));
        }

        private static IEnumerable<string> GetMethods<T>() {
            var type = typeof(T);
            return type.GetMethods().Select(m => m.Name).ToArray();
        }
    }

    public class LoggerInterceptor : ICallHandler
    {
        private readonly IServiceLogger _logger;

        public LoggerInterceptor(IUnityContainer container) {
            _logger = container.Resolve<IServiceLogger>();
        }

        public IMethodReturn Invoke(IMethodInvocation input, GetNextHandlerDelegate getNext) {
            var methodName = input.Target.GetType().FullName + "." + input.MethodBase.Name;
            _logger.Info(string.Format("Method '{0}' called", methodName));
            var result = getNext()(input, getNext);

            _logger.Info(result.Exception == null
                             ? string.Format("Call '{0}' complete", methodName)
                             : string.Format("Call '{0}' completes with exception: {1}", methodName, result.Exception.GetType()));

            return result;
        }

        public int Order { get; set; }
    }
}