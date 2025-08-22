using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MandelbrotViewerDotNet
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}
	}

	public class ImageWizard
	{
		BitmapSource bitmap;

		public int ImageWidth { get; set; } = 2600;
		public int ImageHeight { get; set; } = 1600;

		public ImageSource GenerateImage(int width, int height, ComplexNumber center, double step, int[] palette)
		{
			bitmap output = new WriteableBitmap(ImageWidth, ImageHeight, 96, 96, PixelFormats.Bgra32, null);
		}
	}

	public class Iterator
	{

	}

	public static class MrCalc
	{
		public static int Iterate(ComplexNumber c, int limit)
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
	}
}