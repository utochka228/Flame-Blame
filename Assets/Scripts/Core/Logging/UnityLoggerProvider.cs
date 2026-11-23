using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace FlameBlame.Core.Logging
{
    public class UnityLoggerProvider : ILoggerProvider
    {
        private readonly LogLevel _minimumLevel;

        public UnityLoggerProvider(LogLevel minimumLevel)
        {
            _minimumLevel = minimumLevel;
        }

        public ILogger CreateLogger(string categoryName) => new UnityLogger(categoryName, _minimumLevel);

        public void Dispose()
        {
        }

        private class UnityLogger : ILogger
        {
            private readonly string _categoryName;
            private readonly LogLevel _minimumLevel;

            public UnityLogger(string categoryName, LogLevel minimumLevel)
            {
                _categoryName = categoryName;
                _minimumLevel = minimumLevel;
            }

            public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLevel && logLevel != LogLevel.None;

            public IDisposable BeginScope<TState>(TState state) => null;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel)) return;

                var message = $"[{_categoryName}] {formatter(state, exception)}";
                if (exception != null)
                    message = $"{message}{Environment.NewLine}{exception}";

                var context = FindContext(state);

                switch (logLevel)
                {
                    case LogLevel.Critical:
                    case LogLevel.Error:
                        UnityEngine.Debug.LogError(message, context);
                        Loggers.CrashReporter?.RecordError(message, exception);
                        break;
                    case LogLevel.Warning:
                        UnityEngine.Debug.LogWarning(message, context);
                        break;
                    default:
                        UnityEngine.Debug.Log(message, context);
                        break;
                }
            }

            private static UnityEngine.Object FindContext<TState>(TState state)
            {
                if (state is IReadOnlyList<KeyValuePair<string, object>> values)
                {
                    for (var index = 0; index < values.Count; index++)
                    {
                        if (values[index].Value is UnityEngine.Object unityObject && unityObject)
                            return unityObject;
                    }
                }

                return null;
            }
        }
    }
}
