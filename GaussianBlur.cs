using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace View
{
    public static class GaussianBlur
    {
        /// <summary>
        /// 高斯权重矩阵-卷积核
        /// 在更改半径值时重新计算即可
        /// </summary>
        //private double[,] gaussianWeightMatrix;
        
        /// <summary>
        /// 高斯模糊计算--请在后台进程中调用此函数，避免窗体无响应
        /// <para>运作方式就是和对应半径的权重矩阵做卷积</para>
        /// </summary>
        /// <param name="image"></param>
        /// <param name="R">模糊半径</param>
        /// <returns></returns>
        public static Image Gaussianblur(Image image, int R)
        {
            double[,] gaussianWeightMatrix = WeightMatrix(R);//获得卷积核
            int maxSize = (2 * R + 1) * (2 * R + 1);//矩阵的总个数
            Color[,] imageMatrix = new Color[image.Width + 2 * R, image.Height + 2 * R];
            //将图像矩阵增大相应半径的单位，避免在计算卷积核时边界出现的问题
            Color[,] imageMatrixNew = new Color[image.Width, image.Height];

            //将整个颜色矩阵填充满，被增大的地方用对应位置替换
            for (int y = 0; y < image.Height + 2 * R; y++)
            {
                for (int x = 0; x < image.Width + 2 * R; x++)
                {
                    //四个边角的填充
                    if (x < R && y < R)
                    {
                        imageMatrix[x, y] = ((Bitmap)image).GetPixel((x + 2 * (R - x)) - R, (y + 2 * (R - y)) - R);
                    }
                    else if (x >= image.Width + R && y < R)
                    {
                        imageMatrix[x, y] = ((Bitmap)image).GetPixel((x - 2 * (x - image.Width)), (y + 2 * (R - y)) - R);
                    }
                    else if (x < R && y >= image.Height + R)
                    {
                        imageMatrix[x, y] = ((Bitmap)image).GetPixel((x + 2 * (R - x)) - R, (y - 2 * (y - image.Height)));
                    }
                    else if (x >= image.Width + R && y >= image.Height + R)
                    {
                        imageMatrix[x, y] = ((Bitmap)image).GetPixel((x - 2 * (x - image.Width)), (y - 2 * (y - image.Height)));
                    }
                    //四条对边的填充
                    else if (x < R && y >= R && y < image.Height + R)
                    {
                        imageMatrix[x, y] = ((Bitmap)image).GetPixel((x + 2 * (R - x)) - R, y - R);
                    }
                    else if (x >= image.Width + R && y >= R && y < image.Height + R)
                    {
                        imageMatrix[x, y] = ((Bitmap)image).GetPixel((x - 2 * (x - image.Width)) + R - 1, y - R);
                    }
                    else if (y < R && x >= R && x < image.Width + R)
                    {
                        imageMatrix[x, y] = ((Bitmap)image).GetPixel(x - R, (y + 2 * (R - y)) - R);
                    }
                    else if (y >= image.Height + R && x >= R && x < image.Width + R)
                    {
                        imageMatrix[x, y] = ((Bitmap)image).GetPixel(x - R, (y - 2 * (y - image.Height)) + R - 1);
                    }
                    //剩下的原图部分
                    else
                    {
                        imageMatrix[x, y] = ((Bitmap)image).GetPixel(x - R, y - R);
                    }
                }
            }

            #region 检验图像边角是否被正确对称填充
            //for (int y = 0; y < image.Height + 2 * R; y++)
            //{
            //    for (int x = 0; x < image.Width + 2 * R; x++)
            //    {
            //        ((Bitmap)tempImage).SetPixel(x, y, imageMatrix[x, y]);
            //    }
            //}
            //return tempImage;
            #endregion
            
            //计算卷积过程
            for (int y = R; y < image.Height + R; y++)
            {
                for (int x = R; x < image.Width + R; x++)
                {
                    int tempx = 0, tempy = 0;
                    double r = 0, g = 0, b = 0;
                    for (int j = y - R; j <= y + R; j++)//调试的时候这里因为没有<=,而是<---导致半径较小的时候，图像明显变暗，甚至是变黑。因为没有卷积过程没有进行完成，所以在权重矩阵较小时问题就会暴露出来
                    {
                        tempx = 0;
                        for (int i = x - R; i <= x + R; i++)
                        {
                            r += (imageMatrix[i, j].R * gaussianWeightMatrix[tempx, tempy]);
                            g += (imageMatrix[i, j].G * gaussianWeightMatrix[tempx, tempy]);
                            b += (imageMatrix[i, j].B * gaussianWeightMatrix[tempx, tempy]);
                            tempx += 1;
                        }
                        tempy += 1;
                    }
                    if (r > 255) r = 255;
                    if (r < 0) r = 0;
                    if (g > 255) g = 255;
                    if (g < 0) g = 0;
                    if (b > 255) b = 255;
                    if (b < 0) b = 0;
                    imageMatrixNew[x - R, y - R] = Color.FromArgb((int)r, (int)g, (int)b);
                }
            }
            
            Image tempImage = new Bitmap(image.Width, image.Height);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    ((Bitmap)tempImage).SetPixel(x, y, imageMatrixNew[x, y]);
                }
            }
            return tempImage;
        }

        #region 根据高斯分布计算权重矩阵-卷积核
        /// <summary>
        /// 计算指定半径并以(0,0)为原点的权重矩阵
        /// <para>此权重矩阵相当于对应半径的'高斯滤波器'</para>
        /// </summary>
        /// <param name="R">权重矩阵的半径</param>
        /// <returns></returns>
        private static double[,] WeightMatrix(int R)
        {
            double sigma = (2 * R + 1) / 2.0;//σ-标准差--近似计算---(2 * R + 1) / 2.0
            int maxSize = (2 * R + 1) * (2 * R + 1);//矩阵的总个数
            double[,] weightmatrix = new double[2 * R + 1, 2 * R + 1];

            for (int y = 0; y < 2 * R + 1; y++)
            {
                for (int x = 0; x < 2 * R + 1; x++)
                {
                    weightmatrix[x, y] = WeightMatrixPoint(x - R, y - R, sigma);
                }
            }
            double temp = 0;
            for (int y = 0; y < 2 * R + 1; y++)
            {
                for (int x = 0; x < 2 * R + 1; x++)
                {
                    temp += weightmatrix[x, y];
                }
            }
            for (int y = 0; y < 2 * R + 1; y++)
            {
                for (int x = 0; x < 2 * R + 1; x++)
                {
                    weightmatrix[x, y] /= temp;
                }
            }
            #region 检验权重之和知否等于1
            //temp = 0;
            //for (int y = 0; y < 2 * R + 1; y++)
            //{
            //    for (int x = 0; x < 2 * R + 1; x++)
            //    {
            //        temp += weightmatrix[x, y];
            //    }
            //}
            #endregion
            return weightmatrix;
        }

        /// <summary>
        /// 计算权重矩阵某点的权重
        /// </summary>
        /// <param name="x">该点的x坐标</param>
        /// <param name="y">该点的y坐标</param>
        /// <param name="sigma">指定一个西格玛的值 σ</param>
        /// <returns></returns>
        private static double WeightMatrixPoint(int x, int y, double sigma)
        {
            double tempOneNumber = 1 / (2 * Math.PI * sigma * sigma);
            double tempTowNumber = Math.Pow(Math.E, (-(x * x + y * y) / (2 * sigma * sigma)));
            return tempOneNumber * tempTowNumber;
        }
        #endregion
    }
}
