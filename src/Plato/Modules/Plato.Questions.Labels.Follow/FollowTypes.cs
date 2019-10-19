﻿using System.Collections.Generic;
using Plato.Follows.Models;
using Plato.Follows.Services;

namespace Plato.Questions.Labels.Follow
{

    public class FollowTypes : IFollowTypeProvider
    {

        public static readonly FollowType Label =
            new FollowType(
                "QuestionLabel",
                "Follow Label",
                "Follow this label to get notified when new questions are posted with this label",
                "Unsubscribe",
                "You are following this label and will be notified when questions are posted with this label, click to unsubscribe",
                "Login to follow this label",
                "You don't have permission to follow this label");

        public IEnumerable<IFollowType> GetFollowTypes()
        {
            return new[]
            {
                Label
            };
        }

    }

}
