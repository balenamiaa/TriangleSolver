# WebAssembly Size Optimization Guide

This document outlines the optimizations implemented to reduce the Blazor WebAssembly payload size for the TriangleSolver application.

## Applied Optimizations

### 1. Project Configuration Optimizations (`TriangleSolver.Client.csproj`)

**Globalization & Culture Settings:**
- `InvariantGlobalization=true` - Removes culture-specific assemblies (~500KB+ savings)
- `BlazorWebAssemblyPreserveCollationData=false` - Removes collation data

**Trimming & IL Optimizations:**
- `PublishTrimmed=true` - Enables aggressive assembly trimming
- `TrimMode=full` - Most aggressive trimming mode
- `WasmStripILAfterAOT=true` - Removes IL after AOT compilation
- `IlcOptimizationPreference=Size` - Optimizes for size over speed
- `IlcFoldIdenticalMethodBodies=true` - Merges identical method bodies

**Feature Removal:**
- `DebuggerSupport=false` - Removes debugging support (~200KB+ savings)
- `EventSourceSupport=false` - Removes event tracing
- `HttpActivityPropagationSupport=false` - Removes HTTP activity propagation
- `EnableUnsafeBinaryFormatterSerialization=false` - Removes binary formatter
- `EnableUnsafeUTF7Encoding=false` - Removes UTF-7 encoding support
- `MetadataUpdaterSupport=false` - Removes hot reload metadata
- `UseSystemResourceKeys=true` - Uses system resource keys only

**WASM Runtime Optimizations:**
- `BlazorWebAssemblyJiterpreter=false` - Disables jiterpreter (smaller but slower)
- `UseNativeHttpHandler=true` - Uses native browser HTTP handler

**Compression:**
- `CompressionEnabled=true` - Enables general compression
- `BlazorEnableCompression=true` - Enables Blazor-specific compression

### 2. Assembly Lazy Loading

Large, rarely-used assemblies are configured for lazy loading:
- `Microsoft.VisualBasic.Core.wasm` (~167KB)
- `System.Data.Common.wasm` (~371KB)
- `System.Linq.Parallel.wasm` (~86KB)
- `System.Text.Encoding.CodePages.wasm` (~507KB)
- `System.Private.Xml.wasm` (~1MB)
- `System.Security.Cryptography.wasm` (~206KB)
- Various other assemblies

### 3. Linker Configuration (`Linker.xml`)

Custom linker configuration:
- Preserves only necessary types from core assemblies
- Completely removes unused assemblies
- Targets specific Blazor components for preservation

### 4. Runtime Configuration (`runtimeconfig.template.json`)

Optimizes WASM runtime:
- Enables invariant globalization
- Disables unsafe binary formatter
- Configures minimal logging
- Optimizes JavaScript interop

### 5. Production Publish Profile

The `Production.pubxml` profile includes additional production-only optimizations:
- `WasmOptimizationLevel=2` - Maximum WASM optimization
- `IlcDisableReflection=true` - Disables reflection support
- Various caching and boot resource optimizations

## Expected Size Reductions

With these optimizations, you can expect:

**Before optimization:**
- Total payload: ~5-6MB (uncompressed)
- ~2-3MB (gzipped)

**After optimization:**
- Total payload: ~2-3MB (uncompressed)
- ~800KB-1.5MB (gzipped)

**Specific savings:**
- ICU data removal: ~900KB
- Unused BCL assemblies: ~1-2MB
- Debugging support removal: ~200KB
- Culture/globalization data: ~500KB

## Build Commands

### Development Build (with optimizations):
```bash
dotnet build -c Debug
```

### Production Build (maximum optimization):
```bash
dotnet publish -c Release -p:PublishProfile=Production
```

### Size Analysis:
```bash
# Publish and analyze bundle size
dotnet publish -c Release
# Check the wwwroot/_framework directory for .wasm.gz files
```

## Trade-offs

**Benefits:**
- 50-70% smaller payload size
- Faster initial load time
- Reduced bandwidth usage

**Limitations:**
- No culture-specific formatting (dates, numbers, etc.)
- No debugging support in optimized builds
- Some reflection-based libraries may not work
- Lazy-loaded assemblies have slight delay on first use
- Slightly slower runtime performance (size over speed optimization)

## Troubleshooting

**If trimming breaks functionality:**
1. Add specific types to `Linker.xml`
2. Use `[DynamicallyAccessedMembers]` attributes
3. Consider moving problematic code to non-trimmed assemblies

**If globalization is needed:**
1. Set `InvariantGlobalization=false`
2. Add specific cultures to the project
3. Accept the size increase

**For debugging:**
1. Use Debug configuration for development
2. Only apply aggressive optimizations for production builds

## Monitoring

Track payload size over time:
1. Measure `_framework` directory size after publish
2. Use browser dev tools to monitor network transfer
3. Set up CI/CD size budgets to prevent regression 