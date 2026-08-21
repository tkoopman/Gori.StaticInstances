; Shipped analyzer releases
; https://github.com/dotnet/roslyn/blob/main/src/RoslynAnalyzers/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
GSSI001 | Gori.StaticInstances | Error | NonPartialType
GSSI002 | Gori.StaticInstances | Error | NoDefaultConstructor
GSSI003 | Gori.StaticInstances | Info | NonPrivateDefaultConstructor
GSSI004 | Gori.StaticInstances | Error | InvalidInstanceName
GSSI005 | Gori.StaticInstances | Error | DuplicateInstanceName
GSSI006 | Gori.StaticInstances | Warning | IdenticalInstances
GSSI007 | Gori.StaticInstances | Error | ArgumentCountMismatch
GSSI008 | Gori.StaticInstances | Warning | ArgumentTypeMismatch
GSSI009 | Gori.StaticInstances | Info | ValueArgumentIsNull
GSSI010 | Gori.StaticInstances | Error | StaticInstanceOnPrimaryConstructor

## Release 1.0.2

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
GSSI011 | Gori.StaticInstances | Error | LanguageVersionTooLow
