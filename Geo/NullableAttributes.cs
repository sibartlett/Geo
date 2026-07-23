// Polyfill for the nullable-flow-analysis attributes that live in the BCL from
// netstandard2.1 / netcoreapp3.0 onwards. The Geo library targets
// netstandard2.0, so we declare the ones we use here as internal types. When a
// consuming runtime already provides them, its copies win via type forwarding;
// these are only compiled into the netstandard2.0 assembly.
#if NETSTANDARD2_0
using System;

namespace System.Diagnostics.CodeAnalysis;

/// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter will not be null even if the corresponding type allows it.</summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class NotNullWhenAttribute : Attribute
{
    public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

    public bool ReturnValue { get; }
}
#endif
