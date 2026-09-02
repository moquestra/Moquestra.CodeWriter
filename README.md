# Moquestra.CodeWriter

Source-only text writer for code generation.

`CodeBuilder` accumulates interpolated strings. Literal text is written as-is,
and a multiline interpolated value is reindented from its second line with the
prefix of the line where the interpolation begins - so a template stays
readable while the generated code keeps its indentation.

```csharp
var body = "a();\nb();";
var builder = new CodeBuilder();

builder.WriteLine($"void M()");
builder.WriteLine($"{{");
builder.WriteLine($"    {body}");
builder.WriteLine($"}}");
```

```
void M()
{
    a();
    b();
}
```

With C# 11 raw string literals the same template reads like the code it
produces - no escaped braces, no `\n`:

```csharp
builder.Write($$"""
    void M()
    {
        {{body}}
    }
    """);
```

The prefix is copied verbatim by default, which also works for comment
templates:

```csharp
var summary = "First line.\nSecond line.";

builder.Write($"/// {summary}");
```

```
/// First line.
/// Second line.
```

## Installation

### NuGet

```
dotnet add package Moquestra.CodeWriter
```

The package ships C# source files instead of an assembly. They compile into your
project as `internal` types, so every consuming assembly gets its own copy and
different versions never collide. The package is a development dependency:
`dotnet add package` writes `PrivateAssets="all"` for you, and the reference does
not flow to consumers of your own package.

### Copying the sources

Projects that do not use NuGet can copy the sources from a clone of this
repository with one of the scripts in the root:

```
# PowerShell
./copy-source.ps1 -Destination <folder>

# sh
./copy-source.sh <folder>

# cmd
copy-source.bat <folder>
```

Each script copies the source files verbatim into the folder.

#### Sharing a copy between assemblies

The types are `internal`, so a copy is visible only inside the assembly it was
compiled into. To let other assemblies use the same copy, add this to any C#
file in the assembly that holds it:

```csharp
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("FriendAssembly")]
```

## Usage

### Writing

- `Write(FormattableString)` renders the interpolated string and accumulates it.
- `WriteLine()` ends the current line; `WriteLine(FormattableString)` writes and
  ends the line.
- `ToString()` returns everything accumulated so far.

Values that implement `IFormattable` are rendered with the invariant culture;
other values fall back to `ToString()`, and `null` renders as an empty string. A
`CodeBuilder` can be interpolated into another builder, so fragments compose.

An interpolation hole holds a plain expression. Format and alignment components
inside a hole - `{value:X}`, `{value,-10}` - are not supported and throw
`FormatException`.

### Continuation prefixes

Each non-empty line of a multiline value after the first receives a
continuation prefix derived from the line where the interpolation began.
`PreservedPrefixParts` selects which characters of that line are kept verbatim;
the rest are each replaced with a space.

| Value | Prefix `"\t- "` becomes |
|---|---|
| `All` (default) | `"\t- "` |
| `Tabs` | `"\t  "` |
| `NonWhitespace` | `" - "` |
| `None` | `"   "` |

Empty lines get no prefix. When a line ends holding only prefix and value
whitespace, it is emptied (`TrimWhitespaceOnlyValueLines`).

`Write(FormattableString, PreservedPrefixParts)` overrides the selection for
one call.

### Settings

`CodeBuilderSettings` is immutable and groups the output options:

```csharp
var settings = new CodeBuilderSettings(
    newLine: "\r\n",                    // "\n" (default) or "\r\n"
    trimWhitespaceOnlyValueLines: true, // default
    prefixParts: PreservedPrefixParts.All);

var builder = new CodeBuilder(settings);
```

Input line endings - CRLF, CR and LF - are normalized to the configured newline,
in literals and in interpolated values alike.

## Requirements

The sources compile with C# 7.3 or later and use only APIs available in
.NET Standard 2.0, so they can be added to any project whose target framework
supports .NET Standard 2.0. They contain no nullable annotations and compile
without warnings whether nullable reference types are enabled or disabled.

## Unity

Use one of the copy scripts to place the sources in the folder of the assembly
definition that will use them - usually an Editor assembly - and share the copy
with other assemblies as described above if needed. NuGetForUnity can install the
package too, but it puts the files under `Assets/Packages` without an assembly
definition, so they compile into `Assembly-CSharp`, which assembly-definition
based code cannot reference.
