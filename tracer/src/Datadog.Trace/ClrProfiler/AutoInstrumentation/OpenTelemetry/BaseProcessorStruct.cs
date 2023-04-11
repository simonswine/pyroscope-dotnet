// <copyright file="BaseProcessorStruct.cs" company="Datadog">
// Unless explicitly stated otherwise all files in this repository are licensed under the Apache 2 License.
// This product includes software developed at Datadog (https://www.datadoghq.com/). Copyright 2017 Datadog, Inc.
// </copyright>

#nullable enable

using Datadog.Trace.DuckTyping;

namespace Datadog.Trace.ClrProfiler.AutoInstrumentation.OpenTelemetry
{
    /// <summary>
    /// Ducktype for type OpenTelemetry.BaseProcessor`1
    /// </summary>
    [DuckCopy]
    internal struct BaseProcessorStruct
    {
        public object? ParentProvider;
    }
}
