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
        public bool IsATruncatedByColor
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsBTruncatedByColor
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsMTruncatedByColor
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsJTruncatedByColor
        {
            get { throw new NotImplementedException(); }
        }

        public IConverter Converter
        {
            get { throw new NotImplementedException(); }
        }

        public int Width
        {
            get { throw new NotImplementedException(); }
        }

        public int Height
        {
            get { throw new NotImplementedException(); }
        }

        public int Depth
        {
            get { throw new NotImplementedException(); }
        }

        public float Step
        {
            get { throw new NotImplementedException(); }
        }

        public IGrid ConvertToGrid()
        {
            throw new NotImplementedException();
        }

        public int[] GetLayerX(VariableType variable, int x)
        {
            throw new NotImplementedException();
        }

        public void SetLayerX(int[] data, VariableType variable, int x)
        {
            throw new NotImplementedException();
        }

        public int[] GetLayerY(VariableType variable, int y)
        {
            throw new NotImplementedException();
        }

        public void SetLayerY(int[] data, VariableType variable, int y)
        {
            throw new NotImplementedException();
        }

        public int[] GetLayerZ(VariableType variable, int z)
        {
            throw new NotImplementedException();
        }

        public void SetLayerZ(int[] data, VariableType variable, int z)
        {
            throw new NotImplementedException();
        }

        public int GetPoint(VariableType variable, int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public void Save(string projectName)
        {
            throw new NotImplementedException();
        }
    }
}