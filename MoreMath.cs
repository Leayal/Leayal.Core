using System;

namespace Leayal
{
    public static class MoreMath
    {
        public static float Max(float num1, float num2)
        {
            return num1 > num2 ? num1 : num2;
        }

        public static float Min(float num1, float num2)
        {
            return num1 > num2 ? num2 : num1;
        }

        public static double Max(double num1, double num2)
        {
            return num1 > num2 ? num1 : num2;
        }

        public static double Min(double num1, double num2)
        {
            return num1 > num2 ? num2 : num1;
        }
    }
}
