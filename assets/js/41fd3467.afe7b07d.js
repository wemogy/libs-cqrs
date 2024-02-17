"use strict";(self.webpackChunkwemogy=self.webpackChunkwemogy||[]).push([[912],{5680:(e,n,r)=>{r.d(n,{xA:()=>u,yg:()=>y});var t=r(6540);function a(e,n,r){return n in e?Object.defineProperty(e,n,{value:r,enumerable:!0,configurable:!0,writable:!0}):e[n]=r,e}function o(e,n){var r=Object.keys(e);if(Object.getOwnPropertySymbols){var t=Object.getOwnPropertySymbols(e);n&&(t=t.filter((function(n){return Object.getOwnPropertyDescriptor(e,n).enumerable}))),r.push.apply(r,t)}return r}function i(e){for(var n=1;n<arguments.length;n++){var r=null!=arguments[n]?arguments[n]:{};n%2?o(Object(r),!0).forEach((function(n){a(e,n,r[n])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(r)):o(Object(r)).forEach((function(n){Object.defineProperty(e,n,Object.getOwnPropertyDescriptor(r,n))}))}return e}function c(e,n){if(null==e)return{};var r,t,a=function(e,n){if(null==e)return{};var r,t,a={},o=Object.keys(e);for(t=0;t<o.length;t++)r=o[t],n.indexOf(r)>=0||(a[r]=e[r]);return a}(e,n);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(e);for(t=0;t<o.length;t++)r=o[t],n.indexOf(r)>=0||Object.prototype.propertyIsEnumerable.call(e,r)&&(a[r]=e[r])}return a}var d=t.createContext({}),l=function(e){var n=t.useContext(d),r=n;return e&&(r="function"==typeof e?e(n):i(i({},n),e)),r},u=function(e){var n=l(e.components);return t.createElement(d.Provider,{value:n},e.children)},s="mdxType",m={inlineCode:"code",wrapper:function(e){var n=e.children;return t.createElement(t.Fragment,{},n)}},p=t.forwardRef((function(e,n){var r=e.components,a=e.mdxType,o=e.originalType,d=e.parentName,u=c(e,["components","mdxType","originalType","parentName"]),s=l(r),p=a,y=s["".concat(d,".").concat(p)]||s[p]||m[p]||o;return r?t.createElement(y,i(i({ref:n},u),{},{components:r})):t.createElement(y,i({ref:n},u))}));function y(e,n){var r=arguments,a=n&&n.mdxType;if("string"==typeof e||a){var o=r.length,i=new Array(o);i[0]=p;var c={};for(var d in n)hasOwnProperty.call(n,d)&&(c[d]=n[d]);c.originalType=e,c[s]="string"==typeof e?e:a,i[1]=c;for(var l=2;l<o;l++)i[l]=r[l];return t.createElement.apply(null,i)}return t.createElement.apply(null,r)}p.displayName="MDXCreateElement"},6989:(e,n,r)=>{r.r(n),r.d(n,{assets:()=>d,contentTitle:()=>i,default:()=>m,frontMatter:()=>o,metadata:()=>c,toc:()=>l});var t=r(9668),a=(r(6540),r(5680));const o={},i="Delayed commands",c={unversionedId:"delayed-commands",id:"delayed-commands",title:"Delayed commands",description:"Idea",source:"@site/docs-general/03-delayed-commands.md",sourceDirName:".",slug:"/delayed-commands",permalink:"/delayed-commands",draft:!1,editUrl:"https://github.com/wemogy/libs-cqrs/edit/main/docs-general/03-delayed-commands.md",tags:[],version:"current",sidebarPosition:3,frontMatter:{},sidebar:"tutorialSidebar",previous:{title:"Commands",permalink:"/commands"},next:{title:"Queries",permalink:"/queries"}},d={},l=[{value:"Idea",id:"idea",level:2}],u={toc:l},s="wrapper";function m(e){let{components:n,...r}=e;return(0,a.yg)(s,(0,t.A)({},u,r,{components:n,mdxType:"MDXLayout"}),(0,a.yg)("h1",{id:"delayed-commands"},"Delayed commands"),(0,a.yg)("h2",{id:"idea"},"Idea"),(0,a.yg)("pre",null,(0,a.yg)("code",{parentName:"pre",className:"language-csharp"},'\nclass MyDelayedCommand : IDelayedCommand\n{\n  // properties...\n}\n\n// Startup:\nservices\n    .AddAzureServiceBus(...) // Returns AzureServiceBusSetupEnvironment\n    .WithAutomaticSubscription(enabled: handlingEnabled) // Returns AzureServiceBusSetupEnvironment\n    .AddSender<MyServiceBusSenderService>("my-queue-name") // Returns AzureServiceBusSetupEnvironment\n    .AddHandler<MyDelayedCommand, MyDelayedCommandHandler>("my-queue-name").When(handlingEnabled) // Returns AzureServiceBusSetupEnvironment\n\n')))}m.isMDXComponent=!0}}]);