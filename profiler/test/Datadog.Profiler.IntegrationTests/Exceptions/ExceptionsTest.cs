// <copyright file="ExceptionsTest.cs" company="Datadog">
// Unless explicitly stated otherwise all files in this repository are licensed under the Apache 2 License.
// This product includes software developed at Datadog (https://www.datadoghq.com/). Copyright 2022 Datadog, Inc.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Datadog.Profiler.IntegrationTests.Helpers;
using Datadog.Profiler.SmokeTests;
using FluentAssertions;
using Perftools.Profiles;
using Xunit;
using Xunit.Abstractions;

namespace Datadog.Profiler.IntegrationTests.Exceptions
{
    public class ExceptionsTest
    {
        private const string Scenario1 = "--scenario 1";
        private const string Scenario2 = "--scenario 2";
        private const string Scenario3 = "--scenario 3";
        private const string ScenarioMeasureException = "--scenario 6";

        private readonly ITestOutputHelper _output;

        public ExceptionsTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [TestAppFact("Samples.ExceptionGenerator")]
        public void ThrowExceptionsInParallel(string appName, string framework, string appAssembly)
        {
            StackTrace expectedStack;

            if (framework == "net45")
            {
                expectedStack = new StackTrace(
                    new StackFrame("|lm:Samples.ExceptionGenerator |ns:Samples.ExceptionGenerator |ct:ParallelExceptionsScenario |fn:ThrowExceptions"),
                    new StackFrame("|lm:mscorlib |ns:System.Threading |ct:ThreadHelper |fn:ThreadStart"));
            }
            else if (framework == "net6.0")
            {
                expectedStack = new StackTrace(
                    new StackFrame("|lm:Samples.ExceptionGenerator |ns:Samples.ExceptionGenerator |ct:ParallelExceptionsScenario |fn:ThrowExceptions"),
                    new StackFrame("|lm:System.Private.CoreLib |ns:System.Threading |ct:Thread |fn:StartCallback"));
            }
            else if (framework == "net7.0")
            {
                expectedStack = new StackTrace(
                    new StackFrame("|lm:Samples.ExceptionGenerator |ns:Samples.ExceptionGenerator |ct:ParallelExceptionsScenario |fn:ThrowExceptions"));
            }
            else
            {
                expectedStack = new StackTrace(
                    new StackFrame("|lm:Samples.ExceptionGenerator |ns:Samples.ExceptionGenerator |ct:ParallelExceptionsScenario |fn:ThrowExceptions"),
                    new StackFrame("|lm:System.Private.CoreLib |ns:System.Threading |ct:ThreadHelper |fn:ThreadStart"));
            }

            var runner = new TestApplicationRunner(appName, framework, appAssembly, _output, commandLine: Scenario2);
            runner.Environment.SetVariable(EnvironmentVariables.WallTimeProfilerEnabled, "0");
            runner.Environment.SetVariable(EnvironmentVariables.CpuProfilerEnabled, "0");
            runner.Environment.SetVariable(EnvironmentVariables.ExceptionProfilerEnabled, "1");
            runner.Environment.SetVariable(EnvironmentVariables.ExceptionSampleLimit, "10000");

            using var agent = MockDatadogAgent.CreateHttpAgent(_output);

            runner.Run(agent);

            Assert.True(agent.NbCallsOnProfilingEndpoint > 0);

            var exceptionSamples = ExtractExceptionSamples(runner.Environment.PprofDir).ToArray();

            long total = 0;

            foreach (var sample in exceptionSamples)
            {
                total += sample.Count;
                sample.Type.Should().Be("System.Exception");
                sample.Message.Should().BeEmpty();
                Assert.True(sample.Stacktrace.EndWith(expectedStack));
            }

            foreach (var file in Directory.GetFiles(runner.Environment.LogDir))
            {
                _output.WriteLine($"Log file: {file}");
            }

            var logFile = Directory.GetFiles(runner.Environment.LogDir)
                .Single(f => Path.GetFileName(f).StartsWith("DD-DotNet-Profiler-Native-"));

            // Stackwalk will fail if the walltime profiler tries to inspect the thread at the same time as the exception profiler
            // This is expected so we remove those from the expected count
            var missedExceptions = File.ReadLines(logFile)
                .Count(l => l.Contains("Failed to walk stack for thrown exception: CORPROF_E_STACKSNAPSHOT_UNSAFE (80131360)"));

            int expectedExceptionCount = (4 * 1000) - missedExceptions;

            expectedExceptionCount.Should().BeGreaterThan(0, "only a few exceptions should be missed");

            total.Should().Be(expectedExceptionCount);
        }

