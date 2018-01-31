using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace FineUIMvc.EmptyProject.Tools
{
    public class Tools
    {

    }

    public static class Base64Tool
    {
        #region base64编码的文本 转为    图片
        public static string Base64StringToImage(string imgData, string mapaddr)
        {
            int i = imgData.IndexOf(',');

            imgData = imgData.Substring(i + 1);

            //过滤特殊字符即可   
            string dummyData = imgData.Trim().Replace("%", "").Replace(",", "").Replace(" ", "+");
            if (dummyData.Length % 4 > 0)
            {
                dummyData = dummyData.PadRight(dummyData.Length + 4 - dummyData.Length % 4, '=');
            }

            byte[] imageBytes = Convert.FromBase64String(dummyData);
            //转成图片
            string message = "";

            DateTime now = DateTime.Now;
            var filename = now.ToString("yyyy-MM-dd-HH-mm-ss") + new Random().Next(1, 999).ToString();

            File.WriteAllBytes(mapaddr + "UploadFile" + $@"\{filename}.jpg", imageBytes);

            message = "UploadFile" + $@"\{ filename}.jpg";

            return message;
        }
        #endregion
    }

    public class FastPropertyComparer<T> : IEqualityComparer<T>
    {
        private Func<T, Object> getPropertyValueFunc = null;


        # region 通过propertyName 获取PropertyInfo对象
        public FastPropertyComparer(string propertyName)
        {
            PropertyInfo _PropertyInfo = typeof(T).GetProperty(propertyName,
            BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);
            if (_PropertyInfo == null)
            {
                throw new ArgumentException(string.Format("{0} is not a property of type {1}.",
                    propertyName, typeof(T)));
            }

            ParameterExpression expPara = Expression.Parameter(typeof(T), "obj");
            MemberExpression me = Expression.Property(expPara, _PropertyInfo);
            getPropertyValueFunc = Expression.Lambda<Func<T, object>>(me, expPara).Compile();
        }
        #endregion

        #region IEqualityComparer<T> Members

        public bool Equals(T x, T y)
        {
            object xValue = getPropertyValueFunc(x);
            object yValue = getPropertyValueFunc(y);

            if (xValue == null)
                return yValue == null;

            return xValue.Equals(yValue);
        }

        public int GetHashCode(T obj)
        {
            object propertyValue = getPropertyValueFunc(obj);

            if (propertyValue == null)
                return 0;
            else
                return propertyValue.GetHashCode();
        }

        #endregion
    }

    public static class PagingHelper<T>
    {
        public static List<T> GetPagedDataTable(int pageIndex, int pageSize, int recordCount, List<T> list)
        {
            //// 对传入的 pageIndex 进行有效性验证//////////////
            int pageCount = recordCount / pageSize;
            if (recordCount % pageSize != 0)
            {
                pageCount++;
            }
            if (pageIndex > pageCount - 1)
            {
                pageIndex = pageCount - 1;
            }
            if (pageIndex < 0)
            {
                pageIndex = 0;
            }
            ///////////////////////////////////////////////

            List<T> retList = new List<T>();

            int rowbegin = pageIndex * pageSize;
            int rowend = (pageIndex + 1) * pageSize;
            if (rowend > list.Count)
            {
                rowend = list.Count;
            }

            retList = list.Skip(pageIndex * pageSize).Take(pageSize).ToList();

            return retList;
        }
    }
}