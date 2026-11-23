using System;
using Microsoft.Extensions.Logging;

namespace FlameBlame.Core.Logging
{
    public static class Loggers
    {
        // Compile-time, not Debug.isDebugBuild: this type initializer runs from field initializers
        // during ScriptableObject deserialization, where that property throws.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const LogLevel MinimumLevel = LogLevel.Information;
#else
        private const LogLevel MinimumLevel = LogLevel.Warning;
#endif

        private static readonly ILoggerFactory Factory =
            new LoggerFactory(new ILoggerProvider[] { new UnityLoggerProvider(MinimumLevel) });

        public static ICrashReporter CrashReporter { get; set; }

        public static ILogger<T> For<T>() => Factory.CreateLogger<T>();

        public static ILogger For(Type type) => Factory.CreateLogger(type);
    }
}
