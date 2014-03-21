namespace FiniteDifferenceMethod
{
    enum VariableType
    {
        APotential,
        BMagneticField,
        BAbsoluteValue,
        JCurrent,
        MPermiability
    }

    enum ModelState
    {
        Idleing,
        Interpolating,
        Injectioning,
        Imageing,
        Generating,
        Solving,
        Interrupting,
        Autoscaling,
        Saving,
        Loading
    }

    internal interface IModel
    {
        float Width { get; }
        float Height { get; }
        float Depth { get; }
        void Interpolation();
        void Injection();
        void Solve(double error, int stride, double time = 1);
        IGridImage ShowState();
        void StopExecution();
        Cell GetCell(int x, int y, int z);
        IConverter Converter { get; set; }
        void AutoScaleConverter();
    }

    internal interface IGrid
    {
        int Width { get; }
        int Height { get; }
        int Depth { get; }
        float Step { get; }
        void FastIterateTo(IGrid next);
        double IterateTo(IGrid next);
        void DoPreCalculations();
        void DoBPostCalculations();
        void InterpolateAFrom(IGrid coarser);
        IGrid Coarser();
        Cell this[int x, int y, int z] { get; set; }
        void GetScales(out float a, out float b, out float j, out float m);
    }

    internal interface IGridImage
    {
        bool IsATruncatedByColor { get; }
        bool IsBTruncatedByColor { get; }
        bool IsMTruncatedByColor { get; }
        bool IsJTruncatedByColor { get; }
        IConverter Converter { get; }
        int Width { get; }
        int Height { get; }
        int Depth { get; }
        float Step { get; }
        IGrid ConvertToGrid();
        int[] GetLayerX(VariableType variable, int x);
        void SetLayerX(int[] data, VariableType variable, int x);
        int[] GetLayerY(VariableType variable, int y);
        void SetLayerY(int[] data, VariableType variable, int y);
        int[] GetLayerZ(VariableType variable, int z);
        void SetLayerZ(int[] data, VariableType variable, int z);
        int GetPoint(VariableType variable, int x, int y, int z);
        void Save(string projectName);
    }

    interface IConverter
    {
        string Type { get; }
        IConverter Clone();
        float AScale { get; set; }
        float BScale { get; set; }
        float MScale { get; set; }
        float JScale { get; set; }
        bool Vector2Color(float x, float y, float z, float floatRange, out int color);
        void Color2Vector(int color, out float x, out float y, out float z, float floatRange);
        bool Scalar2Color(float s, float floatRange, out int color);
        bool PositiveScalar2Color(float s, float floatRange, out int color);
        float Color2Scalar(int color, float floatRange);
        float Color2PositiveScalar(int color, float floatRange);
        void Save(string fileName);
    }

    internal interface IView
    {
        event View.SwitchToIdleEventHandler SwitchToIdle;
        VariableType DisplayMode { get; set; }
        int PositionX { get; set; }
        int PositionY { get; set; }
        int PositionZ { get; set; }
        void SetImage(IGridImage image);
        void SetController(Controller controller);
        IGridImage GetImage();
        void Display();
        int GetSizeX();
        int GetSizeY();
        int GetSizeZ();
    }
}

