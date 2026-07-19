#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OpenRA.FileFormats;
using OpenRA.Graphics;
using OpenRA.Primitives;

namespace OpenRA.Mods.Cydonian.UtilityCommands
{
	sealed class RemapPaletteCommand : IUtilityCommand
	{
		const int MaxSheetEdge = 2048;
		const int GamutWarnDistance = 48;

		// Player remap band — reserved for PlayerColorPalette; never invent paint here.
		const int RemapBandMin = 80;
		const int RemapBandMax = 95;

		string IUtilityCommand.Name => "--remap-palette";

		bool IUtilityCommand.ValidateArguments(string[] args)
		{
			return args.Length >= 2 && TryParse(args, out _, out _, out _, out _, out _, out _);
		}

		[Desc("INPUT --palette PALETTE --output OUTPUT [--frame-size W,H] [--frame-amount N] [--offset X,Y]",
			"Remap a PNG sheet to the 256-color Cydonian palette (indexed8). Enforces the 2048x2048 sheet cap.")]
		void IUtilityCommand.Run(Utility utility, string[] args)
		{
			if (!TryParse(args, out var inputPath, out var palettePath, out var outputPath,
				out var frameSize, out var frameAmount, out var offset))
			{
				Console.WriteLine(
					"Usage: OpenRA.Utility.exe cydonian --remap-palette INPUT.png " +
					"--palette PALETTE.pal --output OUTPUT.png [--frame-size W,H] [--frame-amount N] [--offset X,Y]");
				Console.WriteLine("Paths resolve from cwd, then the mod root (parent of engine/).");
				return;
			}

			inputPath = ResolveExistingPath(inputPath, "Input PNG");
			palettePath = ResolveExistingPath(palettePath, "Palette file");
			outputPath = ResolveOutputPath(outputPath);

			var paletteColors = LoadPaletteColors(palettePath);
			Png source;
			using (var stream = File.OpenRead(inputPath))
				source = new Png(stream);

			if (source.Width > MaxSheetEdge || source.Height > MaxSheetEdge)
				throw new InvalidDataException(
					$"Sheet {source.Width}x{source.Height} exceeds the OpenRA {MaxSheetEdge}x{MaxSheetEdge} cap. " +
					"Split the sequence (hull/turret/move) before remapping.");

			var indexed = new byte[source.Width * source.Height];
			var outOfGamut = 0;

			for (var i = 0; i < source.Width * source.Height; i++)
			{
				Color src;
				if (source.Type == SpriteFrameType.Indexed8)
				{
					var idx = source.Data[i];
					src = source.Palette != null && idx < source.Palette.Length
						? source.Palette[idx]
						: Color.FromArgb(0, 0, 0);
				}
				else
				{
					var stride = source.PixelStride;
					var o = i * stride;
					var r = source.Data[o];
					var g = source.Data[o + 1];
					var b = source.Data[o + 2];
					var a = stride >= 4 ? source.Data[o + 3] : (byte)255;
					src = Color.FromArgb(a, r, g, b);
				}

				if (src.A < 16)
				{
					indexed[i] = 0;
					continue;
				}

				// Preserve intentional remap-band indices from already-indexed sources.
				if (source.Type == SpriteFrameType.Indexed8 &&
					source.Data[i] >= RemapBandMin && source.Data[i] <= RemapBandMax)
				{
					indexed[i] = source.Data[i];
					continue;
				}

				var best = NearestIndex(src, paletteColors, out var distance);
				indexed[i] = (byte)best;

				if (distance > GamutWarnDistance)
					outOfGamut++;
			}

			var embedded = new Dictionary<string, string>(source.EmbeddedData);
			if (frameSize != null)
				embedded["FrameSize"] = $"{frameSize.Value.Width},{frameSize.Value.Height}";
			if (frameAmount != null)
				embedded["FrameAmount"] = frameAmount.Value.ToString(CultureInfo.InvariantCulture);
			if (offset != null)
				embedded["Offset"] = $"{offset.Value.X},{offset.Value.Y}";

			if (embedded.TryGetValue("FrameSize", out var fs) && embedded.TryGetValue("FrameAmount", out var fa))
			{
				var size = FieldLoader.GetValue<Size>("FrameSize", fs);
				var amount = FieldLoader.GetValue<int>("FrameAmount", fa);
				if (size.Width <= 0 || size.Height <= 0)
					throw new InvalidDataException($"Invalid FrameSize {size}.");

				var capacity = (source.Width / size.Width) * (source.Height / size.Height);
				if (amount > capacity)
					throw new InvalidDataException(
						$"FrameAmount {amount} does not fit sheet {source.Width}x{source.Height} at FrameSize {size} (capacity {capacity}).");

				var framesPerRow = Math.Max(1, source.Width / size.Width);
				var rows = (amount + framesPerRow - 1) / framesPerRow;
				if (size.Width * framesPerRow > MaxSheetEdge || size.Height * rows > MaxSheetEdge)
					throw new InvalidDataException(
						$"FrameSize {size} x FrameAmount {amount} would exceed the {MaxSheetEdge} sheet cap after layout.");
			}

			var directory = Path.GetDirectoryName(outputPath);
			if (!string.IsNullOrEmpty(directory))
				Directory.CreateDirectory(directory);

			var png = new Png(indexed, SpriteFrameType.Indexed8, source.Width, source.Height, paletteColors, embedded);
			png.Save(outputPath);

			Console.WriteLine($"Remapped {inputPath} -> {outputPath} ({source.Width}x{source.Height}, indexed8)");
			Console.WriteLine($"Palette: {palettePath}");
			if (outOfGamut > 0)
			{
				Console.WriteLine(
					$"WARNING: color out of gamut — {outOfGamut} pixel(s) exceeded nearest-palette distance {GamutWarnDistance}. " +
					"Review source art; do not accept silent drift on faction remap bands.");
			}
			else
				Console.WriteLine("Gamut: clean (no out-of-gamut pixels).");

			Console.WriteLine($"Remap band {RemapBandMin}-{RemapBandMax}: reserved (nearest-match skips band).");
			Console.WriteLine($"Size gate: PASS ({source.Width}x{source.Height} <= {MaxSheetEdge}).");
		}

