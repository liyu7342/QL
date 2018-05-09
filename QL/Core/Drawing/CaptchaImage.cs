using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace QL.Core.Drawing
{
    /// <summary>
    /// 验证码图片类
    /// </summary>
    public class CaptchaImage
    {
        /// <summary>
        /// 构造默认实例
        /// </summary>
        public CaptchaImage()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public CaptchaImage(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        #region 设置图片的大小
        private int m_width = 0;

        /// <summary>
        /// 返回图片的宽度
        /// </summary>
        public int Width
        {
            get
            {
                return m_width;
            }
            set
            {
                m_width = value;
            }
        }

        private int m_height = 0;
        /// <summary>
        /// 设置或返回图片的高度
        /// </summary>
        public int Height
        {
            get
            {
                return m_height;
            }
            set
            {
                m_height = value;
            }
        }
        #endregion

        #region 画布
        private Bitmap picture;
        #endregion

        #region 保存图片
        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="stream">数据流</param>
        public void Save(Stream stream)
        {
            Save(stream, ImageFormat.Gif);
        }
        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="stream">数据流</param>
        /// <param name="format">图片格式</param>
        public void Save(Stream stream, ImageFormat format)
        {
            if (picture == null) return;

            picture.Save(stream, format);
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="filename">保存的文件名</param>
        public void Save(string filename)
        {
            Save(filename, ImageFormat.Gif);
        }
        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="filename">保存的文件名</param>
        /// <param name="format">图片格式</param>
        public void Save(string filename, ImageFormat format)
        {
            if (picture == null) return;

            picture.Save(filename, format);
        }
        #endregion

        #region 释放内容资源
        /// <summary>
        /// 释放内容资源
        /// </summary>
        public void Dispose()
        {
            if (picture == null) return;

            picture.Dispose();
            picture = null;
        }
        #endregion

        #region 设置字体
        private System.Drawing.Font m_font = new System.Drawing.Font(new FontFamily("Arial"), 12, FontStyle.Bold);
        /// <summary>
        /// 设置或返回字体
        /// </summary>
        public System.Drawing.Font Font
        {
            get
            {
                return m_font;
            }
            set
            {
                m_font = value;
            }
        }
        #endregion

        #region 设置颜色
        private Color m_foreColor = Color.Black;
        /// <summary>
        /// 设置或返回前景色
        /// </summary>
        public Color ForeColor
        {
            get
            {
                return m_foreColor;
            }
            set
            {
                m_foreColor = value;
            }
        }

        private Color m_backgroundColor = Color.White;
        /// <summary>
        /// 设置或返回背景色
        /// </summary>
        public Color BackgroundColor
        {
            get
            {
                return m_backgroundColor;
            }
            set
            {
                m_backgroundColor = value;
            }
        }
        #endregion

        #region 作图
        private string m_RndCodeChars = null;
        /// <summary>
        /// 返回或设置随机码的字符列,如"0123456789"
        /// </summary>
        public string RndCodeChars
        {
            get
            {
                return m_RndCodeChars;
            }
            set
            {
                m_RndCodeChars = value;
            }
        }
        /// <summary>
        /// 获取随机的字符串
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        private string GetRndCode(int len)
        {
            string code = this.m_RndCodeChars;
            //默认的值
            if (code == null || code.Length < 2) code = "45679FHJKLPTUVXY";

            return Utility.CreateRndCode(code, len);
        }

        /// <summary>
        /// 设置前景颜色为随机颜色
        /// </summary>
        public void SetRndForeColor()
        {
            //文字颜色
            Color[] rndColor = new Color[] {Color.Red,Color.Green,Color.Gray,Color.Blue,Color.Black,
											Color.Purple,Color.DarkKhaki,Color.Orange,Color.LimeGreen,
											Color.SandyBrown,Color.Silver
										   };

            System.Random random = new Random(unchecked((int)DateTime.Now.Ticks));

            this.m_foreColor = rndColor[random.Next(0, rndColor.Length - 1)];
        }

        /// <summary>
        /// 画验证码
        /// </summary>
        /// <returns>返回当前的验证码图片</returns>
        public string Draw(int len)
        {
            string code = GetRndCode(len);

            //计算文字占用的大小
            GetTextWidth(code);

            //实例化图像
            picture = new System.Drawing.Bitmap(this.m_width, this.m_height);

            Graphics g = Graphics.FromImage(picture);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            // 画背景
            DrawBackground(g);

            // 画干扰线
            DrawDisturbLine(g);

            // 画字符
            DrawText(g, code);

            g.Save();
            g.Dispose();

            return code;
        }

        /// <summary>
        /// 获取文字的宽度
        /// </summary>
        /// <param name="text"></param>
        private void GetTextWidth(string text)
        {
            //用户已设置大小则不再处理
            if (this.m_width > 1 && this.m_height > 1) return;

            //计算文字的宽度
            Graphics tg = Graphics.FromImage(new Bitmap(100, 100));
            SizeF size = tg.MeasureString(text, this.Font);
            tg.Dispose();

            if (this.m_width < 1) this.m_width = (int)size.Width + 2;		//加上前后的两个象素点
            if (this.m_height < 1) this.m_height = (int)size.Height + 2;		//加上上下的两个象素点
        }
        /// <summary>
        /// 填充背景颜色
        /// </summary>
        /// <param name="g"></param>
        private void DrawBackground(Graphics g)
        {
            g.FillRectangle(new System.Drawing.SolidBrush(this.m_backgroundColor), 0, 0, this.m_width, this.m_height);
        }

        /// <summary>
        /// 画文本
        /// </summary>
        /// <param name="g"></param>
        /// <param name="text"></param>
        private void DrawText(Graphics g, string text)
        {
            //设置字符的摆放位置
            StringFormat f = new StringFormat();
            f.Alignment = StringAlignment.Center;
            f.LineAlignment = StringAlignment.Center;

            //画字符
            g.DrawString(text, this.m_font, new SolidBrush(this.m_foreColor), this.m_width / 2, this.m_height / 2, f);
        }

        /// <summary>
        /// 画干扰线
        /// </summary>
        private void DrawDisturbLine(Graphics g)
        {
            //初始化随机数
            System.Random random = new Random(unchecked((int)DateTime.Now.Ticks));

            Pen blackPen = new Pen(this.m_foreColor, 1);
            PointF start = new PointF(random.Next(1, 3), random.Next(1, 20));
            PointF control1 = new PointF(random.Next(7, 13), random.Next(1, 20));
            PointF control2 = new PointF(random.Next(15, 22), random.Next(1, 20));
            PointF end1 = new PointF(random.Next((this.m_width / 2) - 3, (this.m_width / 2) + 3), random.Next(1, 20));
            PointF control3 = new PointF(random.Next(30, 42), random.Next(1, 20));
            PointF control4 = new PointF(random.Next(47, 50), random.Next(1, 20));
            PointF end2 = new PointF(random.Next(this.m_width - 3, this.m_width), random.Next(1, 20));
            PointF[] bezierPoints1 =
			{
				start, control1, control2, end1,
				control3, control4, end2
			};
            g.DrawBeziers(blackPen, bezierPoints1);

            int MaxX = 40;
            int TempLineX1 = 0, TempLineX2 = 0;

            TempLineX1 = random.Next(1, this.m_width - MaxX); TempLineX2 = random.Next(TempLineX1, this.m_width);
            if ((TempLineX2 - TempLineX1) > MaxX) TempLineX2 = TempLineX1 + 40;
            g.DrawLine(blackPen, new PointF(TempLineX1, random.Next(1, 20)), new PointF(TempLineX2, random.Next(1, 20)));
        }
        #endregion
    }
}
