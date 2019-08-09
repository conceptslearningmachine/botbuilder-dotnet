﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Events
{

    /// <summary>
    /// Event triggered when a Activity of a given type is received 
    /// </summary>
    public class OnActivity : OnDialogEvent
    {
        [JsonConstructor]
        public OnActivity(string type = null, List<IDialog> actions = null, string constraint = null, int priority = 0, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(
            events: new List<string>()
            {
                AdaptiveEvents.ActivityReceived
            },
            actions: actions,
            constraint: constraint,
            priority: priority,
            callerPath: callerPath, 
            callerLine: callerLine)
        {
            Type = type;
        }

        /// <summary>
        /// ActivityType
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        protected override Expression BuildExpression(IExpressionParser factory)
        {

            // add constraints for activity type
            return Expression.AndExpression(factory.Parse($"turn.dialogEvent.value.type == '{this.Type}'"),
                base.BuildExpression(factory));
        }

        protected override ActionChangeList OnCreateChangeList(SequenceContext planning, object dialogOptions = null)
        {
            return new ActionChangeList()
            {
                Actions = Actions.Select(s => new ActionState()
                {
                    DialogStack = new List<DialogInstance>(),
                    DialogId = s.Id,
                    Options = dialogOptions
                }).ToList()
            };
        }

        public override string GetIdentity()
        {
            return $"{this.GetType().Name}({this.Type})[{this.Constraint}]";
        }
    }

}