		// utility.cmd cds into engine/ before invoking OpenRA.Utility.exe; prefer mod-root paths from skills/docs.
		static string ResolveExistingPath(string path, string label)
		{
			foreach (var candidate in PathCandidates(path))
			{
				if (File.Exists(candidate))
					return candidate;
			}

			throw new FileNotFoundException($"{label} not found.", path);
		}

		static string ResolveOutputPath(string path)
		{
			if (Path.IsPathRooted(path))
				return path;

			// Prefer writing under the mod root when invoked via utility.cmd from engine/.
			var modRootCandidate = Path.GetFullPath(Path.Combine("..", path));
			var modRoot = Path.GetFullPath(Path.Combine(".."));
			if (Directory.Exists(Path.Combine(modRoot, "mods")))
				return modRootCandidate;

			return Path.GetFullPath(path);
		}

		static IEnumerable<string> PathCandidates(string path)
		{
			yield return path;
			if (Path.IsPathRooted(path))
				yield break;

			yield return Path.GetFullPath(path);
			yield return Path.GetFullPath(Path.Combine("..", path));
		}

		static bool TryParse(string[] args, out string inputPath, out string palettePath, out string outputPath,
			out Size? frameSize, out int? frameAmount, out float2? offset)
		{
			inputPath = null;
			palettePath = null;
			outputPath = null;
			frameSize = null;
			frameAmount = null;
			offset = null;

			// args[0] == --remap-palette
			if (args.Length < 2)
				return false;

			inputPath = args[1];
			for (var i = 2; i < args.Length; i++)
			{
				switch (args[i])
				{
					case "--palette":
						if (i + 1 >= args.Length)
							return false;
						palettePath = args[++i];
						break;
					case "--output":
						if (i + 1 >= args.Length)
							return false;
						outputPath = args[++i];
						break;
					case "--frame-size":
						if (i + 1 >= args.Length)
							return false;
						frameSize = FieldLoader.GetValue<Size>("FrameSize", args[++i]);
						break;
					case "--frame-amount":
						if (i + 1 >= args.Length)
							return false;
						frameAmount = int.Parse(args[++i], CultureInfo.InvariantCulture);
						break;
					case "--offset":
						if (i + 1 >= args.Length)
							return false;
						offset = FieldLoader.GetValue<float2>("Offset", args[++i]);
						break;
					default:
						return false;
				}
			}

			return !string.IsNullOrEmpty(inputPath)
				&& !string.IsNullOrEmpty(palettePath)
				&& !string.IsNullOrEmpty(outputPath);
		}

		static Color[] LoadPaletteColors(string path)
		{
			var colors = new Color[Palette.Size];
			using (var reader = new StreamReader(path))
			{
				var header = reader.ReadLine()?.Trim();
				if (header == "JASC-PAL" || header == "GIMP Palette")
				{
					byte a = 255;
					var i = 0;
					string line;
					while ((line = reader.ReadLine()) != null && i < Palette.Size)
					{
						if (string.IsNullOrWhiteSpace(line) || !char.IsDigit(line.Trim()[0]) ||
							line.Trim() == "0100" || line.Trim() == "256")
							continue;

						var rgba = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
						if (rgba.Length < 3)
							throw new InvalidDataException($"Invalid RGB(A) in palette: {line}");

						if (!byte.TryParse(rgba[0], out var r) ||
							!byte.TryParse(rgba[1], out var g) ||
							!byte.TryParse(rgba[2], out var b))
							throw new InvalidDataException($"Invalid RGB components in palette: {line}");

						if (i == 0)
							colors[i] = Color.FromArgb(0, 0, 0, 0);
						else if (rgba.Length > 3 && byte.TryParse(rgba[3], out a))
							colors[i] = Color.FromArgb(a, r, g, b);
						else
							colors[i] = Color.FromArgb(255, r, g, b);

						i++;
					}

					if (i < Palette.Size)
						throw new InvalidDataException($"Palette `{path}` has {i} colors; expected {Palette.Size}.");

					return colors;
				}
			}

			// Binary VGA .pal (6-bit components), matching ImmutablePalette.
			var palette = new ImmutablePalette(path, new[] { 0 }, Array.Empty<int>());
			for (var i = 0; i < Palette.Size; i++)
				colors[i] = palette.GetColor(i);
			colors[0] = Color.FromArgb(0, 0, 0, 0);
			return colors;
		}

		static int NearestIndex(Color src, Color[] palette, out int distance)
		{
			var best = 1;
			var bestDist = int.MaxValue;

			// Skip transparent (0) and player remap band (80-95).
			for (var i = 1; i < palette.Length; i++)
			{
				if (i >= RemapBandMin && i <= RemapBandMax)
					continue;

				var c = palette[i];
				var d = Math.Abs(c.R - src.R) + Math.Abs(c.G - src.G) + Math.Abs(c.B - src.B);
				if (d < bestDist)
				{
					bestDist = d;
					best = i;
				}
			}

			distance = bestDist;
			return best;
		}
	}
}
