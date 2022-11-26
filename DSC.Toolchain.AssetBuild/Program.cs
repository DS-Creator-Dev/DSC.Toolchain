using DSC.Toolchain.AssetBuild;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections;
using System.IO;
using static System.Net.Mime.MediaTypeNames;


internal partial class Program
{    
    private static int get_color_depth(string arg)
    {
        return int.Parse(arg.Substring(0,arg.Length-3)); // remove "bpp"
    }

    private static void Main(string[] args)
    {
        Console.WriteLine("DSC Asset build tool\n");
        ArgsProc.Instance.LoadArgs(args);
        foreach (var key in ArgsProc.Instance.Keys)
        {
            Console.WriteLine(key + " = " + ArgsProc.Instance[key] ?? "");
        }

        if (ArgsProc.Instance["image"] == null)
        {
            Console.WriteLine("No input provided");
            Environment.Exit(-1);
        }

        int color_depth = get_color_depth(ArgsProc.Instance["color_depth"]);
        int metatile_width = int.Parse(ArgsProc.Instance["metatileW"] ?? "1");
        int metatile_height = int.Parse(ArgsProc.Instance["metatileH"] ?? "1");

        var options = new ImageProcessorOptions()
            .ColorDepth(color_depth)
            .Metatile(metatile_width, metatile_height);

        if (ArgsProc.Instance["type"] == "bitmap")
            options = options.Bitmap();
        else
            options = options.Tiles();

        var processor = new ImageProcessor(ArgsProc.Instance["image"]);

        var data = processor.Convert(options);

        var filename = ArgsProc.Instance["image"];

        var writer = new AssetWriter();
        writer.Name = "ROA_" + Path.GetFileName(filename).Split('.')[0] + get_color_depth(ArgsProc.Instance["color_depth"]).ToString();
        writer.Width = processor.Width / 8;
        writer.Height = processor.Height / 8;
        writer.IsBitmap = ArgsProc.Instance["type"] == "bitmap";
        writer.ColorDepth = get_color_depth(ArgsProc.Instance["color_depth"]);
        writer.Data = data.ToShorts();
        writer.IsFile = ArgsProc.Instance["save"] == "file";
        writer.MetatileWidth = metatile_width;
        writer.MetatileHeight = metatile_height;

        writer.WriteHeader(ArgsProc.Instance["header"]);
        writer.WriteAssembly(ArgsProc.Instance["asm"]);
    }          
}
