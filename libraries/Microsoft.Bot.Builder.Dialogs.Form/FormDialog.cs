﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Events;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Form
{
    public class FormDialog : AdaptiveDialog
    {
        public FormDialog(DialogSchema inputSchema, DialogSchema outputSchema)
        {
            InputSchema = inputSchema;
            OutputSchema = outputSchema;
            Selector = new FirstSelector();  // new SlotMapSelector(outputSchema);

            if (outputSchema.Schema["type"].Value<string>() != "object")
            {
                throw new ArgumentException("Forms must be an object schema.");
            }

            GenerateRules();
        }

        public DialogSchema InputSchema { get; }

        public DialogSchema OutputSchema { get; }

        // For simple singleton slot:
        //  Set values
        //      count(@@foo) == 1 -> foo == @foo
        //      count(@@foo) > 1 -> "Which {@@foo} do you want for {slotName}"
        //  Constraints (which are more specific)
        //      count(@@foo) == 1 && @foo < 0 -> "{@foo} is too small for {slotname}"
        //      count(@@foo) > 1 && count(where(@@foo, foo, foo < 0)) > 0 -> "{where(@@foo, foo, foo < 0)} are too small for {slotname}"
        // For simple array slot:
        //  Set values:
        //      @@foo -> foo = @@foo
        //  Constraints: (which are more specific)
        //      @@foo && count(where(@@foo, foo, foo < 0)) > 0 -> "{where(@@foo, foo, foo < 0) are too small for {slotname}"
        //  Modification--based on intent?
        //      add: @@foo && @intent == add -> Append(@@foo, foo)
        //      // This is to make this more specific than both the simple constraint and the intent
        //      add: @@foo && count(where(@@foo, foo, foo < 0)) > 0 && @intent == add -> "{where(@@foo, foo, foo < 0)} are too small for {slotname}"
        //      delete: @@foo @intent == delete -> Delete(@@foo, foo)
        // For structured singleton slot
        //  count(@@foo) == 1 -> Apply child constraints, i.e. make a new singleton object to apply child property rule sets to it.
        //  count(@@foo) > 1 -> "Which one did you want?" with replacing @@foo with the singleton selection
        //
        // Children slots can either:
        // * Refer to parent structure which turns into count(first(parent).child) == 1
        // * Refer to independent entity, i.e. count(datetime) > 1
        //
        // Assumptions:
        // * In order to map structured entities to structured slots, parent structures must be singletons before child can map them.
        // * We will only generate a single instance of the form.  (Although there can be multiple ones inside.)
        // * You can map directly, but then must deal with that complexity of structures.  For example if you have multiple flight(origin, destination) and you want to map to hotel(location)
        //   you have to figure out how to deal with multiple flight structures and the underlying entity structures.
        // * For now only leaves can be arrays.  If you need more, I think it is a subform, but we could probably automatically generate a foreach step on top.
        //
        // 1) Find all most specific matches
        // 2) Identify any slots that compete for the same entity.  Select by in expected, then keep as slot ambiguous.
        // 3) For each entity either: a) Do its set, b) queue up clarification, c) surface as unhandled
        // 
        // Two cases:
        // 1) Flat entity resolution, treat properties as independent.
        // 2) Hierarchical, the first level you get to count(@@flight) == 1, then for count(first(@@flight).origin) == 1
        // We know which is which by entity path, i.e. flight.origin -> hierarchical whereas origin is flat.
        //
        // In order to robustly handle we need a progression of transformations, i.e. to map @meat to meatSlot singleton:
        // @meat -> meatSlot_choice (m->1) ->
        //                          (1->1) -> foreach meatslot_clarify -> set meat slot (clears others)
        // If we get a new @meat, then it would reset them all.
        // Should this be a flat set of rules?
        protected void GenerateRules()
        {
            AddEvent(new OnMessageActivity
            {
                Actions = new List<IDialog>
                {
                    MemoryTest()
                }
            });
            /*
            foreach (var child in OutputSchema.Property.Children)
            {
                GenerateRules(child);
            }
            */
        }

        protected void GenerateRules(PropertySchema property)
        {
            if (property.Children.Count() > 0)
            {
                throw new ArgumentException($"{property.Path} is a complex object and that is not supported yet.");
            }

            var events = new List<string> { AdaptiveEvents.RecognizedIntent };
            foreach (var mapping in property.Mappings)
            {
                var expr = mapping.Value<string>()?.Replace("[]", string.Empty);
                var path = FormPath(property.Path);
                if (expr != null)
                {
                    if (property.IsArray)
                    {
                        AddEvent(new OnDialogEvent(
                            events: events,
                            constraint: $"{expr}",
                            actions: new List<IDialog>
                            {
                                    new SetProperty
                                    {
                                        Property = path,
                                        Value = expr
                                    }
                            }));
                    }
                    else
                    {
                        // Just set value to singleton


                        // Disambiguate to singleton
                        AddEvent(new OnDialogEvent(
                            events: events,
                            constraint: $"count({expr}) > 1",
                            actions: new List<IDialog>
                            {
                                new FormInput(
                                    text: $"[disambiguate({expr}, {path})]",
                                    expectedSlots: new List<string> { path })
                            }));
                    }
                }
                else
                {
                    // TODO: How to load IRule?
                }
            }
        }

        protected IDialog MemoryTest()
        {
            var dialog = new AdaptiveDialog("test")
            {
                Events = new List<IOnEvent>
                {
                    new OnMessageActivity
                    {
                        Actions = new List<IDialog>
                        {
                            new IfCondition
                            {
                                Condition = $"dialog.isChild",
                                Actions = new List<IDialog>
                                {
                                    new SendActivity("inChild"),
                                    new SetProperty()
                                    {
                                        Property = "$dialog.isChild",
                                        Value = "false"
                                    },
                                    new DebugBreak(),
                                    new EndDialog()
                                },
                                ElseActions = new List<IDialog>
                                {
                                    new SetProperty {
                                        Property = "dialog.isChild",
                                        Value = "true"
                                    },
                                    new BeginDialog("test"),
                                    new SendActivity("Value {dialog.isChild}")
                                }
                            },
                        }
                    }
                }
            };
            return dialog;
        }

        // If one @@entity then goes to foreach
        protected IOnEvent SingletonEntityStart(string entity, string slot)
        {
            var name = entity;
            var slots = new List<string> { slot };
            var choice = $"{slot}_choice";
            return new OnDialogEvent(
                events: new List<string> { AdaptiveEvents.RecognizedIntent },
                constraint: $"{entity}",
                actions: new List<IDialog>
                {
                    // If multiple choices for singleton ask which one.
                    new IfCondition()
                    {
                        Condition = $"count({entity}) > 1",
                        Actions = new List<IDialog>
                        {
                            // Ask which one
                            new SetProperty { Property = $"dialog.{slot}_choice", Value = entity },
                            new FormInput($"which({name}, {entity})]", expectedSlots: new List<string>())
                            // TODO: Create new list of just selected value.
                        },
                        ElseActions = new List<IDialog>
                        {

                        }
                    },

                    // Loop over each entity to clarify
                    new Foreach()
                    {
                        ValueProperty = entity,
                        Actions = new List<IDialog>
                        {
                            new IfCondition()
                            {
                                Condition = $"count(dialog.value) > 1",
                                Actions = new List<IDialog>
                                {
                                    // Clarify value
                                    new FormInput($"[clarification({name})]", slots),
                                    new IfCondition()
                                    {
                                        Condition = ""
                                    }
                                },
                                ElseActions = new List<IDialog>
                                {
                                    // TODO: Fill in 
                                    new SetProperty
                                    {
                                        Property = slot,
                                        Value = "dialog.value"
                                    }
                                }
                            }
                        }
                    }
                });
        }

        protected IDialog Clarify(string slot, string source, string working, string result)
        {
            return new IfCondition
            {
                Condition = $"count(where({source}, value, count(value) > 1)) > 0",
                Actions = new List<IDialog>
                {
                    new SetProperty {Property = working, Value = source},
                    new Foreach
                    {
                         ListProperty = source,
                         Actions = new List<IDialog>
                         {
                             new IfCondition
                             {
                                 Condition = $"count(working[dialog.index]) > 1",
                                 Actions = new List<IDialog>
                                 {
                                     new FormInput($"[clarify({slot}, dialog.value)]")
                                 }
                             }
                         }
                    }
                },
                ElseActions = new List<IDialog>
                {
                    new SetProperty { Property = result, Value = source}
                }
            };
        }

        protected string FormPath(string schemaPath) => $"$form.{schemaPath.Replace("[]", string.Empty)}";
    }
}
