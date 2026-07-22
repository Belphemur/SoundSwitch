#nullable enable
using Spectre.Console.Cli;

namespace SoundSwitch.CLI.Commands;

/// <summary>
/// Base settings shared by every CLI command that supports machine-readable output.
/// Inheriting from this class automatically registers the <c>--json</c> option.
/// </summary>
public class JsonCommandSettings : CommandSettings
{
    [CommandOption("--json")]
    public bool Json { get; set; }
}
