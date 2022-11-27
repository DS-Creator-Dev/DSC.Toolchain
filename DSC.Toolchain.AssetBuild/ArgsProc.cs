using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DSC.Toolchain.AssetBuild
{
    internal class ArgumentClass
    {
        public string Name = "";
        public string Prefix = "";
        public List<string> Values = new List<string>();
        public string? Default = null;
        public ArgumentClass(string name, string prefix, List<string> values, string? _default = null)
        {
            Name = name;
            Prefix = prefix;
            Values = values;
            Default = _default;
        }        

        public string? Match(string arg)
        {
            if (arg == null)
                return null;
            if (arg.Length == 0)
                return null;
            if (arg[0] != '-')
                return null;
            arg = arg.Substring(1);
            if(Prefix!="")
            {
                if (!arg.StartsWith(Prefix))
                    return null;
                arg = arg.Substring(Prefix.Length);
            }            
            if(Values.Count>0)
            {
                if (!Values.Contains(arg))
                    return null;
            }
            return arg;
        }
    }

    internal class ArgsProc
    {
        List<ArgumentClass> argclasses = new List<ArgumentClass>();

        private ArgsProc(List<ArgumentClass> argclasses)
        {
            this.argclasses = argclasses;
        }

        private static ArgsProc _Instance = new ArgsProc(new List<ArgumentClass>()
        {
            new ArgumentClass("type", "", new List<string>{"tiles", "bitmap"} , "bitmap"),
            new ArgumentClass("save", "", new List<string>{"file", "wram"}, "wram"),
            new ArgumentClass("color_depth", "", new List<string>{"4bpp", "8bpp", "16bpp"}, "8bpp"),
            // + quant(medcut), dither(true/false) etc...
            new ArgumentClass("header", "h", new List<string>(), "asset.h"),
            new ArgumentClass("asm", "s", new List<string>(), "asset.s"),
            new ArgumentClass("image", "i", new List<string>()),
            new ArgumentClass("metatileW", "mw", new List<string>()),
            new ArgumentClass("metatileH", "mh", new List<string>()),
            
        });

        Dictionary<string, string> data = new Dictionary<string, string>();

        public string? this[string key] 
        {
            get
            {
                if(data.ContainsKey(key))
                    return data[key];
                return null;
            }
        }

        public void LoadArgs(string[] args, bool check_asset_file = true, bool init_defaults = true)
        {
            if (init_defaults)
            {
                foreach (var argclass in argclasses)
                {
                    if (argclass.Default != null)
                    {
                        data[argclass.Name] = argclass.Default;
                    }
                }
            }
            foreach (var arg in args)
            {                
                foreach(var argclass in argclasses)
                {
                    string? val = argclass.Match(arg);                    
                    if (val != null) 
                    {
                        data[argclass.Name] = val;
                    }
                }
            }
            if (check_asset_file && data["image"]!=null)
            {
                // try load options from attached ".asset" file
                string asset_opts =
                    Path.Combine(Path.GetDirectoryName(data["image"]) ?? "",Path.GetFileNameWithoutExtension(data["image"]) + ".asset");
                Console.WriteLine(asset_opts);
                if(File.Exists(asset_opts))
                {
                    Console.WriteLine("Asset options found");
                    string content = File.ReadAllText(asset_opts);
                    string[] opts = content.Split(null); // split by whitespace
                    LoadArgs(opts, false, false);
                }
            }
        }

        public List<string> Keys { get => data.Keys.ToList(); }

        public static ArgsProc Instance
        {
            get => _Instance;
        }
    }
}
