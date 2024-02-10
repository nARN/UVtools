﻿using System;
using System.Drawing;
using Emgu.CV.Structure;

namespace UVtools.Core.Extensions;

public static class DrawingExtensions
{
    public static Color FactorColor(this Color color, byte pixelColor, byte min = 0, byte max = byte.MaxValue) =>
        FactorColor(color, pixelColor / 255.0, min, max);

    public static Color FactorColor(this Color color, double factor, byte min = 0, byte max = byte.MaxValue)
    {
        byte r = (byte)(color.R == 0 ? 0 :
            Math.Min(Math.Max(min, color.R * factor), max));

        byte g = (byte)(color.G == 0 ? 0 :
            Math.Min(Math.Max(min, color.G * factor), max));
            
        byte b = (byte)(color.B == 0 ? 0 :
            Math.Min(Math.Max(min, color.B * factor), max));
        return Color.FromArgb(r, g, b);
    }

    public static double CalculatePolygonSideLengthFromRadius(double radius, int sides)
    {
        return 2 * radius * Math.Sin(Math.PI / sides);
    }

    public static double CalculatePolygonVerticalLengthFromRadius(double radius, int sides)
    {
        return radius * Math.Cos(Math.PI / sides);
    }

    public static double CalculatePolygonRadiusFromSideLength(double length, int sides)
    {
        var theta = 360.0 / sides;
        return length / (2 * Math.Cos((90 - theta / 2) * Math.PI / 180.0));
    }

    public static Point[] GetPolygonVertices(int sides, int radius, Point center, double startingAngle = 0, bool flipHorizontally = false, bool flipVertically = false)
    {
        if (sides < 3)
            throw new ArgumentException("Polygons can't have less than 3 sides...", nameof(sides));

        var vertices = new Point[sides];

        /*if (sides == 4)
        {
            var rotatedRect = new RotatedRect(center, new SizeF(radius * 2, radius * 2), (float)startingAngle);
            var verticesF = rotatedRect.GetVertices();
            for (var i = 0; i < verticesF.Length; i++)
            {
                vertices[i] = verticesF[i].ToPoint();
            }
            return vertices;
        }*/

        

        var deg = 360.0 / sides;//calculate the rotation angle
        var rad = Math.PI / 180.0;

        var x0 = center.X + radius * Math.Cos(-(((180 - deg) / 2) + startingAngle) * rad);
        var y0 = center.Y - radius * Math.Sin(-(((180 - deg) / 2) + startingAngle) * rad);

        var x1 = center.X + radius * Math.Cos(-(((180 - deg) / 2) + deg + startingAngle) * rad);
        var y1 = center.Y - radius * Math.Sin(-(((180 - deg) / 2) + deg + startingAngle) * rad);

        vertices[0] = new(
            (int) Math.Round(x0, MidpointRounding.AwayFromZero),
            (int) Math.Round(y0, MidpointRounding.AwayFromZero)
        );

        vertices[1] = new(
            (int) Math.Round(x1, MidpointRounding.AwayFromZero),
            (int) Math.Round(y1, MidpointRounding.AwayFromZero)
        );

        for (int i = 0; i < sides - 2; i++)
        {
            var dsinrot = Math.Sin(deg * (i + 1) * rad);
            var dcosrot = Math.Cos(deg * (i + 1) * rad);

            vertices[i + 2] = new(
                (int)Math.Round(center.X + dcosrot * (x1 - center.X) - dsinrot * (y1 - center.Y), MidpointRounding.AwayFromZero),
                (int)Math.Round(center.Y + dsinrot * (x1 - center.X) + dcosrot * (y1 - center.Y), MidpointRounding.AwayFromZero)
            );
        }

        if (flipHorizontally)
        {
            var startX = center.X - radius;
            var endX = center.X + radius;
            for (int i = 0; i < sides; i++)
            {
                vertices[i].X = endX - (vertices[i].X - startX);
            }
        }

        if (flipVertically)
        {
            var startY = center.Y - radius;
            var endY = center.Y + radius;
            for (int i = 0; i < sides; i++)
            {
                vertices[i].Y = endY - (vertices[i].Y - startY);
            }
        }

        return vertices;
    }
}