param(
    [Parameter(Mandatory = $true)]
    [string]$Destination
)

$ErrorActionPreference = "Stop"

$sourceDirectory = Join-Path $PSScriptRoot "src/Moquestra.CodeWriter"

if (Test-Path -LiteralPath $Destination -PathType Leaf) {
    throw "Destination is an existing file: $Destination"
}

if (-not (Test-Path -LiteralPath $Destination)) {
    New-Item -ItemType Directory -Path $Destination | Out-Null
}

Get-ChildItem -Path $sourceDirectory -Filter "*.cs" |
    Copy-Item -Destination $Destination -Force
