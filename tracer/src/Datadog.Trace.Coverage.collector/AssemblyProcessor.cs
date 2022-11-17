// <copyright file="AssemblyProcessor.cs" company="Datadog">
// Unless explicitly stated otherwise all files in this repository are licensed under the Apache 2 License.
// This product includes software developed at Datadog (https://www.datadoghq.com/). Copyright 2017 Datadog, Inc.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Datadog.Trace.Ci.Configuration;
using Datadog.Trace.Ci.Coverage;
using Datadog.Trace.Ci.Coverage.Attributes;
using Datadog.Trace.Ci.Coverage.Metadata;
using Mono.Cecil;
using Mono.Cecil.Cil;
using CallSite = Mono.Cecil.CallSite;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace Datadog.Trace.Coverage.Collector
{
    internal class AssemblyProcessor
    {
        private static readonly object PadLock = new();
        private static readonly CultureInfo UsCultureInfo = new("us-US");
        private static readonly Regex NetCorePattern = new(@".NETCoreApp,Version=v(\d.\d)", RegexOptions.Compiled);
        private static readonly MethodInfo ArrayEmptyOfIntMethod = typeof(Array).GetMethod("Empty")!.MakeGenericMethod(typeof(int));
        private static readonly Assembly TracerAssembly = typeof(CoverageReporter).Assembly;
        private static readonly string[] IgnoredAssemblies =
        {
            "NUnit3.TestAdapter.dll",
            "xunit.abstractions.dll",
            "xunit.assert.dll",
            "xunit.core.dll",
            "xunit.execution.dotnet.dll",
            "xunit.runner.reporters.netcoreapp10.dll",
            "xunit.runner.utility.netcoreapp10.dll",
            "xunit.runner.visualstudio.dotnetcore.testadapter.dll",
            "Xunit.SkippableFact.dll",
        };

        private readonly CIVisibilitySettings? _ciVisibilitySettings;
        private readonly ICollectorLogger _logger;
        private readonly string _tracerHome;
        private readonly string _assemblyFilePath;

        private byte[]? _strongNameKeyBlob;

        public AssemblyProcessor(string filePath, string tracerHome, ICollectorLogger? logger = null, CIVisibilitySettings? ciVisibilitySettings = null)
        {
            _tracerHome = tracerHome;
            _logger = logger ?? new ConsoleCollectorLogger();
            _ciVisibilitySettings = ciVisibilitySettings;
            _assemblyFilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(_assemblyFilePath))
            {
                throw new FileNotFoundException($"Assembly not found in path: {_assemblyFilePath}");
            }

            if (!File.Exists(Path.ChangeExtension(filePath, ".pdb")))
            {
                Ci.Coverage.Exceptions.PdbNotFoundException.Throw();
            }
        }

        public string FilePath => _assemblyFilePath;

        public void Process()
        {
            try
            {
                _logger.Debug($"Processing: {_assemblyFilePath}");

                // Check if the assembly is in the ignored assemblies list.
                var assemblyFullName = Path.GetFileName(_assemblyFilePath);
                if (Array.Exists(IgnoredAssemblies, i => assemblyFullName == i))
                {
                    return;
                }

                // Open the assembly
                var customResolver = new CustomResolver(_logger, _assemblyFilePath);
                customResolver.AddSearchDirectory(Path.GetDirectoryName(_assemblyFilePath));
                using var assemblyDefinition = AssemblyDefinition.ReadAssembly(_assemblyFilePath, new ReaderParameters
                {
                    ReadSymbols = true,
                    ReadWrite = true,
                    AssemblyResolver = customResolver,
                });

                var avoidCoverageAttributeFullName = typeof(AvoidCoverageAttribute).FullName;
                var coveredAssemblyAttributeFullName = typeof(CoveredAssemblyAttribute).FullName;
                var internalsVisibleToAttributeFullName = typeof(InternalsVisibleToAttribute).FullName;
                var hasInternalsVisibleAttribute = false;
                foreach (var cAttr in assemblyDefinition.CustomAttributes)
                {
                    var attrFullName = cAttr.Constructor.DeclaringType.FullName;
                    if (attrFullName == avoidCoverageAttributeFullName)
                    {
                        _logger.Debug($"Assembly: {FilePath}, ignored.");
                        return;
                    }

                    if (attrFullName == coveredAssemblyAttributeFullName)
                    {
                        _logger.Debug($"Assembly: {FilePath}, already have coverage information.");
                        return;
                    }

                    hasInternalsVisibleAttribute |= attrFullName == internalsVisibleToAttributeFullName;
                }

                // Gets the Datadog.Trace target framework
                var tracerTarget = GetTracerTarget(assemblyDefinition);

                if (assemblyDefinition.Name.HasPublicKey)
                {
                    _logger.Debug($"Assembly: {FilePath} is signed.");

                    var snkFilePath = _ciVisibilitySettings?.CodeCoverageSnkFilePath;
                    _logger.Debug($"Assembly: {FilePath} loading .snk file: {snkFilePath}.");
                    if (!string.IsNullOrWhiteSpace(snkFilePath) && File.Exists(snkFilePath))
                    {
                        _logger.Debug($"{snkFilePath} exists.");
                        _strongNameKeyBlob = File.ReadAllBytes(snkFilePath);
                        _logger.Debug($"{snkFilePath} loaded.");
                    }
                    else if (tracerTarget == TracerTarget.Net461)
                    {
                        _logger.Warning($"Assembly: {FilePath}, is a net461 signed assembly, a .snk file is required ({Configuration.ConfigurationKeys.CIVisibility.CodeCoverageSnkFile} environment variable).");
                        return;
                    }
                    else if (hasInternalsVisibleAttribute)
                    {
                        _logger.Warning($"Assembly: {FilePath}, is a signed assembly with the InternalsVisibleTo attribute. A .snk file is required ({Configuration.ConfigurationKeys.CIVisibility.CodeCoverageSnkFile} environment variable).");
                        return;
                    }
                }

                // We open the exact datadog assembly to be copied to the target, this is because the AssemblyReference lists
                // differs depends on the target runtime. (netstandard, .NET 5.0 or .NET 4.6.2)
                using var datadogTracerAssembly = AssemblyDefinition.ReadAssembly(GetDatadogTracer(tracerTarget));

                var isDirty = false;

                // Process all modules in the assembly
                var module = assemblyDefinition.MainModule;
                _logger.Debug($"Processing module: {module.Name}");

                // Process all types defined in the module
                var moduleTypes = module.Types;

                var moduleCoverageMetadataTypeDefinition = datadogTracerAssembly.MainModule.GetType(typeof(ModuleCoverageMetadata).FullName);
                var moduleCoverageMetadataTypeReference = module.ImportReference(moduleCoverageMetadataTypeDefinition);

                var moduleCoverageMetadataImplTypeDef = new TypeDefinition(
                    typeof(ModuleCoverageMetadata).Namespace + ".Target",
                    "ModuleCoverage",
                    TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.NotPublic,
                    moduleCoverageMetadataTypeReference);

                var moduleCoverageMetadataImplCtor = new MethodDefinition(
                    ".ctor",
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName,
                    module.TypeSystem.Void);

                // Create number of types array
                var moduleCoverageMetadataImplMetadataField = new FieldReference("Metadata", new ArrayType(new ArrayType(module.TypeSystem.Int32)), moduleCoverageMetadataTypeReference);
                moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, moduleTypes.Count));
                moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Newarr, new ArrayType(module.TypeSystem.Int32)));
                moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, moduleCoverageMetadataImplMetadataField));

                moduleCoverageMetadataImplTypeDef.Methods.Add(moduleCoverageMetadataImplCtor);

                var coverageReporterTypeDefinition = datadogTracerAssembly.MainModule.GetType(typeof(CoverageReporter<>).FullName);
                var coverageReporterTypeReference = module.ImportReference(coverageReporterTypeDefinition);
                var reportTypeGenericInstance = new GenericInstanceType(coverageReporterTypeReference);
                reportTypeGenericInstance.GenericArguments.Add(moduleCoverageMetadataImplTypeDef);

                var reportTryGetScopeMethod = new MethodReference("TryGetScope", module.TypeSystem.Boolean, reportTypeGenericInstance);
                reportTryGetScopeMethod.HasThis = false;
                reportTryGetScopeMethod.Parameters.Add(new ParameterDefinition(module.TypeSystem.Int32) { Name = "typeIndex" });
                reportTryGetScopeMethod.Parameters.Add(new ParameterDefinition(module.TypeSystem.Int32) { Name = "methodIndex" });
                reportTryGetScopeMethod.Parameters.Add(new ParameterDefinition(new ByReferenceType(new ArrayType(module.TypeSystem.Int32))) { Name = "scope", IsOut = true });

                GenericInstanceMethod? arrayEmptyOfIntMethodReference = null;
                for (var typeIndex = 0; typeIndex < moduleTypes.Count; typeIndex++)
                {
                    var moduleType = moduleTypes[typeIndex];

                    _logger.Debug($"\t{moduleType.FullName}");
                    var moduleTypeMethods = moduleType.Methods;

                    // Create Type number of methods array
                    moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                    moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, moduleCoverageMetadataImplMetadataField));
                    moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, typeIndex));
                    if (moduleTypeMethods.Count > 0)
                    {
                        moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, moduleTypeMethods.Count));
                        moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Newarr,  module.TypeSystem.Int32));
                    }
                    else
                    {
                        // Emit Array.Empty<int>() call
                        if (arrayEmptyOfIntMethodReference is null)
                        {
                            var assemblyReferences = module.AssemblyReferences.ToArray();
                            arrayEmptyOfIntMethodReference = (GenericInstanceMethod)module.ImportReference(ArrayEmptyOfIntMethod);
                            arrayEmptyOfIntMethodReference.DeclaringType.Scope = module.TypeSystem.CoreLibrary;
                            arrayEmptyOfIntMethodReference.GenericArguments[0] = module.TypeSystem.Int32;

                            // If the `ImportReference` sentence add a new assembly reference (cross runtime versions)
                            // we revert the reference list at the previous state.
                            if (assemblyReferences.Length != module.AssemblyReferences.Count)
                            {
                                module.AssemblyReferences.Clear();
                                foreach (var assemblyReference in assemblyReferences)
                                {
                                    module.AssemblyReferences.Add(assemblyReference);
                                }
                            }
                        }

                        moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Call, arrayEmptyOfIntMethodReference));
                    }

                    moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Stelem_Ref));

                    // Process all Methods in the type
                    for (var methodIndex = 0; methodIndex < moduleTypeMethods.Count; methodIndex++)
                    {
                        var moduleTypeMethod = moduleTypeMethods[methodIndex];
                        if (moduleTypeMethod.DebugInformation is null || !moduleTypeMethod.DebugInformation.HasSequencePoints)
                        {
                            _logger.Debug($"\t\t[NO] {moduleTypeMethod.FullName}");
                            continue;
                        }

                        _logger.Debug($"\t\t[YES] {moduleTypeMethod.FullName}.");

                        // Extract body from the method
                        if (moduleTypeMethod.HasBody)
                        {
                            /*** This block rewrites the method body code that looks like this:
                             *
                             *  public static class MyMathClass
                             *  {
                             *      public static int Factorial(int value)
                             *      {
                             *          if (value == 1)
                             *          {
                             *              return 1;
                             *          }
                             *          return value * Factorial(value - 1);
                             *      }
                             *  }
                             *
                             *** To this:
                             *
                             *  using Datadog.Trace.Ci.Coverage;
                             *
                             *  public static int Factorial(int value)
                             *  {
                             *      if (!CoverageReporter<ModuleCoverage>.TryGetScope(1, 1, out var counters))
                             *      {
                             *          if (value == 1)
                             *          {
                             *              return 1;
                             *          }
                             *          return value * Factorial(value - 1);
                             *      }
                             *      counters[0]++;
                             *      counters[1]++;
                             *      int result;
                             *      if (value == 1)
                             *      {
                             *          counters[2]++;
                             *          counters[3]++;
                             *          result = 1;
                             *      }
                             *      else
                             *      {
                             *          counters[4]++;
                             *          result = value * Factorial(value - 1);
                             *      }
                             *      counters[5]++;
                             *      return result;
                             *  }
                             */

                            var methodBody = moduleTypeMethod.Body;
                            var instructions = methodBody.Instructions;
                            var instructionsOriginalLength = instructions.Count;
                            if (instructions.Capacity < instructionsOriginalLength * 2)
                            {
                                instructions.Capacity = instructionsOriginalLength * 2;
                            }

                            var sequencePoints = moduleTypeMethod.DebugInformation.SequencePoints;
                            var sequencePointsOriginalLength = sequencePoints.Count;

                            // Step 1 - Clone instructions
                            for (var i = 0; i < instructionsOriginalLength; i++)
                            {
                                instructions.Add(CloneInstruction(instructions[i]));
                            }

                            // Step 2 - Fix jumps in cloned instructions
                            for (var i = 0; i < instructionsOriginalLength; i++)
                            {
                                var currentInstruction = instructions[i];

                                if (currentInstruction.Operand is Instruction jmpTargetInstruction)
                                {
                                    // Normal jump

                                    // Get index of the jump target
                                    var jmpTargetInstructionIndex = instructions.IndexOf(jmpTargetInstruction);

                                    // Modify the clone instruction with the cloned jump target
                                    var clonedInstruction = instructions[i + instructionsOriginalLength];
                                    RemoveShortOpCodes(clonedInstruction);
                                    clonedInstruction.Operand = instructions[jmpTargetInstructionIndex + instructionsOriginalLength];
                                }
                                else if (currentInstruction.Operand is Instruction[] jmpTargetInstructions)
                                {
                                    // Switch jumps

                                    // Create a new array of instructions with the cloned jump targets
                                    var newJmpTargetInstructions = new Instruction[jmpTargetInstructions.Length];
                                    for (var j = 0; j < jmpTargetInstructions.Length; j++)
                                    {
                                        newJmpTargetInstructions[j] = instructions[instructions.IndexOf(jmpTargetInstructions[j]) + instructionsOriginalLength];
                                    }

                                    // Modify the clone instruction with the cloned jump target
                                    var clonedInstruction = instructions[i + instructionsOriginalLength];
                                    RemoveShortOpCodes(clonedInstruction);
                                    clonedInstruction.Operand = newJmpTargetInstructions;
                                }
                            }

                            // Step 3 - Clone exception handlers
                            if (methodBody.HasExceptionHandlers)
                            {
                                var exceptionHandlers = methodBody.ExceptionHandlers;
                                var exceptionHandlersOrignalLength = exceptionHandlers.Count;

                                for (var i = 0; i < exceptionHandlersOrignalLength; i++)
                                {
                                    var currentExceptionHandler = exceptionHandlers[i];
                                    var clonedExceptionHandler = new ExceptionHandler(currentExceptionHandler.HandlerType);
                                    clonedExceptionHandler.CatchType = currentExceptionHandler.CatchType;

                                    if (currentExceptionHandler.TryStart is not null)
                                    {
                                        clonedExceptionHandler.TryStart = instructions[instructions.IndexOf(currentExceptionHandler.TryStart) + instructionsOriginalLength];
                                    }

                                    if (currentExceptionHandler.TryEnd is not null)
                                    {
                                        clonedExceptionHandler.TryEnd = instructions[instructions.IndexOf(currentExceptionHandler.TryEnd) + instructionsOriginalLength];
                                    }

                                    if (currentExceptionHandler.HandlerStart is not null)
                                    {
                                        clonedExceptionHandler.HandlerStart = instructions[instructions.IndexOf(currentExceptionHandler.HandlerStart) + instructionsOriginalLength];
                                    }

                                    if (currentExceptionHandler.HandlerEnd is not null)
                                    {
                                        clonedExceptionHandler.HandlerEnd = instructions[instructions.IndexOf(currentExceptionHandler.HandlerEnd) + instructionsOriginalLength];
                                    }

                                    if (currentExceptionHandler.FilterStart is not null)
                                    {
                                        clonedExceptionHandler.FilterStart = instructions[instructions.IndexOf(currentExceptionHandler.FilterStart) + instructionsOriginalLength];
                                    }

                                    methodBody.ExceptionHandlers.Add(clonedExceptionHandler);
                                }
                            }

                            // Step 4 - Clone sequence points
                            var clonedInstructionsWithSequencePoints = new List<Instruction>();
                            for (var i = 0; i < sequencePointsOriginalLength; i++)
                            {
                                var currentSequencePoint = sequencePoints[i];
                                var currentInstruction = instructions.First(i => i.Offset == currentSequencePoint.Offset);
                                var clonedInstruction = instructions[instructions.IndexOf(currentInstruction) + instructionsOriginalLength];

                                if (!currentSequencePoint.IsHidden)
                                {
                                    clonedInstructionsWithSequencePoints.Add(clonedInstruction);
                                }

                                var clonedSequencePoint = new SequencePoint(clonedInstruction, currentSequencePoint.Document);
                                clonedSequencePoint.StartLine = currentSequencePoint.StartLine;
                                clonedSequencePoint.StartColumn = currentSequencePoint.StartColumn;
                                clonedSequencePoint.EndLine = currentSequencePoint.EndLine;
                                clonedSequencePoint.EndColumn = currentSequencePoint.EndColumn;
                                sequencePoints.Add(clonedSequencePoint);
                            }

                            // Step 6 - Modify local var to add the Coverage counters instance.
                            var countersVariable = new VariableDefinition(new ArrayType(module.TypeSystem.Int32));
                            methodBody.Variables.Add(countersVariable);

                            // Create methods sequence points array
                            moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                            moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, moduleCoverageMetadataImplMetadataField));
                            moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, typeIndex));
                            moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldelem_Ref));
                            moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, methodIndex));
                            moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, clonedInstructionsWithSequencePoints.Count));
                            moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Stelem_I4));

                            instructions.Insert(0, Instruction.Create(OpCodes.Ldc_I4, typeIndex));
                            instructions.Insert(1, Instruction.Create(OpCodes.Ldc_I4, methodIndex));
                            instructions.Insert(2, Instruction.Create(OpCodes.Ldloca, countersVariable));
                            instructions.Insert(3, Instruction.Create(OpCodes.Call, reportTryGetScopeMethod));
                            instructions.Insert(4, Instruction.Create(OpCodes.Brtrue, instructions[instructionsOriginalLength + 4]));

                            // Step 7 - Insert line reporter
                            for (var i = 0; i < clonedInstructionsWithSequencePoints.Count; i++)
                            {
                                var currentInstruction = clonedInstructionsWithSequencePoints[i];
                                var currentInstructionIndex = instructions.IndexOf(currentInstruction);
                                var currentInstructionClone = CloneInstruction(currentInstruction);

                                currentInstruction.OpCode = OpCodes.Ldloc;
                                currentInstruction.Operand = countersVariable;
                                instructions.Insert(currentInstructionIndex + 1, Instruction.Create(OpCodes.Ldc_I4, i));
                                instructions.Insert(currentInstructionIndex + 2, Instruction.Create(OpCodes.Ldelema, module.TypeSystem.Int32));
                                instructions.Insert(currentInstructionIndex + 3, Instruction.Create(OpCodes.Dup));
                                instructions.Insert(currentInstructionIndex + 4, Instruction.Create(OpCodes.Ldind_I4));
                                instructions.Insert(currentInstructionIndex + 5, Instruction.Create(OpCodes.Ldc_I4_1));
                                instructions.Insert(currentInstructionIndex + 6, Instruction.Create(OpCodes.Add));
                                instructions.Insert(currentInstructionIndex + 7, Instruction.Create(OpCodes.Stind_I4));
                                instructions.Insert(currentInstructionIndex + 8, currentInstructionClone);
                            }

                            isDirty = true;
                        }
                    }
                }

                moduleTypes.Add(moduleCoverageMetadataImplTypeDef);
                moduleCoverageMetadataImplCtor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

                // Change attributes to drop native bits
                if ((module.Attributes & ModuleAttributes.ILLibrary) == ModuleAttributes.ILLibrary)
                {
                    module.Architecture = TargetArchitecture.I386;
                    module.Attributes &= ~ModuleAttributes.ILLibrary;
                    module.Attributes |= ModuleAttributes.ILOnly;
                }

                // Save assembly if we modify it successfully
                if (isDirty)
                {
                    var coveredAssemblyAttributeTypeReference = module.ImportReference(datadogTracerAssembly.MainModule.GetType(typeof(CoveredAssemblyAttribute).FullName));
                    assemblyDefinition.CustomAttributes.Add(new CustomAttribute(new MethodReference(".ctor", module.TypeSystem.Void, coveredAssemblyAttributeTypeReference)
                    {
                        HasThis = true
                    }));

                    _logger.Debug($"Saving assembly: {_assemblyFilePath}");

                    // Create backup for dll and pdb and copy the Datadog.Trace assembly
                    var tracerAssemblyLocation = CopyRequiredAssemblies(assemblyDefinition, tracerTarget);
                    customResolver.SetTracerAssemblyLocation(tracerAssemblyLocation);

                    assemblyDefinition.Write(new WriterParameters
                    {
                        WriteSymbols = true,
                        StrongNameKeyBlob = _strongNameKeyBlob
                    });
                }

                _logger.Debug($"Done: {_assemblyFilePath} [Modified:{isDirty}]");
            }
            catch (SymbolsNotFoundException)
            {
                Ci.Coverage.Exceptions.PdbNotFoundException.Throw();
            }
            catch (SymbolsNotMatchingException)
            {
                Ci.Coverage.Exceptions.PdbNotFoundException.Throw();
            }
        }

        private static void RemoveShortOpCodes(Instruction instruction)
        {
            if (instruction.OpCode == OpCodes.Br_S) { instruction.OpCode = OpCodes.Br; }
            if (instruction.OpCode == OpCodes.Brfalse_S) { instruction.OpCode = OpCodes.Brfalse; }
            if (instruction.OpCode == OpCodes.Brtrue_S) { instruction.OpCode = OpCodes.Brtrue; }
            if (instruction.OpCode == OpCodes.Leave_S) { instruction.OpCode = OpCodes.Leave; }
            if (instruction.OpCode == OpCodes.Blt_S) { instruction.OpCode = OpCodes.Blt; }
            if (instruction.OpCode == OpCodes.Blt_Un_S) { instruction.OpCode = OpCodes.Blt_Un; }
            if (instruction.OpCode == OpCodes.Ble_S) { instruction.OpCode = OpCodes.Ble; }
            if (instruction.OpCode == OpCodes.Ble_Un_S) { instruction.OpCode = OpCodes.Ble_Un; }
            if (instruction.OpCode == OpCodes.Bgt_S) { instruction.OpCode = OpCodes.Bgt; }
            if (instruction.OpCode == OpCodes.Bgt_Un_S) { instruction.OpCode = OpCodes.Bgt_Un; }
            if (instruction.OpCode == OpCodes.Bge_S) { instruction.OpCode = OpCodes.Bge; }
            if (instruction.OpCode == OpCodes.Bge_Un_S) { instruction.OpCode = OpCodes.Bge_Un; }
            if (instruction.OpCode == OpCodes.Beq_S) { instruction.OpCode = OpCodes.Beq; }
            if (instruction.OpCode == OpCodes.Bne_Un_S) { instruction.OpCode = OpCodes.Bne_Un; }
        }

        private static Instruction CloneInstruction(Instruction instruction)
        {
            return instruction.Operand switch
            {
                null => Instruction.Create(instruction.OpCode),
                string strOp => Instruction.Create(instruction.OpCode, strOp),
                int intOp => Instruction.Create(instruction.OpCode, intOp),
                long lngOp => Instruction.Create(instruction.OpCode, lngOp),
                byte byteOp => Instruction.Create(instruction.OpCode, byteOp),
                sbyte sbyteOp => Instruction.Create(instruction.OpCode, sbyteOp),
                double dblOp => Instruction.Create(instruction.OpCode, dblOp),
                FieldReference fRefOp => Instruction.Create(instruction.OpCode, fRefOp),
                MethodReference mRefOp => Instruction.Create(instruction.OpCode, mRefOp),
                CallSite callOp => Instruction.Create(instruction.OpCode, callOp),
                Instruction instOp => Instruction.Create(instruction.OpCode, instOp),
                Instruction[] instsOp => Instruction.Create(instruction.OpCode, instsOp),
                VariableDefinition vDefOp => Instruction.Create(instruction.OpCode, vDefOp),
                ParameterDefinition pDefOp => Instruction.Create(instruction.OpCode, pDefOp),
                TypeReference tRefOp => Instruction.Create(instruction.OpCode, tRefOp),
                float sOp => Instruction.Create(instruction.OpCode, sOp),
                _ => throw new Exception($"Instruction: {instruction.OpCode} cannot be cloned.")
            };
        }

        private string GetDatadogTracer(TracerTarget tracerTarget)
        {
            // Get the Datadog.Trace path

            if (string.IsNullOrEmpty(_tracerHome))
            {
                // If tracer home is empty then we try to load the Datadog.Trace.dll in the current folder.
                return "Datadog.Trace.dll";
            }

            var targetFolder = "net461";
            switch (tracerTarget)
            {
                case TracerTarget.Net461:
                    targetFolder = "net461";
                    break;
                case TracerTarget.Netstandard20:
                    targetFolder = "netstandard2.0";
                    break;
                case TracerTarget.Netcoreapp31:
                    targetFolder = "netcoreapp3.1";
                    break;
                case TracerTarget.Net60:
                    targetFolder = "net6.0";
                    break;
            }

            return Path.Combine(_tracerHome, targetFolder, "Datadog.Trace.dll");
        }

        private string CopyRequiredAssemblies(AssemblyDefinition assemblyDefinition, TracerTarget tracerTarget)
        {
            try
            {
                // Get the Datadog.Trace path
                string targetFolder = "net461";
                switch (tracerTarget)
                {
                    case TracerTarget.Net461:
                        targetFolder = "net461";
                        break;
                    case TracerTarget.Netstandard20:
                        targetFolder = "netstandard2.0";
                        break;
                    case TracerTarget.Netcoreapp31:
                        targetFolder = "netcoreapp3.1";
                        break;
                    case TracerTarget.Net60:
                        targetFolder = "net6.0";
                        break;
                }

                var datadogTraceDllPath = Path.Combine(_tracerHome, targetFolder, "Datadog.Trace.dll");
                var datadogTracePdbPath = Path.Combine(_tracerHome, targetFolder, "Datadog.Trace.pdb");

                // Global lock for copying the Datadog.Trace assembly to the output folder
                lock (PadLock)
                {
                    // Copying the Datadog.Trace assembly
                    var assembly = typeof(Tracer).Assembly;
                    var assemblyLocation = assembly.Location;
                    var outputAssemblyDllLocation = Path.Combine(Path.GetDirectoryName(_assemblyFilePath) ?? string.Empty, Path.GetFileName(assemblyLocation));
                    var outputAssemblyPdbLocation = Path.Combine(Path.GetDirectoryName(_assemblyFilePath) ?? string.Empty, Path.GetFileNameWithoutExtension(assemblyLocation) + ".pdb");
                    if (!File.Exists(outputAssemblyDllLocation) ||
                        assembly.GetName().Version >= AssemblyName.GetAssemblyName(outputAssemblyDllLocation).Version)
                    {
                        _logger.Debug($"CopyRequiredAssemblies: Writing ({tracerTarget}) {outputAssemblyDllLocation} ...");

                        if (File.Exists(datadogTraceDllPath))
                        {
                            File.Copy(datadogTraceDllPath, outputAssemblyDllLocation, true);
                        }

                        if (File.Exists(datadogTracePdbPath))
                        {
                            File.Copy(datadogTracePdbPath, outputAssemblyPdbLocation, true);
                        }
                    }

                    return outputAssemblyDllLocation;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return string.Empty;
        }

        internal TracerTarget GetTracerTarget(AssemblyDefinition assemblyDefinition)
        {
            foreach (var customAttribute in assemblyDefinition.CustomAttributes)
            {
                if (customAttribute.AttributeType.FullName == "System.Runtime.Versioning.TargetFrameworkAttribute")
                {
                    var targetValue = (string)customAttribute.ConstructorArguments[0].Value;
                    if (targetValue.Contains(".NETFramework,Version="))
                    {
                        _logger.Debug($"GetTracerTarget: Returning TracerTarget.Net461 from {targetValue}");
                        return TracerTarget.Net461;
                    }

                    var matchTarget = NetCorePattern.Match(targetValue);
                    if (matchTarget.Success)
                    {
                        var versionValue = matchTarget.Groups[1].Value;
                        if (float.TryParse(versionValue, NumberStyles.AllowDecimalPoint, UsCultureInfo, out var version))
                        {
                            if (version >= 2.0 && version <= 3.0)
                            {
                                _logger.Debug($"GetTracerTarget: Returning TracerTarget.Netstandard20 from {targetValue}");
                                return TracerTarget.Netstandard20;
                            }

                            if (version > 3.0 && version <= 5.0)
                            {
                                _logger.Debug($"GetTracerTarget: Returning TracerTarget.Netcoreapp31 from {targetValue}");
                                return TracerTarget.Netcoreapp31;
                            }

                            if (version > 5.0)
                            {
                                _logger.Debug($"GetTracerTarget: Returning TracerTarget.Net60 from {targetValue}");
                                return TracerTarget.Net60;
                            }
                        }
                    }
                }
            }

            var coreLibrary = assemblyDefinition.MainModule.TypeSystem.CoreLibrary;
            _logger.Debug($"GetTracerTarget: Calculating TracerTarget from: {((AssemblyNameReference)coreLibrary).FullName} in {assemblyDefinition.FullName}");
            switch (coreLibrary.Name)
            {
                case "netstandard" when coreLibrary is AssemblyNameReference coreAsmRef && coreAsmRef.Version.Major == 2:
                case "System.Private.CoreLib":
                case "System.Runtime":
                    _logger.Debug("GetTracerTarget: Returning TracerTarget.Netstandard20");
                    return TracerTarget.Netstandard20;
            }

            _logger.Debug("GetTracerTarget: Returning TracerTarget.Net461");
            return TracerTarget.Net461;
        }

        private class CustomResolver : BaseAssemblyResolver
        {
            private readonly ICollectorLogger _logger;
            private DefaultAssemblyResolver _defaultResolver;
            private string _tracerAssemblyLocation;
            private string _assemblyFilePath;

            public CustomResolver(ICollectorLogger logger, string assemblyFilePath)
            {
                _tracerAssemblyLocation = string.Empty;
                _logger = logger;
                _assemblyFilePath = assemblyFilePath;
                _defaultResolver = new DefaultAssemblyResolver();
            }

            public override AssemblyDefinition Resolve(AssemblyNameReference name)
            {
                AssemblyDefinition assembly;
                try
                {
                    assembly = _defaultResolver.Resolve(name);
                }
                catch (AssemblyResolutionException ex)
                {
                    var tracerAssemblyName = TracerAssembly.GetName();
                    if (name.Name == tracerAssemblyName.Name && name.Version == tracerAssemblyName.Version)
                    {
                        if (!string.IsNullOrEmpty(_tracerAssemblyLocation))
                        {
                            assembly = AssemblyDefinition.ReadAssembly(_tracerAssemblyLocation);
                        }
                        else
                        {
                            assembly = AssemblyDefinition.ReadAssembly(TracerAssembly.Location);
                        }
                    }
                    else
                    {
                        var folder = Path.GetDirectoryName(_assemblyFilePath);
                        var pathTest = Path.Combine(folder ?? string.Empty, name.Name + ".dll");
                        _logger.Debug($"Looking for: {pathTest}");
                        if (File.Exists(pathTest))
                        {
                            return AssemblyDefinition.ReadAssembly(pathTest);
                        }

                        _logger.Error(ex, $"Error in the Custom Resolver processing '{_assemblyFilePath}' for: {name.FullName}");
                        throw;
                    }
                }

                return assembly;
            }

            public void SetTracerAssemblyLocation(string assemblyLocation)
            {
                _tracerAssemblyLocation = assemblyLocation;
            }
        }
    }
}
