﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Form.Actions
{
    public class Ask : SendActivity
    {
        [JsonConstructor]
        public Ask(
            string text = null, 
            IList<string> expectedSlots = null, 
            [CallerFilePath] string callerPath = "", 
            [CallerLineNumber] int callerLine = 0)
        : base(text, callerPath, callerLine)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.Activity = new ActivityTemplate(text ?? string.Empty);
            this.ExpectedSlots = expectedSlots;
        }

        /// <summary>
        /// Gets or sets slots expected to be filled by response.
        /// </summary>
        /// <value>
        /// Slots expected to be filled by response.
        /// </value>
        public IList<string> ExpectedSlots { get; set; }

        protected override async Task<DialogTurnResult> OnRunCommandAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (ExpectedSlots != null)
            {
                dc.Context.TurnState.Add("expectedSlots", ExpectedSlots);
            }

            return await base.OnRunCommandAsync(dc, options, cancellationToken);
        }
    }
}