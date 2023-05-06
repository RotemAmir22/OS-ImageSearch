using System;
using System.Drawing;
using System.Threading;
public class Program
{
    static void Main(String[] args)
    {
        try
        {
            ImageSearch(args[0], args[1], int.Parse(args[2]), args[3]);
            Console.ReadLine();
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }

    }
    /**
     * image search - gets two images, prints the top left corner of the occurencie of the smaller image in the larger.
     */
    static void ImageSearch(String image1path, String image2path, int n, String algorithm)
    {
        if (image1path == null || image2path == null || n <= 0 || algorithm == null) // validate input
        {
            Console.WriteLine("Invalid input");
            return;
        }
        // check path type
        try
        {
            Bitmap image1 = new Bitmap(image1path);
            Bitmap image2 = new Bitmap(image2path);
            Color[,] pixels_2 = ConvertToColorPixels(image2);
            Color[,] original = ConvertToColorPixels(image1);
            Bitmap[] pixelsDiv = Split(image1, n);
            // algorithm type
            if (algorithm.Equals("exact"))
            {
                Thread[] threads = new Thread[n];
                for (int i = 0; i < n; i++)
                {
                    Color[,] pixels_1 = ConvertToColorPixels(pixelsDiv[i]);
                    threads[i] = new Thread(() => exact(pixels_1, pixels_2, original, i));
                    threads[i].Start();
                }
                for (int i = 0; i < n; i++)
                {
                    threads[i].Join();
                }
            }
            else if (algorithm.Equals("euclidian"))
            {
                Thread[] threads = new Thread[n];
                for (int i = 0; i < n; i++)
                {
                    Color[,] pixels_1 = ConvertToColorPixels(pixelsDiv[i]);
                    threads[i] = new Thread(() => euclidian(pixels_1, pixels_2, original, i));
                    threads[i].Start();
                }
                for (int i = 0; i < n; i++)
                {
                    threads[i].Join();
                }
            }
            else
            {
                Console.WriteLine("Invalid algorithm type");
                return;
            }
        }
        catch
        {
            Console.WriteLine("Wrong type of path");
            return;
        }
        // convert to color pixels

    }

