﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GradientGenerator
{
	public class Generate
	{
		private static Object lockObject = new Object();

		public static WriteableBitmap Gradient(
			int width = 1920, int height = 1080,
			int seed = 0,
			int minRandom = -1000, int maxRandom = 1000,
			int zeroMaxRandom = 16777215,
			int minOffsetX = -1, int maxOffsetX = 1,
			int minOffsetY = -1, int maxOffsetY = 1)
		{
			int[,] gradient = new int[height, width];

			Random random;
			if (seed == 0) random = new Random();
			else random = new Random(seed);

			gradient[0, 0] = random.Next(zeroMaxRandom) + 1; // 256*256*256 - 1

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					// anoother cool effects:
					// int randomColorInt = random.Next(maxRandomNumber - minRandomNumber) - minRandomNumber;
					// int randomColorInt = random.Next(maxRandomNumber + minRandomNumber) + minRandomNumber;

					int randomColorInt = random.Next(maxRandom - minRandom) + minRandom;

					for (int offsetY = minOffsetY; offsetY <= maxOffsetY; offsetY++)
					{
						for (int offsetX = minOffsetX; offsetX <= maxOffsetX; offsetX++)
						{
							int currentX = x + offsetX;
							int currentY = y + offsetY;

							if (currentX < 0) continue;
							if (currentX > width - 1) continue;
							if (currentY < 0) continue;
							if (currentY > height - 1) continue;

							if (gradient[currentY, currentX] == 0)
							{
								gradient[currentY, currentX] = gradient[y, x] + randomColorInt;
							}
						}
					}
				}
			}

			return CreateGradientBitmap(gradient);
		}

		public static WriteableBitmap DistortedSquareCentric(
			int width = 1920, int height = 1080,
			int seed = 0,
			int minRandom = -1000, int maxRandom = 1000,
			int zeroMaxRandom = 16777215,
			int minOffsetX = -1, int maxOffsetX = 1,
			int minOffsetY = -1, int maxOffsetY = 1)
		{
			int[,] gradient = new int[height, width];

			Random random;
			if (seed == 0) random = new Random();
			else random = new Random(seed);

			gradient[0, 0] = random.Next(zeroMaxRandom) + 1; // 256*256*256 - 1

			// <x, y, newColorInt>
			ConcurrentQueue<Tuple<int, int, int>> processingQueue = new ConcurrentQueue<Tuple<int, int, int>>();

			//first item: begin
			int firstRandomColorInt = random.Next(maxRandom - minRandom) + minRandom;
			int firstNewColorInt = gradient[0, 0] + firstRandomColorInt;

			for (int offsetY = minOffsetY; offsetY <= maxOffsetY; offsetY++)
			{
				for (int offsetX = minOffsetX; offsetX <= maxOffsetX; offsetX++)
				{
					if (offsetX < 0) continue;
					if (offsetX > width - 1) continue;
					if (offsetY < 0) continue;
					if (offsetY > height - 1) continue;

					if (gradient[offsetY, offsetX] == 0)
					{
						gradient[offsetY, offsetX] = firstNewColorInt;
						processingQueue.Enqueue(new Tuple<int, int, int>(offsetX, offsetY, firstNewColorInt));
					}
				}
			}
			// first item: end

			while(!processingQueue.IsEmpty)
			{
				int[] nums = Enumerable.Range(0, processingQueue.Count).ToArray();
				Parallel.ForEach(nums, (currentLoop, state) =>
				{
					bool success = processingQueue.TryDequeue(out Tuple<int, int, int> pixelToProcess);
					if (!success)
					{
						state.Break();
						return;
					}

					int randomColorInt = random.Next(maxRandom - minRandom) + minRandom;
					int newColorInt = pixelToProcess.Item3 + randomColorInt;

					for (int offsetY = minOffsetY; offsetY <= maxOffsetY; offsetY++)
					{
						for (int offsetX = minOffsetX; offsetX <= maxOffsetX; offsetX++)
						{
							int currentX = pixelToProcess.Item1 + offsetX;
							int currentY = pixelToProcess.Item2 + offsetY;

							if (currentX < 0) continue;
							if (currentX > width - 1) continue;
							if (currentY < 0) continue;
							if (currentY > height - 1) continue;

							if (gradient[currentY, currentX] == 0)
							{
								gradient[currentY, currentX] = newColorInt;
								processingQueue.Enqueue(new Tuple<int, int, int>(currentX, currentY, newColorInt));
							}
						}
					}
				});
			}

			return CreateGradientBitmap(gradient);
		}

		public static WriteableBitmap DistortedSquareCentricPatches(
			int width = 1920, int height = 1080,
			int seed = 0,
			int minRandom = -1000, int maxRandom = 1000,
			int zeroMaxRandom = 16777215,
			int minOffsetX = -1, int maxOffsetX = 1,
			int minOffsetY = -1, int maxOffsetY = 1,
			int threadCount = 8)
		{
			int[,] gradient = new int[height, width];

			Random random;
			if (seed == 0) random = new Random();
			else random = new Random(seed);

			
			// <x, y, newColorInt>
			Queue<Tuple<int, int, int>>[] processingQueues = new Queue<Tuple<int, int, int>>[threadCount];
			for(int i = 0; i < processingQueues.Length; i++)
			{
				processingQueues[i] = new Queue<Tuple<int, int, int>>();
			}

			//first items: begin
			for(int i = 0; i < threadCount; i++)
			{
				int x = random.Next(width);
				int y = random.Next(height);

				gradient[y, x] = random.Next(zeroMaxRandom) + 1; // 256*256*256 - 1

				int firstRandomColorInt = random.Next(maxRandom - minRandom) + minRandom;
				int firstNewColorInt = gradient[y, x] + firstRandomColorInt;

				for (int offsetY = minOffsetY; offsetY <= maxOffsetY; offsetY++)
				{
					for (int offsetX = minOffsetX; offsetX <= maxOffsetX; offsetX++)
					{
						int currentX = x + offsetX;
						int currentY = y + offsetY;

						if (currentX < 0) continue;
						if (currentX > width - 1) continue;
						if (currentY < 0) continue;
						if (currentY > height - 1) continue;

						if (gradient[currentY, currentX] == 0)
						{
							gradient[currentY, currentX] = firstNewColorInt;
							processingQueues[i].Enqueue(new Tuple<int, int, int>(currentX, currentY, firstNewColorInt));
						}
					}
				}
			}
			// first items: end

			
			int[] nums = Enumerable.Range(0, threadCount).ToArray();
			Parallel.ForEach(nums, (i, state) =>
			{
				while(processingQueues[i].Count != 0)
				{
					bool success = processingQueues[i].TryDequeue(out Tuple<int, int, int> pixelToProcess);
					if (!success)
					{
						return;
					}

					int randomColorInt = random.Next(maxRandom - minRandom) + minRandom;
					int newColorInt = pixelToProcess.Item3 + randomColorInt;

					for (int offsetY = minOffsetY; offsetY <= maxOffsetY; offsetY++)
					{
						for (int offsetX = minOffsetX; offsetX <= maxOffsetX; offsetX++)
						{
							int currentX = pixelToProcess.Item1 + offsetX;
							int currentY = pixelToProcess.Item2 + offsetY;

							if (currentX < 0) continue;
							if (currentX > width - 1) continue;
							if (currentY < 0) continue;
							if (currentY > height - 1) continue;

							if (gradient[currentY, currentX] == 0)
							{
								gradient[currentY, currentX] = newColorInt;
								processingQueues[i].Enqueue(new Tuple<int, int, int>(currentX, currentY, newColorInt));
							}
						}
					}
				}
			});

			return CreateGradientBitmap(gradient);
		}

		public static WriteableBitmap GradientPatches(
			int width = 1920, int height = 1080,
			int seed = 0,
			int minRandom = -1000, int maxRandom = 1000,
			int zeroMaxRandom = 16777215,
			int minOffsetX = -1, int maxOffsetX = 1,
			int minOffsetY = -1, int maxOffsetY = 1,
			int threadCount = 8)
		{
			int[,] gradient = new int[height, width];

			

			Random random;
			if (seed == 0) random = new Random();
			else random = new Random(seed);

			// <x, y, newColorInt>
			Queue<Tuple<int, int, int>>[] processingQueues = new Queue<Tuple<int, int, int>>[threadCount];
			for (int i = 0; i < processingQueues.Length; i++)
			{
				processingQueues[i] = new Queue<Tuple<int, int, int>>();
			}

			//first items: begin
			for (int i = 0; i < threadCount; i++)
			{
				int x = random.Next(width);
				int y = random.Next(height);

				gradient[y, x] = random.Next(zeroMaxRandom) + 1; // 256*256*256 - 1

				int firstRandomColorInt = random.Next(maxRandom - minRandom) + minRandom;
				int firstNewColorInt = gradient[y, x] + firstRandomColorInt;

				for (int offsetY = minOffsetY; offsetY <= maxOffsetY; offsetY++)
				{
					for (int offsetX = minOffsetX; offsetX <= maxOffsetX; offsetX++)
					{
						int currentX = x + offsetX;
						int currentY = y + offsetY;

						if (currentX < 0) continue;
						if (currentX > width - 1) continue;
						if (currentY < 0) continue;
						if (currentY > height - 1) continue;

						if (gradient[currentY, currentX] == 0)
						{
							gradient[currentY, currentX] = firstNewColorInt;
							processingQueues[i].Enqueue(new Tuple<int, int, int>(currentX, currentY, firstNewColorInt));
						}
					}
				}
			}
			// first items: end

			Parallel.For(0, threadCount, (i, state) =>
			{
				while (processingQueues[i].Count != 0)
				{
					bool success = processingQueues[i].TryDequeue(out Tuple<int, int, int> pixelToProcess);
					if (!success)
					{
						return;
					}

					int randomColorInt;
					lock (lockObject)
					{
						randomColorInt = random.Next(maxRandom - minRandom) + minRandom;
					}

					int newColorInt = pixelToProcess.Item3 + randomColorInt;

					for (int offsetY = minOffsetY; offsetY <= maxOffsetY; offsetY++)
					{
						for (int offsetX = minOffsetX; offsetX <= maxOffsetX; offsetX++)
						{
							int currentX = pixelToProcess.Item1 + offsetX;
							int currentY = pixelToProcess.Item2 + offsetY;

							if (currentX < 0) continue;
							if (currentX > width - 1) continue;
							if (currentY < 0) continue;
							if (currentY > height - 1) continue;

							if (gradient[currentY, currentX] == 0)
							{
								gradient[currentY, currentX] = newColorInt;
								processingQueues[i].Enqueue(new Tuple<int, int, int>(currentX, currentY, newColorInt));
							}
						}
					}
				}
			});

			return CreateGradientBitmap(gradient);
		}

		public static WriteableBitmap DistortedGradient(
			int width = 1920, int height = 1080,
			int seed = 0,
			int minRandom = -1000, int maxRandom = 1000,
			int zeroMaxRandom = 16777215,
			int minOffsetX = -1, int maxOffsetX = 1,
			int minOffsetY = -1, int maxOffsetY = 1)
		{
			int[,] gradient = new int[height, width];

			Random random;
			if (seed == 0) random = new Random();
			else random = new Random(seed);


			int x = random.Next(width);
			int y = random.Next(height);
			gradient[y, x] = random.Next(zeroMaxRandom) + 1; // 256*256*256 - 1

			// <x, y, newColorInt>
			ConcurrentQueue<Tuple<int, int, int>> processingQueue = new ConcurrentQueue<Tuple<int, int, int>>();

			//first item: begin
			int firstRandomColorInt = random.Next(maxRandom - minRandom) + minRandom;
			int firstNewColorInt = gradient[y, x] + firstRandomColorInt;

			for (int offsetY = minOffsetY; offsetY <= maxOffsetY; offsetY++)
			{
				for (int offsetX = minOffsetX; offsetX <= maxOffsetX; offsetX++)
				{
					if (offsetX < 0) continue;
					if (offsetX > width - 1) continue;
					if (offsetY < 0) continue;
					if (offsetY > height - 1) continue;

					if (gradient[offsetY, offsetX] == 0)
					{
						gradient[offsetY, offsetX] = firstNewColorInt;
						processingQueue.Enqueue(new Tuple<int, int, int>(offsetX, offsetY, firstNewColorInt));
					}
				}
			}
			// first item: end

			while (!processingQueue.IsEmpty)
			{
				int[] nums = Enumerable.Range(0, processingQueue.Count).ToArray();
				Parallel.ForEach(nums, (currentLoop, state) =>
				{
					bool success = processingQueue.TryDequeue(out Tuple<int, int, int> pixelToProcess);
					if (!success)
					{
						state.Break();
						return;
					}

					int randomColorInt = 0;
					lock (lockObject)
					{
						randomColorInt = random.Next(maxRandom - minRandom) + minRandom;
					}
					
					int newColorInt = pixelToProcess.Item3 + randomColorInt;

					for (int offsetY = minOffsetY; offsetY <= maxOffsetY; offsetY++)
					{
						for (int offsetX = minOffsetX; offsetX <= maxOffsetX; offsetX++)
						{
							int currentX = pixelToProcess.Item1 + offsetX;
							int currentY = pixelToProcess.Item2 + offsetY;

							if (currentX < 0) continue;
							if (currentX > width - 1) continue;
							if (currentY < 0) continue;
							if (currentY > height - 1) continue;

							if (gradient[currentY, currentX] == 0)
							{
								gradient[currentY, currentX] = newColorInt;
								processingQueue.Enqueue(new Tuple<int, int, int>(currentX, currentY, newColorInt));
							}
						}
					}
				});
			}

			return CreateGradientBitmap(gradient);
		}

		private static WriteableBitmap CreateGradientBitmap(int[,] gradient)
		{
			int width = gradient.GetLength(1);
			int height = gradient.GetLength(0);
			WriteableBitmap gradientBitmap = BitmapFactory.New(width, height);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int argb = gradient[y, x];
					byte red = (byte)((argb >> 16) & 0xFF);
					byte green = (byte)((argb >> 8) & 0xFF);
					byte blue = (byte)((argb >> 0) & 0xFF);
					gradientBitmap.SetPixel(x, y, red, green, blue);
				}
			}
			gradientBitmap.Freeze();
			return gradientBitmap;
		}
	}
}
