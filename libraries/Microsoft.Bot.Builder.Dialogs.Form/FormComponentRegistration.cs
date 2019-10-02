﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resolvers;
using Microsoft.Bot.Builder.Dialogs.Form.Converters;
using Microsoft.Bot.Builder.Dialogs.Form.Events;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public class FormComponentRegistration : ComponentRegistration
    {
        public override IEnumerable<TypeRegistration> GetTypes()
        {
            yield return new TypeRegistration<FormDialog>("Microsoft.FormDialog");
            yield return new TypeRegistration<OnAsk>("Microsoft.OnAsk");
            yield return new TypeRegistration<OnChooseProperty>("Microsoft.OnChooseSlot");
            yield return new TypeRegistration<OnChooseEntity>("Microsoft.OnChooseSlotValue");
            yield return new TypeRegistration<OnClarifyEntity>("Microsoft.OnClarifySlotValue");
            yield return new TypeRegistration<OnClearProperty>("Microsoft.OnClearSlot");
            yield return new TypeRegistration<OnSetProperty>("Microsoft.OnSetSlot");
        }

        public override IEnumerable<JsonConverter> GetConverters(ISourceMap sourceMap, IRefResolver refResolver, Stack<string> paths)
        {
            yield return new DialogSchemaConverter(refResolver);
        }
    }
}
