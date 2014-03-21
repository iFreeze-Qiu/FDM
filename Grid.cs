using System;
// ReSharper restore TooWideLocalVariableScopeusing System;
// ReSharper disable TooWideLocalVariableScope

namespace FiniteDifferenceMethod
{
    class Grid : IGrid
    {
        public int Width { get { return _x; } }
        public int Height { get { return _y; } }
        public int Depth { get { return _z; } }
        public float Step { get { return _h; } }

        private readonly int _x, _y, _z;
        private readonly float _h;
        private readonly Cell[] _cells;
        //for multiprocessing
        private delegate double IterateToDelegate(IGrid next, int startX, int endX);
        private delegate void SimpleProcessing(int startX, int endX);
        private delegate void SimpleGridProcessing(IGrid grid, int startX, int endX);
        private delegate void ScaleProcessing(int startX, int endX, out float a, out float b, out float j, out float m);
        private static readonly int LogicalProcessors = Environment.ProcessorCount;
        private readonly IAsyncResult[] _results;
        //precalculated shortcuts
        private readonly int _stride;
        private readonly float _hsquared;
        private readonly float _hinverted2;

        public Grid(int width, int height, int depth, float step, bool asZero = false)
        {
            _results = new IAsyncResult[LogicalProcessors];
            _x = width;
            _y = height;
            _z = depth;
            _stride = _y * _z;
            _h = step;
            _hinverted2 = 0.5f / _h;
            _hsquared = _h * _h;
            _cells = new Cell[_x * _y * _z];
            if (!asZero) return;
            SimpleProcessing caller = SubDefineM;
            for (int i = 0; i < LogicalProcessors; i++)
            {
                int st = (int)(_x * ((double)i / LogicalProcessors) + 0.5);
                int en = (int)(_x * ((i + 1.0) / LogicalProcessors) + 0.5);
                _results[i] = caller.BeginInvoke(st, en, null, null);
            }
            for (int i = 0; i < LogicalProcessors; i++)
                caller.EndInvoke(_results[i]);
        }
        private void SubDefineM(int startX, int endX)
        {
            int indx = startX * _stride;
            for (int x = startX; x < endX; x++)
                for (int y = 0; y < _y; y++)
                    for (int z = 0; z < _z; z++)
                    {
                        _cells[indx].M = 1f;
                        indx++;
                    }
        }

        public Grid(IGrid grid)
            : this(grid.Width, grid.Height, grid.Depth, grid.Step)
        {
            SimpleGridProcessing caller = SubCopyGrid;
            for (int i = 0; i < LogicalProcessors; i++)
            {
                int st = (int)(_x * ((double)i / LogicalProcessors) + 0.5);
                int en = (int)(_x * ((i + 1.0) / LogicalProcessors) + 0.5);
                _results[i] = caller.BeginInvoke(grid, st, en, null, null);
            }
            for (int i = 0; i < LogicalProcessors; i++)
                caller.EndInvoke(_results[i]);
        }
        private void SubCopyGrid(IGrid grid, int startX, int endX)
        {
            for (int x = startX; x < endX; x++)
                for (int y = 0; y < _y; y++)
                    for (int z = 0; z < _z; z++)
                        this[x, y, z] = grid[x, y, z];
        }

