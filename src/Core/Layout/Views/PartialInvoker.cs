﻿using System;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.Extensions.Caching.Memory;

namespace PlatoCore.Layout.Views
{

    public class PartialInvoker : IPartialInvoker
    {

        private readonly IViewBufferScope _viewBufferScope;
        private readonly ICompositeViewEngine _viewEngine;        
        private readonly IMemoryCache _memoryCache;

        public PartialInvoker(
            IViewBufferScope viewBufferScope,
            ICompositeViewEngine viewEngine,
            IMemoryCache memoryCache) 
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _viewEngine = viewEngine ?? throw new ArgumentNullException(nameof(viewEngine));
            _viewBufferScope = viewBufferScope ?? throw new ArgumentNullException(nameof(viewBufferScope));                        
        }
        
        public ViewContext ViewContext { get; set; }

        public void Contextualize(ViewContext viewContext)
        {
            ViewContext = viewContext;
        }

        public async Task<IHtmlContent> InvokeAsync(string viewName, object model, ViewDataDictionary viewData)
        {
            
            // We always need a view name to invoke
            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentNullException(nameof(viewName));
            }
            
            var builder = new HtmlContentBuilder();
            var result = FindView(viewName);
            if (!result.Success)
            {
                throw new Exception($"A view with the name \"{viewName}\" could not be found!");
            }

            using (var writer = new StringWriter())
            {
                // Render view
                await RenderPartialViewAsync(writer, model, viewData, result.View);
                // Write results
                builder.WriteTo(writer, HtmlEncoder.Default);
                // Return builder
                return builder.SetHtmlContent(writer.ToString());
            }

        }

        // -----------
        
        ViewEngineResult FindView(string partialName)
        {

            var viewEngineResult = _viewEngine.GetView(ViewContext.ExecutingFilePath, partialName, isMainPage: false);
            var getViewLocations = viewEngineResult.SearchedLocations;
            if (!viewEngineResult.Success)
            {
                viewEngineResult = _viewEngine.FindView(ViewContext, partialName, isMainPage: false);
            }

            if (!viewEngineResult.Success)
            {
                var searchedLocations = Enumerable.Concat(getViewLocations, viewEngineResult.SearchedLocations);
                return ViewEngineResult.NotFound(partialName, searchedLocations);
            }

            return viewEngineResult;
        }

        async Task RenderPartialViewAsync(
            TextWriter writer,
            object model,
            ViewDataDictionary viewData,
            Microsoft.AspNetCore.Mvc.ViewEngines.IView view)
        {
            // Determine which ViewData we should use to construct a new ViewData
            var baseViewData = viewData ?? ViewContext.ViewData;
            var newViewData = new ViewDataDictionary<object>(baseViewData, model);
            var partialViewContext = new ViewContext(ViewContext, view, newViewData, writer);

            using (view as IDisposable)
            {
                await view.RenderAsync(partialViewContext);
            }

        }

    }

}
