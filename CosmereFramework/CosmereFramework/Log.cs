using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Verse;

namespace CosmereFramework {
    public static class Log {
        private static readonly Dictionary<LogLevel, Color> logColors = new Dictionary<LogLevel, Color> {
            { LogLevel.None, Color.black },
            { LogLevel.Important, Color.green },
            { LogLevel.Error, Color.red },
            { LogLevel.Warning, Color.yellow },
            { LogLevel.Info, Color.blue },
            { LogLevel.Verbose, Color.gray },
        };

        private static CosmereSettings CosmereSettings {
            get => LoadedModManager.GetMod<CosmereFramework>().Settings;
        }

        public static void Message(string message, LogLevel level = LogLevel.Info) {
            if (level > CosmereSettings.logLevel || level == LogLevel.None) return;
            if (level == LogLevel.Error && DebugSettings.pauseOnError && Current.ProgramState == ProgramState.Playing) Find.TickManager.Pause();

            var ns = "None";
            var stack = "";
            var stackTrace = new StackTrace(1, true);
            for (var i = 0; i < stackTrace.FrameCount; i++) {
                var frame = stackTrace.GetFrame(i); // 1 = immediate caller
                var method = frame.GetMethod();
                if (method.DeclaringType?.FullName?.Contains("CosmereFramework.Log") ?? false) {
                    continue;
                }

                ns = method.DeclaringType?.Namespace?.Split('.')[0].Replace("Cosmere", "");
                if (frame.GetFileName() != null) {
                    stack = $"[{frame.GetFileName()}:{frame.GetFileLineNumber()}]";
                }

                break;
            }

            var prefix = string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGBA(logColors[level]), $"[Cosmere - {ns}]{stack}[{level.ToString()}]");

            Verse.Log.Message($"{prefix} {message}");
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
}