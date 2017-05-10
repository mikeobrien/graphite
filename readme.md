![Graphite](https://github.com/mikeobrien/graphite/raw/master/misc/header.png)

[![Nuget](http://img.shields.io/nuget/v/GraphiteWeb.svg?style=flat)](http://www.nuget.org/packages/GraphiteWeb/) [![TeamCity Build Status](https://img.shields.io/teamcity/http/build.mikeobrien.net/s/Graphite.svg?style=flat)](http://build.mikeobrien.net/viewType.html?buildTypeId=Graphite&guest=1) [![Join the chat at https://gitter.im/Graphite-Web-Framework/Discuss](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/Graphite-Web-Framework/Discuss?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)


*...taking the friction out of Web Api.*

Graphite is a simple, lightweight, convention based web framework built on Web Api. It is heavily inspired by the brilliant web framework FubuMVC.

## Getting Started

Getting started guide and documentation can be found [here](http://graphiteweb.io).


## Install

Graphite can be found on nuget. Install the following packages:

    PM> Install-Package Microsoft.AspNet.WebApi.WebHost (If hosting with ASP.NET)
    PM> Install-Package GraphiteWeb
    PM> Install-Package GraphiteWeb.StructureMap

## Features

- Can run side-by-side with existing Web Api controllers.
- IoC is baked in from the ground up.
- Everything is async by default.
- Heavy emphasis on testability.
- Heavy emphasis on flexibility and configurability. IoW maximizing extensibility and minimizing opinions so you can build your API how **you** want to.
- Lightweight and simple, [out of the box functionality is purposely kept to a minimum](https://www.brainyquote.com/quotes/quotes/a/antoinedes103610.html).
- POCO's, [no inheritance](https://en.wikipedia.org/wiki/Composition_over_inheritance).
- Conventional routing and urls out of the box.
- Conventionally override almost any part of the framework.
- Supports the handler approach (i.e. a single class with one action).
- Supports action sources which allow you to generate routes.
- Supports [behavior chains](http://www.mkmurray.com/blog/2011/06/13/fubumvc-behavior-chains-the-bmvc-pattern/) (AKA the [Russian Doll Model](http://codebetter.com/jeremymiller/2011/01/09/fubumvcs-internal-runtime-the-russian-doll-model-and-how-it-compares-to-asp-net-mvc-and-openrasta/)).
- Diagnostics out of the box.
- Supports multiple querystring and form values with the same name.
- Supports wildcard url parameters.
- Currently supports only [StructureMap](http://structuremap.github.io/) out of the box but any IoC container can be plugged in.
- Currently supports [Json.NET](http://www.newtonsoft.com/json), [Bender](https://github.com/mikeobrien/Bender) and the FCL XML serializer but you can plug in any serializer.

Props
------------

Thanks to [FubuMVC](https://fubumvc.github.io/) for the inspiration.

Thanks to [JetBrains](http://www.jetbrains.com/) for providing OSS licenses! 