using System;

namespace FlameBlame.Core.Logging
{
    public interface ICrashReporter
    {
        void RecordError(string message, Exception exception);
    }
}
