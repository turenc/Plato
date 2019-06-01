﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Plato.Core.Models;
using Plato.Internal.Layout.ViewProviders;

namespace Plato.Search.ViewProviders
{
    public class HomeViewProvider : BaseViewProvider<HomeIndex>
    {

        public override Task<IViewProviderResult> BuildIndexAsync(HomeIndex viewModel, IViewProviderContext context)
        {

            return Task.FromResult(Views(
                View<HomeIndex>("Core.Search.Index.Content", model => viewModel).Zone("content")
            ));

        }

        public override Task<IViewProviderResult> BuildDisplayAsync(HomeIndex viewModel, IViewProviderContext context)
        {
            throw new NotImplementedException();
        }

        public override Task<IViewProviderResult> BuildEditAsync(HomeIndex viewModel, IViewProviderContext context)
        {
            throw new NotImplementedException();
        }

        public override Task<IViewProviderResult> BuildUpdateAsync(HomeIndex viewModel, IViewProviderContext context)
        {
            throw new NotImplementedException();
        }

    }



}
