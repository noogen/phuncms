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
 - Make sure phuncms.Bootstraper.Initialize is run after all route are registered.
 - Make sure to provide correct configuration for content repository: file, sql, etc...
 - Login to your site as content admin.
 
from the browser
=======
 - Use /[phuncontentroute]/filemanager to manage your content
 - Once you created a page, you can visit /[phuncontentroute]/edit?path=/custom/yourpage to edit your page.
 - You can also visit /custom/yourpage to view your page.
 - You can create customcontroller and override this page with your own razor page.
 
from razor or view
========
 - You can use custom helpers to render partial content and scripts.
 - Content get server-side rendered when using html helper.
 
@Html.PhunRenderPartialContent("LeftHeader") or @Html.PhunRenderPartialForInlineEdit("h2", "LeftHeader")

@section scripts
{
    @Html.PhunRenderBundles()
}

from any html page
=========
<div data-phun="LeftHeader"></div><div data-phun="RightHeader"></div>
- When content of above tag is empty, the contents are ajax loaded and will become inline editable for content admin.

<div data-phun="LeftHeader">%LeftHeader%</div><div data-phun="RightHeader"></div>
- Example will render LeftHeader on server-side, while RightHeader get ajax load.
- Phun javascript and css are automatically get injected before content </head> or you can reference them yourself.
