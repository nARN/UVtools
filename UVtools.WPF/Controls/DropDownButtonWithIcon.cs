﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using System;
using System.Collections.Generic;

namespace UVtools.WPF.Controls;

public class DropDownButtonWithIcon : DropDownButton
{
    protected override Type StyleKeyOverride => typeof(DropDownButton);

    private readonly List<IDisposable> _disposableSubscribes = new();

    public static readonly StyledProperty<string> TextProperty =
        ButtonWithIcon.TextProperty.AddOwner<DropDownButtonWithIcon>();

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<ButtonWithIcon.IconPlacementType> IconPlacementProperty =
        ButtonWithIcon.IconPlacementProperty.AddOwner<DropDownButtonWithIcon>();

    public ButtonWithIcon.IconPlacementType IconPlacement
    {
        get => GetValue(IconPlacementProperty);
        set => SetValue(IconPlacementProperty, value);
    }

    public static readonly StyledProperty<string> IconProperty =
        ButtonWithIcon.IconProperty.AddOwner<DropDownButtonWithIcon>();

    public string Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<double> SpacingProperty =
        ButtonWithIcon.SpacingProperty.AddOwner<DropDownButtonWithIcon>();

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public DropDownButtonWithIcon()
    {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        foreach (var disposableSubscribe in _disposableSubscribes)
        {
            disposableSubscribe.Dispose();
        }
        _disposableSubscribes.Clear();

        _disposableSubscribes.Add(TextProperty.Changed.Subscribe(_ => RebuildContent()));
        _disposableSubscribes.Add(IconProperty.Changed.Subscribe(_ => RebuildContent()));
        _disposableSubscribes.Add(IconPlacementProperty.Changed.Subscribe(_ => RebuildContent()));
        RebuildContent();
    }

    public Projektanker.Icons.Avalonia.Icon MakeIcon()
    {
        return new Projektanker.Icons.Avalonia.Icon { Value = Icon };
    }

    private void RebuildContent()
    {
        if (string.IsNullOrWhiteSpace(Icon))
        {
            if (!string.IsNullOrWhiteSpace(Text))
            {
                Content = Text;
            }
            return;
        }

        if (string.IsNullOrWhiteSpace(Text))
        {
            if (!string.IsNullOrWhiteSpace(Icon))
            {
                Content = MakeIcon();
            }

            return;
        }

        var panel = new StackPanel
        {
            Spacing = Spacing,
            VerticalAlignment = VerticalAlignment.Stretch,
            Orientation = IconPlacement is ButtonWithIcon.IconPlacementType.Left or ButtonWithIcon.IconPlacementType.Right
                ? Orientation.Horizontal
                : Orientation.Vertical
        };

        if (IconPlacement is ButtonWithIcon.IconPlacementType.Left or ButtonWithIcon.IconPlacementType.Top) panel.Children.Add(MakeIcon());
        panel.Children.Add(new TextBlock { VerticalAlignment = VerticalAlignment.Center, Text = Text });
        if (IconPlacement is ButtonWithIcon.IconPlacementType.Right or ButtonWithIcon.IconPlacementType.Bottom) panel.Children.Add(MakeIcon());

        Content = panel;
    }
}