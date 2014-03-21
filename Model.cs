using System.Collections.Generic;
using System.Diagnostics;

namespace FiniteDifferenceMethod
{
    class Model : IModel
    {
        public IConverter Converter
        {
            get { return _converter; }
            set
            {
                _converter = value;
                _isChanged = true;
            }
        }
        public float Width { get; private set; }
        public float Height { get; private set; }
        public float Depth { get; private set; }

        private readonly Stack<IGrid> _grids;
        private IConverter _converter;
        private GridImage _image;
        private bool _isChanged = true;
        private object _lockObject = new object();
        private bool _stopFlag = false;

        public Model(string projectName)
        {
            _grids = new Stack<IGrid>();
            _converter = new Converter();
            LoadGrid(projectName);
            IGrid firstGrid = _grids.Peek();
            Width = firstGrid.Width * firstGrid.Step;
            Height = firstGrid.Height * firstGrid.Step;
            Depth = firstGrid.Depth * firstGrid.Step;
            AutoScaleConverter();
        }
        public Model(IGrid grid)
        {
            _grids = new Stack<IGrid>();
            _converter = new Converter();
            _grids.Push(grid);
            grid.DoPreCalculations();
            grid.DoBPostCalculations();
            IGrid firstGrid = _grids.Peek();
            Width = firstGrid.Width * firstGrid.Step;
            Height = firstGrid.Height * firstGrid.Step;
            Depth = firstGrid.Depth * firstGrid.Step;
            AutoScaleConverter();
        }

        public void Interpolation()
        {
            if (_grids.Count < 2) return;
            IGrid coarser = _grids.Pop();
            _grids.Peek().InterpolateAFrom(coarser);
            _isChanged = true;
        }
        public void Injection()
        {
            if (_grids.Count == 0) return;
            _grids.Push(_grids.Peek().Coarser());
            _isChanged = true;
        }

        public void Solve(double error, int stride, double adaptiveErrorCheckTime = 1)
        {
            if (_grids.Count == 0) return;
            IGrid gridCurrent = _grids.Pop();
            IGrid gridNext = new Grid(gridCurrent);

            Stopwatch timer = new Stopwatch();
            double lastError = double.PositiveInfinity;
            while (lastError >= error)
            {
                timer.Start();
                for (int i = 0; i < stride; i++)
                {
                    gridCurrent.FastIterateTo(gridNext);
                    SwapGrids(ref gridNext, ref gridCurrent);
                }
                lastError = gridCurrent.IterateTo(gridNext);
                SwapGrids(ref gridNext, ref gridCurrent);
                timer.Stop();
                stride = (int)((1000.0 * stride) * adaptiveErrorCheckTime / timer.ElapsedMilliseconds + 0.9);
                if (stride < 5) stride = 4;
                timer.Reset();
                lock (_lockObject)
                {
                    if (!_stopFlag) continue;
                    _stopFlag = false;
                    break;
                }
            }
            gridCurrent.DoBPostCalculations();
            _grids.Push(gridCurrent);
            _isChanged = true;
        }
        private static void SwapGrids(ref IGrid gridNext, ref IGrid gridCurrent)
        {
            IGrid temp = gridNext;
            gridNext = gridCurrent;
            gridCurrent = temp;
        }

        public IGridImage ShowState()
        {
            if (_grids.Count == 0) return null;
            if (_isChanged)
            {
                _image = new GridImage(_grids.Peek(), _converter);
                _isChanged = false;
            }
            return _image;
        }
        public void StopExecution()
        {
            lock (_lockObject)
            {
                _stopFlag = true;
            }
        }

        public Cell GetCell(int x, int y, int z)
        {
            return _isChanged ? new Cell() : _grids.Peek()[x, y, z];
        }

        public void AutoScaleConverter()
        {
            float a, b, j, m;
            _grids.Peek().GetScales(out a, out b, out j, out m);
            _converter.AScale = a;
            _converter.BScale = b;
            _converter.JScale = j;
            _converter.MScale = m;
            _isChanged = true;
        }

        private void LoadGrid(string projectName)
        {
            Grid grid = new Grid(1, 1, 1, 1);
            //loading grid
            grid.DoPreCalculations();
            _grids.Push(grid);
            _isChanged = true;
        }
    }
}

