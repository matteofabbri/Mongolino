using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mongolino
{
    static class Helper
    {
        public static readonly Lazy<char[]> Removable = new Lazy<char[]>(() =>
        {
            var list = new List<char>();
            for (char i = char.MinValue; i < char.MaxValue; i++)
                if (char.IsWhiteSpace(i) || char.IsPunctuation(i) || char.IsSeparator(i))
                    list.Add(i);

            return list.ToArray();
        });

        public static T SanifyHtml<T>(T obj)
        {
            //FIXME: REpalce me with expression tree
            var props = typeof(T).GetProperties().Where(x => x.PropertyType == typeof(string));

            foreach (var item in props)
            {
                var str = item.GetValue(obj);

                if (str != null)
                {
                    item.SetValue(obj,HttpUtility.HtmlEncode(str));
                }
            }
            return obj;
        }
    }
}