        [TestAppFact("Samples.ExceptionGenerator")]
        public void Sampling(string appName, string framework, string appAssembly)
        {
            var runner = new TestApplicationRunner(appName, framework, appAssembly, _output, commandLine: Scenario3);
            runner.Environment.SetVariable(EnvironmentVariables.WallTimeProfilerEnabled, "0");
            runner.Environment.SetVariable(EnvironmentVariables.CpuProfilerEnabled, "0");
            runner.Environment.SetVariable(EnvironmentVariables.ExceptionProfilerEnabled, "1");
            runner.Environment.SetVariable(EnvironmentVariables.ExceptionSampleLimit, "100");

            using var agent = MockDatadogAgent.CreateHttpAgent(_output);
            runner.Run(agent);

            agent.NbCallsOnProfilingEndpoint.Should().BeGreaterThan(0);

            var exceptionSamples = ExtractExceptionSamples(runner.Environment.PprofDir).ToArray();

            var exceptionCounts = exceptionSamples.GroupBy(s => s.Type)
                .ToDictionary(g => g.Key, g => g.Sum(s => s.Count));

            // Check that fewer than 500 System.Exception were seen
            // The limit was set to 100, but the sampling algorithm seems to keep more exceptions,
            // so we just check that we are in the right order of magnitude.
            exceptionCounts.Should().ContainKey("System.Exception").WhichValue.Should().BeLessThan(500);

            // System.InvalidOperationException is seen only once, so it should be sampled
            // despite the sampler being saturated by the 4000 System.Exception
            exceptionCounts.Should().ContainKey("System.InvalidOperationException").WhichValue.Should().Be(1);
        }

        [TestAppFact("Samples.ExceptionGenerator")]
        public void GetExceptionSamples(string appName, string framework, string appAssembly)
        {
            var runner = new TestApplicationRunner(appName, framework, appAssembly, _output, commandLine: Scenario1);
            runner.Environment.SetVariable(EnvironmentVariables.WallTimeProfilerEnabled, "0");
            runner.Environment.SetVariable(EnvironmentVariables.CpuProfilerEnabled, "0");
            runner.Environment.SetVariable(EnvironmentVariables.ExceptionProfilerEnabled, "1");

            CheckExceptionProfiles(runner);
        }

        [TestAppFact("Samples.ExceptionGenerator")]
        public void DisableExceptionProfiler(string appName, string framework, string appAssembly)
        {
            var runner = new TestApplicationRunner(appName, framework, appAssembly, _output, commandLine: Scenario1);

            // Test that the exception profiler is disabled by default
            runner.Environment.SetVariable(EnvironmentVariables.WallTimeProfilerEnabled, "1");
            runner.Environment.SetVariable(EnvironmentVariables.CpuProfilerEnabled, "0");

            using var agent = MockDatadogAgent.CreateHttpAgent(_output);

            runner.Run(agent);

            // On alpine, this check is flaky.
            // Disable it on alpine for now
            if (!EnvironmentHelper.IsAlpine)
            {
                Assert.True(agent.NbCallsOnProfilingEndpoint > 0);
            }

            // only walltime profiler enabled so should see 1 value per sample
            SamplesHelper.CheckSamplesValueCount(runner.Environment.PprofDir, 1);
        }

        [TestAppFact("Samples.ExceptionGenerator")]
        public void ExplicitlyDisableExceptionProfiler(string appName, string framework, string appAssembly)
        {
            var runner = new TestApplicationRunner(appName, framework, appAssembly, _output, commandLine: Scenario1);

            runner.Environment.SetVariable(EnvironmentVariables.WallTimeProfilerEnabled, "1");
            runner.Environment.SetVariable(EnvironmentVariables.CpuProfilerEnabled, "0");
            runner.Environment.SetVariable(EnvironmentVariables.ExceptionProfilerEnabled, "0");

            using var agent = MockDatadogAgent.CreateHttpAgent(_output);

            runner.Run(agent);

            // On alpine, this check is flaky.
            // Disable it on alpine for now
            if (!EnvironmentHelper.IsAlpine)
            {
                Assert.True(agent.NbCallsOnProfilingEndpoint > 0);
            }

            // only walltime profiler enabled so should see 1 value per sample
            SamplesHelper.CheckSamplesValueCount(runner.Environment.PprofDir, 1);
        }

