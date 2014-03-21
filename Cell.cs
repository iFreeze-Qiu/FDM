namespace FiniteDifferenceMethod
{
    struct Cell
    {
        public float Ax, Ay, Az;
        public float Jx, Jy, Jz;
        public float M;
        //postcalculated vector
        public float Bx, By, Bz;
        //precalculated shortcuts
        public float GradMx, GradMy, GradMz;
        public float InvertedM;

        public static Cell operator +(Cell c1, Cell c2)
        {
            return new Cell
            {
                Ax = c1.Ax + c2.Ax,
                Ay = c1.Ay + c2.Ay,
                Az = c1.Az + c2.Az,
                Jx = c1.Jx + c2.Jx,
                Jy = c1.Jy + c2.Jy,
                Jz = c1.Jz + c2.Jz,
                M = c1.M + c2.M
            };
        }
        public static Cell operator *(Cell c, float f)
        {
            return new Cell
            {
                Ax = f * c.Ax,
                Ay = f * c.Ay,
                Az = f * c.Az,
                Jx = f * c.Jx,
                Jy = f * c.Jy,
                Jz = f * c.Jz,
                M = f * c.M
            };
        }
        public static Cell operator /(Cell c, float f)
        {
            return new Cell
            {
                Ax = c.Ax / f,
                Ay = c.Ay / f,
                Az = c.Az / f,
                Jx = c.Jx / f,
                Jy = c.Jy / f,
                Jz = c.Jz / f,
                M = c.M / f
            };
        }
        public void CopyConditions(Cell cell)
        {
            Jx = cell.Jx;
            Jy = cell.Jy;
            Jz = cell.Jz;
            M = cell.M;
        }
    }

    struct ImageCell
    {
        public int A;
        public int J;
        public int M;
    }
}

