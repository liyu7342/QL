/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  CSVTextReader
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using QL.Core.Extensions;
namespace QL.Core.Data
{
    /// <summary>
    /// CSV数据读取器
    /// </summary>
    /// <example>
    /// <code>
    /// ///读取每行数据
    /// using(var reader = new CSVTextReader(@"c:\data.csv", Encoding.UTF8)){
    ///     while(!reader.EOS){
    ///         Console.WriteLine(string.Join(",",reader.ReadRow()));
    ///     }
    /// }
    /// 
    /// ///读取整个文件数据到某个DataTable
    /// using(var reader = new CSVTextReader(@"c:\data.csv", Encoding.UTF8)){
    ///     var table = reader.ReadAll();
    ///     Console.WriteLine(table.Columns.Count);
    ///     Console.WriteLine(table.Rows.Count);
    /// } 
    /// </code>
    /// </example>
    public class CSVTextReader : IDisposable
    {
        /// <summary>
        /// 采用逗号","做为分隔符，并根据CSV格式文本内容实例化
        /// </summary>
        /// <param name="text"></param>
        public CSVTextReader(string text)
            : this(text, ',')
        {
        }
        /// <summary>
        /// 根据CSV格式文本内容实例化
        /// </summary>
        /// <param name="text"></param>
        /// <param name="separator">分隔符</param>
        public CSVTextReader(string text, char separator)
        {
            this.Parse(new StringReader(text), separator);
        }
        /// <summary>
        /// 采用逗号","做为分隔符，并根据文件地址实例化
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="charset"></param>
        public CSVTextReader(string fileName, Encoding charset)
            : this(fileName, charset, ',')
        {
        }
        /// <summary>
        /// 根据文件地址实例化
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="charset"></param>
        /// <param name="separator">分隔符</param>
        public CSVTextReader(string fileName, Encoding charset, char separator)
        {
            this.Parse(new StreamReader(fileName, charset), separator);
        }

        /// <summary>
        /// 采用逗号","做为分隔符，并根据TextReader实例化
        /// </summary>
        /// <param name="reader"></param>
        public CSVTextReader(TextReader reader)
            : this(reader, ',')
        {
        }
        /// <summary>
        /// 根据TextReader实例化
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="separator">分隔符</param>
        public CSVTextReader(TextReader reader, char separator)
        {
            this.Parse(reader, separator);
        }

        /// <summary>
        /// 解析CSV内容
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="separator"></param>
        private void Parse(TextReader reader, char separator)
        {
            this.DataReader = reader;
            this.Separator = separator;
        }

        /// <summary>
        /// 数据流是否已读取结束
        /// </summary>
        /// <returns></returns>
        public bool EOS
        {
            get
            {
                if (this.DataReader == null) return true;
                return this.DataReader.Peek() == -1;
            }
        }
        /// <summary>
        /// 是否自动去除每项值的前后空白字符。
        /// </summary>
        public bool AutoTrim { get; set; }

        /// <summary>
        /// 分隔符，默认是逗号
        /// </summary>
        public char Separator { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        private TextReader DataReader;

        /// <summary>
        /// 读取一行数据并返回。如果已读取到流末尾，则返回null
        /// </summary>
        /// <returns></returns>
        public string[] ReadRow()
        {
            List<string> items = new List<string>();
            bool breakLine = false;
            while (!this.EOS && !breakLine)
            {
                int c = this.DataReader.Read();
                bool dq = false;
                if (c == '"')
                {
                    //分隔符
                    dq = true;
                    c = this.DataReader.Read();
                }
                bool inDQ = dq;
                List<char> buffer = new List<char>();
                do
                {
                    if (!dq)
                    {
                        if (c == this.Separator)
                        {
                            //分隔符后是换行符则跳过
                            if (this.DataReader.Peek() == '\r')
                            {
                                breakLine = true; //已换行
                                this.DataReader.Read();
                            }
                            if (this.DataReader.Peek() == '\n')
                            {
                                breakLine = true; //已换行
                                this.DataReader.Read();
                            }
                            break;  //已找到不在双引号内的分隔符则表示找到一列数据
                        }
                        if (c == '\r')
                        {
                            if (this.DataReader.Peek() == '\n') this.DataReader.Read();
                            breakLine = true; //已换行
                            break;
                        }
                        else if (c == '\n')
                        {
                            breakLine = true; //已换行
                            break;
                        }
                    }
                    if (dq && c == '"')
                    {
                        if (this.DataReader.Peek() == '"')
                        {
                            //两个双引号同时出现.则是一个"双引号"字符
                            buffer.Add((char)c);
                            this.DataReader.Read();
                        }
                        else
                        {
                            //找到配对的双引号
                            dq = false;
                        }
                    }
                    else
                    {
                        buffer.Add((char)c);
                    }
                } while ((c = this.DataReader.Read()) != -1);
                if (buffer.Count == 0)
                {
                    items.Add(string.Empty);
                }
                else
                {
                    string value = new string(buffer.ToArray());
                    if (this.AutoTrim) value = value.Trim();  //如果值不包含在双引号内则去除前后空白字符
                    items.Add(value);
                }
            }
            return (items.Count == 0 && this.EOS) ? null : items.ToArray();
        }

        /// <summary>
        /// 读取所有数据并返回为System.Data.DataTable结构数据，其中将把第一行数据作为表格头看待
        /// </summary>
        /// <returns></returns>
        public DataTable ReadAll()
        {
            if (this.EOS) return null;
            string[] column = this.ReadRow();
            DataTable table = new DataTable();
            foreach (string k in column) table.Columns.Add(k, typeof(string));

            while (!this.EOS)
            {
                string[] data = this.ReadRow();
                if (data.Length > 0)
                {
                    var row = table.NewRow();
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        row[i] = (i < data.Length ? data[i] : string.Empty);
                    }
                    table.Rows.Add(row);
                }
            }
            return table;
        }

        /// <summary>
        /// 将csv文本解析为一个数据表
        /// </summary>
        /// <param name="csvText"></param>
        /// <returns></returns>
        public static DataTable ReadAll(string csvText)
        {
            using (CSVTextReader reader = new CSVTextReader(csvText))
            {
                return reader.ReadAll();
            }
        }

        /// <summary>
        /// 将csv文本文件解析为一个数据表
        /// </summary>
        /// <param name="csvFile"></param>
        /// <param name="charset"></param>
        /// <returns></returns>
        public static DataTable ReadAll(string csvFile, Encoding charset)
        {
            using (CSVTextReader reader = new CSVTextReader(csvFile, charset))
            {
                return reader.ReadAll();
            }
        }

        #region IDisposable 成员
        /// <summary>
        /// 释放内存资源
        /// </summary>
        public void Dispose()
        {
            if (this.DataReader != null)
            {
                this.DataReader.Close();
                this.DataReader.Dispose();
                this.DataReader = null;
            }
        }

        #endregion
    }
}
