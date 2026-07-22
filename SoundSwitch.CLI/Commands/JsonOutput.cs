#nullable enable
using System.Text.Json;

namespace SoundSwitch.CLI.Commands;

/// <summary>
/// Centralized JSON output helpers for CLI commands running with <c>--json</c>.
/// All output is written to <see cref="Console.Out"/> via <see cref="Console.WriteLine"/>
/// so it is safe to pipe into tools like <c>jq</c> or capture into a variable.
/// Spectre.Console helpers (status spinners, markup) must never be used in this path
/// because they emit ANSI escape sequences that corrupt piped output.
/// </summary>
internal static class JsonOutput
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static void Write(object value) => Console.WriteLine(JsonSerializer.Serialize(value, Options));

    public static void WriteError(string message) => Write(new { error = message });
}