        public void FastIterateTo(IGrid next)
        {
            IterateToDelegate caller = FastSubIterateTo;
            for (int i = 0; i < LogicalProcessors; i++)
            {
                int st = (int)((_x - 2) * ((double)i / LogicalProcessors) + 1.5);
                int en = (int)((_x - 2) * ((i + 1.0) / LogicalProcessors) + 1.5);
                _results[i] = caller.BeginInvoke(next, st, en, null, null);
            }
            for (int i = 0; i < LogicalProcessors; i++)
                caller.EndInvoke(_results[i]);
        }
        private double FastSubIterateTo(IGrid next, int startX, int endX)
        {
            int indx;
            float c, bx, by, bz;
            Cell temp;
            for (int x = startX; x < endX; x++)
                for (int y = 1; y < _y - 1; y++)
                    for (int z = 1; z < _z - 1; z++)
                    {
                        indx = (x * _y + y) * _z + z;
                        temp = _cells[indx];
                        bx = (_cells[indx + _z].Az - _cells[indx - _z].Az - _cells[indx + 1].Ay + _cells[indx - 1].Ay) * _hinverted2;
                        by = (_cells[indx + 1].Ax - _cells[indx - 1].Ax - _cells[indx + _stride].Az + _cells[indx - _stride].Az) * _hinverted2;
                        bz = (_cells[indx + _stride].Ay - _cells[indx - _stride].Ay - _cells[indx + _z].Ax + _cells[indx - _z].Ax) * _hinverted2;
                        c = (temp.GradMz * by - temp.GradMy * bz) * temp.InvertedM - temp.M * temp.Jx;
                        temp.Ax = (_cells[indx + 1].Ax + _cells[indx - 1].Ax +
                            _cells[indx + _z].Ax + _cells[indx - _z].Ax +
                            _cells[indx + _stride].Ax + _cells[indx - _stride].Ax -
                            _hsquared * c) / 6;
                        c = (temp.GradMx * bz - temp.GradMz * bx) * temp.InvertedM - temp.M * temp.Jy;
                        temp.Ay = (_cells[indx + 1].Ay + _cells[indx - 1].Ay +
                            _cells[indx + _z].Ay + _cells[indx - _z].Ay +
                            _cells[indx + _stride].Ay + _cells[indx - _stride].Ay -
                            _hsquared * c) / 6;
                        c = (temp.GradMy * bx - temp.GradMx * by) * temp.InvertedM - temp.M * temp.Jz;
                        temp.Az = (_cells[indx + 1].Az + _cells[indx - 1].Az +
                            _cells[indx + _z].Az + _cells[indx - _z].Az +
                            _cells[indx + _stride].Az + _cells[indx - _stride].Az -
                            _hsquared * c) / 6;
                        next[x, y, z] = temp;
                    }
            return 0;
        }
        public double IterateTo(IGrid next)
        {
            double error = 0;
            IterateToDelegate caller = SubIterateTo;
            for (int i = 0; i < LogicalProcessors; i++)
            {
                int st = (int)((_x - 2) * ((double)i / LogicalProcessors) + 1.5);
                int en = (int)((_x - 2) * ((i + 1.0) / LogicalProcessors) + 1.5);
                _results[i] = caller.BeginInvoke(next, st, en, null, null);
            }
            for (int i = 0; i < LogicalProcessors; i++)
                error = Math.Max(caller.EndInvoke(_results[i]), error);
            return error;
        }
        private double SubIterateTo(IGrid next, int startX, int endX)
        {
            double error = 0;
            int indx;
            float c, bx, by, bz;
            Cell temp;
            //M^2*J = -M*lapl(A) - [grad(M), B]
            for (int x = startX; x < endX; x++)
                for (int y = 1; y < _y - 1; y++)
                    for (int z = 1; z < _z - 1; z++)
                    {
                        indx = (x * _y + y) * _z + z;
                        temp = _cells[indx];
                        //calculate B = rot(A)
                        //   A' = (A[+1] - A[0]) / h - possible divergence to +Infinity, -Infinity grid
                        //bx = (_cells[indx + _z].Az - temp.Az - _cells[indx + 1].Ay + temp.Ay) * _hinverted;
                        //by = (_cells[indx + 1].Ax - temp.Ax - _cells[indx + _stride].Az + temp.Az) * _hinverted;
                        //bz = (_cells[indx + _stride].Ay - temp.Ay - _cells[indx + _z].Ax + temp.Ax) * _hinverted;
                        //   A' = (A[+1] - A[-1]) / h
                        bx = (_cells[indx + _z].Az - _cells[indx - _z].Az - _cells[indx + 1].Ay + _cells[indx - 1].Ay) * _hinverted2;
                        by = (_cells[indx + 1].Ax - _cells[indx - 1].Ax - _cells[indx + _stride].Az + _cells[indx - _stride].Az) * _hinverted2;
                        bz = (_cells[indx + _stride].Ay - _cells[indx - _stride].Ay - _cells[indx + _z].Ax + _cells[indx - _z].Ax) * _hinverted2;
                        //calculate C = -[grad(M), B]/M - MJ
                        //calculate A.next = (A.prev - C*h^2)/6
                        //   x
                        c = (temp.GradMz * by - temp.GradMy * bz) * temp.InvertedM - temp.M * temp.Jx;
                        temp.Ax = (_cells[indx + 1].Ax + _cells[indx - 1].Ax +
                            _cells[indx + _z].Ax + _cells[indx - _z].Ax +
                            _cells[indx + _stride].Ax + _cells[indx - _stride].Ax -
                            _hsquared * c) / 6;
                        //   y
                        c = (temp.GradMx * bz - temp.GradMz * bx) * temp.InvertedM - temp.M * temp.Jy;
                        temp.Ay = (_cells[indx + 1].Ay + _cells[indx - 1].Ay +
                            _cells[indx + _z].Ay + _cells[indx - _z].Ay +
                            _cells[indx + _stride].Ay + _cells[indx - _stride].Ay -
                            _hsquared * c) / 6;
                        //   z
                        c = (temp.GradMy * bx - temp.GradMx * by) * temp.InvertedM - temp.M * temp.Jz;
                        temp.Az = (_cells[indx + 1].Az + _cells[indx - 1].Az +
                            _cells[indx + _z].Az + _cells[indx - _z].Az +
                            _cells[indx + _stride].Az + _cells[indx - _stride].Az -
                            _hsquared * c) / 6;
                        next[x, y, z] = temp;
                        if (Math.Abs(temp.Ax - _cells[indx].Ax) > error) error = Math.Abs(temp.Ax - _cells[indx].Ax);
                        if (Math.Abs(temp.Ay - _cells[indx].Ay) > error) error = Math.Abs(temp.Ay - _cells[indx].Ay);
                        if (Math.Abs(temp.Az - _cells[indx].Az) > error) error = Math.Abs(temp.Az - _cells[indx].Az);
                    }
            return error;
        }

