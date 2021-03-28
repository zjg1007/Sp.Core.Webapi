using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Linq;
using Blog.Core.Common;

namespace Blog.Core
{
    public static class LinqHelper
    {
        /// <summary>
        /// 利用反射把一个集合的列赋值到另外一个集合相同属性名
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="list"></param>
        /// <param name="Titem"></param>
        /// <returns></returns>
        /// <summary>
        /// 利用反射把一个集合的列赋值到另外一个集合相同属性名  2020-02-28_zjg
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="list"></param>
        /// <param name="Titem"></param>
        /// <returns></returns>
        public static List<TEntity> SelectByItem<T, TEntity>(this List<T> list, params Expression<Func<T, object>>[] Titem)
            where TEntity : new()
            where T : new()
        {
            //批量处理传过来的参数属性名称
            IEnumerable<string> result = Titem.Select(m => (m.Body.ToString().Split(",")[0].Substring(m.Body.ToString().LastIndexOf(".") + 1)));
                
            TEntity Tmodel;
            List<TEntity> ListModel = new List<TEntity>();
            Type entityType = typeof(TEntity);
            Type Ttype = typeof(T);
            for (int i = 0; i < list.Count; i++)
            {
                //需要重新创建对象否则会覆盖前面对象的值
                Tmodel = new TEntity();
                foreach (var item in result)
                {
                    entityType.GetProperty(item).SetValue(Tmodel, Ttype.GetProperty(item).GetValue(list[i]));
                }
                ListModel.Add(Tmodel);
            }
            return ListModel;
        }
       
    }
}
