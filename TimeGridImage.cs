using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FiniteDifferenceMethod
{
    class TimeGridImage : IGridImage
    {
        private static readonly string DEFAULT_DIRECTORY_NAME = "Data";
        private static double STEP = 0.001;

        private List<GridImage> gridImageList = new List<GridImage>();
        private List<double> time = new List<double>();

        private GridImage currentGridImage;

        private TimeGridImage(List<GridImage> gridImageList, List<double> time)
        {
            this.gridImageList = gridImageList;
            this.time = time;
            currentGridImage = gridImageList[1];
        }

        public static TimeGridImage loadGridImages()
        {
            int index = 0;
            string directoryName = DEFAULT_DIRECTORY_NAME + Path.DirectorySeparatorChar + index.ToString();
            List<GridImage> gridImageList = new List<GridImage>();
            List<double> time = new List<double>();

            if (!Directory.Exists(directoryName))
            {
                //TODO: exception
                Console.WriteLine("No time data");
                return null;
            }

            while (Directory.Exists(directoryName))
            {
                Console.WriteLine("Loading data from " + directoryName);
                gridImageList.Add(GridImage.LoadFromProject(directoryName));

                time.Add(STEP * index);

                index++;
                directoryName = DEFAULT_DIRECTORY_NAME + Path.DirectorySeparatorChar + index.ToString();
            }
            return new TimeGridImage(gridImageList, time);
        }

        public GridImage getGridImage(int index)
        {
            if (index >= gridImageList.Count)
            {
                //TODO: exception
                Console.WriteLine("Index out of bounds. Size = " + gridImageList.Count.ToString() + " Required = " + index.ToString());
                return null;
            }

            return gridImageList[index];
        }

        //------------------------------------ ЗАПОЛНИТЬ!---------------------

        public bool IsATruncatedByColor { get; private set; }
        public bool IsBTruncatedByColor { get; private set; }
        public bool IsMTruncatedByColor { get; private set; }
        public bool IsJTruncatedByColor { get; private set; }


        public IConverter Converter { get; private set; }


        public int Width { get { return currentGridImage.Width; } }
        public int Height { get { return currentGridImage.Height; } }
        public int Depth { get { return currentGridImage.Depth; } }
        public float Step { get { return currentGridImage.Step; } }

        public IGrid ConvertToGrid()
        {
            return currentGridImage.ConvertToGrid();
        }

        public int[] GetLayerX(VariableType variable, int x)
        {
            return currentGridImage.GetLayerX(variable, x);
        }

        public void SetLayerX(int[] data, VariableType variable, int x)
        {
            currentGridImage.SetLayerX(data, variable, x);
        }

        public int[] GetLayerY(VariableType variable, int y)
        {
            return currentGridImage.GetLayerY(variable, y);
        }

        public void SetLayerY(int[] data, VariableType variable, int y)
        {
            currentGridImage.SetLayerY(data, variable, y);
        }

        public int[] GetLayerZ(VariableType variable, int z)
        {
            return currentGridImage.GetLayerZ(variable, z);
        }

        public void SetLayerZ(int[] data, VariableType variable, int z)
        {
            currentGridImage.SetLayerZ(data, variable, z);
        }

        public int GetPoint(VariableType variable, int x, int y, int z)
        {
            return currentGridImage.GetPoint(variable, x, y, z);
        }

        public void Save(string projectName)
        {
            currentGridImage.Save(projectName);
        }
    }
}