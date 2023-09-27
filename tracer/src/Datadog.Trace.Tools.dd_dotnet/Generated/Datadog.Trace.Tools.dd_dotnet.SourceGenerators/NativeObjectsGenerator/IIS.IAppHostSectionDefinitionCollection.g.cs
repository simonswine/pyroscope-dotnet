﻿
using System;
using System.Runtime.InteropServices;

namespace NativeObjects;

internal unsafe class IAppHostSectionDefinitionCollection : Datadog.Trace.Tools.dd_dotnet.Checks.Windows.IIS.IAppHostSectionDefinitionCollection
{
    public static IAppHostSectionDefinitionCollection Wrap(IntPtr obj) => new IAppHostSectionDefinitionCollection(obj);

    private readonly IntPtr _implementation;

    public IAppHostSectionDefinitionCollection(IntPtr implementation)
    {
        _implementation = implementation;
    }

    private nint* VTable => (nint*)*(nint*)_implementation;

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        if (_implementation != IntPtr.Zero)
        {
            Release();
        }
    }

    ~IAppHostSectionDefinitionCollection()
    {
        Dispose();
    }

    public int QueryInterface(in System.Guid a0, out nint a1)
    {
        var func = (delegate* unmanaged[Stdcall]<IntPtr, in System.Guid, out nint, int>)*(VTable + 0);
        var result = func(_implementation, in a0, out a1);
        return result;
    }
    public int AddRef()
    {
        var func = (delegate* unmanaged[Stdcall]<IntPtr, int>)*(VTable + 1);
        var result = func(_implementation);
        return result;
    }
    public int Release()
    {
        var func = (delegate* unmanaged[Stdcall]<IntPtr, int>)*(VTable + 2);
        var result = func(_implementation);
        return result;
    }
    public int Count()
    {
        var func = (delegate* unmanaged[Stdcall]<IntPtr, out int, int>)*(VTable + 3);
        var result = func(_implementation, out var returnvalue);
        if (result != 0)
        {
            throw new System.ComponentModel.Win32Exception(result);
        }
        return returnvalue;
    }
    public Datadog.Trace.Tools.dd_dotnet.Checks.Windows.IIS.IAppHostSectionDefinition GetSectionDefinition(Datadog.Trace.Tools.dd_dotnet.Checks.Windows.IIS.Variant a0)
    {
        var func = (delegate* unmanaged[Stdcall]<IntPtr, Datadog.Trace.Tools.dd_dotnet.Checks.Windows.IIS.Variant, out IntPtr, int>)*(VTable + 4);
        var result = func(_implementation, a0, out var returnptr);
        var returnvalue = NativeObjects.IAppHostSectionDefinition.Wrap(returnptr);
        if (result != 0)
        {
            throw new System.ComponentModel.Win32Exception(result);
        }
        return returnvalue;
    }
    public Datadog.Trace.Tools.dd_dotnet.Checks.Windows.IIS.IAppHostSectionDefinition AddSection(string a0)
    {
        var str0 = Marshal.StringToBSTR(a0);
        var func = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, out IntPtr, int>)*(VTable + 5);
        var result = func(_implementation, str0, out var returnptr);
        var returnvalue = NativeObjects.IAppHostSectionDefinition.Wrap(returnptr);
        Marshal.FreeBSTR(str0);
        if (result != 0)
        {
            throw new System.ComponentModel.Win32Exception(result);
        }
        return returnvalue;
    }
    public void DeleteSection(Datadog.Trace.Tools.dd_dotnet.Checks.Windows.IIS.Variant a0)
    {
        var func = (delegate* unmanaged[Stdcall]<IntPtr, Datadog.Trace.Tools.dd_dotnet.Checks.Windows.IIS.Variant, int>)*(VTable + 6);
        var result = func(_implementation, a0);
        if (result != 0)
        {
            throw new System.ComponentModel.Win32Exception(result);
        }
    }


}
