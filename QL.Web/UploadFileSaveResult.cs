/* ***********************************************
 * Author		:  kingthy
 * Email		:  kingthy@gmail.com
 * Description	:  UploadFileSaveResult
 *
 * ***********************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QL.Web
{
    /// <summary>
    /// 上传文件的保存结果
    /// </summary>
    public enum UploadFileSaveResult
    {   
        /// <summary>
        /// 保存成功
        /// </summary>
        Success = 0,
        /// <summary>
        /// 空上传文件
        /// </summary>
        EmptyFile = 1,
        /// <summary>
        /// 上传的文件大小超出限制
        /// </summary>
        DeniableFileSize = 2,
        /// <summary>
        /// 上传的文件格式不符合
        /// </summary>
        DeniableFileExt = 3,
        /// <summary>
        /// 保存文件时出现错误
        /// </summary>
        Error = 4
    }
}
