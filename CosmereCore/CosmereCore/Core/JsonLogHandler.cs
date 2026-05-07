using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using Verse;
using static Cosmere.Core.Mod;

namespace Cosmere.Core;

[StaticConstructorOnStartup]
public static class JsonLogHandlerInstaller {
    static JsonLogHandlerInstaller() {
        Install();
    }

    public static void Install() {
        ILogHandler current = UnityEngine.Debug.unityLogger.logHandler;
        if (current is JsonLogHandler) return;
        UnityEngine.Debug.unityLogger.logHandler = new JsonLogHandler(current);
    }
}

public class JsonLogHandler : ILogHandler {
    private static readonly Regex StripRichTextRegex = new Regex(
        @"<color=#[0-9a-fA-F]+>|<color=\w+>|</color>|<b>|</b>|<i>|</i>",
        RegexOptions.Compiled
    );

    public readonly ILogHandler Inner;

    public JsonLogHandler(ILogHandler inner) {
        Inner = inner;
    }

    public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args) {
        if (!debugMode) {
            Inner.LogFormat(logType, context, format, args);
            return;
        }

        string raw;
        try {
            raw = args is { Length: > 0 } ? string.Format(format, args) : format;
        }
        catch {
            raw = format;
        }

        string json = BuildJson(logType, raw, null, context);
        Inner.LogFormat(logType, context, "{0}", json);
    }

    public void LogException(Exception exception, UnityEngine.Object context) {
        if (!debugMode) {
            Inner.LogException(exception, context);
            return;
        }

        string json = BuildJson(LogType.Exception, exception.Message ?? "<no message>", exception, context);
        Inner.LogFormat(LogType.Exception, context, "{0}", json);
    }

    private static string BuildJson(
        LogType logType,
        string message,
        Exception? exception,
        UnityEngine.Object? context
    ) {
        string clean = StripRichTextRegex.Replace(message, "");

        StackTrace stackTrace = exception != null
            ? new StackTrace(exception, true)
            : new StackTrace(2, true);

        List<FrameInfo> frames = ExtractFrames(stackTrace);
        FrameInfo? top = frames.Count > 0 ? frames[0] : null;

        StringBuilder sb = new StringBuilder(512);
        sb.Append('{');
        AppendField(sb, "timestamp", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture), false);
        AppendField(sb, "level", MapLogType(logType), true);
        AppendField(sb, "message", clean, true);

        if (top != null) {
            AppendField(sb, "file", top.File, true);
            sb.Append(",\"line\":").Append(top.Line);
            AppendField(sb, "method", top.Method, true);
        }

        AppendField(
            sb,
            "tick",
            Current.ProgramState == ProgramState.Playing && Find.TickManager != null
                ? Find.TickManager.TicksGame.ToString(CultureInfo.InvariantCulture)
                : "-1",
            true
        );
        AppendField(
            sb,
            "thread",
            Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture),
            true
        );

        if (context != null) {
            AppendField(sb, "context", context.ToString(), true);
        }

        if (exception != null) {
            AppendField(sb, "exceptionType", exception.GetType().FullName ?? exception.GetType().Name, true);
            string? exMessage = exception.Message;
            if (exMessage != null) {
                AppendField(sb, "exceptionMessage", exMessage, true);
            }
        }

        sb.Append(",\"stack\":[");
        for (int i = 0; i < frames.Count; i++) {
            if (i > 0) sb.Append(',');
            FrameInfo f = frames[i];
            sb.Append('{');
            AppendField(sb, "file", f.File, false);
            sb.Append(",\"line\":").Append(f.Line);
            AppendField(sb, "method", f.Method, true);
            sb.Append('}');
        }
        sb.Append(']');
        sb.Append('}');
        return sb.ToString();
    }

    private static List<FrameInfo> ExtractFrames(StackTrace stackTrace) {
        List<FrameInfo> frames = [];
        for (int i = 0; i < stackTrace.FrameCount; i++) {
            StackFrame? frame = stackTrace.GetFrame(i);
            MethodBase? method = frame?.GetMethod();
            string? declaring = method?.DeclaringType?.FullName;
            if (declaring != null &&
                (declaring.Contains("Cosmere.Core.Logger") ||
                 declaring.Contains("Cosmere.Core.JsonLogHandler") ||
                 declaring.StartsWith("UnityEngine.Logger") ||
                 declaring.StartsWith("UnityEngine.Debug"))) {
                continue;
            }

            string file = NormalizeFile(frame?.GetFileName());
            string methodName = declaring != null && method?.Name != null
                ? $"{declaring}.{method.Name}"
                : method?.Name ?? "<unknown>";

            frames.Add(new FrameInfo {
                File = file,
                Line = frame?.GetFileLineNumber() ?? 0,
                Method = methodName,
            });
        }
        return frames;
    }

    private static string NormalizeFile(string? raw) {
        if (raw == null) return "";
        string file = Regex.Replace(
            raw,
            @"^.*?(RimworldCosmere[\\/]RimworldCosmere[\\/]|RimWorld[\\/]Mods[\\/])+[\\/]*",
            ""
        );
        file = Regex.Replace(file, @"^(\w+)[\\/]\1[\\/]", "$1\\");
        return file
            .TrimStart('\\')
            .TrimStart('/')
            .Replace(".cs", "");
    }

    private static string MapLogType(LogType type) {
        switch (type) {
            case LogType.Error: return "Error";
            case LogType.Assert: return "Assert";
            case LogType.Warning: return "Warning";
            case LogType.Log: return "Info";
            case LogType.Exception: return "Exception";
            default: return type.ToString();
        }
    }

    private static void AppendField(StringBuilder sb, string key, string value, bool leadingComma) {
        if (leadingComma) sb.Append(',');
        sb.Append('"').Append(key).Append("\":\"");
        EscapeJson(sb, value);
        sb.Append('"');
    }

    private static void EscapeJson(StringBuilder sb, string value) {
        if (value == null) return;
        for (int i = 0; i < value.Length; i++) {
            char c = value[i];
            switch (c) {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < 0x20) {
                        sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                    }
                    else {
                        sb.Append(c);
                    }
                    break;
            }
        }
    }

    private class FrameInfo {
        public string File = "";
        public int Line;
        public string Method = "";
    }
}