        [TestAppFact("Samples.ExceptionGenerator")]
        public void MeasureExceptions(string appName, string framework, string appAssembly)
        {
            var runner = new TestApplicationRunner(appName, framework, appAssembly, _output, commandLine: ScenarioMeasureException);

            runner.Environment.SetVariable(EnvironmentVariables.WallTimeProfilerEnabled, "0");
            runner.Environment.SetVariable(EnvironmentVariables.CpuProfilerEnabled, "0");
            runner.Environment.SetVariable(EnvironmentVariables.ExceptionProfilerEnabled, "1");

            using var agent = MockDatadogAgent.CreateHttpAgent(_output);

            runner.Run(agent);

            Assert.True(agent.NbCallsOnProfilingEndpoint > 0);

            var exceptionSamples = ExtractExceptionSamples(runner.Environment.PprofDir).ToArray();
            exceptionSamples.Should().NotBeEmpty();

            // this test always succeeds: it is used to display the differences between sampled and real exceptions
            Dictionary<string, int> profiledExceptions = GetProfiledExceptions(exceptionSamples);
            Dictionary<string, int> realExceptions = GetRealExceptions(runner.ProcessOutput);

            _output.WriteLine("Comparing exceptions");
            _output.WriteLine("-------------------------------------------------------");
            _output.WriteLine("      Count          Type");
            _output.WriteLine("-------------------------------------------------------");
            foreach (var exception in profiledExceptions)
            {
                var exceptionCount = exception.Value;
                var type = exception.Key;
                int pos = type.LastIndexOf('.');
                if (pos != -1)
                {
                    type = type.Substring(pos + 1);
                }

                // TODO: dump real and profiled count
                if (!realExceptions.TryGetValue(type, out var stats))
                {
                    continue;
                }

                StringBuilder builder = new StringBuilder();
                builder.AppendLine($"{exceptionCount,11} {type}");
                builder.AppendLine($"{stats,11}");
                _output.WriteLine(builder.ToString());
            }
        }

        private static Dictionary<string, int> GetRealExceptions(string output)
        {
            const string startToken = "Exceptions start";
            const string endToken = "Exceptions end";

            var realExceptions = new Dictionary<string, int>();
            if (output == null)
            {
                return realExceptions;
            }

            // look for the following sections with type=count,size
            /*
                Exceptions start
                ArgumentException=3879
                SystemException=4000
                InvalidOperationException=4023
                InvalidCastException=4053
                TimeoutException=4045
                Exceptions end
            */
            int pos = 0;
            int next = 0;
            int end = 0;
            while (true)
            {
                // look for an exceptions block
                next = output.IndexOf(startToken, pos);
                if (next == -1)
                {
                    break;
                }

                next += startToken.Length;
                next = GotoEoL(output, next);
                if (next == -1)
                {
                    break;
                }

                pos = next + 1; // point to the beginning of the first exception stats

                // look for the end of the exceptions block
                end = output.IndexOf(endToken, pos);
                if (end == -1)
                {
                    break;
                }

                // extract line by line the exception stats from this block
                int eol = 0;
                while (true)
                {
                    next = GotoEoL(output, pos);
                    if (next == -1)
                    {
                        break;
                    }

                    // handle Windows (\r\n) and Linux (\n) cases
                    if (output[next - 1] == '\r')
                    {
                        eol = next - 1;
                    }
                    else
                    {
                        eol = next;
                    }

                    // extract type and count
                    //   ArgumentException=3879
                    var line = output.AsSpan(pos, eol - pos);

                    // get type name
                    var current = output.IndexOf('=', pos);
                    if (current == -1)
                    {
                        next = -1;
                        break;
                    }

                    var name = output.Substring(pos, current - pos);
                    pos = current + 1;
                    if (pos >= end)
                    {
                        next = -1;
                        break;
                    }

                    // get count
                    var text = output.AsSpan(pos, eol - pos);
                    var count = int.Parse(text);

                    // add the stats
                    if (!realExceptions.TryGetValue(name, out var stats))
                    {
                        realExceptions.Add(name, 0);
                    }

                    stats += count;
                    realExceptions[name] = stats;

                    // goto the next line (or the end token)
                    pos = next + 1;

                    // check for the last stats
                    if (next == (end - 1))
                    {
                        break;
                    }
                }

                if (next == -1)
                {
                    break;
                }
            }

            return realExceptions;
        }

        private static int GotoEoL(string text, int pos)
        {
            var next = text.IndexOf('\n', pos);
            return next;
        }

