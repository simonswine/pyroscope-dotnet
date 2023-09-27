﻿
using System;
using System.Runtime.InteropServices;

namespace NativeObjects;

internal unsafe class IAppHostProperty : Datadog.Trace.Tools.dd_dotnet.Checks.Windows.IIS.IAppHostProperty
{
    public static IAppHostProperty Wrap(IntPtr obj) => new IAppHostProperty(obj);

    private readonly IntPtr _implementation;

    public IAppHostProperty(IntPtr implementation)
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

    ~IAppHostProperty()
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
    public string Name()
    {
        var func = (delegate* unmanaged[Stdcall]<IntPtr, out IntPtr, int>)*(VTable + 3);
        var result = func(_implementation, out var returnstr);
        var returnvalue = Marshal.PtrToStringBSTR(returnstr);
        Marshal.FreeBSTR(returnstr);
        if (result != 0)
        {
            throw new System.ComponentModel.Win32Exception(result);
        }
        return returnvalue;
    }
    public Datadog.Trace.Tools.dd_dotnet.Checks.Windows.IIS.Variant GetValue()
    {
        var func = (delegate* unmanaged[Stdcall]<IntPtr, out Datadog.Trace.Tools.dd_dotnet.Checks.Windows.IIS.Variant, int>)*(VTable + 4);
        var result = func(_implementation, out var returnvalue);
        if (result != 0)
        {
            throw new System.ComponentModel.Win32Exception(result);
        }
        return returnvalue;
    }
    public void SetValue(Datadog.Trace.Tools.dd_dotnet.Checks.Windows.IIS.Variant a0)
    {
        var func = (delegate* unmanaged[Stdcall]<IntPtr, Datadog.Trace.Tools.dd_dotnet.Checks.Windows.IIS.Variant, int>)*(VTable + 5);
        var result = func(_implementation, a0);
        if (result != 0)
        {
            throw new System.ComponentModel.Win32Exception(result);
        }
    }
    public void Clear()
    {
        var func = (delegate* unmanaged[Stdcall]<IntPtr, int>)*(VTable + 6);
        var result = func(_implementation);
        if (result != 0)
        {
            throw new System.ComponentModel.Win32Exception(result);
        }
    }
    public string StringValue()
    {
        var func = (delegate* unmanaged[Stdcall]<IntPtr, out IntPtr, int>)*(VTable + 7);
        var result = func(_implementation, out var returnstr);
        var returnvalue = Marshal.PtrToStringBSTR(returnstr);
        Marshal.FreeBSTR(returnstr);
        if (result != 0)
        {
            throw new System.ComponentModel.Win32Exception(result);
        }
        return returnvalue;
    }
    public Datadog.Trace.Tools.dd_dotnet.Checks.Windows.IIS.IAppHostPropertyException Exception()
    {
        var func = (delegate* unmanaged[Stdcall]<IntPtr, out IntPtr, int>)*(VTable + 8);
        var result = func(_implementation, out var returnptr);
        var returnvalue = NativeObjects.IAppHostPropertyException.Wrap(returnptr);
        if (result != 0)
        {
            throw new System.ComponentModel.Win32Exception(result);
        }
        return returnvalue;
    }
    public Datadog.Trace.Tools.dd_dotnet.Checks.Windows.IIS.Variant GetMetadata(string a0)
    {
        var str0 = Marshal.StringToBSTR(a0);
        var func = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, out Datadog.Trace.Tools.dd_dotnet.Checks.Windows.IIS.Variant, int>)*(VTable + 9);
        var result = func(_implementation, str0, out var returnvalue);
        Marshal.FreeBSTR(str0);
        if (result != 0)
        {
            throw new System.ComponentModel.Win32Exception(result);
        }
        return returnvalue;
    }
    public void SetMetadata(string a0, Datadog.Trace.Tools.dd_dotnet.Checks.Windows.IIS.Variant a1)
    {
        var str0 = Marshal.StringToBSTR(a0);
        var func = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, Datadog.Trace.Tools.dd_dotnet.Checks.Windows.IIS.Variant, int>)*(VTable + 10);
        var result = func(_implementation, str0, a1);
        Marshal.FreeBSTR(str0);
        if (result != 0)
        {
            throw new System.ComponentModel.Win32Exception(result);
        }
    }
    public Datadog.Trace.Tools.dd_dotnet.Checks.Windows.IIS.IAppHostPropertySchema Schema()
    {
        var func = (delegate* unmanaged[Stdcall]<IntPtr, out IntPtr, int>)*(VTable + 11);
        var result = func(_implementation, out var returnptr);
        var returnvalue = NativeObjects.IAppHostPropertySchema.Wrap(returnptr);
        if (result != 0)
        {
            throw new System.ComponentModel.Win32Exception(result);
        }
        return returnvalue;
    }


}
