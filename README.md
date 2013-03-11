phuncms
======

Quickly add CMS capability to your new or existing Asp.NET MVC 4 project.

goal
======
Provide a simple way to add content management capability to any Asp.NET MVC 4 project.

Base on the modular CMS movement
   - utilize createjs client-side CMS ui for support
   - server CRUD interface/repository/connector for content storage
   - basic server + client interface for file management
   
howto
=======
 - Make sure to provide correct configuration for content repository: file, sql, etc...
 - Login to your site as content admin.
 
from the browser
=======
 - Use /phuncms/filemanager to manage your content
 - Once you created a page, you can visit /phuncms/edit?path=/custom/yourpage to edit your page.
 - You can also visit /custom/yourpage to view your page.
 - You can create CustomController class and override this page with your own razor page.
 
from razor or view
========
 - You can use custom helpers to render partial content and scripts.
 - Content get server-side rendered when using html helper.
 
@Html.PhunRenderPartialContent("LeftHeader") or @Html.PhunRenderPartialForInlineEdit("h2", "LeftHeader", new { @class= "one" })

@section scripts
{
    @Html.PhunRenderBundles()
}

from any html page
=========
```html
<div data-cmscontent="LeftHeader"></div><div data-cmscontent="RightHeader"></div>
```
- The contents are ajax loaded and will become inline editable for content admin.

```html
<div>%LeftHeader%</div><div data-cmscontent="RightHeader"></div>
```
- Example will render LeftHeader on server-side, while RightHeader get ajax load.
- Phun javascript and css are automatically get injected before content </head> or you can reference them yourself.

demo
========
- I'm still working getting the demo on my free WebSpark Azure account.
- For now, go ahead and try it.  Grab the source and run Phun.Demo.Web or try out the nuget PhunCMS package.  Project requires Visual Studio 2012.
- Drop a comment, suggestion or request on github issue page.
