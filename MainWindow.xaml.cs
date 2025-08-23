using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MandelbrotViewerDotNet
{
	public partial class MainWindow : Window
	{
		public List<ColoredPixel> Pixels = new();
		public int MandelTargetWidth { get; set; } = 800;
		public int MandelTargetHeight { get; set; } = 600;
		public ImageWizard ImageWizard { get; set; }

		public MainWindow()
		{
			InitializeComponent();
			ImageWizard = new(MandelTargetWidth, MandelTargetHeight);

			// Subscribe to per-frame rendering event
			CompositionTarget.Rendering += (f, f2) => CompositionTarget_Rendering(f, f2);
		}

		// Called every frame.
		private void CompositionTarget_Rendering(object sender, EventArgs e)
		{
			
		}

		private void UpdateMainImage()
		{
			Pixels.Clear();

			double xMin = -1.0, xMax = 1.0;
			double yMin = -0.8, yMax = 0.8;

			for (int x = 0; x < 800; x++)
			{
				for (int y = 0; y < 600; y++)
				{
					double xPos = xMin + (x / 799.0) * (xMax - xMin);
					double yPos = yMin + (y / 599.0) * (yMax - yMin);
					ComplexNumber cN = new(xPos, yPos);
					var step = MrCalc.CalculateMandelbrot(cN, 300);
					Pixels.Add(new ColoredPixel(x, y, MrCalc.ConvertToColor(step, 300)));
				}
			}

			if (Pixels.Count < 1) return;

			var bmp = ImageWizard.GenerateImageWithLockBits(MandelTargetWidth, MandelTargetHeight, Pixels);
			var hBitmap = bmp.GetHbitmap();
			MainImage.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
				hBitmap,
				IntPtr.Zero,
				Int32Rect.Empty,
				BitmapSizeOptions.FromEmptyOptions());

			// Delete the HBitmap to avoid memory leaks
			[DllImport("gdi32.dll")]
			[return: MarshalAs(UnmanagedType.Bool)]
			static extern bool DeleteObject(IntPtr hObject);
			bmp.Dispose();
			DeleteObject(hBitmap);
		}

		private void InputKeyDown(object sender, KeyEventArgs e)
		{
			Key key = e.Key;
			switch (key)
			{
				case Key.Escape:
					Close();
					break;
				case Key.Space:
					UpdateMainImage();
					break;
				default:
					break;
			}
		}
	}

	public class ImageWizard(int w, int h)
	{
		public int Width { get; set; } = w;
		public int Height { get; set; } = h;

		public Bitmap GenerateImageWithLockBits(int width, int height, List<ColoredPixel> pixels)
		{
			Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			Rectangle rect = new Rectangle(0, 0, width, height);
			BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);

			int stride = bmpData.Stride;
			IntPtr ptr = bmpData.Scan0;
			int bytes = stride * height;
			byte[] rgbValues = new byte[bytes];

			for (int i = 0; i < Math.Min(pixels.Count, width * height); i++)
			{
				int x = pixels[i].X;
				int y = pixels[i].Y;
				int index = (y * stride) + (x * 3);

				rgbValues[index] = pixels[i].Color.B;      // Blue
				rgbValues[index + 1] = pixels[i].Color.G;  // Green
				rgbValues[index + 2] = pixels[i].Color.R;  // Red
			}

			Marshal.Copy(rgbValues, 0, ptr, bytes);
			bmp.UnlockBits(bmpData);

			return bmp;
		}
	}

	//public class Iterator
	//{
	//	public int Width { get; set; } = 800;
	//	public int Height { get; set; } = 600;
	//	public int MaxStep { get; set; } = 300;

	//	public Iterator(int w, int h)
	//	{
	//		Width = w;
	//		Height = h;
	//	}

	//	public void Iterate(List<ColoredPixel> pixels, int w = -1, int h = -1, int maxStep = -1)
	//	{
	//		if (w == -1) w = Width;
	//		if (h == -1) h = Height;
	//		if (maxStep == -1) maxStep = MaxStep;

	//		for (int x = 0; x < Width; x++)
	//		{
	//			for (int y = 0; y < Height; y++)
	//			{
	//				// Calculate the complex number for this pixel.
	//				var xPos = (x / Width) - (0.5 * Width);
	//				var yPos = (y / Height) - (0.5 * Height);
	//				ComplexNumber cN = new(xPos, yPos);
	//				int mNumber = MrCalc.CalculateMandelbrot(cN, maxStep);
	//				var pixel = new ColoredPixel();
	//				if (mNumber == maxStep)
	//				{
	//					pixel = new ColoredPixel(x, y, System.Drawing.Color.FromArgb(1, 0, 0, 0));
	//				}
	//				else
	//				{
	//					pixel = new ColoredPixel(x, y, MrCalc.ConvertToColor(mNumber % 360));
	//				}

	//				pixels.Add(pixel);

	//				//var a = (x - (Width / 2.0)) * (4.0 / Width);
	//				//var b = (y - (Height / 2.0)) * (4.0 / Width);
	//				//var c = new ComplexNumber(a, b);
	//				//var step = MrCalc.Iterate(c, maxStep);
	//			}
	//		}
	//	}
	//}

	public static class MrCalc
	{
		public static int CalculateMandelbrot(ComplexNumber c, int limit)
		{
			var z = new ComplexNumber(0, 0);

			for (var i = 0; i < limit; i++)
			{
				z = (z * z) + c;
				if (z.Modulus() >= 2)
				{
					return i;
				}
			}

			return limit;
		}

		public static System.Drawing.Color ConvertToColor(int number, int maxIterations)
		{
			float hue = (number % 360);
			float saturation = 1.0f;
			float valueBrightness = number < maxIterations ? 1.0f : 0.0f;
			return HsvToRgb(hue, saturation, valueBrightness);
		}

		/// <summary>
		/// Courtesy of Chris Hulbert from Splinter.com.au.
		/// </summary>
		/// <param name="hue">The hue value.</param>
		/// <param name="saturation">The saturation value.</param>
		/// <param name="value">The brightness (AKA "value") value.</param>
		/// <returns>The converted RGB color (as System.Windows.Media.Color).</returns>
		public static System.Drawing.Color HsvToRgb(float hue, float saturation, float value)
		{
			// Ensure hue is in [0, 360)
			hue %= 360f;
			if (hue < 0) hue += 360f;

			// Convert HSV to RGB
			int hi = (int)(hue / 60f) % 6;
			float f = hue / 60f - (int)(hue / 60f);
			float p = value * (1f - saturation);
			float q = value * (1f - f * saturation);
			float t = value * (1f - (1f - f) * saturation);

			float r, g, b;
			switch (hi)
			{
				case 0: r = value; g = t; b = p; break;
				case 1: r = q; g = value; b = p; break;
				case 2: r = p; g = value; b = t; break;
				case 3: r = p; g = q; b = value; break;
				case 4: r = t; g = p; b = value; break;
				case 5: r = value; g = p; b = q; break;
				default: r = g = b = 0; break;
			}

			// Convert to 0-255 range and return Color
			return System.Drawing.Color.FromArgb(
				255,
				(int)(r * 255),
				(int)(g * 255),
				(int)(b * 255)
			);
		}
	}

	public struct ColoredPixel
	{
		public int X;
		public int Y;
		public System.Drawing.Color Color { get; set; }

		public ColoredPixel(int x, int y, System.Drawing.Color color)
		{
			X = x;
			Y = y;
			Color = color;
		}
	}
}