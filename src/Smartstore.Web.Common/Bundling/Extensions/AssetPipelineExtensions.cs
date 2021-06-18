﻿using System;
using System.Collections.Generic;
using WebOptimizer;
using Smartstore.Web.Bundling.Processors;
using Smartstore;
using NUglify.Css;
using Microsoft.Extensions.DependencyInjection;

namespace Smartstore.Web.Bundling
{
    public static class AssetPipelineExtensions
    {
        /// <summary>
        /// Creates a JavaScript bundle on the specified route and adds configuration-aware processors (like minifier, concatenator etc.).
        /// Always call this method instead of WebOptimizer's "AddJavaScriptBundle()" to register a script bundle unless you intend to do some custom stuff.
        /// </summary>
        public static IAsset RegisterJsBundle(this IAssetPipeline assetPipeline, string route, params string[] sourceFiles)
        {
            var bundle = Guard.NotNull(assetPipeline, nameof(assetPipeline))
                .AddBundle(route, "text/javascript; charset=UTF-8", sourceFiles)
                .EnforceFileExtensions(".js", ".jsx", ".es5", ".es6")
                .MinifyJavaScriptWithJsMin()
                .AddResponseHeader("X-Content-Type-Options", "nosniff")
                .Concatenate();

            return bundle;
        }

        /// <summary>
        /// Creates a Stylesheet bundle on the specified route and adds configuration-aware processors (like minifier, concatenator, path adjuster etc.).
        /// Always call this method instead of WebOptimizer's "AddCssBundle()" to register a script bundle unless you intend to do some heavy custom stuff.
        /// </summary>
        public static IAsset RegisterCssBundle(this IAssetPipeline assetPipeline, string route, params string[] sourceFiles)
        {
            var bundle = Guard.NotNull(assetPipeline, nameof(assetPipeline))
                .AddBundle(route, "text/css; charset=UTF-8", sourceFiles)
                .EnforceFileExtensions(".css")
                .AdjustRelativePaths()
                .MinifyCss(new CssSettings { FixIE8Fonts = false, ColorNames = CssColor.Strict })
                .Concatenate()
                .FingerprintUrls()
                .AddResponseHeader("X-Content-Type-Options", "nosniff");

            return bundle;
        }

        /// <summary>
        /// Runs the DouglasCrockford JavaScript minifier on the content (instead of running NUglify).
        /// </summary>
        public static IAsset MinifyJavaScriptWithJsMin(this IAsset asset)
        {
            return Guard.NotNull(asset, nameof(asset)).AddProcessor(new CrockfordJsMinProcessor());
        }

        /// <summary>
        /// Runs the DouglasCrockford JavaScript minifier on the content (instead of running NUglify).
        /// </summary>
        public static IEnumerable<IAsset> MinifyJavaScriptWithJsMin(this IEnumerable<IAsset> assets)
        {
            Guard.NotNull(assets, nameof(assets));

            assets.Each(x => x.Processors.Add(new CrockfordJsMinProcessor()));
            return assets;
        }

        /// <summary>
        /// Adds processors to the asset pipeline.
        /// </summary>
        public static IAsset AddProcessor(this IAsset asset, params IProcessor[] processors)
        {
            Guard.NotNull(asset, nameof(asset));

            asset.Processors.AddRange(processors);
            return asset;
        }
    }
}