using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace FiniteDifferenceMethod
{
    class TimeGridImage : IGridImage
    {
        private static readonly string DEFAULT_DIRECTORY_NAME = "Data";
        private static readonly int    IMAGES_IN_MEMORY = 3;

        /*private List<GridImage> gridImageList = new List<GridImage>();
        private List<double> time = new List<double>();
=======*/
        
        //
        private static Dictionary<int, GridImage> gridImageDictionary = new Dictionary<int, GridImage>();

        private GridImage currentGridImage;

/*        private TimeGridImage(List<GridImage> gridImageList, List<double> time)
        {
            this.gridImageList = gridImageList;
            this.time = time;
            currentGridImage = gridImageList[1];
        }*/

/*        public static TimeGridImage loadGridImages()
        {
            loadGridImages(getIndexesToLoad(0, IMAGES_IN_MEMORY));
        }*/
        
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

        public void setCurrentGridImage(double time)
        {
          /*  if (time >= gridImageList.Count)
            {
                //TODO: exception
                Console.WriteLine("Index out of bounds. Size = " + gridImageList.Count.ToString() + " Required = " + time.ToString());
            }

            currentGridImage = gridImageList[timeToIndex(time)];*/
        }

        public GridImage getCurrentGridImage()
        {
            int index = 0;
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

        private int timeToIndex(double time)
        {
            return (int)time;
        }
    }
}
