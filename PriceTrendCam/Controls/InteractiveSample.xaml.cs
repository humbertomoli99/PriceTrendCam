// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.WinUI.UI.Animations;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using CommunityToolkit.WinUI.UI.Controls;
using Windows.System;

namespace PriceTrendCam.Controls;

/// <summary>
/// A simple control that acts as a frame for an interactive sample.
/// </summary>
public sealed class InteractiveSample : ContentControl
{
    /// <summary>
    /// Creates a new <see cref="InteractiveSample"/> instance.
    /// </summary>
    public InteractiveSample()
    {
        Implicit.SetAnimations(this, new ImplicitAnimationSet { new OffsetAnimation() });
    }

    /// <summary>
    /// Gets or sets the <see cref="string"/> representing the C# code to display.
    /// </summary>
    public string CSharpCode
    {
        get => (string)GetValue(CSharpCodeProperty);
        set => SetValue(CSharpCodeProperty, $"```csharp\n{value.Trim()}\n```");
    }

    /// <summary>
    /// The <see cref="DependencyProperty"/> backing <see cref="CSharpCode"/>.
    /// </summary>
    public static readonly DependencyProperty CSharpCodeProperty = DependencyProperty.Register(
        nameof(CSharpCode),
        typeof(string),
        typeof(InteractiveSample),
        new PropertyMetadata(default(string)));

    /// <summary>
    /// Gets or sets the <see cref="string"/> representing the XAML code to display.
    /// </summary>
    public string XamlCode
    {
        get => (string)GetValue(XamlCodeProperty);
        set => SetValue(XamlCodeProperty, $"```xml\n{value.Trim()}\n```");
    }

    /// <summary>
    /// The <see cref="DependencyProperty"/> backing <see cref="CSharpCode"/>.
    /// </summary>
    public static readonly DependencyProperty XamlCodeProperty = DependencyProperty.Register(
        nameof(XamlCode),
        typeof(string),
        typeof(InteractiveSample),
        new PropertyMetadata(default(string)));
}
