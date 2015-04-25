# Notice
This repository is no longer maintained. 

phuncms
======

Quickly add CMS capability to your new or existing Asp.NET MVC 4 project.

goal
======
Provide a simple way to add content management capability to any Asp.NET MVC 4 project.

Base on the decoupled CMS concept - http://decoupledcms.org/
   - utilize createjs client-side for editing support
   - server CRUD interface/repository/connector for content storage
   - basic server + client interface for file management

![Architecture](http://i.imgur.com/chzYYGN.png)

howto 
======
 - https://github.com/noogen/phuncms/wiki/How-to
 
quick start
========
 - You can use custom helpers to render partial content and scripts.
 - Content get server-side rendered when using HtmlHelper.

```c#
@Html.PhunPartial("LeftHeader") 
```
or

```c#
@Html.PhunPartialEditable("h2", "LeftHeader", new { @class= "one" })

@section scripts
{
    @Html.PhunBundles()
}
```

demo
======
- http://phuncms.azurewebsites.net/
- For now, go ahead and try it.  Grab the source and run Phun.Demo.Web or try out the nuget PhunCMS package.  Project requires Visual Studio 2012.
- Drop a comment, suggestion or request on github issue page.

What's new in 1.0.4
======
- back to the basic where phuncms core should be simple
- upgrade filemanager UI to help with page creation workflow

[What is a Page?](https://github.com/noogen/phuncms/wiki/What-is-a-Page%3F)
