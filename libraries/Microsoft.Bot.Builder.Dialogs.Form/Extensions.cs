﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public static class Extensions
    {
        public static bool IsPropertyValue(this JToken value, string name) =>
            value.Parent is JProperty jprop && jprop.Name == name;

        public static T Dequeue<T>(this List<T> queue)
        {
            var result = default(T);
            if (queue.Count > 0)
            {
                result = queue[0];
                queue.RemoveAt(0);
            }
            return result;
        }
    }
}