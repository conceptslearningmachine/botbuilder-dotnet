﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Action which emits an event declaratively.
    /// </summary>
    public class EmitEvent : Dialog
    {
        [JsonConstructor]
        public EmitEvent(string eventName = null, string eventValue = null, bool bubble = true, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base()
        {
            this.RegisterSourceLocation(callerPath, callerLine);
            this.EventName = eventName;
            this.EventValue = eventValue;
            this.BubbleEvent = bubble;
        }

        /// <summary>
        /// Gets or sets the name of the event to emit.
        /// </summary>
        /// <value>
        /// The name of the event to emit.
        /// </value>
        public string EventName { get; set; }

        /// <summary>
        /// Gets or sets the memory property path to use to get the value to send as part of the event.
        /// </summary>
        /// <value>
        /// The memory property path to use to get the value to send as part of the event.
        /// </value>
        public string EventValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets whether the event should bubble or not.
        /// </summary>
        /// <value>
        /// A value indicating whether gets or sets whether the event should bubble or not.
        /// </value>
        public bool BubbleEvent { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            var eventValue = (this.EventValue != null) ? dc.GetState().GetValue<object>(this.EventValue) : null;
            var handled = await dc.EmitEventAsync(EventName, eventValue, BubbleEvent, false, cancellationToken).ConfigureAwait(false);
            return await dc.EndDialogAsync(handled, cancellationToken).ConfigureAwait(false);
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}[{EventName ?? string.Empty}]";
        }
    }
}
