﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Plato.Internal.Hosting.Abstractions;
using Plato.Internal.Layout.Alerts;
using Plato.Internal.Layout.ModelBinding;
using Plato.Internal.Layout.ViewProviders;
using Plato.Internal.Models.Users;
using Plato.Internal.Navigation.Abstractions;
using Plato.Internal.Stores.Abstractions.Users;
using Plato.Articles.Models;
using Plato.Entities.ViewModels;
using Plato.Internal.Features.Abstractions;
using Plato.Internal.Layout;
using Plato.Internal.Layout.Titles;

namespace Plato.Articles.Controllers
{
    public class UserController : Controller, IUpdateModel
    {
        
        private readonly IViewProviderManager<UserIndex> _userViewProvider;
        private readonly IBreadCrumbManager _breadCrumbManager;
        private readonly IPlatoUserStore<User> _platoUserStore;
        private readonly IContextFacade _contextFacade;
        private readonly IFeatureFacade _featureFacade;
        private readonly IPageTitleBuilder _pageTitleBuilder;

        public IHtmlLocalizer T { get; }

        public IStringLocalizer S { get; }

        public UserController(
            IStringLocalizer<HomeController> stringLocalizer,
            IHtmlLocalizer<HomeController> localizer,
            IAlerter alerter, IBreadCrumbManager breadCrumbManager,
            IViewProviderManager<UserIndex> userViewProvider,
            IPlatoUserStore<User> platoUserStore,
            IContextFacade contextFacade,
            IFeatureFacade featureFacade,
            IPageTitleBuilder pageTitleBuilder)
        {
            _breadCrumbManager = breadCrumbManager;
            _userViewProvider = userViewProvider;
            _platoUserStore = platoUserStore;
            _featureFacade = featureFacade;
            _pageTitleBuilder = pageTitleBuilder;
            _contextFacade = contextFacade;

            T = localizer;
            S = stringLocalizer;

        }

        public async Task<IActionResult> Index(EntityIndexOptions opts, PagerOptions pager)
        {

            // Default options
            if (opts == null)
            {
                opts = new EntityIndexOptions();
            }

            // Default pager
            if (pager == null)
            {
                pager = new PagerOptions();
            }

            // Get user
            var user = await _platoUserStore.GetByIdAsync(opts.CreatedByUserId);

            // Ensure user exists
            if (user == null)
            {
                return NotFound();
            }
            
            // Get default options
            var defaultViewOptions = new EntityIndexOptions();
            var defaultPagerOptions = new PagerOptions();

            // Add non default route data for pagination purposes
            if (opts.Search != defaultViewOptions.Search)
                this.RouteData.Values.Add("opts.search", opts.Search);
            if (opts.Sort != defaultViewOptions.Sort)
                this.RouteData.Values.Add("opts.sort", opts.Sort);
            if (opts.Order != defaultViewOptions.Order)
                this.RouteData.Values.Add("opts.order", opts.Order);
            if (opts.Filter != defaultViewOptions.Filter)
                this.RouteData.Values.Add("opts.filter", opts.Filter);
            if (pager.Page != defaultPagerOptions.Page)
                this.RouteData.Values.Add("pager.page", pager.Page);
            if (pager.Size != defaultPagerOptions.Size)
                this.RouteData.Values.Add("pager.size", pager.Size);

            // Build view model
            var viewModel = await GetIndexViewModelAsync(opts, pager);

            // Add view model to context
            HttpContext.Items[typeof(EntityIndexViewModel<Article>)] = viewModel;

            // If we have a pager.page querystring value return paged results
            if (int.TryParse(HttpContext.Request.Query["pager.page"], out var page))
            {
                if (page > 0)
                    return View("GetArticles", viewModel);
            }
            
            // Build page title
            _pageTitleBuilder
                .AddSegment(S["Users"])
                .AddSegment(S[user.DisplayName])
                .AddSegment(S["Articles"]);

            // Build breadcrumb
            _breadCrumbManager.Configure(builder =>
            {
                builder.Add(S["Home"], home => home
                    .Action("Index", "Home", "Plato.Core")
                    .LocalNav()
                ).Add(S["Users"], users => users
                    .Action("Index", "Home", "Plato.Users")
                    .LocalNav()
                ).Add(S[user.DisplayName], name => name
                    .Action("Display", "Home", "Plato.Users", new RouteValueDictionary()
                    {
                        ["opts.id"] = user.Id,
                        ["opts.alias"] = user.Alias
                    })
                    .LocalNav()
                ).Add(S["Articles"]);
            });
            
            //// Return view
            return View((LayoutViewModel) await _userViewProvider.ProvideDisplayAsync(new UserIndex()
            {
                Id = user.Id
            }, this));

        }

        async Task<EntityIndexViewModel<Article>> GetIndexViewModelAsync(EntityIndexOptions options, PagerOptions pager)
        {

            // Get current feature
            var feature = await _featureFacade.GetFeatureByIdAsync(RouteData.Values["area"].ToString());

            // Restrict results to current feature
            if (feature != null)
            {
                options.FeatureId = feature.Id;
            }

            // Set pager call back Url
            pager.Url = _contextFacade.GetRouteUrl(pager.Route(RouteData));
            
            // Return updated model
            return new EntityIndexViewModel<Article>()
            {
                Options = options,
                Pager = pager
            };

        }


    }

}
