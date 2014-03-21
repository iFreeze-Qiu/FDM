using System;
using System.Globalization;
using System.IO;

namespace FiniteDifferenceMethod
{
    static class ConverterFactory
    {
        public static IConverter LoadConverter(string fileName)
        {
            if (!File.Exists(fileName)) return null;
            string[] separators = new[] {" ", "\n", "\r", "\t"};
            string info = System.IO.File.ReadAllText(fileName);
            string[] infos = info.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            float a, b, j, m;
            if (!float.TryParse(infos[1], NumberStyles.Float, CultureInfo.InvariantCulture, out a)) return null;
            if (!float.TryParse(infos[2], NumberStyles.Float, CultureInfo.InvariantCulture, out b)) return null;
            if (!float.TryParse(infos[3], NumberStyles.Float, CultureInfo.InvariantCulture, out j)) return null;
            if (!float.TryParse(infos[4], NumberStyles.Float, CultureInfo.InvariantCulture, out m)) return null;
            if (infos[0].Equals(Converter.ConverterType)) return new Converter(a, b, j, m);
            return null;
        }
    }
}

