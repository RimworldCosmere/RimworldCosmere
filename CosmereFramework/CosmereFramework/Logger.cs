using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using static CosmereFramework.CosmereFramework;
using Debug = UnityEngine.Debug;

namespace CosmereFramework;

public static class Logger {
    private static readonly Dictionary<LogLevel, Color> LOGColors = new Dictionary<LogLevel, Color> {
        { LogLevel.None, Color.black },
        { LogLevel.Important, Color.green },
        { LogLevel.Error, Color.red },
        { LogLevel.Warning, Color.yellow },
        { LogLevel.Info, Color.blue },
        { LogLevel.Verbose, Color.gray },
    };

    private static bool CurrentlyLoggingError;

    public static void Message(string message, LogLevel level = LogLevel.Info) {
        try {
            if (level > logLevel || level == LogLevel.None) return;
            switch (level) {
                case LogLevel.Error
                    when DebugSettings.pauseOnError && Current.ProgramState == ProgramState.Playing:
                    Find.TickManager.Pause(); break;
                case >= LogLevel.Warning when Prefs.OpenLogOnWarnings: Log.TryOpenLogWindow(); break;
            }

            string? ns = "None";
            string stack = "";
            StackTrace stackTrace = new StackTrace(1, true);
            for (int i = 0; i < stackTrace.FrameCount; i++) {
                StackFrame? frame = stackTrace.GetFrame(i); // 1 = immediate caller
                MethodBase? method = frame?.GetMethod();
                if (method?.DeclaringType?.FullName?.Contains("CosmereFramework.Logger") ?? false) {
                    continue;
                }

                string? mod = method?.DeclaringType?.Assembly.GetName().Name;
                ns = mod?.Split('.')[0].Replace("Cosmere", "");
                if (frame?.GetFileName() != null && mod != null) {
                    string filename = Regex.Replace(
                        frame.GetFileName()!,
                        @"^.*?(RimworldCosmere\\RimworldCosmere\\|RimWorld\\Mods\\)+\\*",
                        ""
                    );
                    filename = filename.Replace(mod, "").TrimStart('\\');
                    if (filename == ".cs") filename = $"{mod}.cs";
                    filename = filename.TrimStart('\\');

                    stack = $"[{filename}:{frame.GetFileLineNumber()}]";
                }

                break;
            }

            if (CurrentlyLoggingError) return;

            if (level >= LogLevel.Error) CurrentlyLoggingError = true;
            Log.Message(
                $"{ColoredMessage(LOGColors[level], $"[Cosmere - {ns}]{stack}[{level.ToString()}]")} {message}"
            );
            Log.ResetMessageCount();
        } catch (Exception e) {
            Debug.LogException(new Exception("An error occured while logging an error: ", e));
        } finally {
            if (level >= LogLevel.Error) CurrentlyLoggingError = false;
        }
    }

    public static string ColoredBoolean(Color color, bool boolean) {
        return string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(color), boolean);
    }

    public static string ColoredMessage(Color color, string message) {
        return string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(color), message);
    }

    public static void Message(FormattableString message, LogLevel level = LogLevel.Info) {
        Message(message.ToString(), level);
    }

    public static void Info(string message) {
        Message(message);
    }

    public static void Info(FormattableString message) {
        Message(message);
    }

    public static void Warning(string message) {
        Message(message, LogLevel.Warning);
    }

    public static void Warning(FormattableString message) {
        Message(message, LogLevel.Warning);
    }

    public static void Error(string message) {
        Message(message, LogLevel.Error);
    }

    public static void Error(FormattableString message) {
        Message(message, LogLevel.Error);
    }

    public static void Verbose(string message) {
        Message(message, LogLevel.Verbose);
    }

    public static void Verbose(FormattableString message) {
        Message(message, LogLevel.Verbose);
    }

    public static void Important(string message) {
        Message(message, LogLevel.Important);
    }

    public static void Important(FormattableString message) {
        Message(message, LogLevel.Important);
    }
}