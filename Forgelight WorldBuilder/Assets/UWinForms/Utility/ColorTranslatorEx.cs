﻿namespace UWinForms.Utility
{
    using System.Drawing;
    using global::System;

    public static class ColorTranslatorEx
    {
        public static Color FromHsb(byte hue, byte saturation, byte brigthness)
        {
            double dh = (double)hue / 255;
            double ds = (double)saturation / 255;
            double db = (double)brigthness / 255;
            return FromHsb(dh, ds, db);
        }
        public static Color FromHsb(double hue, double saturation, double brigthness)
        {
            double r = 0, g = 0, b = 0;
            if (brigthness != 0)
            {
                if (saturation == 0)
                    r = g = b = brigthness;
                else
                {
                    double temp2 = _GetTemp2(hue, saturation, brigthness);
                    double temp1 = 2.0f * brigthness - temp2;

                    r = _GetColorComponent(temp1, temp2, hue + 1.0f / 3.0f);
                    g = _GetColorComponent(temp1, temp2, hue);
                    b = _GetColorComponent(temp1, temp2, hue - 1.0f / 3.0f);
                }
            }
            return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
        }
        public static Color FromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            if (hi == 4)
                return Color.FromArgb(255, t, p, v);

            return Color.FromArgb(255, v, p, q);
        }
        public static string ToHexString(this Color c)
        {
            return c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2") + c.A.ToString("X2");
        }
        public static void ToHSV(Color color, out double hue, out double saturation, out double value)
        {
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            hue = color.GetHue();
            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        private static double _GetColorComponent(double temp1, double temp2, double temp3)
        {
            temp3 = _MoveIntoRange(temp3);
            if (temp3 < 1.0f / 6.0f)
                return temp1 + (temp2 - temp1) * 6.0f * temp3;
            else if (temp3 < 0.5f)
                return temp2;
            else if (temp3 < 2.0f / 3.0f)
                return temp1 + ((temp2 - temp1) * ((2.0f / 3.0f) - temp3) * 6.0f);
            else
                return temp1;
        }
        private static double _GetTemp2(double h, double s, double l)
        {
            double temp2;
            if (l < 0.5f)
                temp2 = l * (1.0f + s);
            else
                temp2 = l + s - (l * s);
            return temp2;
        }
        private static double _MoveIntoRange(double temp3)
        {
            if (temp3 < 0.0f)
                temp3 += 1.0f;
            else if (temp3 > 1.0f)
                temp3 -= 1.0f;
            return temp3;
        }
    }
}
