﻿using System.Collections.Generic;
using PlatoCore.Models.Notifications;
using PlatoCore.Notifications.Abstractions;
using PlatoCore.Security.Abstractions;

namespace Plato.Docs.StopForumSpam.NotificationTypes
{

    public class WebNotifications : INotificationTypeProvider
    {

        public static readonly WebNotification DocSpam =
            new WebNotification("DocSpamWeb", "Doc Spam",
                "Show me a web notification for each doc detected as SPAM.");

        public static readonly WebNotification CommentSpam =
            new WebNotification("DocCommentSpamWeb", "Doc Comment Spam",
                "Show me a web notification for each doc comment detected as SPAM.");
        
        public IEnumerable<DefaultNotificationTypes> GetNotificationTypes()
        {
            return new[]
            {
                new DefaultNotificationTypes
                {
                    RoleName = DefaultRoles.Administrator,
                    NotificationTypes = new[]
                    {
                        DocSpam,
                        CommentSpam
                    }
                },
                new DefaultNotificationTypes
                {
                    RoleName = DefaultRoles.Staff,
                    NotificationTypes = new[]
                    {
                        DocSpam,
                        CommentSpam
                    }
                }

            };
        }

        public IEnumerable<DefaultNotificationTypes> GetDefaultNotificationTypes()
        {
            return new[]
            {
                new DefaultNotificationTypes
                {
                    RoleName = DefaultRoles.Administrator,
                    NotificationTypes = new[]
                    {
                        DocSpam,
                        CommentSpam
                    }
                },
                new DefaultNotificationTypes
                {
                    RoleName = DefaultRoles.Staff,
                    NotificationTypes = new[]
                    {
                        DocSpam,
                        CommentSpam
                    }
                }
            };
        }

    }

}
