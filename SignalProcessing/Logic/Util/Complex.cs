using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalProcessing.Logic.Util
{
    class ComplexNumber
    {
        public readonly double real;
        public readonly double imaginary;

        public ComplexNumber(double real, double imaginary)
        {
            this.real = real;
            this.imaginary = imaginary;
        }

        public static double module(ComplexNumber a)
        {
            return Math.Sqrt(a.real * a.real + a.imaginary * a.imaginary);
        }

        public static ComplexNumber conjugate(ComplexNumber a)
        {
            return new ComplexNumber(a.real, -a.imaginary);
        }

        public static ComplexNumber divideWithReal(ComplexNumber a, double ratio)
        {
            return new ComplexNumber(a.real / ratio, a.imaginary / ratio);
        }

        public static ComplexNumber add(ComplexNumber a, ComplexNumber b) 
        {
            return new ComplexNumber(a.real + b.real, a.imaginary + b.imaginary);
        }

        public static ComplexNumber substract(ComplexNumber a, ComplexNumber b)
        {
            return new ComplexNumber(a.real - b.real, a.imaginary - b.imaginary);
        }

        public static ComplexNumber multiply(ComplexNumber a, ComplexNumber b)
        {
            return new ComplexNumber(a.real * b.real - a.imaginary * b.imaginary, a.real * b.imaginary + a.imaginary * b.real);
        }

        public static ComplexNumber divide(ComplexNumber a, ComplexNumber b)
        {
            return divideWithReal(multiply(a, conjugate(b)), module(b));
        }
    }
}
