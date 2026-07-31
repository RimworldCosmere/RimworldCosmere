using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace Cosmere.Core.BetaHub;

public sealed class SubmitResult {
    public SubmitOutcome Outcome;
    public string? IssueUrl;
    public string? ServerMessage;
}

/// <summary>
///     Creates a BetaHub issue or feature request, then attaches the log and screenshot.
/// </summary>
public static class BetaHubClient {
    public static void Submit(FeedbackReport report, byte[]? screenshotJpeg, Action<SubmitResult> onDone) {
        Post(report, screenshotJpeg, onDone, includeReleaseLabel: true);
    }

    private static void Post(
        FeedbackReport report,
        byte[]? screenshotJpeg,
        Action<SubmitResult> onDone,
        bool includeReleaseLabel
    ) {
        DiagnosticsFacts facts = DiagnosticsBundle.Collect();
        string url = report.Kind == FeedbackKind.Suggestion
            ? BetaHubConfig.FeatureRequestsUrl
            : BetaHubConfig.IssuesUrl;
        string releaseKey = $"{BetaHubFormEncoder.ParameterRoot(report.Kind)}[release_label]";

        WWWForm form = new WWWForm();
        foreach (KeyValuePair<string, string> pair in BetaHubFormEncoder.Encode(report, facts)) {
            if (!includeReleaseLabel && pair.Key == releaseKey) continue;

            form.AddField(pair.Key, pair.Value);
        }

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        Authorize(request);

        BetaHubRequestPump.Send(
            request,
            done => {
                SubmitOutcome outcome = BetaHubStatusMapper.Map(done.responseCode);
                string body = done.downloadHandler?.text ?? string.Empty;

                SubmitResult result = new SubmitResult {
                    Outcome = outcome,
                    IssueUrl = ReadJsonString(body, "url"),
                    ServerMessage = ReadJsonString(body, "error"),
                };

                if (outcome != SubmitOutcome.Success) {
                    Logger.Warning($"BetaHub submit failed with {done.responseCode}: {body}");

                    // A 403 naming release permission means CI has not published a release for
                    // this build yet. Resending without the label files it against the latest
                    // release, which beats losing the report.
                    if (includeReleaseLabel && BetaHubStatusMapper.IsMissingReleasePermission(done.responseCode, body)) {
                        Logger.Warning("Retrying the BetaHub submit without a release label.");
                        Post(report, screenshotJpeg, onDone, includeReleaseLabel: false);

                        return;
                    }

                    onDone(result);

                    return;
                }

                if (report.Kind == FeedbackKind.Bug) {
                    string? issuePath = ReadIssuePath(result.IssueUrl);
                    if (issuePath != null) {
                        AttachLog(issuePath);
                        if (report.IncludeScreenshot && screenshotJpeg != null) {
                            AttachScreenshot(issuePath, screenshotJpeg);
                        }
                    }
                }

                onDone(result);
            }
        );
    }

    private static void Authorize(UnityWebRequest request) {
        request.SetRequestHeader("Authorization", $"FormUser {BetaHubToken.Value}");
        request.SetRequestHeader("BetaHub-Project-ID", BetaHubConfig.ProjectId);
        request.SetRequestHeader("Accept", "application/json");
    }

    private static void AttachLog(string issuePath) {
        byte[] bytes = Encoding.UTF8.GetBytes(DiagnosticsText.Build(DiagnosticsBundle.Collect(), DiagnosticsBundle.ReadLogTail()));

        WWWForm form = new WWWForm();
        form.AddBinaryData("log_file[file]", bytes, "cosmere-diagnostics.log", "text/plain");

        // Unlisted rather than encrypted. The URL is unguessable but not access controlled,
        // which is why nothing beyond the SteamID and the log goes in here.
        form.AddField("log_file[developer_private]", "true");

        UnityWebRequest request = UnityWebRequest.Post(BetaHubConfig.LogFilesUrl(issuePath), form);
        Authorize(request);

        BetaHubRequestPump.Send(request, done => LogAttachmentOutcome("log", done));
    }

    private static void AttachScreenshot(string issuePath, byte[] jpeg) {
        WWWForm form = new WWWForm();
        form.AddBinaryData("screenshot[image]", jpeg, "cosmere-screenshot.jpg", "image/jpeg");

        UnityWebRequest request = UnityWebRequest.Post(BetaHubConfig.ScreenshotsUrl(issuePath), form);
        Authorize(request);

        BetaHubRequestPump.Send(request, done => LogAttachmentOutcome("screenshot", done));
    }

    private static void LogAttachmentOutcome(string what, UnityWebRequest done) {
        if (BetaHubStatusMapper.Map(done.responseCode) == SubmitOutcome.Success) return;

        Logger.Warning($"BetaHub {what} upload failed with {done.responseCode}: {done.downloadHandler?.text}");
    }

    // A published create returns the scoped id (/issues/8), a draft returns the g- form.
    // Media endpoints accept either, so the last url segment is used as-is.
    private static string? ReadIssuePath(string? issueUrl) {
        if (string.IsNullOrEmpty(issueUrl)) return null;

        int lastSlash = issueUrl!.LastIndexOf('/');

        return lastSlash < 0 ? null : issueUrl.Substring(lastSlash + 1);
    }

    // Verse.Json has no usable deserializer in the shipped assembly (verified against
    // Assembly-CSharp.dll: no Verse.Json type exists at all, in any namespace). Pulling in a
    // full JSON library just to read two flat string fields off a response body is not worth
    // the dependency for a mod that ships into RimWorld, so this reads "key":"value" text
    // directly instead of parsing the document.
    private static string? ReadJsonString(string body, string key) {
        if (string.IsNullOrEmpty(body)) return null;

        try {
            Match match = Regex.Match(
                body,
                $"\"{Regex.Escape(key)}\"\\s*:\\s*(?:\"(?<value>(?:\\\\.|[^\"\\\\])*)\"|null)",
                RegexOptions.None,
                TimeSpan.FromMilliseconds(200)
            );

            return match.Success && match.Groups["value"].Success ? Unescape(match.Groups["value"].Value) : null;
        } catch (Exception) {
            return null;
        }
    }

    private static string Unescape(string value) {
        if (value.IndexOf('\\') < 0) return value;

        StringBuilder builder = new StringBuilder(value.Length);
        for (int i = 0; i < value.Length; i++) {
            char c = value[i];
            if (c != '\\' || i == value.Length - 1) {
                builder.Append(c);
                continue;
            }

            i++;
            builder.Append(value[i] switch {
                'n' => '\n',
                'r' => '\r',
                't' => '\t',
                'b' => '\b',
                'f' => '\f',
                var other => other,
            });
        }

        return builder.ToString();
    }
}
