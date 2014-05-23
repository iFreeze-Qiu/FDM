using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace FiniteDifferenceMethod
{
    class GridImage : IGridImage
    {
        public int Width { get { return _x; } }
        public int Height { get { return _y; } }
        public int Depth { get { return _z; } }
        public float Step { get { return _h; } }
        public bool IsATruncatedByColor { get; private set; }
        public bool IsBTruncatedByColor { get; private set; }
        public bool IsMTruncatedByColor { get; private set; }
        public bool IsJTruncatedByColor { get; private set; }
        public IConverter Converter { get; private set; }

        private readonly int _x, _y, _z;
        private readonly float _h;
        private readonly int[] _a;
        private readonly int[] _b;
        private readonly int[] _m;
        private readonly int[] _j;
        //test
        private readonly int[] _ab;
        //for memory allocation speed up
        private readonly int[] _buffer;
        //for multiprocessing
        private delegate void SimpleProcessing(int startX, int endX);
        private delegate void SimpleGridProcessing(IGrid grid, int startX, int endX);
        private static readonly int LogicalProcessors = Environment.ProcessorCount;
        private readonly IAsyncResult[] _results;
        //for time support
        private int folderNumber;
        private float timeStep;
        private float time;

        private GridImage(int width, int height, int depth, float step, int folderNumber, float timeStep, float time, IConverter converter, bool initZeros = false)
        {
            _results = new IAsyncResult[LogicalProcessors];
            _x = width;
            _y = height;
            _z = depth;
            _h = step;
            this.folderNumber = folderNumber;
            this.timeStep = timeStep;
            this.time = time;
            Converter = converter.Clone();
            _a = new int[_x * _y * _z];
            _b = new int[_x * _y * _z];
            _m = new int[_x * _y * _z];
            _j = new int[_x * _y * _z];
            _ab = new int[_x * _y * _z];
            int buffSize = _x * _y * _z / Math.Min(_x, Math.Min(_y, _z));
            _buffer = new int[buffSize];
            IsATruncatedByColor = false;
            IsBTruncatedByColor = false;
            IsJTruncatedByColor = false;
            IsMTruncatedByColor = false;
            if (!initZeros) return;
            SimpleProcessing caller = SubInitZeros;
            for (int i = 0; i < LogicalProcessors; i++)
            {
                int st = (int)(_x * _y * _z * ((double)i / LogicalProcessors) + 0.5);
                int en = (int)(_x * _y * _z * ((i + 1.0) / LogicalProcessors) + 0.5);
                _results[i] = caller.BeginInvoke(st, en, null, null);
            }
            for (int i = 0; i < LogicalProcessors; i++)
                caller.EndInvoke(_results[i]);
        }
        private void SubInitZeros(int startX, int endX)
        {
            for (int i = startX; i < endX; i++)
            {
                Converter.Vector2Color(0, 0, 0, Converter.AScale, out _a[i]);
                Converter.Vector2Color(0, 0, 0, Converter.BScale, out _b[i]);
                Converter.Vector2Color(0, 0, 0, Converter.JScale, out _j[i]);
                Converter.PositiveScalar2Color(0, Converter.MScale, out _m[i]);
                Converter.PositiveScalar2Color(0, Converter.BScale, out _ab[i]);
            }
        }
        public GridImage(IGrid grid, IConverter converter)
            : this(grid.Width, grid.Height, grid.Depth, grid.Step, 0, 0, 0, converter) //It should work with 0,0,0. But i didn't check it.
        {
            SimpleGridProcessing caller = SubGridImageing;
            for (int i = 0; i < LogicalProcessors; i++)
            {
                int st = (int)(_x * ((double)i / LogicalProcessors) + 0.5);
                int en = (int)(_x * ((i + 1.0) / LogicalProcessors) + 0.5);
                _results[i] = caller.BeginInvoke(grid, st, en, null, null);
            }
            for (int i = 0; i < LogicalProcessors; i++)
                caller.EndInvoke(_results[i]);
        }
        private void SubGridImageing(IGrid grid, int startX, int endX)
        {
            for (int x = startX; x < endX; x++)
                for (int y = 0; y < _y; y++)
                    for (int z = 0; z < _z; z++)
                    {
                        int indx = (x * _y + y) * _z + z;
                        Cell temp = grid[x, y, z];
                        IsATruncatedByColor |= Converter.Vector2Color(temp.Ax, temp.Ay, temp.Az, Converter.AScale, out _a[indx]);
                        IsBTruncatedByColor |= Converter.Vector2Color(temp.Bx, temp.By, temp.Bz, Converter.BScale, out _b[indx]);
                        Converter.PositiveScalar2Color((float)Math.Sqrt(temp.Bx * temp.Bx + temp.By * temp.By + temp.Bz * temp.Bz), Converter.BScale, out _ab[indx]);
                        IsJTruncatedByColor |= Converter.Vector2Color(temp.Jx, temp.Jy, temp.Jz, Converter.JScale, out _j[indx]);
                        IsMTruncatedByColor |= Converter.PositiveScalar2Color(temp.M, Converter.MScale, out _m[indx]);
                    }
        }

        public IGrid ConvertToGrid()
        {
            Grid grid = new Grid(_x, _y, _z, _h);
            SimpleGridProcessing caller = SubConvertToGrid;
            for (int i = 0; i < LogicalProcessors; i++)
            {
                int st = (int)(_x * ((double)i / LogicalProcessors) + 0.5);
                int en = (int)(_x * ((i + 1.0) / LogicalProcessors) + 0.5);
                _results[i] = caller.BeginInvoke(grid, st, en, null, null);
            }
            for (int i = 0; i < LogicalProcessors; i++)
                caller.EndInvoke(_results[i]);
            grid.DoPreCalculations();
            return grid;
        }
        private void SubConvertToGrid(IGrid grid, int startX, int endX)
        {
            int indx = startX * _y * _z;
            Cell temp = new Cell();
            for (int x = startX; x < endX; x++)
                for (int y = 0; y < _y; y++)
                    for (int z = 0; z < _z; z++)
                    {
                        Converter.Color2Vector(_a[indx], out temp.Ax, out temp.Ay, out temp.Az, Converter.AScale);
                        Converter.Color2Vector(_j[indx], out temp.Jx, out temp.Jy, out temp.Jz, Converter.JScale);
                        temp.M = Converter.Color2PositiveScalar(_m[indx], Converter.MScale);
                        grid[x, y, z] = temp;
                        indx++;
                    }
        }

        public int[] GetLayerX(VariableType variable, int x)
        {
            int[] proc = GetArray(variable);
            int resindx = 0;
            for (int yy = 0; yy < _z; yy++)
            {
                for (int xx = 0; xx < _y; xx++)
                {
                    int indx = (x * _y + xx) * _z + _z - yy - 1;
                    _buffer[resindx] = proc[indx];
                    resindx++;
                }
            }
            return _buffer;
        }
        public void SetLayerX(int[] data, VariableType variable, int x)
        {
            if (data == null) return;
            int[] proc = GetArray(variable);
            int resindx = 0;
            for (int yy = 0; yy < _z; yy++)
            {
                for (int xx = 0; xx < _y; xx++)
                {
                    int indx = (x * _y + xx) * _z + _z - yy - 1;
                    proc[indx] = data[resindx];
                    resindx++;
                }
            }
        }
        public int[] GetLayerY(VariableType variable, int y)
        {
            int[] proc = GetArray(variable);
            int resindx = 0;
            for (int yy = 0; yy < _z; yy++)
            {
                for (int xx = 0; xx < _x; xx++)
                {
                    int indx = ((_x - xx - 1) * _y + y) * _z + _z - yy - 1;
                    _buffer[resindx] = proc[indx];
                    resindx++;
                }
            }
            return _buffer;
        }
        public void SetLayerY(int[] data, VariableType variable, int y)
        {
            if (data == null) return;
            int[] proc = GetArray(variable);
            int resindx = 0;
            for (int yy = 0; yy < _z; yy++)
            {
                for (int xx = 0; xx < _x; xx++)
                {
                    int indx = ((_x - xx - 1) * _y + y) * _z + _z - yy - 1;
                    proc[indx] = data[resindx];
                    resindx++;
                }
            }
        }
        public int[] GetLayerZ(VariableType variable, int z)
        {
            int[] proc = GetArray(variable);
            int resindx = 0;
            for (int yy = 0; yy < _x; yy++)
            {
                for (int xx = 0; xx < _y; xx++)
                {
                    int indx = (yy * _y + xx) * _z + z;
                    _buffer[resindx] = proc[indx];
                    resindx++;
                }
            }
            return _buffer;
        }
        public void SetLayerZ(int[] data, VariableType variable, int z)
        {
            if (data == null) return;
            int[] proc = GetArray(variable);
            int resindx = 0;
            for (int yy = 0; yy < _x; yy++)
            {
                for (int xx = 0; xx < _y; xx++)
                {
                    int indx = (yy * _y + xx) * _z + z;
                    proc[indx] = data[resindx];
                    resindx++;
                }
            }
        }
        public int GetPoint(VariableType variable, int x, int y, int z)
        {
            int[] proc = GetArray(variable);
            return proc[(x * _y + y) * _z + z];
        }

        public void Save(string projectName)
        {
            //Volume
            string fileName = projectName + Path.DirectorySeparatorChar;
            for (int i = 0; i < Depth; i++)
            {
                SaveAsBmp(Height, Width, GetLayerZ(VariableType.JCurrent, i), fileName + "J" + Path.DirectorySeparatorChar +
                    "J_z_" + i.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0') + ".bmp");
                SaveAsBmp(Height, Width, GetLayerZ(VariableType.MPermiability, i), fileName + "M" + Path.DirectorySeparatorChar +
                    "M_z_" + i.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0') + ".bmp");
                SaveAsBmp(Height, Width, GetLayerZ(VariableType.APotential, i), fileName + "A" + Path.DirectorySeparatorChar +
                    "A_z_" + i.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0') + ".bmp");
                SaveAsBmp(Height, Width, GetLayerZ(VariableType.BMagneticField, i), fileName + "B" + Path.DirectorySeparatorChar +
                    "B_z_" + i.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0') + ".bmp");
            }
            //Converter
            Converter.Save(fileName + "Converter.inf");
            SaveInfo(fileName + "GridImage.inf");
            //Border
            fileName += "BorderA" + Path.DirectorySeparatorChar;
            SaveAsBmp(Height, Depth, GetLayerX(VariableType.APotential, Width - 1), fileName + "A_x_positive.bmp");
            SaveAsBmp(Height, Depth, GetLayerX(VariableType.APotential, 0), fileName + "A_x_negative.bmp");
            SaveAsBmp(Width, Depth, GetLayerY(VariableType.APotential, Height - 1), fileName + "A_y_positive.bmp");
            SaveAsBmp(Width, Depth, GetLayerY(VariableType.APotential, 0), fileName + "A_y_negative.bmp");
            SaveAsBmp(Height, Width, GetLayerZ(VariableType.APotential, Depth - 1), fileName + "A_z_positive.bmp");
            SaveAsBmp(Height, Width, GetLayerZ(VariableType.APotential, 0), fileName + "A_z_negative.bmp");
        }

        public static GridImage LoadFromProject(string projectName)
        {
            string fileName = projectName + Path.DirectorySeparatorChar;
            if (!Directory.Exists(fileName)) {
                
                return null;
            }            
            IConverter converter = ConverterFactory.LoadConverter(fileName + "Converter.inf");
            if (converter == null) return null;
            
            int w, h, d, folderNumber;
            float s, stepTime, time;
            if (!File.Exists(fileName + "GridImage.inf")) return null;
            LoadInfo(fileName + "GridImage.inf", out w, out h, out d, out s, out folderNumber, out stepTime, out time);
            GridImage image = new GridImage(w, h, d, s, folderNumber, stepTime, time, converter, true);
            string subDir;
            if (Directory.Exists(fileName + "BorderA"))
            {
                subDir = fileName + "BorderA" + Path.DirectorySeparatorChar;
                image.SetLayerX(LoadFromBmp(h, d, subDir + "A_x_positive.bmp"), VariableType.APotential, w - 1);
                image.SetLayerX(LoadFromBmp(h, d, subDir + "A_x_negative.bmp"), VariableType.APotential, 0);
                image.SetLayerY(LoadFromBmp(w, d, subDir + "A_y_positive.bmp"), VariableType.APotential, h - 1);
                image.SetLayerY(LoadFromBmp(w, d, subDir + "A_y_negative.bmp"), VariableType.APotential, 0);
                image.SetLayerZ(LoadFromBmp(h, w, subDir + "A_z_positive.bmp"), VariableType.APotential, d - 1);
                image.SetLayerZ(LoadFromBmp(h, w, subDir + "A_z_negative.bmp"), VariableType.APotential, 0);
            }
            if (Directory.Exists(fileName + "A"))
            {
                subDir = fileName + "A" + Path.DirectorySeparatorChar + "A_z_";
                for (int i = 0; i < d; i++) image.SetLayerZ(LoadFromBmp(h, w, subDir +
                    i.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0') + ".bmp"), VariableType.APotential, i);
            }
            //don't need to load if Grid will be formed from that GridImage
            //if (Directory.Exists(fileName + "B"))
            //{
            //    subDir = fileName + "B" + Path.DirectorySeparatorChar + "B_z_";
            //    for (int i = 0; i < d; i++) image.SetLayerZ(LoadFromBmp(h, w, subDir +
            //        i.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0') + ".bmp"), VariableType.BMagneticField, i);
            //}
            if (Directory.Exists(fileName + "J"))
            {
                subDir = fileName + "J" + Path.DirectorySeparatorChar + "J_z_";
                for (int i = 0; i < d; i++) image.SetLayerZ(LoadFromBmp(h, w, subDir +
                    i.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0') + ".bmp"), VariableType.JCurrent, i);
            }
            if (Directory.Exists(fileName + "M"))
            {
                subDir = fileName + "M" + Path.DirectorySeparatorChar + "M_z_";
                for (int i = 0; i < d; i++) image.SetLayerZ(LoadFromBmp(h, w, subDir +
                    i.ToString(CultureInfo.InvariantCulture).PadLeft(4, '0') + ".bmp"), VariableType.MPermiability, i);
            }
            return image;
        }

        private void SaveInfo(string fileName)
        {
            string info = Width + "\n" + Height + "\n" + Depth + "\n" + Step.ToString(CultureInfo.InvariantCulture);
            File.WriteAllText(fileName, info);
        }
        private static void LoadInfo(string fileName, out int width, out int height, out int depth, out float step, out int folderNumber, out float timeStep, out float time)
        {
            string[] separators = new[] { " ", "\n", "\r", "\t" };
            string info = File.ReadAllText(fileName);
            string[] infos = info.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            width = int.Parse(infos[0], NumberStyles.Integer, CultureInfo.InvariantCulture);
            height = int.Parse(infos[1], NumberStyles.Integer, CultureInfo.InvariantCulture);
            depth = int.Parse(infos[2], NumberStyles.Integer, CultureInfo.InvariantCulture);
            step = float.Parse(infos[3], NumberStyles.Float, CultureInfo.InvariantCulture);
            folderNumber = int.Parse(infos[4], NumberStyles.Integer, CultureInfo.InvariantCulture);
            timeStep = float.Parse(infos[5], NumberStyles.Float, CultureInfo.InvariantCulture);
            time = float.Parse(infos[6], NumberStyles.Float, CultureInfo.InvariantCulture);
        }
        private static void SaveAsBmp(int width, int height, int[] data, string name)
        {
            Bitmap drawHere = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            BitmapData bData = drawHere.LockBits(new Rectangle(new Point(), drawHere.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            Marshal.Copy(data, 0, bData.Scan0, data.Length);
            drawHere.UnlockBits(bData);
            if (!Directory.Exists(name)) Directory.CreateDirectory(Path.GetDirectoryName(name));
            drawHere.Save(name, ImageFormat.Bmp);
        }
        private static int[] LoadFromBmp(int width, int height, string name)
        {
            if (!File.Exists(name)) return null;
            Bitmap drawHere = new Bitmap(name);
            if ((drawHere.Width != width) || (drawHere.Height != height)) return null;
            int[] data = new int[width * height];
            BitmapData bData = drawHere.LockBits(new Rectangle(new Point(), drawHere.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            Marshal.Copy(bData.Scan0, data, 0, data.Length);
            drawHere.UnlockBits(bData);
            drawHere.Dispose();
            return data;
        }
        private int[] GetArray(VariableType variable)
        {
            switch (variable)
            {
                case VariableType.APotential:
                    return _a;
                case VariableType.JCurrent:
                    return _j;
                case VariableType.BMagneticField:
                    return _b;
                case VariableType.MPermiability:
                    return _m;
                case VariableType.BAbsoluteValue:
                    return _ab;
                default:
                    throw new Exception("Invalid variable type");
            }
        }

        public int getFolderNumber()
        {
            return this.folderNumber;
        }

        public float getTimeStep()
        {
            return this.timeStep;
        }

        public float getTime()
        {
            return this.time;
        }

    }
}

