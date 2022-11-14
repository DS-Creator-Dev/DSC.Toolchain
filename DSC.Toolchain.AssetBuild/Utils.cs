using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSC.Toolchain.AssetBuild
{
    internal static class Utils
    {
        public static short ToBGR15(this Rgba32 color)
        {
            short r = color.R; r = (short)(r * 31 / 255);
            short g = color.G; g = (short)(g * 31 / 255);
            short b = color.B; b = (short)(b * 31 / 255);
            short cl = (short)(r + 32 * g + 32 * 32 * b);
            return cl;
        }

        public static Rgba32[,] GetPixels(this Image<Rgba32> image)
        {
            Rgba32[,] result = new Rgba32[image.Height, image.Width];

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        result[y, x] = pixelRow[x];                        
                    }
                }
            });

            return result;
        }

        public static Rgba32[] Unique(this Rgba32[,] colors)
        {
            Dictionary<Rgba32, bool> result = new Dictionary<Rgba32, bool>();

            for(int y=0;y<colors.GetLength(0);y++)
            {
                for(int x=0;x<colors.GetLength(1);x++)
                {
                    result[colors[y,x]] = true;
                }
            }

            return result.Keys.ToArray();
        }

        public static List<Bucket> MergeClosestTwo(this List<Bucket> buckets)
        {
            List<Bucket> result = new List<Bucket>();

            int distance(Rgba32 c1, Rgba32 c2)
            {
                int r = c1.R - c2.R;
                int g = c1.G - c2.G;
                int b = c1.B - c2.B;
                return r * r + g * g + b * b;
            }

            int p1 = 0, p2 = 0;
            int min = 256 * 256 * 3;

            for (int i = 0; i < buckets.Count - 1; i++) 
            {
                for (int j = i + 1; j < buckets.Count; j++) 
                {
                    int d = distance(buckets[i].Average, buckets[j].Average);                    
                    if(d<min)
                    {
                        Console.WriteLine($"{d}: {i}, {j}");
                        min = d;
                        p1 = i;
                        p2 = j;
                    }
                }
            }
            Console.WriteLine($"{p1} {p2}");

            result.Add(buckets[p1].Merge(buckets[p2]));

            foreach(var bucket in buckets)
            {
                if (bucket != buckets[p1] && bucket != buckets[p2])
                    result.Add(bucket);
            }

            return result;
        }       
    }
}
