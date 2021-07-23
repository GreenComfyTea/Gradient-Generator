using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GradientGenerator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private WriteableBitmap gradientBitmap;

		public MainWindow()
		{
			InitializeComponent();

			threadCount_Slider.Maximum = Environment.ProcessorCount;
			threadCount_Slider.Value = Environment.ProcessorCount;

			EnableUI();
		}

		private void EnableUI()
		{
			generateGradientButton.IsEnabled = true;
			generateGradientPatchesButton.IsEnabled = true;
			generateDistortedGradientButton.IsEnabled = true;
			generateDistortedSquareCentricButton.IsEnabled = true;
			generateDistortedSquareCentricPatchesButton.IsEnabled = true;
		}

		private void DisableUI()
		{
			generateGradientButton.IsEnabled = false;
			generateGradientPatchesButton.IsEnabled = false;
			generateDistortedGradientButton.IsEnabled = false;
			generateDistortedSquareCentricButton.IsEnabled = false;
			generateDistortedSquareCentricPatchesButton.IsEnabled = false;
		}

		private void AllowSaving()
		{
			saveFileButton.IsEnabled = true;
		}

		private void ForbidSaving()
		{
			saveFileButton.IsEnabled = false;
		}

		private void Width_Changed(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			e.Handled = !Int32.TryParse(e.Text, out int width);
		}

		private void Height_Changed(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			e.Handled = !Int32.TryParse(e.Text, out int height);
		}

		private void Seed_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (seedTextBox == null) return;
			seedTextBox.Text = seedSlider.Value.ToString();
		}

		private void MinRandom_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (minRandomTextBox == null) return;

			if (minRandomSlider.Value < maxRandomSlider.Value)
			{
				minRandomTextBox.Text = minRandomSlider.Value.ToString();
			}
			else
			{
				minRandomSlider.Value = maxRandomSlider.Value;
			}
		}

		private void MaxRandom_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (maxRandomTextBox == null) return;

			if (minRandomSlider.Value < maxRandomSlider.Value)
			{
				maxRandomTextBox.Text = maxRandomSlider.Value.ToString();
			}
			else
			{
				maxRandomSlider.Value = minRandomSlider.Value;
			}
		}

		private void ZeroMaxRandom_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (zeroMaxRandomTextBox == null) return;
			zeroMaxRandomTextBox.Text = zeroMaxRandomSlider.Value.ToString();
		}

		private void MinOffsetX_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (minOffsetX_TextBox == null) return;
			minOffsetX_TextBox.Text = minOffsetX_Slider.Value.ToString();
		}

		private void MaxOffsetX_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (maxOffsetX_TextBox == null) return;
			maxOffsetX_TextBox.Text = maxOffsetX_Slider.Value.ToString();
		}

		private void MinOffsetY_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (minOffsetY_TextBox == null) return;
			minOffsetY_TextBox.Text = minOffsetY_Slider.Value.ToString();
		}

		private void MaxOffsetY_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (maxOffsetY_TextBox == null) return;
			maxOffsetY_TextBox.Text = maxOffsetY_Slider.Value.ToString();
		}

		private void ThreadCount_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (threadCount_TextBox == null) return;
			threadCount_TextBox.Text = threadCount_Slider.Value.ToString();
		}

		private void RestoreDefaults_Click(object sender, RoutedEventArgs e)
		{
			widthTextBox.Text = Constants.DEFAULT_WIDTH.ToString();
			heightTextBox.Text = Constants.DEFAULT_HEIGHT.ToString();
			seedSlider.Value = Constants.DEFAULT_SEED;
			minRandomSlider.Value = Constants.DEFAULT_MIN_RANDOM;
			maxRandomSlider.Value = Constants.DEFAULT_MAX_RANDOM;
			zeroMaxRandomSlider.Value = Constants.DEFAULT_ZERO_MAX_RANDOM;
			minOffsetX_Slider.Value = Constants.DEFAULT_MIN_OFFSET_X;
			maxOffsetX_Slider.Value = Constants.DEFAULT_MAX_OFFSET_X;
			minOffsetY_Slider.Value = Constants.DEFAULT_MIN_OFFSET_Y;
			maxOffsetY_Slider.Value = Constants.DEFAULT_MAX_OFFSET_Y;
		}

		private void GenerateGradient_Click(object sender, RoutedEventArgs e)
		{
			int width = Int32.Parse(widthTextBox.Text);
			int height = Int32.Parse(heightTextBox.Text);
			int seed = Convert.ToInt32(seedSlider.Value);
			int minRandom = Convert.ToInt32(minRandomSlider.Value);
			int maxRandom = Convert.ToInt32(maxRandomSlider.Value);
			int zeroMaxRandom = Convert.ToInt32(zeroMaxRandomSlider.Value);
			int minOffsetX = Convert.ToInt32(minOffsetX_Slider.Value);
			int maxOffsetX = Convert.ToInt32(maxOffsetX_Slider.Value);
			int minOffsetY = Convert.ToInt32(minOffsetY_Slider.Value);
			int maxOffsetY = Convert.ToInt32(maxOffsetY_Slider.Value);

			DisableUI();
			ForbidSaving();
			Task.Run(() => GenerateGradient(width, height, seed, minRandom, maxRandom, zeroMaxRandom, minOffsetX, maxOffsetX, minOffsetY, maxOffsetY));
		}

		private void GenerateGradient(
			int width = 1920, int height = 1080,
			int seed = 0,
			int minRandom = -1000, int maxRandom = 1000,
			int zeroMaxRandom = 16777215,
			int minOffsetX = -1, int maxOffsetX = 1,
			int minOffsetY = -1, int maxOffsetY = 1)
		{

			gradientBitmap = Generate.Gradient(width, height, seed, minRandom, maxRandom, zeroMaxRandom, minOffsetX, maxOffsetX, minOffsetY, maxOffsetY);

			Dispatcher.Invoke(() =>
			{
				EnableUI();
				AllowSaving();
				resultImage.Source = gradientBitmap;
			});
		}

		private void GenerateGradientPatches_Click(object sender, RoutedEventArgs e)
		{
			int width = Int32.Parse(widthTextBox.Text);
			int height = Int32.Parse(heightTextBox.Text);
			int seed = Convert.ToInt32(seedSlider.Value);
			int minRandom = Convert.ToInt32(minRandomSlider.Value);
			int maxRandom = Convert.ToInt32(maxRandomSlider.Value);
			int zeroMaxRandom = Convert.ToInt32(zeroMaxRandomSlider.Value);
			int minOffsetX = Convert.ToInt32(minOffsetX_Slider.Value);
			int maxOffsetX = Convert.ToInt32(maxOffsetX_Slider.Value);
			int minOffsetY = Convert.ToInt32(minOffsetY_Slider.Value);
			int maxOffsetY = Convert.ToInt32(maxOffsetY_Slider.Value);
			int threadCount = Convert.ToInt32(threadCount_Slider.Value);

			DisableUI();
			ForbidSaving();
			Task.Run(() => GenerateGradientPatches(width, height, seed, minRandom, maxRandom, zeroMaxRandom, minOffsetX, maxOffsetX, minOffsetY, maxOffsetY, threadCount));
		}

		private void GenerateGradientPatches(
			int width = 1920, int height = 1080,
			int seed = 0,
			int minRandom = -1000, int maxRandom = 1000,
			int zeroMaxRandom = 16777215,
			int minOffsetX = -1, int maxOffsetX = 1,
			int minOffsetY = -1, int maxOffsetY = 1,
			int threadCount = 8)
		{
			gradientBitmap = Generate.GradientPatches(width, height, seed, minRandom, maxRandom, zeroMaxRandom, minOffsetX, maxOffsetX, minOffsetY, maxOffsetY, threadCount);

			Dispatcher.Invoke(() =>
			{
				EnableUI();
				AllowSaving();
				resultImage.Source = gradientBitmap;
			});
		}

		private void GenerateDistortedGradient_Click(object sender, RoutedEventArgs e)
		{
			int width = Int32.Parse(widthTextBox.Text);
			int height = Int32.Parse(heightTextBox.Text);
			int seed = Convert.ToInt32(seedSlider.Value);
			int minRandom = Convert.ToInt32(minRandomSlider.Value);
			int maxRandom = Convert.ToInt32(maxRandomSlider.Value);
			int zeroMaxRandom = Convert.ToInt32(zeroMaxRandomSlider.Value);
			int minOffsetX = Convert.ToInt32(minOffsetX_Slider.Value);
			int maxOffsetX = Convert.ToInt32(maxOffsetX_Slider.Value);
			int minOffsetY = Convert.ToInt32(minOffsetY_Slider.Value);
			int maxOffsetY = Convert.ToInt32(maxOffsetY_Slider.Value);

			DisableUI();
			ForbidSaving();
			Task.Run(() => GenerateDistortedGradient(width, height, seed, minRandom, maxRandom, zeroMaxRandom, minOffsetX, maxOffsetX, minOffsetY, maxOffsetY));
		}

		private void GenerateDistortedGradient(
			int width = 1920, int height = 1080,
			int seed = 0,
			int minRandom = -1000, int maxRandom = 1000,
			int zeroMaxRandom = 16777215,
			int minOffsetX = -1, int maxOffsetX = 1,
			int minOffsetY = -1, int maxOffsetY = 1)
		{
			gradientBitmap = Generate.DistortedGradient(width, height, seed, minRandom, maxRandom, zeroMaxRandom, minOffsetX, maxOffsetX, minOffsetY, maxOffsetY);

			Dispatcher.Invoke(() =>
			{
				EnableUI();
				AllowSaving();
				resultImage.Source = gradientBitmap;
			});
		}

		private void GenerateDistortedSquareCentric_Click(object sender, RoutedEventArgs e)
		{
			int width = Int32.Parse(widthTextBox.Text);
			int height = Int32.Parse(heightTextBox.Text);
			int seed = Convert.ToInt32(seedSlider.Value);
			int minRandom = Convert.ToInt32(minRandomSlider.Value);
			int maxRandom = Convert.ToInt32(maxRandomSlider.Value);
			int zeroMaxRandom = Convert.ToInt32(zeroMaxRandomSlider.Value);
			int minOffsetX = Convert.ToInt32(minOffsetX_Slider.Value);
			int maxOffsetX = Convert.ToInt32(maxOffsetX_Slider.Value);
			int minOffsetY = Convert.ToInt32(minOffsetY_Slider.Value);
			int maxOffsetY = Convert.ToInt32(maxOffsetY_Slider.Value);

			DisableUI();
			ForbidSaving();
			Task.Run(() => GenerateDistortedSquareCentric(width, height, seed, minRandom, maxRandom, zeroMaxRandom, minOffsetX, maxOffsetX, minOffsetY, maxOffsetY));
		}

		private void GenerateDistortedSquareCentric(
			int width = 1920, int height = 1080,
			int seed = 0,
			int minRandom = -1000, int maxRandom = 1000,
			int zeroMaxRandom = 16777215,
			int minOffsetX = -1, int maxOffsetX = 1,
			int minOffsetY = -1, int maxOffsetY = 1)
		{
			gradientBitmap = Generate.DistortedSquareCentric(width, height, seed, minRandom, maxRandom, zeroMaxRandom, minOffsetX, maxOffsetX, minOffsetY, maxOffsetY);

			Dispatcher.Invoke(() =>
			{
				EnableUI();
				AllowSaving();
				resultImage.Source = gradientBitmap;
			});
		}

		private void GenerateDistortedSquareCentricPatches_Click(object sender, RoutedEventArgs e)
		{
			int width = Int32.Parse(widthTextBox.Text);
			int height = Int32.Parse(heightTextBox.Text);
			int seed = Convert.ToInt32(seedSlider.Value);
			int minRandom = Convert.ToInt32(minRandomSlider.Value);
			int maxRandom = Convert.ToInt32(maxRandomSlider.Value);
			int zeroMaxRandom = Convert.ToInt32(zeroMaxRandomSlider.Value);
			int minOffsetX = Convert.ToInt32(minOffsetX_Slider.Value);
			int maxOffsetX = Convert.ToInt32(maxOffsetX_Slider.Value);
			int minOffsetY = Convert.ToInt32(minOffsetY_Slider.Value);
			int maxOffsetY = Convert.ToInt32(maxOffsetY_Slider.Value);
			int threadCount = Convert.ToInt32(threadCount_Slider.Value);

			DisableUI();
			ForbidSaving();
			Task.Run(() => GenerateDistortedSquareCentricPatches(width, height, seed, minRandom, maxRandom, zeroMaxRandom, minOffsetX, maxOffsetX, minOffsetY, maxOffsetY, threadCount));
		}

		private void GenerateDistortedSquareCentricPatches(
			int width = 1920, int height = 1080,
			int seed = 0,
			int minRandom = -1000, int maxRandom = 1000,
			int zeroMaxRandom = 16777215,
			int minOffsetX = -1, int maxOffsetX = 1,
			int minOffsetY = -1, int maxOffsetY = 1, int threadCount  = 8)
		{
			gradientBitmap = Generate.DistortedSquareCentricPatches(width, height, seed, minRandom, maxRandom, zeroMaxRandom, minOffsetX, maxOffsetX, minOffsetY, maxOffsetY, threadCount);

			Dispatcher.Invoke(() =>
			{
				EnableUI();
				AllowSaving();
				resultImage.Source = gradientBitmap;
			});
		}

		private void SaveFile_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			saveFileDialog.Filter = "Img Files|*.jpg;*.jpeg;*.png;*.bmp";

			saveFileDialog.FileName = string.Format("Gradient_{0}.png", DateTime.Now.ToString().Replace(':', '-').Replace(' ', '_'));

			if (saveFileDialog.ShowDialog() == true)
			{
				if (saveFileDialog.FileName.Length > 0)
				{
					using (FileStream stream = new FileStream(saveFileDialog.FileName, FileMode.Create))
					{
						PngBitmapEncoder encoder = new PngBitmapEncoder();
						encoder.Frames.Add(BitmapFrame.Create(gradientBitmap));
						encoder.Save(stream);
					}
				}
			}
		}
	}
}
