using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSC.Toolchain.AssetBuild
{
    internal class ImageProcessorOptions
    {
        public int _ColorDepth { get; private set; } = 8;
        public bool _Tiles { get; private set; } = false;
        public int _MetatileWidth { get; private set; } = 1;
        public int _MetatileHeight { get; private set; } = 1;

        public Rgba32 _TransparentColor { get; private set; } = new Rgba32(0, 0, 0);

        public ImageProcessorOptions() { }

        public ImageProcessorOptions Tiles() 
        {
            _Tiles = true;
            return this;
        }

        public ImageProcessorOptions Bitmap()
        {
            _Tiles = false;
            return this;
        }

        public ImageProcessorOptions ColorDepth(int value)
        {
            if(value!=4 && value!=8 && value!=16)
            {
                throw new ArgumentException("Invalid color depth.");
            }
            _ColorDepth = value;
            return this;
        }

        public ImageProcessorOptions Metatile(int width, int height)
        {
            _MetatileWidth = width;
            _MetatileHeight = height;
            return this;
        }

        public ImageProcessorOptions TransparentColor(Rgba32 color)
        {
            _TransparentColor = color;
            return this;
        }

    }
    internal class ImageProcessor
    {
        public int Width { get; }
        public int Height { get; }
        Rgba32[,] Pixels;
        public ImageProcessor(string filename)
        {
            try
            {
                using (Image<Rgba32> image = Image.Load<Rgba32>(filename))
                {
                    validateImage(image);
                    Pixels = image.GetPixels();
                    Width = image.Width;
                    Height = image.Height;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not process the image");
                Console.WriteLine(e.Message);
                Environment.Exit(-1);
            }
        }

        public ConvertedData Convert(ImageProcessorOptions options)
        {
            short[] gfx = new short[0];
            short[] pal = new short[0];
            if (options._ColorDepth != 16) 
            {
                var colors = Pixels.Unique().ToList();
                colors.Remove(options._TransparentColor);

                List<Bucket> buckets = new MedianCut(Pixels.Unique().ToList()).Split(1 << options._ColorDepth).ToList();                                                                        

                buckets = buckets.Where(b => b.Items.Count > 0).ToList();

                foreach (var b in buckets)
                {
                    //Console.WriteLine("Bucket");
                    //Console.WriteLine("[" + string.Join(", ", b.Items.Select(c => c.ToHex())) + "]");
                    //Console.WriteLine(b.Average.ToHex());
                }

                if (buckets.Count == (1<<options._ColorDepth))
                {
                    Console.WriteLine("HERE?");
                    buckets = buckets.MergeClosestTwo();
                    Console.WriteLine(buckets.Count);
                }

                pal = new short[1 + buckets.Count];


                Dictionary<Rgba32, short> colors16 = new Dictionary<Rgba32, short>();
                Dictionary<short, int> davg = new Dictionary<short, int>();

                buckets.ToList().ForEach(bucket=>
                    {
                        short avg = bucket.Average.ToBGR15();
                        //Console.WriteLine("AVG = " + avg.ToString("X4"));
                        davg[avg] = 1;
                        bucket.Items.ForEach(color => colors16[color] = avg);
                    });

                for(int i=0;i<davg.Keys.ToList().Count;i++)
                {
                    short key = davg.Keys.ToList()[i];
                    pal[i + 1] = key;
                    davg[key] = i + 1;
                }

                pal[0] = options._TransparentColor.ToBGR15();
                var keys16 = colors16.Keys.ToList();
                for (int i = 0; i < keys16.Count; i++) 
                {
                    colors16[keys16[i]] = (short)(davg[colors16[keys16[i]]]);
                }
                colors16[options._TransparentColor] = 0;

                gfx = new short[Width * Height * options._ColorDepth / 8 / sizeof(short)];

                int k = 0;
                if(!options._Tiles)
                {
                    if(options._ColorDepth!=8)
                    {
                        throw new ArgumentException("Paletted Bitmap must be 8-bit");
                    }
                    Console.WriteLine("8-bit bitmap");
                    Console.WriteLine("Palette size = "+pal.Length.ToString());
                    for (int y = 0; y < Height; y++)
                    {
                        for (int x = 0; x < Width; x++) 
                        {
                            short index = colors16[Pixels[y, x]];
                            if (options._ColorDepth == 8) 
                            {
                                gfx[k / 2] |= (short)(index << (8 * (k % 2)));
                                k++;
                            }
                            else if (options._ColorDepth == 4)
                            {
                                gfx[k / 4] |= (short)(index << (4 * (k % 4)));
                                k++;
                            }
                        }
                    }
                }
                else
                {
                    if ((Width / 8) % options._MetatileWidth != 0 ||
                        (Height / 8) % options._MetatileHeight != 0)
                    {
                        throw new ArgumentException("Bad metatile alignment");
                    }

                    for (int metay = 0; metay < Height / 8 / options._MetatileHeight; metay++)
                    {
                        for (int metax = 0; metax < Width / 8 / options._MetatileWidth; metax++) 
                        {
                            for (int ty = 0; ty < options._MetatileHeight; ty++)
                            {
                                for (int tx = 0; tx < options._MetatileWidth; tx++)
                                {
                                    for (int iy = 0; iy < 8; iy++)
                                    {
                                        for (int ix = 0; ix < 8; ix++)
                                        {
                                            int x = (metax * options._MetatileWidth + tx) * 8 + ix;
                                            int y = (metay * options._MetatileHeight + ty) * 8 + iy;
                                            short index = colors16[Pixels[y, x]];
                                            if (options._ColorDepth == 8)
                                            {
                                                gfx[k / 2] |= (short)(index << (8 * (k % 2)));
                                                k++;
                                            }
                                            else if (options._ColorDepth == 4)
                                            {
                                                gfx[k / 4] |= (short)(index << (4 * (k % 4)));
                                                k++;
                                            }
                                        }
                                    }
                                }
                            }                            
                        }
                    }
                }
            }
            else
            {
                gfx = new short[Width * Height];
                pal = new short[0];

                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        gfx[Width * y + x] = Pixels[y, x].ToBGR15();
            }

            return new ConvertedData { Gfx = gfx, Palette = pal };
        }

        static void validateImage(Image img)
        {
            if (img.Width % 8 != 0 || img.Height % 8 != 0)
            {
                throw new ArgumentException("Image sizes must be multiple of 8.");
            }
        }
    }
}
