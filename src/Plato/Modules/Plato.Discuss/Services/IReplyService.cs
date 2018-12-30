﻿using System.Threading.Tasks;
using Plato.Discuss.Models;
using Plato.Discuss.ViewModels;
using Plato.Internal.Data.Abstractions;
using Plato.Internal.Navigation;

namespace Plato.Discuss.Services
{
    public interface IReplyService
    {
        Task<IPagedResults<Reply>> GetRepliesAsync(TopicOptions options, PagerOptions pager);

    }

}