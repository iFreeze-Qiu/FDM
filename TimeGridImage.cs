using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FiniteDifferenceMethod
{
    class TimeGridImage
    {
        private static readonly string DEFAULT_DIRECTORY_NAME = "Data";
        private static double STEP = 0.001;

        private static List<GridImage> gridImageList = new List<GridImage>();
        private static List<double> time = new List<double>();

        public static void loadGridImages()
        {
            int index = 0;
            string directoryName = DEFAULT_DIRECTORY_NAME + Path.DirectorySeparatorChar + index.ToString();

            if (!Directory.Exists(directoryName))
            {
                //TODO: exception
                Console.WriteLine("No time data");
                return;
            }

            while (Directory.Exists(directoryName))
            {
                Console.WriteLine("Loading data from " + directoryName);
                gridImageList.Add(GridImage.LoadFromProject(directoryName));
                
                time.Add(STEP * index);
                
                index++;
                directoryName = DEFAULT_DIRECTORY_NAME + Path.DirectorySeparatorChar + index.ToString();
            }
        }

        public static GridImage getGridImage(int index)
        {
            if (index >= gridImageList.Count)
            {
                //TODO: exception
                Console.WriteLine("Index out of bounds. Size = " + gridImageList.Count.ToString() + " Required = " + index.ToString());
                return null;
            }

            return gridImageList[index];
        }

    }
}
