﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlatoCore.Features.Abstractions;
using PlatoCore.Models.Features;

namespace PlatoCore.Features
{
    
    public class FeatureEventContext  : IFeatureEventContext
    {
        public IServiceProvider ServiceProvider { get; set; }

        public IShellFeature Feature { get; set; }

        public IDictionary<string, string> Errors { get; set; } = new Dictionary<string, string>();

        public ILogger Logger { get; set; }

        public FeatureEventContext()
        {

        }

        public FeatureEventContext(IShellFeature feature)
        {
            this.Feature = feature;
        }

    }

}
