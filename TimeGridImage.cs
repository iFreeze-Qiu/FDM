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
        private static readonly int    IMAGES_IN_MEMORY = 3;

        private static Dictionary<int, GridImage> gridImageDictionary = new Dictionary<int, GridImage>();

        public static void loadGridImages()
        {
            loadGridImages(getIndexesToLoad(0, IMAGES_IN_MEMORY));
        }
        
        private static void loadGridImages(int[] indexes)
        {
            disposeUnmatched(indexes);
            
            string directoryName;
            foreach (int indexer in indexes) {
                directoryName = DEFAULT_DIRECTORY_NAME + Path.DirectorySeparatorChar + String.Format("{0:d4}", indexer);
                if (Directory.Exists(directoryName) && !gridImageDictionary.ContainsKey(indexer))
                {
                    Console.WriteLine("Loading data from " + directoryName);
                    GridImage gridImage = GridImage.LoadFromProject(directoryName);
                    gridImageDictionary.Add(gridImage.getFolderNumber(), gridImage);
                }
            }            
        }

        private static void disposeUnmatched(int[] indexesToLoad)
        {
            List<int> imagesToRemove = new List<int>();
            foreach (KeyValuePair<int, GridImage> entry in gridImageDictionary)
            {
                if (!Array.Exists(
                        indexesToLoad,
                        delegate(int r) { return r == entry.Key; }
                   ))
                {
                    imagesToRemove.Add(entry.Key);
                }
            }

            foreach (int image in imagesToRemove)
            {
                Console.WriteLine("Dispose " + image.ToString());
                gridImageDictionary.Remove(image);
            }
        }


        //Загрузка необходимых элементов. В зависимости от алгоритма можно изменить метод, чтобы увеличить производительность.
        private static int[] getIndexesToLoad(int headIndex, int imageCountInMemory)
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
