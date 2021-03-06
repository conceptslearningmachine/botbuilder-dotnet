﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters.Facebook.FacebookEvents;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Adapters.Facebook
{
    /// <summary>
    /// Represents the messaging structure.
    /// </summary>
    public class FacebookMessage
    {
        public FacebookMessage(string recipientId, Message message, string messagingType, string tag = null, string notificationType = null, string personalId = null, string senderAction = null, string senderId = null)
        {
            Recipient.Id = recipientId;
            Message = message;
            MessagingType = messagingType;
            Tag = tag;
            NotificationType = notificationType;
            PersonaId = personalId;
            SenderAction = senderAction;
            Sender.Id = senderId;
        }

        /// <summary>
        /// Gets or sets the ID of the recipient.
        /// </summary>
        /// <value>The ID of the recipient.</value>
        [JsonProperty(PropertyName = "recipient")]
        public FacebookBotUser Recipient { get; set; } = new FacebookBotUser();

        /// <summary>
        /// Gets or sets the ID of the sender.
        /// </summary>
        /// <value>The ID of the sender.</value>
        [JsonProperty(PropertyName = "sender")]
        public FacebookBotUser Sender { get; set; } = new FacebookBotUser();

        /// <summary>
        /// Gets or sets the message to be sent.
        /// </summary>
        /// <value>The message.</value>
        [JsonProperty(PropertyName = "message")]
        public Message Message { get; set; }

        /// <summary>
        /// Gets or sets the messaging type.
        /// </summary>
        /// <value>The messaging type.</value>
        [JsonProperty(PropertyName = "messaging_type")]
        public string MessagingType { get; set; }

        /// <summary>
        /// Gets or sets a tag to the message.
        /// </summary>
        /// <value>The tag.</value>
        [JsonProperty(PropertyName = "tag")]
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets the notification type.
        /// </summary>
        /// <value>The notification type.</value>
        [JsonProperty(PropertyName = "notification_type")]
        public string NotificationType { get; set; }

        /// <summary>
        /// Gets or sets the persona ID.
        /// </summary>
        /// <value>The persona ID.</value>
        [JsonProperty(PropertyName = "persona_id")]
        public string PersonaId { get; set; }

        /// <summary>
        /// Gets or sets the sender action.
        /// </summary>
        /// <value>The sender action.</value>
        [JsonProperty(PropertyName = "sender_action")]
        public string SenderAction { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public long TimeStamp { get; set; }

        [JsonProperty(PropertyName = "standby")]
        public bool Standby { get; set; }

        [JsonProperty(PropertyName = "postback")]
        public FacebookPostBack PostBack { get; set; }

        [JsonProperty(PropertyName = "optin")]
        public FacebookRecipient OptIn { get; set; }
    }
}
