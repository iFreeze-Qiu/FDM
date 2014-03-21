using System;

namespace FiniteDifferenceMethod
{
    static class InputGenerator
    {
        public static IGrid GenerateSphereTask(float bx, float by, float bz, float m, double radius, double boundaryLayer)
        {
            Grid grid = new Grid(150, 150, 150, 0.02f);
            float rx = -(grid.Width - 1) * grid.Step / 2;
            float r0y = -(grid.Height - 1) * grid.Step / 2;
            float r0z = -(grid.Depth - 1) * grid.Step / 2;
            for (int x = 0; x < grid.Width; x++)
            {
                float ry = r0y;
                for (int y = 0; y < grid.Height; y++)
                {
                    float rz = r0z;
                    for (int z = 0; z < grid.Depth; z++)
                    {
                        bool border = (x == 0) || (x == grid.Width - 1) || (y == 0) || (y == grid.Height - 1) || (z == 0) || (z == grid.Depth - 1);
                        Cell temp = new Cell
                            {
                                Ax = !border ? 0 : 0.5f * (by * rz - bz * ry),
                                Ay = !border ? 0 : 0.5f * (bz * rx - bx * rz),
                                Az = !border ? 0 : 0.5f * (bx * ry - by * rx),
                                Jx = 0,
                                Jy = 0,
                                Jz = 0,
                                M = Density(-Math.Sqrt(rx * rx + ry * ry + rz * rz) / r0y, radius, boundaryLayer) * (m - 1) + 1
                            };
                        grid[x, y, z] = temp;
                        rz += grid.Step;
                    }
                    ry += grid.Step;
                }
                rx += grid.Step;
            }
            return grid;
        }
        public static IGrid GenerateWareTask(float j, double radius, double boundaryLayer)
        {
            Grid grid = new Grid(150, 150, 150, 0.02f);
            float rx = -(grid.Width - 1) * grid.Step / 2;
            float r0y = -(grid.Height - 1) * grid.Step / 2;
            for (int x = 0; x < grid.Width; x++)
            {
                float ry = r0y;
                for (int y = 0; y < grid.Height; y++)
                {
                    for (int z = 0; z < grid.Depth; z++)
                    {
                        Cell temp = new Cell
                        {
                            Ax = 0,
                            Ay = 0,
                            Az = 0,
                            Jx = 0,
                            Jy = 0,
                            Jz = j * Density(-Math.Sqrt(rx * rx + ry * ry) / r0y * 3f, radius, boundaryLayer),
                            M = 1
                        };
                        grid[x, y, z] = temp;
                    }
                    ry += grid.Step;
                }
                rx += grid.Step;
            }
            return grid;
        }

        private static float Density(double distance, double radius, double boundaryLayer)
        {
            double f = distance;
            f = 0.5 + (radius - f) / boundaryLayer;
            if (f > 1) f = 1;
            else if (f < 0) f = 0;
            return (float)f;
        }
    }
}

