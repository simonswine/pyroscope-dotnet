﻿// <copyright file="TestClassRunnerStruct.cs" company="Datadog">
// Unless explicitly stated otherwise all files in this repository are licensed under the Apache 2 License.
// This product includes software developed at Datadog (https://www.datadoghq.com/). Copyright 2017 Datadog, Inc.
// </copyright>
#nullable enable

using Datadog.Trace.DuckTyping;

namespace Datadog.Trace.ClrProfiler.AutoInstrumentation.Testing.XUnit;

/// <summary>
/// TestClassRunner`1 structure
/// </summary>
[DuckCopy]
internal struct TestClassRunnerStruct
{
    /// <summary>
    /// Test class
    /// </summary>
    public TestClassStruct TestClass;
}
