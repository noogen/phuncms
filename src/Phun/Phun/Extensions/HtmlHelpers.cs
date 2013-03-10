﻿namespace Phun.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Web.Mvc;

    using Phun.Configuration;
    using Phun.Data;

    /// <summary>
    /// CMS Html Helpers.
    /// </summary>
    public static class HtmlHelpers
    {
        /// <summary>
        /// Renders the content of the CMS.
        /// </summary>
        /// <param name="html">The HTML helper.</param>
        /// <param name="contentName">Name of the content.</param>
        /// <returns>The content data as string.</returns>
        public static string PhunRenderPartialContent(this HtmlHelper html, string contentName = "")
        {
            string result = string.Empty;

            var controller = new PhunCmsContentController();
            var content = new ContentModel()
            {
                Path = contentName.Contains("/") ? contentName : html.ViewContext.RequestContext.HttpContext.Request.Path + "/" + contentName,
                Host = controller.GetCurrentHost(controller.ContentConfig, html.ViewContext.RequestContext.HttpContext.Request.Url)
            };
            controller.ContentRepository.Retrieve(content);
            if (content.Data != null)
            {
                result = System.Text.Encoding.UTF8.GetString(content.Data);
            }

            return result;
        }

        /// <summary>
        /// Renders a div box and content of the CMS.
        /// </summary>
        /// <param name="html">The HTML helper.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="contentName">Name of the content.</param>
        /// <param name="attributes">The attributes.</param>
        public static void PhunRenderPartialForInlineEdit(this HtmlHelper html, string tagName, string contentName, IDictionary<string, object> attributes = null)
        {
            var tagBuilder = new TagBuilder(tagName);
            tagBuilder.Attributes.Add("about", contentName);
            if (attributes != null)
            {
                tagBuilder.MergeAttributes(attributes);
            }

            var writer = html.ViewContext.Writer;
            writer.Write(tagBuilder.ToString(TagRenderMode.StartTag));
            writer.Write("<div property=\"content\">");
            var data = html.PhunRenderPartialContent(contentName);
            writer.Write(string.IsNullOrEmpty(data) ? "&nbsp;" : data);
            writer.Write("</div>");
            writer.Write(tagBuilder.ToString(TagRenderMode.EndTag));
        }

        /// <summary>
        /// return the html resource url.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="path">The path.</param>
        /// <returns>
        /// Simple CMS resource url.
        /// </returns>
        public static MvcHtmlString PhunResourceUrl(this HtmlHelper html, string path)
        {
            var provider = new ResourcePathProvider();
            return new MvcHtmlString(provider.GetResourcePath(path));
        }

        /// <summary>
        /// Renders the simple CMS bundles.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="includeJquery">if set to <c>true</c> [include jquery].</param>
        /// <param name="includeJqueryui">if set to <c>true</c> [include jqueryui].</param>
        /// <param name="includeBackbone">if set to <c>true</c> [include backbone].</param>
        /// <param name="includeCkEditor">if set to <c>true</c> [include ck editor].</param>
        /// <param name="includeEditorInit">if set to <c>true</c> [include editor init].</param>
        /// <returns>
        /// Simple cms resource bundles with default ckeditor.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public static MvcHtmlString PhunRenderBundles(this HtmlHelper html, bool includeJquery = true, bool includeJqueryui = true, bool includeBackbone = true, bool includeCkEditor = true, bool includeEditorInit = true)
        {
            var file = new ResourceVirtualFile("~/", string.Empty);
            return new MvcHtmlString(file.PhunCmsRenderBundles(
                includeJquery, includeJqueryui, includeBackbone, includeCkEditor, includeEditorInit));
        }
    }
}
