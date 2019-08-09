﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public class DialogSchema
    {
        public DialogSchema(JObject schema)
        {
            Schema = schema;
            Property = CreateProperty(Schema);
        }

        public JObject Schema { get; }

        public PropertySchema Property { get; }

        public void Analyze(Func<string, JToken, bool> analyzer)
        {
            Analyze(string.Empty, Schema, analyzer);
        }

        private PropertySchema CreateProperty(JObject schema, string path = "")
        {
            var type = schema["type"].Value<string>();
            var children = new List<PropertySchema>();
            if (type == "array")
            {
                path += "[]";
                var items = schema["items"];
                if (items != null)
                {
                    if (items is JObject itemsSchema)
                    {
                        schema = itemsSchema;
                        type = schema["type"].Value<string>();
                    }
                    else
                    {
                        throw new ArgumentException($"{path} has an items array which is not supported");
                    }
                }
            }

            if (type == "object")
            {
                foreach (JProperty prop in schema["properties"])
                {
                    if (!prop.Name.StartsWith("$"))
                    {
                        var newPath = path == string.Empty ? prop.Name : $"{path}.{prop.Name}";
                        children.Add(CreateProperty((JObject)prop.Value, newPath));
                    }
                }
            }

            return new PropertySchema(path, schema, children);
        }

        private bool Analyze(string path, JToken token, Func<string, JToken, bool> analyzer)
        {
            bool stop = false;
            if (token is JObject jobj)
            {
                if (!(stop = analyzer(path, token)))
                {
                    foreach (var prop in jobj.Properties())
                    {
                        var parent = prop.Parent;
                        var grand = parent?.Parent;
                        var newPath = path;
                        if (grand != null && grand is JProperty grandProp && grandProp.Name == "properties")
                        {
                            newPath = path == string.Empty ? prop.Name : $"{path}.{prop.Name}";
                        }

                        if (stop = Analyze(newPath, prop.Value, analyzer))
                        {
                            break;
                        }
                    }
                }
            }
            else if (token is JArray jarr)
            {
                foreach (var elt in jarr.Children())
                {
                    if (stop = Analyze(path, elt, analyzer))
                    {
                        break;
                    }
                }
            }
            else
            {
                stop = analyzer(path, token);
            }

            return stop;
        }
    }
}