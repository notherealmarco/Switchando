using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HomeAutomation.Dictionaries
{
    static class ColorConverter
    {
        public static uint[] ConvertNameToRGB(string name)
        {
            Color color_cs = Color.FromName(name);
            if (!(color_cs.R == 0 && color_cs.G == 0 && color_cs.B == 0))
            {
                return new uint[3] { color_cs.R, color_cs.G, color_cs.B };
            }


            FileInfo dictionary_file = new FileInfo(@"Dictionaries/colors-rgb_it.txt");
            StreamReader reader = dictionary_file.OpenText();
            string dictionary = reader.ReadToEnd();
            string[] colors = dictionary.Split(';');

            foreach (string color in colors)
            {
                string[] color_name_args = color.Split('=');
                string color_name = color_name_args[0];

                color_name = color_name.Replace("\n", "").Replace("\r", "");

                if (name.ToLower().Equals(color_name.ToLower()))
                {
                    string[] color_rgb_args = color_name_args[1].Split(',');
                    uint[] rgb_values = new uint[3];
                    rgb_values[0] = uint.Parse(color_rgb_args[0]);
                    rgb_values[1] = uint.Parse(color_rgb_args[1]);
                    rgb_values[2] = uint.Parse(color_rgb_args[2]);
                    return rgb_values;
                }
            }
            return new uint[3] { 0, 0, 0 };
        }
    }
}
