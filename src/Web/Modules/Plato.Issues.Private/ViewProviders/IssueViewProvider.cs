﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using PlatoCore.Layout.ViewProviders;
using Plato.Entities.Stores;
using PlatoCore.Layout.ModelBinding;
using PlatoCore.Navigation.Abstractions;
using Plato.Issues.Models;
using Plato.Issues.Private.ViewModels;

namespace Plato.Issues.Private.ViewProviders
{
    public class IssueViewProvider : BaseViewProvider<Issue>
    {

        public static string HtmlName = "visibility";

        private readonly IAuthorizationService _authorizationService;
   
        private readonly HttpRequest _request;
 
        public IssueViewProvider(
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _request = httpContextAccessor.HttpContext.Request;
            _authorizationService = authorizationService;
        }
        
        public override Task<IViewProviderResult> BuildIndexAsync(Issue entity, IViewProviderContext updater)
        {
            return Task.FromResult(default(IViewProviderResult));
        }
        
        public override Task<IViewProviderResult> BuildDisplayAsync(Issue entity, IViewProviderContext updater)
        {

            return Task.FromResult(default(IViewProviderResult));
        }

        public override async Task<IViewProviderResult> BuildEditAsync(Issue entity, IViewProviderContext context)
        {
            // We always need an entity
            if (entity == null)
            {
                return default(IViewProviderResult);
            }

            // Set isPrivate flag
            var isPrivate = entity.Id > 0 && entity.IsPrivate;

            // Ensures we persist selection between post backs
            if (context.Controller.HttpContext.Request.Method == "POST")
            {
                foreach (string key in context.Controller.HttpContext.Request.Form.Keys)
                {
                    if (key.StartsWith(IssueViewProvider.HtmlName))
                    {
                        var values = context.Controller.HttpContext.Request.Form[key];
                        foreach (var value in values)
                        {
                            if (value.IndexOf("private", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                isPrivate = true;
                            }
                            if (value.IndexOf("public", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                isPrivate = false;
                            }
                        }
                    }
                }
            }

            // Build dropdown view model
            var selectedValue = isPrivate ? "private" : "public";
            var viewModel = new VisibilityDropDownViewModel()
            {
                HtmlName = HtmlName,
                SelectedValue = selectedValue,
                DropDown = new SelectDropDown()
                {
                    Title = "Visibility",
                    InnerCssClass = "d-block",
                    Items = new List<SelectDropDownItem>()
                    {
                        new SelectDropDownItem()
                        {
                            Text = "Public",
                            Description = "This issue will be visible to everyone. Chose this option if your sharing public information and don't mind public comments",
                            Value = "public",
                            Checked = selectedValue == "public" ? true : false,
                            Permission = entity.Id == 0
                                ? Permissions.IssuesPrivateCreatePublic
                                : Permissions.IssuesPrivateToPublic
                        },
                        new SelectDropDownItem()
                        {
                            Text = "Private",
                            Description = "This issue will only be visible to you and our team. Choose this option if your sharing private information.",
                            Value = "private",
                            Checked = selectedValue == "private" ? true : false,
                            Permission = entity.Id == 0
                                ? Permissions.IssuesPrivateCreatePrivate
                                : Permissions.IssuesPrivateToPrivate
                        }

                    }
                }
            };

            // For new entities adjust model to ensure the first appropriate
            // option is selected based on our current permissions 
            if (entity.Id == 0)
            {
                await viewModel.AdjustInitiallySelected(_authorizationService, context.Controller.User);
            }

            // Add  dropdown view model to context for use within navigation provider
            context.Controller.HttpContext.Items[typeof(VisibilityDropDownViewModel)] = viewModel;

            // No view modifications 
            return default(IViewProviderResult);

        }

        public override async Task ComposeModelAsync(Issue issue, IUpdateModel updater)
        {

            var model = new SelectDropDownViewModel()
            {
                SelectedValue = GetIsPrivate().ToString()
            };

            await updater.TryUpdateModelAsync(model);

            if (updater.ModelState.IsValid)
            {
                issue.IsPrivate = GetIsPrivate();
            }

        }
        
        public override async Task<IViewProviderResult> BuildUpdateAsync(Issue model, IViewProviderContext context)
        {
            return await BuildEditAsync(model, context);
        }

        bool GetIsPrivate()
        {

            // Get the follow checkbox value
            foreach (var key in _request.Form.Keys)
            {
                if (key.StartsWith(HtmlName))
                {
                    var values = _request.Form[key];
                    foreach (var value in values)
                    {
                        if (value.Equals("private", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;

        }

    }

}
