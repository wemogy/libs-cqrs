"use strict";(self.webpackChunkwemogy=self.webpackChunkwemogy||[]).push([[774],{5680:(e,r,t)=>{t.d(r,{xA:()=>y,yg:()=>g});var n=t(6540);function i(e,r,t){return r in e?Object.defineProperty(e,r,{value:t,enumerable:!0,configurable:!0,writable:!0}):e[r]=t,e}function o(e,r){var t=Object.keys(e);if(Object.getOwnPropertySymbols){var n=Object.getOwnPropertySymbols(e);r&&(n=n.filter((function(r){return Object.getOwnPropertyDescriptor(e,r).enumerable}))),t.push.apply(t,n)}return t}function l(e){for(var r=1;r<arguments.length;r++){var t=null!=arguments[r]?arguments[r]:{};r%2?o(Object(t),!0).forEach((function(r){i(e,r,t[r])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(t)):o(Object(t)).forEach((function(r){Object.defineProperty(e,r,Object.getOwnPropertyDescriptor(t,r))}))}return e}function a(e,r){if(null==e)return{};var t,n,i=function(e,r){if(null==e)return{};var t,n,i={},o=Object.keys(e);for(n=0;n<o.length;n++)t=o[n],r.indexOf(t)>=0||(i[t]=e[t]);return i}(e,r);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(e);for(n=0;n<o.length;n++)t=o[n],r.indexOf(t)>=0||Object.prototype.propertyIsEnumerable.call(e,t)&&(i[t]=e[t])}return i}var u=n.createContext({}),s=function(e){var r=n.useContext(u),t=r;return e&&(t="function"==typeof e?e(r):l(l({},r),e)),t},y=function(e){var r=s(e.components);return n.createElement(u.Provider,{value:r},e.children)},c="mdxType",p={inlineCode:"code",wrapper:function(e){var r=e.children;return n.createElement(n.Fragment,{},r)}},d=n.forwardRef((function(e,r){var t=e.components,i=e.mdxType,o=e.originalType,u=e.parentName,y=a(e,["components","mdxType","originalType","parentName"]),c=s(t),d=i,g=c["".concat(u,".").concat(d)]||c[d]||p[d]||o;return t?n.createElement(g,l(l({ref:r},y),{},{components:t})):n.createElement(g,l({ref:r},y))}));function g(e,r){var t=arguments,i=r&&r.mdxType;if("string"==typeof e||i){var o=t.length,l=new Array(o);l[0]=d;var a={};for(var u in r)hasOwnProperty.call(r,u)&&(a[u]=r[u]);a.originalType=e,a[c]="string"==typeof e?e:i,l[1]=a;for(var s=2;s<o;s++)l[s]=t[s];return n.createElement.apply(null,l)}return n.createElement.apply(null,t)}d.displayName="MDXCreateElement"},8665:(e,r,t)=>{t.r(r),t.d(r,{assets:()=>u,contentTitle:()=>l,default:()=>p,frontMatter:()=>o,metadata:()=>a,toc:()=>s});var n=t(9668),i=(t(6540),t(5680));const o={},l="Queries",a={unversionedId:"queries",id:"queries",title:"Queries",description:"Introduction",source:"@site/docs-general/04-queries.md",sourceDirName:".",slug:"/queries",permalink:"/queries",draft:!1,editUrl:"https://github.com/wemogy/libs-cqrs/edit/main/docs-general/04-queries.md",tags:[],version:"current",sidebarPosition:4,frontMatter:{},sidebar:"tutorialSidebar",previous:{title:"Delayed commands",permalink:"/delayed-commands"}},u={},s=[{value:"Introduction",id:"introduction",level:2},{value:"Principles",id:"principles",level:2},{value:"Query definition",id:"query-definition",level:2},{value:"Query handling",id:"query-handling",level:2},{value:"Query authorization",id:"query-authorization",level:2},{value:"Restricting query execution",id:"restricting-query-execution",level:3},{value:"Filtering",id:"filtering",level:3},{value:"IDatabaseRepositoryFilter",id:"idatabaserepositoryfilter",level:4},{value:"Custom filter",id:"custom-filter",level:4},{value:"Example: Hello world",id:"example-hello-world",level:2},{value:"The Query model",id:"the-query-model",level:3},{value:"The Query handler",id:"the-query-handler",level:3},{value:"Registering the query",id:"registering-the-query",level:3},{value:"Executing the query",id:"executing-the-query",level:3},{value:"FAQ",id:"faq",level:2},{value:"How to allow query executing only for specific context?",id:"how-to-allow-query-executing-only-for-specific-context",level:3},{value:"How to set properties based on a context?",id:"how-to-set-properties-based-on-a-context",level:3}],y={toc:s},c="wrapper";function p(e){let{components:r,...t}=e;return(0,i.yg)(c,(0,n.A)({},y,t,{components:r,mdxType:"MDXLayout"}),(0,i.yg)("h1",{id:"queries"},"Queries"),(0,i.yg)("h2",{id:"introduction"},"Introduction"),(0,i.yg)("p",null,"TBD..."),(0,i.yg)("h2",{id:"principles"},"Principles"),(0,i.yg)("ul",null,(0,i.yg)("li",{parentName:"ul"},"A Query should always only work exactly one way and not support multiple constuctors or execution ways (ref ",(0,i.yg)("a",{parentName:"li",href:"https://github.com/wemogy/libs-cqrs/issues/51"},"GitHub Issue"),")")),(0,i.yg)("h2",{id:"query-definition"},"Query definition"),(0,i.yg)("p",null,"If you want to create a new query, you need to create a class, which implements the generic ",(0,i.yg)("inlineCode",{parentName:"p"},"IQuery<TResult>")," interface, where the genic parameter represents the data type of the result. By nature every query has exactly one result."),(0,i.yg)("pre",null,(0,i.yg)("code",{parentName:"pre",className:"language-csharp"},"public class GetUserQuery : IQuery<User>\n{\n  public Guid Id { get; }\n\n  public GetUserQuery(Guid id)\n  {\n    Id = id;\n  }\n}\n")),(0,i.yg)("h2",{id:"query-handling"},"Query handling"),(0,i.yg)("p",null,"The actual implementation of the defined query goes in the query handler."),(0,i.yg)("pre",null,(0,i.yg)("code",{parentName:"pre",className:"language-csharp"},"public class GetUserQueryHandler : IQueryHandler<GetUserQuery, User>\n{\n  public Task<User> HandleAsync(GetUserQuery query, CancellationToken cancellationToken)\n  {\n    var user = new User()\n    {\n        Firstname = query.Id.ToString(),\n    };\n    return Task.FromResult(user);\n  }\n}\n")),(0,i.yg)("h2",{id:"query-authorization"},"Query authorization"),(0,i.yg)("h3",{id:"restricting-query-execution"},"Restricting query execution"),(0,i.yg)("p",null,"Please first have a look at the ",(0,i.yg)("a",{parentName:"p",href:"#filtering"},"Filtering")," section, which explains how to filter query results on repository level. However, if it's required to restrict the query execution itself, you can implement the ",(0,i.yg)("inlineCode",{parentName:"p"},"IQueryAuthorization<TQuery>")," interface."),(0,i.yg)("pre",null,(0,i.yg)("code",{parentName:"pre",className:"language-csharp"},'public class GetUserQueryAuthorization : IQueryAuthorization<GetUserQuery>\n{\n    public Task AuthorizeAsync(GetUserQuery query)\n    {\n        if (query.FirstName == "ThrowExceptionInGetUserQueryAuthorization")\n        {\n            throw Error.Authorization(\n                "Unauthorized",\n                "You are not allowed to access this resource.");\n        }\n\n        return Task.CompletedTask;\n    }\n}\n')),(0,i.yg)("h3",{id:"filtering"},"Filtering"),(0,i.yg)("p",null,"It's recommended to implement the filtering logic near to the data source request to minimize the risk that you query a data source somewhere in your application and you forget to filter which would lead to a ",(0,i.yg)("strong",{parentName:"p"},"data breach"),"."),(0,i.yg)("h4",{id:"idatabaserepositoryfilter"},"IDatabaseRepositoryFilter"),(0,i.yg)("p",null,"If your query internally talks to a ",(0,i.yg)("inlineCode",{parentName:"p"},"IDatabaseRepository")," service, it's recommended to put the filtering logic in a ",(0,i.yg)("inlineCode",{parentName:"p"},"DatabaseRepositoryFilter"),". For more information checkout: ",(0,i.yg)("a",{parentName:"p",href:"https://"},"MyDocs")),(0,i.yg)("h4",{id:"custom-filter"},"Custom filter"),(0,i.yg)("p",null,"If your query internally talks to an external API or any other custom data source and you need to filter the results based on the current context, it's recommended to implement this filtering logic in the wrapper implementation of the specific data source."),(0,i.yg)("h2",{id:"example-hello-world"},"Example: Hello world"),(0,i.yg)("p",null,"In this little sample of ",(0,i.yg)("inlineCode",{parentName:"p"},"Wemogy.CQRS")," we will implement a query without parameters with the belonging query handler. Moreover we will register ",(0,i.yg)("inlineCode",{parentName:"p"},"wemogy.CQRS")," in the dependency injection and finally execute the query to get a ",(0,i.yg)("inlineCode",{parentName:"p"},"Hello World!")," string back."),(0,i.yg)("h3",{id:"the-query-model"},"The Query model"),(0,i.yg)("p",null,"For each query its required to create a model of the query itself, which contains all information which are required to execute the query."),(0,i.yg)("pre",null,(0,i.yg)("code",{parentName:"pre",className:"language-csharp"},"using Wemogy.CQRS.Queries.Abstractions;\n\npublic class HelloWorldQuery : IQuery<string>\n{\n}\n")),(0,i.yg)("h3",{id:"the-query-handler"},"The Query handler"),(0,i.yg)("p",null,"The second mandatory implementation for a query is a query handler, which contains the actual implementation of the query action."),(0,i.yg)("pre",null,(0,i.yg)("code",{parentName:"pre",className:"language-csharp"},'using System.Threading.Tasks;\nusing Wemogy.CQRS.Queries.Abstractions;\n\npublic class HelloWorldQueryHandler : IQueryHandler<HelloWorldQuery, string>\n{\n  public Task<string> HandleAsync(HelloWorldQuery query)\n  {\n    return "Hello world!";\n  }\n}\n')),(0,i.yg)("h3",{id:"registering-the-query"},"Registering the query"),(0,i.yg)("p",null,"It's required to execute ",(0,i.yg)("inlineCode",{parentName:"p"},"services.AddCQRS();")," in your dependency injection file of the assembly which contains the queries. In addition its also supported to pass one or multiple assemblies to the ",(0,i.yg)("inlineCode",{parentName:"p"},"AddCQRS()")," extension method, in case that you need to call it from another assembly."),(0,i.yg)("h3",{id:"executing-the-query"},"Executing the query"),(0,i.yg)("p",null,"This sample is part of a .NET Core controller class."),(0,i.yg)("pre",null,(0,i.yg)("code",{parentName:"pre",className:"language-csharp"},"using Wemogy.CQRS.Queries.Abstractions;\n\npublic class HelloWorldController : ControllerBase\n{\n  private readonly IQueries _queries;\n\n  public HelloWorldController(IQueries queries)\n  {\n    _queries = queries;\n  }\n\n  [HttpGet]\n  public async Task<ActionResult> SayHelloWorld()\n  {\n    // creating the query with all required information\n    var helloWorldQuery = new HelloWorldQuery();\n\n    // executing the query though the IQueries mediator\n    var result = await _queries.QueryAsync(helloWorldQuery);\n\n    return Ok(result);\n  }\n}\n")),(0,i.yg)("h2",{id:"faq"},"FAQ"),(0,i.yg)("h3",{id:"how-to-allow-query-executing-only-for-specific-context"},"How to allow query executing only for specific context?"),(0,i.yg)("p",null,"As a developer I want to allow query execution only if the ",(0,i.yg)("inlineCode",{parentName:"p"},"IContext.IsAdmin")," flag is set to true. How should I implement this?"),(0,i.yg)("p",null,(0,i.yg)("strong",{parentName:"p"},"TBD...")),(0,i.yg)("h3",{id:"how-to-set-properties-based-on-a-context"},"How to set properties based on a context?"),(0,i.yg)("p",null,"As a developer I want to set the ",(0,i.yg)("inlineCode",{parentName:"p"},"User.CompletedTasksThisWeek")," property only for the current user when users are queried (for privacy reasons, to prevent that someone is monitoring the work speed). How should I implement this?"),(0,i.yg)("p",null,(0,i.yg)("strong",{parentName:"p"},"TBD...")))}p.isMDXComponent=!0}}]);