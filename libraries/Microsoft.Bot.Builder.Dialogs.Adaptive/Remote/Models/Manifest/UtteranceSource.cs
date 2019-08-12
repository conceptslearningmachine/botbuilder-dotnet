﻿using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Remote.Models.Manifest
{
    /// <summary>
    /// Source of utterances for a given locale which form part of an Action within a manifest.
    /// </summary>
    public class UtteranceSource
    {
        [JsonProperty(PropertyName = "locale")]
        public string Locale { get; set; }

        [JsonProperty(PropertyName = "source")]
        public string[] Source { get; set; }
    }
}