        public void DoPreCalculations()
        {
            SimpleProcessing caller = SubDoPreCalculations;
            for (int i = 0; i < LogicalProcessors; i++)
            {
                int st = (int)((_x - 2) * ((double)i / LogicalProcessors) + 1.5);
                int en = (int)((_x - 2) * ((i + 1.0) / LogicalProcessors) + 1.5);
                _results[i] = caller.BeginInvoke(st, en, null, null);
            }
            for (int i = 0; i < LogicalProcessors; i++)
                caller.EndInvoke(_results[i]);
        }
        private void SubDoPreCalculations(int startX, int endX)
        {
            int indx;
            for (int x = startX; x < endX; x++)
                for (int y = 1; y < _y - 1; y++)
                    for (int z = 1; z < _z - 1; z++)
                    {
                        indx = (x * _y + y) * _z + z;
                        _cells[indx].GradMx = (_cells[indx + _stride].M - _cells[indx - _stride].M) * _hinverted2;
                        _cells[indx].GradMy = (_cells[indx + _z].M - _cells[indx - _z].M) * _hinverted2;
                        _cells[indx].GradMz = (_cells[indx + 1].M - _cells[indx - 1].M) * _hinverted2;
                        //_cells[indx].GradMx = (_cells[indx + _stride].M - _cells[indx].M) * _hinverted;
                        //_cells[indx].GradMy = (_cells[indx + _z].M - _cells[indx].M) * _hinverted;
                        //_cells[indx].GradMz = (_cells[indx + 1].M - _cells[indx].M) * _hinverted;
                        _cells[indx].InvertedM = 1 / _cells[indx].M;
                    }
        }

