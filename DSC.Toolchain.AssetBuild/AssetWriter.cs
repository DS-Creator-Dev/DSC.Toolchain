using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DSC.Toolchain.AssetBuild
{
    internal class AssetWriter
    {
        public string Name = "";

        public int Width;
        public int Height;
        public int ColorDepth;
        public bool IsBitmap;
        public short[] Data = new short[0];
        public bool IsFile;

        private int _MetatileWidth = 0;
        private int _MetatileHeight = 0;

        public int MetatileWidth
        {
            get { return 1 << _MetatileWidth; }
            set
            {
                if(!(new List<int> { 1, 2, 4, 8 }.Contains(value)))
                {
                    Console.WriteLine("Metatile width must be a power of 2");
                    Environment.Exit(-1);
                }
                _MetatileWidth = (int)Math.Log2(value);
            }
        }

        public int MetatileHeight
        {
            get { return 1 << _MetatileHeight; }
            set
            {
                if (!(new List<int> { 1, 2, 4, 8 }.Contains(value)))
                {
                    Console.WriteLine("Metatile height must be a power of 2");
                    Environment.Exit(-1);
                }
                _MetatileHeight = (int)Math.Log2(value);
            }
        }



        public string? Filename = null;
        
        public void WriteHeader(string path)
        {
            using (StreamWriter writer = new(path))
            {
                writer.WriteLine("#pragma once");
                writer.WriteLine("#include <DSC>");
                writer.WriteLine("");
                writer.WriteLine($"extern const DSC::AssetData {Name};");
            }
        }

        int ColorDepthFlag
        {
            get => ColorDepth == 4 ? 1 : (ColorDepth == 8 ? 2 : (ColorDepth == 16 ? 3 : 0));
        }

        public short Flags
        {
            get => (short)(
                (IsFile ? 1 : 0)          // ROD_IS_FILE
                | (1 << 2)                  // ROD_TYPE_ASSET
                | (IsBitmap ? 1 : 0) << 8   // ROA_IS_BITMAP
                | (ColorDepthFlag) << 9   // ROA_COLOR_DEPTH
                | (_MetatileWidth) << 11    // ROA_METATILE_WIDTH
                | (_MetatileHeight) << 13   // ROA_METATILE_HEIGHT
                );
        }

        public void WriteAssembly(string path)
        {
            using (StreamWriter writer = new(path))
            {
                writer.WriteLine($"  .section .rodata");
                writer.WriteLine($"  .align 2");
                writer.WriteLine($"  .global {Name}");
                writer.WriteLine($"  .hidden {Name}");
                writer.WriteLine($"{Name}:");

                // write header
                // Header size (2 bytes) = 16
                // Data Length (4 bytes)
                // Data source (4 bytes)
                // Flags       (2 bytes)
                // Width       (2 bytes)
                // Height      (2 bytes)
                short[] header = new short[8];
                header[0] = 16;
                header[1] = (short)((2 * Data.Length) % 65536);
                header[2] = (short)((2 * Data.Length) / 65536);
                header[3] = 0; // .....
                header[4] = 0; // .....
                header[5] = Flags;
                header[6] = (short)Width;
                header[7] = (short)Height;

                writer.WriteLine("\n  @ Resource Header");
                string hline = " .hword ";
                for (int i = 0; i < 8; i++)
                {
                    string hword = header[i].ToString("X4");
                    hline += (i > 0 ? ", " : "") + "0x" + hword;
                }
                writer.WriteLine(hline);

                if (!IsFile)
                {
                    writer.WriteLine("\n  @ Resource Data");
                    // write effective data
                    for (int i = 0; i < Data.Length; i += 16)
                    {
                        string line = " .hword ";
                        int lim = Math.Min(16, Data.Length - i);
                        for (int j = 0; j < lim; j++)
                        {
                            string hword = Data[i + j].ToString("X4");
                            line += (j > 0 ? ", " : "") + "0x" + hword;
                        }
                        writer.WriteLine(line);
                    }
                }
                else
                {
                    Console.WriteLine("Effective data exists in file. To be implemented...");
                    Environment.Exit(-1);
                }
            }
        }
    }
}
