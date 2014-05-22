using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace FiniteDifferenceMethod
{
    class TimeGridImage
    {
        private static readonly string DEFAULT_DIRECTORY_NAME = "Data";
        public static readonly int    IMAGES_IN_MEMORY = 3;

        private static Dictionary<int, GridImage> gridImageDictionary = new Dictionary<int, GridImage>();

        public static void loadGridImages(int[] indexes)
        {
            string directoryName;
            foreach (int indexer in indexes) {
                directoryName = DEFAULT_DIRECTORY_NAME + Path.DirectorySeparatorChar + String.Format("{0:d4}", indexer);
                if (Directory.Exists(directoryName))
                {
                    Console.WriteLine("Loading data from " + directoryName);
                    GridImage gridImage = GridImage.LoadFromProject(directoryName);
                    gridImageDictionary.Add(gridImage.getFolderNumber(), gridImage);
                }
                else
                {
                    Console.WriteLine("No such directory: " + directoryName);
                }
            }
            
            
        }

        public static int[] getIndexesToLoad(int headIndex, int imageCountInMemory)
        {
            int[] indexes = new int[imageCountInMemory];

            for (int i = 0; i < imageCountInMemory; i++)
            {
                indexes[i] = headIndex - imageCountInMemory / 2 + i;
            }

            return indexes;
        }

        public static GridImage getGridImage(int index)
        {
            if (!gridImageDictionary.ContainsKey(index))
            {
                gridImageDictionary.Clear();
                loadGridImages(getIndexesToLoad(index, IMAGES_IN_MEMORY));
                if (!gridImageDictionary.ContainsKey(index))
                {
                    return null;
                }
            }

            return gridImageDictionary[index];
        }

    }
}
