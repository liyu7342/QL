/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  Images
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace QL.Core.Extensions
{
    /// <summary>
    /// 水印的位置
    /// </summary>
    public enum WatermarkPosition
    {
        /// <summary>
        /// 左上角
        /// </summary>
        LeftTop,
        /// <summary>
        /// 上中间
        /// </summary>
        TopMiddle,
        /// <summary>
        /// 右上角
        /// </summary>
        RightTop,
        /// <summary>
        /// 左中间
        /// </summary>
        LeftCenter,
        /// <summary>
        /// 中间
        /// </summary>
        Center,
        /// <summary>
        /// 右中间
        /// </summary>
        RightCenter,
        /// <summary>
        /// 左下角
        /// </summary>
        LeftBottom,
        /// <summary>
        /// 下中间
        /// </summary>
        MiddleBottom,
        /// <summary>
        /// 右下角
        /// </summary>
        RightBottom
    }
    /// <summary>
    /// 对图片的操作
    /// </summary>
    public static class Images
    {
        /// <summary>
        /// 获取水印的位置
        /// </summary>
        /// <returns></returns>
        private static Point GetWatermarkPosition(Image image, Image watermarkImage, WatermarkPosition position)
        {
            switch (position)
            {
                case WatermarkPosition.LeftTop:
                    return new Point(0, 0);
                case WatermarkPosition.TopMiddle:
                    return new Point((image.Width - watermarkImage.Width) / 2, 0);
                case WatermarkPosition.RightTop:
                    return new Point((image.Width - watermarkImage.Width), 0);
                case WatermarkPosition.LeftCenter:
                    return new Point(0, (image.Height - watermarkImage.Height) / 2);
                case WatermarkPosition.Center:
                    return new Point((image.Width - watermarkImage.Width) / 2, (image.Height - watermarkImage.Height) / 2);
                case WatermarkPosition.RightCenter:
                    return new Point((image.Width - watermarkImage.Width), (image.Height - watermarkImage.Height) / 2);
                case WatermarkPosition.LeftBottom:
                    return new Point(0, (image.Height - watermarkImage.Height));
                case WatermarkPosition.MiddleBottom:
                    return new Point((image.Width - watermarkImage.Width) / 2, (image.Height - watermarkImage.Height));
                default:
                    return new Point((image.Width - watermarkImage.Width), (image.Height - watermarkImage.Height));
            }
        }
        /// <summary>
        /// 打水印，如果图片的宽度小于水印图片则不加水印
        /// </summary>
        /// <param name="image">需要打水印的图片</param>
        /// <param name="watermarkImage">水印图片</param>
        /// <param name="transparence">水印图片的透明度，值范围0-1. 0=全透明; 1=不透明</param>
        /// <param name="position">水印的位置</param>
        public static bool Watermark(this Image image, Image watermarkImage, float transparence, WatermarkPosition position)
        {
            return Watermark(image, watermarkImage, transparence, position, false);
        }
        /// <summary>
        /// 打水印
        /// </summary>
        /// <param name="image">需要打水印的图片</param>
        /// <param name="watermarkImage">水印图片</param>
        /// <param name="transparence">水印图片的透明度，值范围0-1. 0=全透明; 1=不透明</param>
        /// <param name="position">水印的位置</param>
        /// <param name="force">是否不管图片的宽度是否小于水印图片都强制打水印</param>
        public static bool Watermark(this Image image, Image watermarkImage, float transparence, WatermarkPosition position, bool force)
        {
            if (!force && (image.Width < watermarkImage.Width || image.Height < watermarkImage.Height)) return false;

            using (Graphics g = Graphics.FromImage(image))
            {
                g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                float[][] ptsArray ={ 
		                        new float[] {1, 0, 0, 0, 0},
		                        new float[] {0, 1, 0, 0, 0},
		                        new float[] {0, 0, 1, 0, 0},
	                            new float[] {0, 0, 0, transparence, 0}, 
	                            new float[] {0, 0, 0, 0, 1}};

                ColorMatrix clrMatrix = new ColorMatrix(ptsArray);
                ImageAttributes imgAttributes = new ImageAttributes();
                imgAttributes.SetColorMatrix(clrMatrix, ColorMatrixFlag.Default,
                ColorAdjustType.Bitmap);

                g.DrawImage(watermarkImage,
                    new Rectangle(GetWatermarkPosition(image, watermarkImage, position), watermarkImage.Size),
                    0, 0, watermarkImage.Width, watermarkImage.Height, GraphicsUnit.Pixel,
                    imgAttributes);
                g.Save();
            }
            return true;
        }
        /// <summary>
        /// 打水印，如果图片的宽度小于水印图片则不加水印
        /// </summary>
        /// <param name="image">需要打水印的图片</param>
        /// <param name="watermarkImage">水印图片</param>
        /// <param name="transparence">水印图片的透明度，值范围0-1. 0=全透明; 1=不透明</param>
        /// <param name="position">水印的位置</param>
        public static bool Watermark(this Image image, string watermarkImage, float transparence, WatermarkPosition position)
        {
            return Watermark(image, watermarkImage, transparence, position, false);
        }
        /// <summary>
        /// 打水印
        /// </summary>
        /// <param name="image">需要打水印的图片</param>
        /// <param name="watermarkImage">水印图片</param>
        /// <param name="transparence">水印图片的透明度，值范围0-1. 0=全透明; 1=不透明</param>
        /// <param name="position">水印的位置</param>
        /// <param name="force">是否不管图片的宽度是否小于水印图片都强制打水印</param>
        /// <returns>是否成功</returns>
        public static bool Watermark(this Image image, string watermarkImage, float transparence, WatermarkPosition position, bool force)
        {
            if (string.IsNullOrEmpty(watermarkImage)) return false;
            watermarkImage = Utility.ToAbsolutePath(watermarkImage);
            if (!File.Exists(watermarkImage)) return false;

            using (Bitmap waterImage = (Bitmap)Bitmap.FromFile(watermarkImage))
            {
                return Watermark(image, waterImage, transparence, position, force);
            }
        }
    }
}