        public void DoBPostCalculations()
        {
            SimpleProcessing caller = SubDoBPostCalculations;
            for (int i = 0; i < LogicalProcessors; i++)
            {
                int st = (int)((_x - 2) * ((double)i / LogicalProcessors) + 1.5);
                int en = (int)((_x - 2) * ((i + 1.0) / LogicalProcessors) + 1.5);
                _results[i] = caller.BeginInvoke(st, en, null, null);
            }
            for (int i = 0; i < LogicalProcessors; i++)
                caller.EndInvoke(_results[i]);
        }
        private void SubDoBPostCalculations(int startX, int endX)
        {
            for (int x = startX; x < endX; x++)
                for (int y = 1; y < _y - 1; y++)
                    for (int z = 1; z < _z - 1; z++)
                    {
                        int indx = (x * _y + y) * _z + z;
                        //_cells[indx].Bx = (_cells[indx + _z].Az - temp.Az - _cells[indx + 1].Ay + temp.Ay) * _hinverted;
                        //_cells[indx].By = (_cells[indx + 1].Ax - temp.Ax - _cells[indx + _stride].Az + temp.Az) * _hinverted;
                        //_cells[indx].Bz = (_cells[indx + _stride].Ay - temp.Ay - _cells[indx + _z].Ax + temp.Ax) * _hinverted;
                        _cells[indx].Bx = (_cells[indx + _z].Az - _cells[indx - _z].Az - _cells[indx + 1].Ay + _cells[indx - 1].Ay) * _hinverted2;
                        _cells[indx].By = (_cells[indx + 1].Ax - _cells[indx - 1].Ax - _cells[indx + _stride].Az + _cells[indx - _stride].Az) * _hinverted2;
                        _cells[indx].Bz = (_cells[indx + _stride].Ay - _cells[indx - _stride].Ay - _cells[indx + _z].Ax + _cells[indx - _z].Ax) * _hinverted2;
                    }
        }

        public void InterpolateAFrom(IGrid coarser)
        {
            SimpleGridProcessing caller = SubInterpolateAFrom;
            for (int i = 0; i < LogicalProcessors; i++)
            {
                int st = (int)((_x - 2) * ((double)i / LogicalProcessors) + 1.5);
                int en = (int)((_x - 2) * ((i + 1.0) / LogicalProcessors) + 1.5);
                _results[i] = caller.BeginInvoke(coarser, st, en, null, null);
            }
            for (int i = 0; i < LogicalProcessors; i++)
                caller.EndInvoke(_results[i]);
            DoBPostCalculations();
        }
        private void SubInterpolateAFrom(IGrid coarser, int startX, int endX)
        {
            int indx;
            Cell temp;
            for (int x = startX; x < endX; x++)
                for (int y = 1; y < _y - 1; y++)
                    for (int z = 1; z < _z - 1; z++)
                    {
                        temp = coarser[x / 2, y / 2, z / 2];
                        temp += coarser[(x + 1) / 2, y / 2, z / 2];
                        temp += coarser[x / 2, (y + 1) / 2, z / 2];
                        temp += coarser[x / 2, y / 2, (z + 1) / 2];
                        temp += coarser[(x + 1) / 2, (y + 1) / 2, z / 2];
                        temp += coarser[(x + 1) / 2, y / 2, (z + 1) / 2];
                        temp += coarser[x / 2, (y + 1) / 2, (z + 1) / 2];
                        temp += coarser[(x + 1) / 2, (y + 1) / 2, (z + 1) / 2];
                        temp *= 0.125f;
                        indx = (x * _y + y) * _z + z;
                        _cells[indx].Ax = temp.Ax;
                        _cells[indx].Ay = temp.Ay;
                        _cells[indx].Az = temp.Az;
                    }
        }

