using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSC.Toolchain.AssetBuild
{
    internal class MedianCut
    {
        private List<Rgba32> Items = new List<Rgba32>();

        public MedianCut(List<Rgba32> items)
        {
            Items = items;
        }

        public Bucket[] Split(int count)
        {
            if (count != 16 && count != 256)
                throw new ArgumentException("Invlaid median cut count. Must be wither 16 or 256");

            List<Bucket> buckets = new List<Bucket>();
            buckets.Add(new Bucket(Items));

            while (buckets.Count < count)
            {
                List<Bucket> new_buckets = new List<Bucket>();
                buckets.ForEach(bucket => new_buckets.AddRange(bucket.Split()));
                buckets = new_buckets;
            }
            return buckets.ToArray();
        }
    }

    internal class Bucket
    {
        public List<Rgba32> Items = new List<Rgba32>();

        public Bucket(List<Rgba32> items)
        {
            Items = items;
        }

        private void Sort()
        {
            if (Items.Count == 0)
                return;
            int rangeR = Items.Max(new RedComparer()).R - Items.Min(new RedComparer()).R;
            int rangeG = Items.Max(new GreenComparer()).G - Items.Min(new GreenComparer()).G;
            int rangeB = Items.Max(new BlueComparer()).B - Items.Min(new BlueComparer()).B;

            int rangeMax = new List<int>() { rangeR, rangeG, rangeB }.Max();

            IComparer<Rgba32> comparer = (rangeR == rangeMax) ? new RedComparer()
                : (rangeG == rangeMax ? new GreenComparer() : new BlueComparer());            

            Items.Sort(comparer);
        }

        public Bucket[] Split()
        {
            Sort();
            return new Bucket[2]
                {
                    new Bucket(Items.Take(Items.Count / 2).ToList()),
                    new Bucket(Items.Skip(Items.Count / 2).ToList())
                };
        }

        bool _AverageComputed = false;
        Rgba32 _Average;
        public Rgba32 Average
        {
            get
            {
                if (_AverageComputed)
                    return _Average;

                if (Items.Count == 0) return new Rgba32(0, 0, 0);
                int r = 0;
                int g = 0;
                int b = 0;
                Items.ForEach(c => { r += c.R; g += c.G; b += c.B; });
                r/=Items.Count;
                g/=Items.Count;
                b/=Items.Count;
                _Average = new Rgba32((byte)r, (byte)g, (byte)b);
                _AverageComputed = true;
                return _Average;
            }
        }

        class RedComparer : IComparer<Rgba32>
        {
            public int Compare(Rgba32 c1, Rgba32 c2)
            {
                return c1.R.CompareTo(c2.R);
            }
        }

        public Bucket Merge(Bucket other)
        {
            List<Rgba32> items = new List<Rgba32>();
            items.AddRange(this.Items);
            items.AddRange(other.Items);
            return new Bucket(items);
        }

        class GreenComparer : IComparer<Rgba32>
        {
            public int Compare(Rgba32 c1, Rgba32 c2)
            {
                return c1.G.CompareTo(c2.G);
            }
        }

        class BlueComparer : IComparer<Rgba32>
        {
            public int Compare(Rgba32 c1, Rgba32 c2)
            {
                return c1.B.CompareTo(c2.B);
            }
        }



    }
}
