using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSC.Toolchain.AssetBuild
{
    internal class ConvertedData
    {
        public short[] Gfx = new short[0];
        public short[] Palette = new short[0];

        public byte[] ToBytes()
        {
            short[] result = Gfx.Concat(Palette).ToArray();
            byte[] rb = new byte[result.Length * 2];
            Buffer.BlockCopy(result, 0, rb, 0, rb.Length);
            return rb;
        }

        public short[] ToShorts()
        {
            short[] result = Gfx.Concat(Palette).ToArray();
            return result;
        }
    }
}