    /**
     * converts the image into a color array
     */
    static Color[,] ConvertToColorPixels(Bitmap image)
    {
        int width = image.Width;
        int height = image.Height;

        Color[,] pixels = new Color[width, height];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Color pixelColor = image.GetPixel(j, i);
                pixels[j, i] = pixelColor;
            }
        }
        return pixels;
    }

    /**
     * splits lthe larger image into n strips
     * each strip is for a different thread
     */
    static Bitmap[] Split(Bitmap image, int n)
    {
        Bitmap[] segments = new Bitmap[n];
        int width = image.Width / n;
        int height = image.Height;

        for (int i = 0; i < n; i++)
        {
            int x = i * width;
            int y = 0;
            Rectangle cropArea = new Rectangle(x, y, width, height);
            Bitmap part = image.Clone(cropArea, image.PixelFormat);
            segments[i] = part;
        }
        return segments;
    }

    /*
     * uses excat algorithm to detemine if the smaller image is a strip of the larger image
     * image1 - strip of the original image
     * image2 - small image to find in the strip
     * original - full original big image
     * diviation - so that the index in the strip matches the itsn index in the original 
     */
    static void exact(Color[,] image1, Color[,] image2, Color[,] original, int diviation)
    {
        for (int i = 0; i < image1.GetLength(0); i++)
        {
            for (int j = 0; j < image1.GetLength(1); j++)
            {
                if (image1[i, j] == image2[0, 0]) // its a match
                {
                    int idxInOrigionalRow = i + (diviation - 1) * image1.GetLength(0);
                    SearchForExact(original, image2, idxInOrigionalRow, j);
                }
            }

        }
    }

    /**
     * gets a starting point and goes over all the pixles of the smaller image and makes sure if they are the same
     * if thay all are identical, printes the top left corner of the smaller image in the large one
     */
    static void SearchForExact(Color[,] originalBig, Color[,] smallImage, int x_Big, int y_Big)
    {
        int idxRows = x_Big - 1;
        int idxCols = y_Big;
        int counter = smallImage.GetLength(0) * smallImage.GetLength(1);
        String ans = "";
        // run on original image
        for (int i = 0; i < smallImage.GetLength(0); i++)
        {
            idxRows++;
            for (int j = 0; j < smallImage.GetLength(1); j++)
            {

                if (idxRows == originalBig.GetLength(0))
                {
                    if (counter == 0) // found the image
                    {
                        Console.WriteLine(ans);
                        return;
                    }
                    return;
                }
                if (idxCols == originalBig.GetLength(1))
                {

                    if (counter == 0) // found the image
                    {
                        Console.WriteLine(ans);
                        return;
                    }
                    idxCols = y_Big;
                }
                if (smallImage[i, j] == originalBig[idxRows, idxCols]) // its a match
                {
                    if (counter == smallImage.GetLength(0) * smallImage.GetLength(1)) // first most left coordinate
                    {
                        ans = idxRows.ToString() + "," + idxCols.ToString();
                    }

                    idxCols++;
                    counter--;
                    if (counter == 0) // found the image
                    {
                        Console.WriteLine(ans);
                        return;
                    }
                }

            }
            idxCols = y_Big;

        }

    }

    /**
     * uses the euclidian method to find a match
     * image1 - strip of the original image
     * image2 - small image to find in the strip
     * original - full original big image
     * diviation - so that the index in the strip matches the itsn index in the original
     */
    static void euclidian(Color[,] image1, Color[,] image2, Color[,] original, int diviation)
    {
        //go over the larger image
        for (int row = 0; row < image1.GetLength(0); row++)
        {
            for (int col = 0; col < image1.GetLength(1); col++)
            {
                // Calculate sum of Euclidean distances between pixels in image1 and image2
                Color color1 = image1[row, col];
                Color color2 = image2[0, 0];
                double distance = Math.Sqrt(
                    Math.Pow(color1.R - color2.R, 2) +
                    Math.Pow(color1.G - color2.G, 2) +
                    Math.Pow(color1.B - color2.B, 2)
                );
                // found first match compare images
                if (distance == 0)
                {
                    //fix index in large image - not strip
                    int idxInOrigionalRow = row + (diviation - 1) * image1.GetLength(0);
                    SearchForEuclidian(original, image2,idxInOrigionalRow, col);
                }
             }
        }
    }

    /**
     * find match of image 2 in image 1 using euclidian sum
     */
    static void SearchForEuclidian(Color[,] originalImage, Color[,] smallImage, int idxRowInOriginal, int idxColInOriginal)
    {
        double distance = 0;
        string ans = idxRowInOriginal.ToString()+","+idxColInOriginal.ToString();
        int idxRows = idxRowInOriginal - 1; // because increments
        int idxCol;
        // run on small image, try to find matches in the large image
        for (int i=0; i<smallImage.GetLength(0); i++)
        {
            idxRows++;
            idxCol = idxColInOriginal;
            for (int j = 0; j < smallImage.GetLength(1); j++,idxCol++)
            {
                //end of large image - rows
                if (idxRows == originalImage.GetLength(0))
                {
                    if (distance == 0)
                    {
                        Console.WriteLine(ans);
                        return;
                    }
                    return;
                }

                //end of large image - col
                if (idxCol == originalImage.GetLength(1))
                {
                    if (distance == 0)
                    {
                        Console.WriteLine(ans);
                        return;
                    }
                    // reset to start point
                    idxCol = idxColInOriginal;
                }

                // calculate distance 
                Color color1 = originalImage[idxRows, idxCol];
                Color color2 = smallImage[i, j];
                distance += Math.Sqrt(
                    Math.Pow(color1.R - color2.R, 2) +
                    Math.Pow(color1.G - color2.G, 2) +
                    Math.Pow(color1.B - color2.B, 2)
                );
                                
            }
        }

        //if the sum is 0 then a match is found
        if(distance == 0 )
            Console.WriteLine(ans);
        
    }
}

