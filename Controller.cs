using System.ComponentModel;

namespace FiniteDifferenceMethod
{
    class Controller
    {
        public ModelState ModelState { get; private set; }
        private IModel _model;
        private readonly IView _view;
        private IGridImage _image;
        private readonly BackgroundWorker _initializationWorker, _injectionWorker, _solveWorker;
        private readonly BackgroundWorker _interpolationWorker, _autoscaleWorker, _saveWorker, _loadWorker;
        private double _precision;
        private int _stride;
        private string _fileName;

        public Controller(IView view)
        {
            RunWorkerCompletedEventHandler goToIdleing = delegate
                {
                    if (ModelState != ModelState.Saving)
                    {
                        ModelState = ModelState.Idleing;
                        RefreshView();
                    }
                    else ModelState = ModelState.Idleing;
                };
            _initializationWorker = new BackgroundWorker();
            _injectionWorker = new BackgroundWorker();
            _solveWorker = new BackgroundWorker();
            _interpolationWorker = new BackgroundWorker();
            _autoscaleWorker = new BackgroundWorker();
            _saveWorker = new BackgroundWorker();
            _loadWorker = new BackgroundWorker();
            _view = view;
            _view.SetController(this);
            ModelState = ModelState.Generating;
            _initializationWorker.DoWork += delegate
                {
                    _model = new Model(InputGenerator.GenerateSphereTask(0.3f, 0, 0, 10f, 0.28, 0.10));//TODO: replace with blank grid
                    //_model = new Model(InputGenerator.GenerateWareTask(14f));
                    ModelState = ModelState.Imageing;
                    _image = _model.ShowState();
                };
            _initializationWorker.RunWorkerCompleted += goToIdleing;
            _initializationWorker.RunWorkerAsync();
            _injectionWorker.DoWork += delegate
                {
                    _model.Injection();
                    ModelState = ModelState.Imageing;
                    _image = _model.ShowState();
                };
            _injectionWorker.RunWorkerCompleted += goToIdleing;
            _solveWorker.DoWork += delegate
                {
                    _model.Solve(_precision, _stride);
                    ModelState = ModelState.Imageing;
                    _image = _model.ShowState();
                };
            _solveWorker.RunWorkerCompleted += goToIdleing;
            _interpolationWorker.DoWork += delegate
                {
                    _model.Interpolation();
                    ModelState = ModelState.Imageing;
                    _image = _model.ShowState();
                };
            _interpolationWorker.RunWorkerCompleted += goToIdleing;
            _autoscaleWorker.DoWork += delegate
                {
                    _model.AutoScaleConverter();
                    ModelState = ModelState.Imageing;
                    _image = _model.ShowState();
                };
            _autoscaleWorker.RunWorkerCompleted += goToIdleing;
            _saveWorker.DoWork += delegate
                {
                    _image.Save(_fileName);
                };
            _saveWorker.RunWorkerCompleted += goToIdleing;
            _loadWorker.DoWork += delegate
                {
                 //   TimeGridImage timeGridImage = TimeGridImage.loadGridImages();
                    //_model = new Model(GridImage.LoadFromProject(_fileName).ConvertToGrid());
//                    _model = new Model(timeGridImage.ConvertToGrid());
                   // _model = new Model(TimeGridImage..ConvertToGrid());
                    //ModelState = ModelState.Imageing;
                    //_image = _model.ShowState();
                };
            _loadWorker.RunWorkerCompleted += goToIdleing;
        }

        public bool GetCell(int x, int y, int z, out Cell cell)
        {
            if ((ModelState != ModelState.Idleing)
                && (ModelState != ModelState.Autoscaling)
                && (ModelState != ModelState.Saving))
            {
                cell = new Cell();
                return false;
            }
            cell = _model.GetCell(x, y, z);
            return true;
        }

        public bool DoInjection()
        {
            if (ModelState != ModelState.Idleing) return false;
            ModelState = ModelState.Injectioning;
            _injectionWorker.RunWorkerAsync();
            return true;
        }
        public bool DoSolve(double precision, int stride)
        {
            if (ModelState != ModelState.Idleing) return false;
            _precision = precision;
            _stride = stride;
            ModelState = ModelState.Solving;
            _solveWorker.RunWorkerAsync();
            return true;
        }
        public bool DoInterpolation()
        {
            if (ModelState != ModelState.Idleing) return false;
            ModelState = ModelState.Interpolating;
            _interpolationWorker.RunWorkerAsync();
            return true;
        }
        public bool DoAutoscale()
        {
            if (ModelState != ModelState.Idleing) return false;
            ModelState = ModelState.Autoscaling;
            _autoscaleWorker.RunWorkerAsync();
            return true;
        }
        public bool DoSave(string fileName)
        {
            if (ModelState != ModelState.Idleing) return false;
            ModelState = ModelState.Saving;
            _fileName = fileName;
            _saveWorker.RunWorkerAsync();
            return true;
        }
        public bool DoLoad(string fileName)
        {
            if (ModelState != ModelState.Idleing) return false;
            ModelState = ModelState.Loading;
            _fileName = fileName;
            _loadWorker.RunWorkerAsync();
            return true;
        }
        public bool DoStop()
        {
            if (ModelState != ModelState.Solving) return false;
            ModelState = ModelState.Interrupting;
            _model.StopExecution();
            return true;
        }

        private void RefreshView()
        {
            _view.SetImage(_image);
            _view.Display();
        }
    }
}

