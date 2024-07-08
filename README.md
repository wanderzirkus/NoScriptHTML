# Why

"Readonly" websites where no user inputs must be processed or other interaction with a server in the backend happens, can go pretty far without JavaScript these days - thanks to the improvements in CSS and modern browsers supporting it.

However, writing pure HTML without a (JS) GUI framework can be a tedious task, due to repetitions of elements like a navigation bar, a header or such. Maintaining this is even more of a pain as everything needs to be changed multiple times in some cases, there are copy&paste errors creeping in, ... I think we all know some of the bsic clean code principles (like [DRY](https://clean-code-developer.com/grades/grade-1-red/#Don8217t_Repeat_Yourself_DRY))

This little commandline tool enables a component system like it is known from e.g., VueJS. In fact it is heavily inspired by the component based architecture of VueJS apps.

But it uses **NO Javascript** to achieve this!

No JavaScript on a website means:
- less hazzle, due to omitted packaging, building and minifying tasks (Webpack, vite, ...)
- more safety (no JS based attack vectors, e.g., CORS, supply chain, ...)
- faster load times (no JS has to be interpreted by the browser)
- SEO likes it! (no JS has to be interpreted by the crawler)
- better ecological footprint (no JS has to be processed)
- your website is displayed perfectly fine to users having their JS disabled - which effectively just applies to people using Tor browser I guess ;)

# How

Basically it is building some source files and puts them into a target directory containing the distributables.
This is done by calling it with the two directories as input parameters. 

`NoScriptHTML "SourceDirektoryPath" "TargetDirectoryPath"`

In HTML, to comply to the standart, the data- tag is used to define and inject components:

| Attribut      | Description   |
| ------------- | ------------- |
| data-cmp-definition="componentname" | define a component |
| data-cmp-inject="componentname" | inject a component |
| data-cmp-param-[1-9]+="parametervalue" | define / pass a parameter used by a component |
| data-cmp-slot="slotname" | define / use slots on component injection |

Note:
- Components can be injected into components, also.
- Components can be placed anywhere (own .html file, in a file together with other HTML nodes, ...)
- Components are removed from the files during the build
- empty files are deleted at the end of the build (e.g., files containing only components in source dir)
- hidden files are NOT copied into dist dir
- a slot will be filled with multiple HTML nodes, if they all share the same slot name. They are filled in the order given in the source file

# Examples

## Basic usage

    <div data-cmp-inject="AwesomeComponent" data-cmp-param-1="Hello">

    <div data-cmp-definition="AwesomeComponent">
        data-cmp-param-1 World!
    </div>

will be built to

    <div>
        Hello World!
    </div>

## Using slots

    <div data-cmp-inject="AwesomeComponent">
        <span data-cmp-slot="CoolSlot">Hello</span>
        <span data-cmp-slot="CoolSlot">World!</span>
    </div>

    <div data-cmp-definition="AwesomeComponent">
        <div data-cmp-slot="CoolSlot">
    </div>

will be built to

    <div>
        Hello World!
    </div>