        private static Dictionary<string, int> GetProfiledExceptions(IEnumerable<(string Type, string Message, long Count, StackTrace Stacktrace)> exceptions)
        {
            var profiledExceptions = new Dictionary<string, int>();

            foreach (var exception in exceptions)
            {
                if (!profiledExceptions.TryGetValue(exception.Type, out var stats))
                {
                    stats = 0;
                    profiledExceptions.Add(exception.Type, 0);
                }

                stats += (int)exception.Count;
                profiledExceptions[exception.Type] = stats;
            }

            return profiledExceptions;
        }

        private static IEnumerable<(string Type, string Message, long Count, StackTrace Stacktrace)> ExtractExceptionSamples(string directory)
        {
            static IEnumerable<(string Type, string Message, long Count, StackTrace Stacktrace, long Time)> SamplesWithTimestamp(string directory)
            {
                foreach (var file in Directory.EnumerateFiles(directory, "*.pprof", SearchOption.AllDirectories))
                {
                    using var stream = File.OpenRead(file);

                    var profile = Profile.Parser.ParseFrom(stream);

                    foreach (var sample in profile.Sample)
                    {
                        var count = sample.Value[0];

                        if (count == 0)
                        {
                            continue;
                        }

                        var labels = sample.Labels(profile).ToArray();

                        var type = labels.Single(l => l.Name == "exception type").Value;
                        var message = labels.Single(l => l.Name == "exception message").Value;

                        yield return (type, message, count, sample.StackTrace(profile), profile.TimeNanos);
                    }
                }
            }

            return SamplesWithTimestamp(directory)
                .OrderBy(s => s.Time)
                .Select(s => (s.Type, s.Message, s.Count, s.Stacktrace));
        }

        private void CheckExceptionProfiles(TestApplicationRunner runner)
        {
            var stack1 = new StackTrace(
                new StackFrame("|lm:Samples.ExceptionGenerator |ns:Samples.ExceptionGenerator |ct:ExceptionsProfilerTestScenario |fn:Throw1_2"),
                new StackFrame("|lm:Samples.ExceptionGenerator |ns:Samples.ExceptionGenerator |ct:ExceptionsProfilerTestScenario |fn:Throw1_1"),
                new StackFrame("|lm:Samples.ExceptionGenerator |ns:Samples.ExceptionGenerator |ct:ExceptionsProfilerTestScenario |fn:Throw1"),
                new StackFrame("|lm:Samples.ExceptionGenerator |ns:Samples.ExceptionGenerator |ct:ExceptionsProfilerTestScenario |fn:Run"),
                new StackFrame("|lm:Samples.ExceptionGenerator |ns:Samples.ExceptionGenerator |ct:Program |fn:Main"));

            var stack2 = new StackTrace(
                new StackFrame("|lm:Samples.ExceptionGenerator |ns:Samples.ExceptionGenerator |ct:ExceptionsProfilerTestScenario |fn:Throw2_3"),
                new StackFrame("|lm:Samples.ExceptionGenerator |ns:Samples.ExceptionGenerator |ct:ExceptionsProfilerTestScenario |fn:Throw2_2"),
                new StackFrame("|lm:Samples.ExceptionGenerator |ns:Samples.ExceptionGenerator |ct:ExceptionsProfilerTestScenario |fn:Throw2_1"),
                new StackFrame("|lm:Samples.ExceptionGenerator |ns:Samples.ExceptionGenerator |ct:ExceptionsProfilerTestScenario |fn:Throw2"),
                new StackFrame("|lm:Samples.ExceptionGenerator |ns:Samples.ExceptionGenerator |ct:ExceptionsProfilerTestScenario |fn:Run"),
                new StackFrame("|lm:Samples.ExceptionGenerator |ns:Samples.ExceptionGenerator |ct:Program |fn:Main"));

            using var agent = MockDatadogAgent.CreateHttpAgent(_output);

            runner.Run(agent);

            Assert.True(agent.NbCallsOnProfilingEndpoint > 0);

            var exceptionSamples = ExtractExceptionSamples(runner.Environment.PprofDir).ToArray();

            exceptionSamples.Should().HaveCount(6);

            exceptionSamples[0].Should().Be(("System.InvalidOperationException", "IOE", 2, stack1));
            exceptionSamples[1].Should().Be(("System.NotSupportedException", "NSE", 2, stack1));
            exceptionSamples[2].Should().Be(("System.NotImplementedException", "NIE", 1, stack1));
            exceptionSamples[3].Should().Be(("System.NotImplementedException", "NIE", 1, stack2));
            exceptionSamples[4].Should().Be(("System.Exception", "E1", 1, stack1));
            exceptionSamples[5].Should().Be(("System.Exception", "E2", 1, stack1));
        }
    }
}
