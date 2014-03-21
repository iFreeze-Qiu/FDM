namespace FiniteDifferenceMethod
{
    class Converter : IConverter
    {
        public static string ConverterType = "Linear_RGB_xyz_Grayscale_Converter";
        public float AScale { get; set; }
        public float BScale { get; set; }
        public float MScale { get; set; }
        public float JScale { get; set; }

        public string Type { get { return ConverterType; } }

        public IConverter Clone()
        {
            return new Converter(AScale, BScale, JScale, MScale);
        }
        public Converter()
        {
            AScale = 0.1f;
            BScale = 0.1f;
            JScale = 0.1f;
            MScale = 1f;
        }

        public Converter(IConverter converter)
            : this(converter.AScale, converter.BScale, converter.JScale, converter.MScale) { }
        public Converter(float aScale, float bScale, float jScale, float mScale)
        {
            AScale = aScale;
            BScale = bScale;
            JScale = jScale;
            MScale = mScale;
        }

        public bool Vector2Color(float x, float y, float z, float floatRange, out int color)
        {
            bool overflow = false;
            floatRange = 127f / floatRange;
            int temp = (int)(128f + x * floatRange);
            if (temp > 255)
            {
                color = 0xFF0000;
                overflow = true;
            }
            else if (temp < 0)
            {
                color = 0;
                overflow = true;
            }
            else color = temp << 16;
            temp = (int)(128f + y * floatRange);
            if (temp > 255)
            {
                color |= 0xFF00;
                overflow = true;
            }
            else if (temp < 0)
            {
                overflow = true;
            }
            else color |= temp << 8;
            temp = (int)(128f + z * floatRange);
            if (temp > 255)
            {
                color |= 0xFF;
                overflow = true;
            }
            else if (temp < 0)
            {
                overflow = true;
            }
            else color |= temp;
            return overflow;
        }
        public void Color2Vector(int color, out float x, out float y, out float z, float floatRange)
        {
            floatRange /= 127f;
            x = floatRange * (((color & 0xFF0000) >> 16) - 128f);
            y = floatRange * (((color & 0xFF00) >> 8) - 128f);
            z = floatRange * ((color & 0xFF) - 128f);
        }

        public bool Scalar2Color(float s, float floatRange, out int color)
        {
            int temp = (int)(128f + s * 127f / floatRange);
            if (temp > 255)
            {
                color = 0xFFFFFF;
                return true;
            }
            if (temp < 0)
            {
                color = 0;
                return true;
            }
            color = temp * 65793;
            return false;
        }
        public float Color2Scalar(int color, float floatRange)
        {
            return floatRange * ((color & 0xFF) - 128f) / 127f;
        }

        public bool PositiveScalar2Color(float s, float floatRange, out int color)
        {
            int temp = (int)( s * 255f / floatRange);
            if (temp > 255)
            {
                color = 0xFFFFFF;
                return true;
            }
            if (temp < 0)
            {
                color = 0;
                return true;
            }
            color = temp * 65793;
            return false;
        }
        public float Color2PositiveScalar(int color, float floatRange)
        {
            return floatRange * (color & 0xFF) / 255f;
        }

        public void Save(string fileName)
        {
            string info = Type + "\n" + AScale + "\n" + BScale + "\n" + JScale + "\n" + MScale;
            System.IO.File.WriteAllText(fileName, info);
        }
    }
}

