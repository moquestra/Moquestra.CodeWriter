# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-09-02

### Added

- `CodeBuilder`, a text accumulator for code generation templates. `Write` and
  `WriteLine` take interpolated strings, render `IFormattable` values with the
  invariant culture, and reindent multiline values from their second line with
  a continuation prefix taken from the line where the interpolation begins.
- `PreservedPrefixParts` to choose which characters of that prefix are kept
  verbatim on continuation lines; the rest become spaces. Selectable per
  builder or per call.
- `CodeBuilderSettings` with the output newline (`"\n"` or `"\r\n"`), trimming
  of whitespace-only value lines, and the default prefix parts. Input CRLF, CR
  and LF are normalized to the configured newline.
- Source-only NuGet package that installs as a development dependency. Its
  sources compile into the consuming assembly as `internal` types, require
  C# 7.3 or later, and use only APIs available in .NET Standard 2.0.
- `copy-source.ps1`, `copy-source.sh` and `copy-source.bat` for copying the
  sources into projects that do not use NuGet, including Unity assemblies.
