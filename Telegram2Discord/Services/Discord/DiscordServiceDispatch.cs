using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Test.Services
{
    partial class DiscordServiceDispatch<T> : DispatchProxy
    {
        private ILogger<DiscordServiceDispatch<T>> _logger;

        public T Target { get; set; }

        public static T Create<T>(T target, IServiceProvider provider) where T : class
        {
            var proxy = Create<T, DiscordServiceDispatch<T>>()
                as DiscordServiceDispatch<T>;

            proxy.Target = target;
            proxy._logger = provider.GetService<ILogger<DiscordServiceDispatch<T>>>();

            return proxy as T;
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();

            var result = targetMethod.Invoke(Target, args);

            sw.Stop();
            _logger.LogDebug(
                $"Method {targetMethod.Name} took {sw.Elapsed} to execute");

            return result;
        }
    }
}
