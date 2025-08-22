using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotViewerDotNet;
public class ComplexNumber : IDisposable
{
	public double A { get; set; }
	public double B { get; set; }

	public ComplexNumber(double a, double b)
	{
		A = a;
		B = b;
	}

	public double Modulus()
	{
		return Math.Sqrt(A*A + B*B);
	}

	public static ComplexNumber operator +(ComplexNumber f, ComplexNumber s)
	{
		return new ComplexNumber(f.A + s.A, f.B + s.B);
	}

	public static ComplexNumber operator *(ComplexNumber f, ComplexNumber s)
	{
		var a = f.A;
		var b = f.B;
		var c = s.A;
		var d = s.B;

		return new ComplexNumber((a * c) - (b * d), (a * d) + (b * c)); // This uses the "Foil" method of multiplying complex numbers.
	}

	private bool disposedValue;

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				// TODO: dispose managed state (managed objects)
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			disposedValue = true;
		}
	}

	// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	// ~ComplexNumber()
	// {
	//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
	//     Dispose(disposing: false);
	// }

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
