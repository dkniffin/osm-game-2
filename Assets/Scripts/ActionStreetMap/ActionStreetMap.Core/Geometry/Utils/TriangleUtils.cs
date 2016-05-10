﻿using System;

namespace ActionStreetMap.Core.Geometry.Utils
{
    /// <summary> Provides some triangle utility functions. </summary>
    internal static class TriangleUtils
    {
        /// <summary> Calculates tirangle area. </summary>
        /// <param name="p1">Triangle point one.</param>
        /// <param name="p2">Triangle point two.</param>
        /// <param name="p3">Triangle point three.</param>
        /// <returns>Triangle area.</returns>
        public static double GetTriangleArea(Vector2d p1, Vector2d p2, Vector2d p3)
        {
            return Math.Abs((p1.X - p3.X)*(p2.Y - p1.Y) - (p1.X - p2.X)*(p3.Y - p1.Y))*0.5;
        }

        /// <summary> Gets random point in triangle. </summary>
        /// <param name="p1">Triangle point one.</param>
        /// <param name="p2">Triangle point two.</param>
        /// <param name="p3">Triangle point three.</param>
        /// <param name="a">Random value in [0,1].</param>
        /// <param name="b">Random value in [0,1].</param>
        /// <returns>Point inside triangle.</returns>
        public static Vector2d GetRandomPoint(Vector2d p1, Vector2d p2, Vector2d p3, double a, double b)
        {
            // actually, sum of a and b should be less or equal than 1
            if (a + b > 1)
            {
                a = 1 - a;
                b = 1 - b;
            }

            var c = 1 - a - b;
            var vX = (a*p1.X) + (b*p2.X) + (c*p3.X);
            var vY = (a*p1.Y) + (b*p2.Y) + (c*p3.Y);

            return new Vector2d((float)vX, (float)vY);
        }
    }
}