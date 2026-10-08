using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PlayerMetadata
{
	/// <summary>Object found in a frame: class, score and box in pixels of the frame.</summary>
	public class Detection
	{
		public string Type { get; set; }
		public float Confidence { get; set; }
		public float X { get; set; }
		public float Y { get; set; }
		public float Width { get; set; }
		public float Height { get; set; }
	}

	/// <summary>
	/// Tiny YOLOv2 object detector (Pascal VOC model: 20 classes), run on the CPU with ONNX Runtime.
	/// </summary>
	/// <remarks>
	/// Model <c>tinyyolov2-8.onnx</c> of the ONNX Model Zoo, downloaded by the build:
	/// <list type="bullet">
	///   <item><description>input: 1 x 3 x 416 x 416, the R, G and B planes of the image, values 0 to 255;</description></item>
	///   <item><description>output: 1 x 125 x 13 x 13, for each cell of a 13 x 13 grid, 5 boxes of
	///   25 values: x, y, width, height, confidence, then the score of each of the 20 classes.</description></item>
	/// </list>
	/// The detection itself is not specific to AppVision: any other model or library can replace this class.
	/// </remarks>
	public sealed class TinyYoloDetector : IDisposable
	{
		/// <summary>Width and height of the image expected by the model.</summary>
		public const int InputSize = 416;

		const int GridSize = 13, BoxesPerCell = 5, ClassCount = 20, CellSize = InputSize / GridSize;

		/// <summary>Width and height of the 5 reference boxes of the model, in cells.</summary>
		static readonly float[] Anchors = { 1.08f, 1.19f, 3.42f, 4.41f, 6.63f, 11.38f, 9.42f, 5.11f, 16.62f, 10.52f };

		static readonly string[] Labels =
		{
			"aeroplane", "bicycle", "bird", "boat", "bottle", "bus", "car", "cat", "chair", "cow",
			"diningtable", "dog", "horse", "motorbike", "person", "pottedplant", "sheep", "sofa", "train", "tvmonitor",
		};

		readonly InferenceSession session;
		readonly string inputName;

		/// <summary>Minimum score (confidence x class probability) of a reported object.</summary>
		public float MinScore { get; set; } = 0.3f;

		static TinyYoloDetector()
		{
			// ONNX Runtime uses System.Memory, shipped in the AppVision Bin folder, which asks for
			// System.Runtime.CompilerServices.Unsafe 4.0.4.1. The Bin folder holds a later version, and appClient.exe.config
			// has no binding redirect for it: give the version of the Bin folder (compatible) when it is asked for.
			AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
				new AssemblyName(args.Name).Name == "System.Runtime.CompilerServices.Unsafe"
					? Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "System.Runtime.CompilerServices.Unsafe.dll"))
					: null;
		}

		/// <summary>Loads the model (about a second): call it off the UI thread.</summary>
		public TinyYoloDetector(string modelPath)
		{
			session = new InferenceSession(modelPath);
			inputName = session.InputMetadata.Keys.First();
		}

		/// <summary>Finds the objects of a frame. Thread-safe, but one call at a time is enough.</summary>
		/// <param name="bgra">The frame resized to <see cref="InputSize"/> x <see cref="InputSize"/>, 4 bytes per
		/// pixel in the B, G, R, A order (<c>PixelFormats.Bgra32</c>).</param>
		/// <param name="frameWidth">Width of the original frame: the boxes are returned in its pixels.</param>
		/// <param name="frameHeight">Height of the original frame.</param>
		public List<Detection> Detect(byte[] bgra, int frameWidth, int frameHeight)
		{
			// Interleaved BGRA bytes -> R, G and B planes of floats.
			const int plane = InputSize * InputSize;
			var data = new float[3 * plane];
			for (int i = 0; i < plane; i++)
			{
				data[i] = bgra[i * 4 + 2];
				data[plane + i] = bgra[i * 4 + 1];
				data[2 * plane + i] = bgra[i * 4];
			}
			var input = new DenseTensor<float>(data, new[] { 1, 3, InputSize, InputSize });

			using (var results = session.Run(new[] { NamedOnnxValue.CreateFromTensor(inputName, input) }))
			{
				var output = results.First().AsTensor<float>();
				var candidates = Decode(output, frameWidth / (float)InputSize, frameHeight / (float)InputSize);
				return NonMaxSuppression(candidates);
			}
		}

		/// <summary>Turns the output grid into boxes, in pixels of the original frame.</summary>
		IEnumerable<Detection> Decode(Tensor<float> output, float scaleX, float scaleY)
		{
			var scores = new float[ClassCount];
			for (int row = 0; row < GridSize; row++)
				for (int col = 0; col < GridSize; col++)
					for (int box = 0; box < BoxesPerCell; box++)
					{
						int channel = box * (5 + ClassCount);
						float confidence = Sigmoid(output[0, channel + 4, row, col]);
						if (confidence < MinScore)
							continue;

						for (int c = 0; c < ClassCount; c++)
							scores[c] = output[0, channel + 5 + c, row, col];
						Softmax(scores);
						int best = Array.IndexOf(scores, scores.Max());
						float score = confidence * scores[best];
						if (score < MinScore)
							continue;

						// Box centre: position in its cell; box size: relative to the reference box.
						float width = (float)Math.Exp(output[0, channel + 2, row, col]) * Anchors[2 * box] * CellSize;
						float height = (float)Math.Exp(output[0, channel + 3, row, col]) * Anchors[2 * box + 1] * CellSize;
						float x = (col + Sigmoid(output[0, channel, row, col])) * CellSize - width / 2;
						float y = (row + Sigmoid(output[0, channel + 1, row, col])) * CellSize - height / 2;

						yield return new Detection
						{
							Type = Labels[best],
							Confidence = score,
							X = x * scaleX,
							Y = y * scaleY,
							Width = width * scaleX,
							Height = height * scaleY,
						};
					}
		}

		/// <summary>Keeps the best box among those that overlap on the same object.</summary>
		static List<Detection> NonMaxSuppression(IEnumerable<Detection> candidates)
		{
			var kept = new List<Detection>();
			foreach (var candidate in candidates.OrderByDescending(d => d.Confidence))
				if (!kept.Any(k => k.Type == candidate.Type && Overlap(k, candidate) > 0.45f))
					kept.Add(candidate);
			return kept;
		}

		/// <summary>Intersection over union of two boxes.</summary>
		static float Overlap(Detection a, Detection b)
		{
			float width = Math.Min(a.X + a.Width, b.X + b.Width) - Math.Max(a.X, b.X);
			float height = Math.Min(a.Y + a.Height, b.Y + b.Height) - Math.Max(a.Y, b.Y);
			if (width <= 0 || height <= 0)
				return 0;
			float intersection = width * height;
			return intersection / (a.Width * a.Height + b.Width * b.Height - intersection);
		}

		static float Sigmoid(float value) => 1 / (1 + (float)Math.Exp(-value));

		static void Softmax(float[] values)
		{
			float max = values.Max(), sum = 0;
			for (int i = 0; i < values.Length; i++)
				sum += values[i] = (float)Math.Exp(values[i] - max);
			for (int i = 0; i < values.Length; i++)
				values[i] /= sum;
		}

		/// <summary>Releases the native memory of the model.</summary>
		public void Dispose() => session.Dispose();
	}
}
