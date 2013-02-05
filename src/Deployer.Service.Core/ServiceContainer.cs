using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using Deployer.Service.Contract.Factories;
using Deployer.Service.Core.Factories;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;
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
using Deployer.Service.Core.Helpers.IISManagers;
using Deployer.Service.Core.Settings;

namespace Deployer.Service.Core
{
    public static class ServiceContainer
    {
        public static void Configure(IUnityContainer container) {
            #region registering simple types

            container.RegisterType<IDeployService, DeployService>(new HierarchicalLifetimeManager());
            container.RegisterType<IServiceLogger, ServiceLogger>(new HierarchicalLifetimeManager());
            container.RegisterType<IPackageManager, PackageManager>(new HierarchicalLifetimeManager());
            container.RegisterType<IValidator, DirectoryValidator>("dir")
                     .RegisterType<IValidator, IISValidator>("iis")
                     .RegisterType<IValidator, DatabaseValidator>("db");
            container.RegisterType<IDeployer, DirectoryDeployer>("dir")
                     .RegisterType<IDeployer, IISDeployer>("iis")
                     .RegisterType<IDeployer, DatabaseDeployer>("db");

            #endregion

            #region registering factories

            container.RegisterType<IConfFactory, ConfFactory>(new HierarchicalLifetimeManager());
            container.RegisterType<Func<IPackageManager>>(new InjectionFactory(c => new Func<IPackageManager>(() => c.Resolve<IPackageManager>())));
            container.RegisterType<Func<IISInfo, string, IIISManager>>(new InjectionFactory(c => new Func<IISInfo, string, IIISManager>(
                (v, s) => {
                    switch (v.Version) {
                        case "6": return new IIS6Manager(v, s);
                        case "7": return new IIS7Manager(v, s);
                        default: throw new ArgumentException("Unknown version of IIS has been specified", "v");
                    }
                }
            )));

            container.RegisterType<IDeployServiceFactory, DeployServiceFactory>(new HierarchicalLifetimeManager());
            container.RegisterType<Func<string, DeployContext, IConfFactory, IConf>>(new InjectionFactory(
                c => new Func<string, DeployContext, IConfFactory, IConf>((sk, dc, cf) => new Conf(sk, dc, cf))));
            container.RegisterType<Func<IDeployEvents>>(new InjectionFactory(
                c => new Func<IDeployEvents>(() => OperationContext.Current.GetCallbackChannel<IDeployEvents>())));
            container.RegisterType<Func<IConf, IEnumerable<IValidator>>>(
                new InjectionFactory(c => new Func<IConf, IEnumerable<IValidator>>(
                    ds => {
                        var ctorParam = new ParameterOverride("conf", ds);
                        return ds.Survey.DeployMode == DeployMode.Install ?
                            new List<IValidator> { c.Resolve<IValidator>("dir", ctorParam), c.Resolve<IValidator>("iis", ctorParam), c.Resolve<IValidator>("db", ctorParam) } :
                            new List<IValidator> { c.Resolve<IValidator>("dir", ctorParam), c.Resolve<IValidator>("iis", ctorParam) };
                    })));
            container.RegisterType<Func<IConf, IEnumerable<IDeployer>>>(
                new InjectionFactory(c => new Func<IConf, IEnumerable<IDeployer>>(
                    ds => {
                        var ctorParam = new ParameterOverride("conf", ds);
                        return ds.Survey.DeployMode == DeployMode.Install ?
                            new List<IDeployer> { c.Resolve<IDeployer>("dir", ctorParam), c.Resolve<IDeployer>("iis", ctorParam), c.Resolve<IDeployer>("db", ctorParam) } :
                            new List<IDeployer> { c.Resolve<IDeployer>("dir", ctorParam) };
                    })));
            container.RegisterType<Func<IConf, string, ISession>>(new InjectionFactory(
                c => new Func<IConf, string, ISession>((ds, u) => new Session(ds, u))));
            container.RegisterType<Func<IDeployEvents, IConf, IServiceLogger, ISession, IDeployExceptionHandler>>(new InjectionFactory(
                c => new Func<IDeployEvents, IConf, IServiceLogger, ISession, IDeployExceptionHandler>(
                    (de, dss, sl, dsn) => new DeployExceptionHandler(de, dss, sl, dsn))));

            #endregion

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
            return typeof(T).GetMethods().Select(m => m.Name).ToArray();
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