        public IGrid Coarser()
        {
            Grid coarser = new Grid((Width + 1) / 2, (Height + 1) / 2, (Depth + 1) / 2, Step * 2);
            SimpleGridProcessing caller = SubCoarser;
            for (int i = 0; i < LogicalProcessors; i++)
            {
                int st = (int)(coarser.Width * ((double)i / LogicalProcessors) + 0.5);
                int en = (int)(coarser.Width * ((i + 1.0) / LogicalProcessors) + 0.5);
                _results[i] = caller.BeginInvoke(coarser, st, en, null, null);
            }
            for (int i = 0; i < LogicalProcessors; i++)
                caller.EndInvoke(_results[i]);
            coarser.DoPreCalculations();
            coarser.DoBPostCalculations();
            return coarser;
        }
        private void SubCoarser(IGrid coarser, int startX, int endX)
        {
            Cell temp;
            int ax, ay, az, w, tw;
            for (int x = startX; x < endX; x++)
                for (int y = 0; y < coarser.Height; y++)
                    for (int z = 0; z < coarser.Depth; z++)
                    {//TODO: make two different loops for inner/outer cells
                        temp = new Cell();
                        w = 0;
                        if (((x != 0) && (x != coarser.Width - 1)) &&
                            ((y != 0) && (y != coarser.Height - 1)) &&
                            ((z != 0) && (z != coarser.Depth - 1)))
                        { // for inner cells
                            for (int xx = -1; xx < 2; xx++)
                                for (int yy = -1; yy < 2; yy++)
                                    for (int zz = -1; zz < 2; zz++)
                                    {
                                        tw = (2 - Math.Abs(xx)) * (2 - Math.Abs(yy)) * (2 - Math.Abs(zz));
                                        temp += this[2 * x + xx, 2 * y + yy, 2 * z + zz] * tw;
                                        w += tw;
                                    }
                            temp /= w;
                        }
                        else
                        { // for border cells
                            for (int xx = -1; xx < 2; xx++)
                                for (int yy = -1; yy < 2; yy++)
                                    for (int zz = -1; zz < 2; zz++)
                                    {
                                        ax = 2 * x + xx;
                                        ay = 2 * y + yy;
                                        az = 2 * z + zz;
                                        //select only my border cells
                                        if ((ax < 0) || (ax > _x - 1) ||
                                            (ay < 0) || (ay > _y - 1) ||
                                            (az < 0) || (az > _z - 1)) continue;
                                        if (((ax != 0) && (ax != _x - 1)) &&
                                            ((ay != 0) && (ay != _y - 1)) &&
                                            ((az != 0) && (az != _z - 1))) continue;
                                        tw = (2 - Math.Abs(xx)) * (2 - Math.Abs(yy)) * (2 - Math.Abs(zz));
                                        temp += this[ax, ay, az] * tw;
                                        w += tw;
                                    }
                            temp /= w;
                        }
                        coarser[x, y, z] = temp;
                    }
        }

        public Cell this[int x, int y, int z]
        {
            get { return _cells[(x * _y + y) * _z + z]; }
            set { _cells[(x * _y + y) * _z + z] = value; }
        }

        public void GetScales(out float a, out float b, out float j, out float m)
        {
            ScaleProcessing caller = SubGetScales;
            float[] aa = new float[LogicalProcessors];
            float[] bb = new float[LogicalProcessors];
            float[] jj = new float[LogicalProcessors];
            float[] mm = new float[LogicalProcessors];
            for (int i = 0; i < LogicalProcessors; i++)
            {
                int st = (int)(_cells.Length * ((double)i / LogicalProcessors) + 0.5);
                int en = (int)(_cells.Length * ((i + 1.0) / LogicalProcessors) + 0.5);
                _results[i] = caller.BeginInvoke(st, en, out aa[i], out bb[i], out jj[i], out mm[i], null, null);
            }
            for (int i = 0; i < LogicalProcessors; i++)
                caller.EndInvoke(out aa[i], out bb[i], out jj[i], out mm[i], _results[i]);
            a = 0.1f;
            b = 0.1f;
            j = 0.1f;
            m = 1f;
            for (int i = 0; i < LogicalProcessors; i++)
            {
                m = Math.Max(mm[i], m);
                a = Math.Max(aa[i], a);
                b = Math.Max(bb[i], b);
                j = Math.Max(jj[i], j);
            }
        }
        private void SubGetScales(int startX, int endX, out float a, out float b, out float j, out float m)
        {
            a = 0.1f;
            b = 0.1f;
            j = 0.1f;
            m = 1f;
            for (int i = startX; i < endX; i++)
            {
                m = Math.Max(Math.Abs(_cells[i].M), m);
                a = Math.Max(Math.Abs(_cells[i].Ax), a);
                a = Math.Max(Math.Abs(_cells[i].Ay), a);
                a = Math.Max(Math.Abs(_cells[i].Az), a);
                b = Math.Max(Math.Abs(_cells[i].Bx), b);
                b = Math.Max(Math.Abs(_cells[i].By), b);
                b = Math.Max(Math.Abs(_cells[i].Bz), b);
                j = Math.Max(Math.Abs(_cells[i].Jx), j);
                j = Math.Max(Math.Abs(_cells[i].Jy), j);
                j = Math.Max(Math.Abs(_cells[i].Jz), j);
            }
        }
    }
}
// ReSharper restore TooWideLocalVariableScope
