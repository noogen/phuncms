namespace Phun.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using Phun.Configuration;
    using Phun.Data;
    using Phun.Routing;

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
        public static string PhunPartial(this HtmlHelper html, string contentName)
        {
            return PhunPartial(contentName, html.ViewContext.HttpContext.Request.Url);
        }

        /// <summary>
        /// Render the partial content name.
        /// </summary>
        /// <param name="contentName">Name of the content.</param>
        /// <param name="url">The URL.</param>
        /// <returns>Partial content.</returns>
        /// <exception cref="System.ArgumentException">contentName is required.</exception>
        public static string PhunPartial(string contentName, Uri url)
        {
            if (string.IsNullOrEmpty(contentName))
            {
                throw new ArgumentException("contentName is required.");
            }

            var result = string.Empty;
            var contentConnector = new ContentConnector();
            var content = new ContentModel()
            {
                Path = contentConnector.ApplyPathConvention(
                          "/page" + (contentName.Contains("/") ? contentName : url.AbsolutePath + "/" + contentName)),
                Host = contentConnector.GetTenantHost(url)
            };
            contentConnector.ContentRepository.Retrieve(content, true);
            if (content.DataLength != null)
            {
                content.SetDataFromStream();
                result = System.Text.Encoding.UTF8.GetString(content.Data).GetHtmlBody();
            }

            return result;
        }

        /// <summary>
        /// Render partial for inline edit.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="contentName">Name of the content.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>Html string.</returns>
        public static HtmlString PhunPartialEditable(
            this HtmlHelper html, string tagName, string contentName, object htmlAttributes)
        {
            return PhunPartialEditable(html, tagName, contentName, new RouteValueDictionary(htmlAttributes));
        }

        /// <summary>
        /// Render partial for inline edit.
        /// </summary>
        /// <param name="html">The HTML helper.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="contentName">Name of the content.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>Html string.</returns>
        public static HtmlString PhunPartialEditable(this HtmlHelper html, string tagName, string contentName, IDictionary<string, object> htmlAttributes)
        {
            return PhunPartialEditable(html.ViewContext.HttpContext.Request.Url, tagName, contentName, htmlAttributes);
        }

        /// <summary>
        /// Render partial for inline edit.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="contentName">Name of the content.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>Editable partial.</returns>
        public static HtmlString PhunPartialEditable(
            Uri url, string tagName, string contentName, object htmlAttributes)
        {
            return PhunPartialEditable(url, tagName, contentName, new RouteValueDictionary(htmlAttributes));
        }

        /// <summary>
        /// Render partial for inline edit.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="contentName">Name of the content.</param>
        /// <param name="htmlAttributes">The HTML attributes.</param>
        /// <returns>Editable partial.</returns>
        public static HtmlString PhunPartialEditable(Uri url, string tagName, string contentName, IDictionary<string, object> htmlAttributes)
        {
            var tagBuilder = new TagBuilder(tagName);
            tagBuilder.Attributes.Add("about", contentName);
            if (htmlAttributes != null)
            {
                tagBuilder.MergeAttributes(htmlAttributes);
            }

            var data = PhunPartial(contentName, url);
            tagBuilder.InnerHtml = string.Concat("<div property=\"content\">", data, "</div>");
            return new HtmlString(tagBuilder.ToString(TagRenderMode.Normal));
        }

        /// <summary>
        /// return the html resource url.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="path">The path.</param>
        /// <returns>
        /// Simple CMS resource url.
        /// </returns>
        public static HtmlString PhunResourceUrl(this HtmlHelper html, string path)
        {
            var provider = new ResourcePathUtility();
            return new HtmlString(provider.GetResourcePath(path));
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
        public static HtmlString PhunBundles(this HtmlHelper html, bool includeJquery = true, bool includeJqueryui = true, bool includeBackbone = true, bool includeCkEditor = true, bool includeEditorInit = true)
        {
            var file = new ResourcePathUtility();
            return new HtmlString(file.PhunCmsRenderBundles(
                includeJquery, includeJqueryui, includeBackbone, includeCkEditor, includeEditorInit));
        }
    }
}
