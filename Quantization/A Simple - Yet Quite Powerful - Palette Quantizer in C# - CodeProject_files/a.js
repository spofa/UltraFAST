var _qevents,googletag;if(typeof DMAds=="undefined"){var SendSearchTermsToServer=window.location.protocol==="http:",OnlySendForCodeProject=!1,CodeProjectPublisherId="lqm.codeproject.site",QuantScriptRequired=!1,EnableMutableAds=!0,EnableViewOnScroll=!0,adServer=adServer||window.location.protocol+"//ads.DeveloperMedia.com/",SearchTermUrl=window.location.protocol+"//apps.developermedia.com/Ads/PageTerms/GetTerms",AdClickUrl=window.location.protocol+"//apps.developermedia.com/Ads/PageTerms/LogClick",DownvoteUrl=window.location.protocol+"//apps.developermedia.com/Ads/AdVote/DownvoteByFingerprint",UndoDownvoteUrl=window.location.protocol+"//apps.developermedia.com/Ads/AdVote/UndoDownvote",ReportAdUrl=window.location.protocol+"//apps.developermedia.com/Ads/AdVote/ReportAd",CloseAdImageUrl=window.location.protocol+"//apps.developermedia.com/Content/images/undo.png",UndoCloseAdImageUrl=window.location.protocol+"//apps.developermedia.com/Content/images/redo.png",DmLogoImageUrl=window.location.protocol+"//apps.developermedia.com/Content/images/dm-logo-150x23.png",GlobalIdUrl=window.location.protocol+"//apps.developermedia.com/Ads/GlobalUserIdentification/",DMAds={GetQueryTerms:function(){function u(n){for(var i,u="",t=0;t<r.length;t++)if(i=r[t],n.indexOf(i.d)>=0){u=i.q;break}return u}function f(n,t){var r=t.toLowerCase().indexOf(n),u,i;return r<0||r+n.length>=t.length?"":(u=t.indexOf("&",r),u<0&&(u=t.length),i=t.substring(r+n.length,u),i=i.replace(/\+/gi," "),i=decodeURIComponent(i),i.replace(/\"/gi,""))}function e(n){if(n===undefined)return"";var t=n.replace(/^\s+|\s+$/gi,"");return t?(t=t.replace(/\bAND\b|\bNOT\b|^NOT\b|\bOR\b|[^A-Z0-9\+\-\#\._\s]+|\b[A-Z0-9]+:/gi," "),t.replace(/\s+/g," ")):""}var r=[{d:"www.google.",q:"q="},{d:"www.bing.com",q:"q="},{d:"search.live.com",q:"q="},{d:"search.yahoo.com",q:"p="},{d:"codeproject.com",q:"q="},{d:"msdn.microsoft.com",q:"query="},{d:"www.ask.com",q:"q="},{d:"yandex.com",q:"text="},{d:"yandex.ru",q:"text="},{d:"www.baidu.com",q:"wd="},{d:"localhost",q:"q="},{d:"mkyong.com",q:"q="},{d:"codeplex.com",q:"query="},{d:"aspsnippets.com",q:"q="},{d:"trirand.com",q:"s="}],n=document.URL,i="",t=u(n);if(t!==""&&(i=e(f(t,n))),i==""){if(n=document.referrer.toLocaleLowerCase(),!n)return"";t=u(n);t!=""&&(i=e(f(t,n)))}return i},GetRandom:function(n,t){for(var u,i="",r=0;r<n;r++)u=Math.floor(Math.random()*t).toString(t).toUpperCase(),i=i+u;return i},PageRandomNumber:null,PageSearchTerms:null,Tile:1,CurrentDocument:null,CreateAdsCalled:!1,GPTRenderingMode:1,BuildIFrameTag:function(n){var t='<iframe id="dmad{tile}" allowtransparency="false" style="z-index:10" ';return t=n&&n.width&&n.width>1?t+'width="{width}" ':t+'width="100%" ',t=n&&n.height&&n.height>0?t+'height="{height}" ':t+'height="0" ',t=t+'marginwidth="0" marginheight="0" frameborder="0" scrolling="no"><\/iframe>',this.ReplacePlaceholders(t,n)},BuildJavaScriptTag:function(n){var t='<script language="JavaScript" src="'+window.location.protocol+'//ad.doubleclick.net/N6839/adj/{sitename}/{zonename};{searchterm}sz={format};{dc_ref}{type}tile={tile};{noadx}ord={timestamp}?" type="text/javascript"><\/script>';return this.ReplacePlaceholders(t,n)},BuildInlineGPTTagScript:function(n){var t="div-gpt-ad-"+n.sitename+"-"+n.zonename+"-"+n.tile,i="/6839/"+n.sitename+"/"+n.zonename,r="["+n.width+", "+n.height+"]",u=(n.tags||"")+","+(DMAds.PageSearchTerms||""),f="<div id='"+t+"'>     <script type='text/javascript'>         googletag.cmd.push(function() {             googletag.defineSlot('"+i+"', "+r+",'"+t+"')                 .addService(googletag.pubads())                 .setTargeting('kw', '"+u+"');             googletag.enableServices();             googletag.display('"+t+"');         });     <\/script> <\/div>";return"<script type='text/javascript'> var googletag = googletag || {}; googletag.cmd = googletag.cmd || []; (function() {     var gads = document.createElement('script');     gads.async = true;     gads.type = 'text/javascript';     var useSSL = 'https:' == document.location.protocol;     gads.src = (useSSL ? 'https:' : 'http:') + '//www.googletagservices.com/tag/js/gpt.js';     var node =document.getElementsByTagName('script')[0];     node.parentNode.insertBefore(gads, node); })(); <\/script>"+f},BuildGPTTag:function(n,t){var i=document.createElement("DIV"),r;i.style.height=n.height+"px";i.style.width=n.width+"px";i.style.boxSizing="border-box";i.style.border="0px";i.style.fontSize="0px";r="div-gpt-ad-"+n.sitename+"/"+n.zonename+"-"+n.tile;i.id=r;i.adIndex=n.index;t.appendChild(i);googletag.cmd.push(function(){var i="/6839/"+n.sitename+"/"+n.zonename,u=(n.tags||"")+","+(DMAds.PageSearchTerms||""),t=googletag.defineSlot(i,[n.width,n.height],r).setTargeting("kw",u).addService(googletag.pubads());n.collapse_empty==="true"&&t.setCollapseEmptyDiv(!0,!0);googletag.display(r);googletag.pubads().refresh([t])})},BuildInlineGPTTag:function(n,t){var i,r,u;if(t.innerHTML=DMAds.BuildIFrameTag(n),i=t.getElementsByTagName("iframe")[0],i.onerror=function(){return!0},r=i.contentDocument||i.contentWindow.document||i.contentWindow.window.document,u=function(){n.height<=1&&(this.height=GetDocHeight(r));DMAds.HideRefs(r,t,n)},i.addEventListener)i.addEventListener("load",u,!1);else try{i.attachEvent("onload",u)}catch(f){}return r.write(DMAds.BuildInlineGPTTagScript(n)),r},ReplacePlaceholders:function(n,t){n=n.replace(/\{sitename\}/g,t.sitename);n=n.replace(/\{zonename\}/g,t.zonename);n=t.tags?n.replace(/\{searchterm\}/g,"kw="+encodeURIComponent(this.EscapeSpecialCharacters(t))+";"):n.replace(/\{searchterm\}/g,"");n=n.replace(/\{tile\}/g,t.tile.toString());n=n.replace(/\{format\}/g,t.format);n=n.replace(/\{width\}/g,t.width);n=n.replace(/\{height\}/g,t.height);n=n.replace(/\{target\}/g,t.target);n=n.replace(/\{timestamp\}/g,this.PageRandomNumber);n=t.type?n.replace(/\{type\}/g,"type="+encodeURIComponent(t.type)+";"):n.replace(/\{type\}/g,"");n=t.noadx&&t.noadx.toLowerCase()==="true"?n.replace(/\{noadx\}/g,"noadx=true;"):n.replace(/\{noadx\}/g,"");var i=encodeURIComponent(location.href);return n.length+i.length<2048?n.replace(/\{dc_ref\}/g,"dc_ref="+i+";"):n.replace(/\{dc_ref\}/g,"")},EscapeSpecialCharacters:function(n){var t=n.tags,i;t=t.replace(/\+/gi,"{plus}");t=t.replace(/\#/gi,"{sharp}");t=t.replace(/\./gi,"{dot}");t=t.replace(/[\#\*\.\(\)\+\<\>\[\]]/gi,"");for(var r=t.split(","),u=[];r.length>0;)i=r.shift(),/[^\u0020-\u007f]/.test(i)||u.push(i);return u.join(",")},IsDragAndDropSupported:function(){var n=document.createElement("div");return"draggable"in n||"ondragstart"in n&&"ondrop"in n},IsFireFox:function(){return navigator.userAgent.toLowerCase().indexOf("firefox")>-1},B64toBlob:function(n,t,i){var e,o,r,f,s,u,h;for(t=t||"",i=i||512,e=atob(n),o=[],r=0;r<e.length;r+=i){for(f=e.slice(r,r+i),s=new Array(f.length),u=0;u<f.length;u++)s[u]=f.charCodeAt(u);h=new Uint8Array(s);o.push(h)}return new Blob(o,{type:t})},tagInfo:[{id:1,n:"Standard Banner",h:60,w:468},{id:2,n:"Product Spotlight",h:2,w:1},{id:3,n:"Hosting Spotlight",h:2,w:1},{id:4,n:"Skyscraper",h:600,w:120},{id:5,n:"Square",h:125,w:125},{id:6,n:"Medium Rectangle",h:250,w:300},{id:7,n:"Large Rectangle",h:280,w:336},{id:8,n:"Leaderboard",h:90,w:728},{id:9,n:"HTML Ad",h:0,w:0},{id:10,n:"Fixed Square",h:125,w:125},{id:11,n:"Fixed Banner",h:60,w:468},{id:12,n:"Half Skyscraper",h:300,w:120},{id:13,n:"IAB Button",h:90,w:120},{id:14,n:"Rectangle",h:120,w:150},{id:15,n:"Thin Horizontal",h:27,w:408},{id:16,n:"Button",h:30,w:100},{id:17,n:"DogEar",h:0,w:0},{id:18,n:"Wide Skyscraper",h:600,w:160},{id:19,n:"Tracking Only",h:1,w:1},{id:20,n:"Mixed 120x90-Text",h:5,w:1},{id:21,n:"Home page top left (150 X 80)",h:80,w:150},{id:22,n:"SponsorEmail",h:0,w:0},{id:23,n:"Email",h:60,w:60},{id:24,n:"TextLinks",h:0,w:0},{id:25,n:"Zone",h:0,w:0},{id:26,n:"Goal group",h:0,w:0},{id:27,n:"Article",h:0,w:0},{id:28,n:"Search Sponsor Box",h:30,w:120},{id:29,n:"Microbar",h:31,w:88},{id:30,n:"Sponsor Link",h:1,w:0}],DetermineTagSize:function(n){var i,u,r,t;if(n.format)if(isNaN(n.format))i=n.format.split("x"),i.length==2&&(isFinite(i[0])&&(n.width=i[0]),isFinite(i[1])&&(n.height=i[1]));else for(u=!1,r=0;r<this.tagInfo.length&&!u;)t=this.tagInfo[r],t.id==n.format&&(t.w!=0&&(n.width=t.w),t.h!=0&&(n.height=t.h),n.type=t.name,u=!0,n.format=""+t.w+"x"+t.h),r++},MapDmIdsToDart:function(n){var t="lqm.",i=".site";n.publisher?(n.sitename=isNaN(n.publisher)?n.publisher:t+"pub"+n.publisher+i,this.MapDmZoneToDartZone(n)):n.site&&(n.sitename=t+"codeplex"+i,n.zonename=n.charity?"donated2charity":n.site.toLowerCase())},GetRequestData:function(n){var e=[],u,r,f;for(adIndex=0;adIndex<n.length;adIndex++){var s=n[adIndex],t={height:0,width:0,publisher:undefined,zone:undefined,site:undefined,tags:undefined,sitename:undefined,zonename:undefined,target:undefined,format:undefined,tile:undefined,type:undefined,noadx:!1,collapse_empty:!1},o=s.attributes,h=o.length,i={};for(u=0;u<h;u++)r=o.item(u),r.nodeName.indexOf("lqm_")==0&&(f=r.nodeName.slice(4),i[f]=r.value||r.nodeValue),r.nodeName.indexOf("data-")==0&&(f=r.nodeName.slice(5),i[f]=r.value||r.nodeValue);t.publisher=i.publisher;t.zone=i.zone;t.site=i.site;t.tags=i.tags;t.format=i.format;t.charity=i.charity;t.noadx=i.noadx;t.collapse_empty=i.collapse_empty;t.tile=adIndex+1;t.index=adIndex;t.target="_blank";t.tags&&(t.tags=decodeURIComponent(t.tags));DMAds.DetermineTagSize(t);t.width=parseInt(t.width);t.height=parseInt(t.height);DMAds.MapDmIdsToDart(t);e[adIndex]=t}return e},zoneInfo:[{id:1,n:"ron"},{id:51,n:"it"},{id:52,n:"designer"},{id:2,n:"above_the_fold"},{id:9,n:"wpf"},{id:14,n:"silverlight"},{id:3,n:"reportingservices"},{id:4,n:"sql"},{id:5,n:"whitepaper"},{id:6,n:"featuredwhitepaper"},{id:7,n:"crystalreports"},{id:10,n:"vs2005video"},{id:11,n:"ros_dogear"},{id:12,n:"homepage_dogear"},{id:13,n:"excludehomepage_dogear"},{id:15,n:"lqm_dogear"},{id:17,n:"mvc"},{id:18,n:"ajax"},{id:38,n:"devexpress_video"},{id:39,n:"devmavens_sidebar"},{id:40,n:"devmavens_offer"},{id:44,n:"silverlight"},{id:45,n:"wpf"},{id:54,n:"csharp_articles"}],MapDmZoneToDartZone:function(n){if(n.zone){for(var i=!1,t=0;t<this.zoneInfo.length&&!i;)this.zoneInfo[t].id==n.zone&&(n.zonename=this.zoneInfo[t].n,i=!0),t++;i||(n.zonename=isNaN(n.zone)?n.zone.toLowerCase():"ron")}else n.zonename="ron"},GetDocHeight:function(n){return n.height||n.body&&n.body.scrollHeight},HideRefs:function(n,t,i){var e=this,u,f,o,r;for(i.format.indexOf("1x")===0?(t.innerHTML=n.body.innerHTML,u=t):u=n,f=u.getElementsByTagName("a"),o=function(n){var t=n.href,i,f=t.indexOf("&adurl="),r,u;if(i=f>0?decodeURIComponent(t.substring(f+7)):adServer+e.GetRandom(4,16)+"-"+e.GetRandom(7,16),n.href=i,r=function(){n.href=t},u=function(){n.href=i},n.addEventListener)n.addEventListener("mousedown",r,!1),n.addEventListener("mouseover",u,!1);else try{n.attachEvent("onmousedown",r);n.attachEvent("onmouseover",u)}catch(o){}},r=0;r<f.length;r++)o(f[r])},GetDomain:function(){var t=document.location.hostname,n=/([^.]+\.[^.]{3,})$/i.exec(t);return n!=null?n[1]:(n=/([^.]+\.[^.]+\.[^.]{2})$/i.exec(t),n!=null?n[1]:t)},SetCookie:function(n,t,i,r,u,f){var e="",o;e=n+"="+t;i>0&&(o=new Date,o.setTime(o.getTime()+i*864e5))&&(e+="; expires="+o.toGMTString());r&&(e+="; path="+r);u&&u.indexOf(".")!=-1&&(e+="; domain="+u);f&&(e+="; secure");document.cookie=e},GetCookieValue:function(n){var r=document.cookie,u=null,t,i;if(r!="")for(t=r.split(";"),index=0;index<t.length;index++)if(i=t[index].replace(/^\s+/,""),i.substring(0,n.length+1)==n+"="){u=i.substring(n.length+1);break}return u},CookiesEnabled:function(){return navigator.cookieEnabled!=void 0?navigator.cookieEnabled:(document.cookie="testcookie=test; max-age=10000",document.cookie.indexOf("testcookie=test")>=0)},CreateDMIFrame:function(n){var t=document.createElement("IFRAME");return t.width=1,t.height=1,t.src=GlobalIdUrl,t.id="DMGlobalUserIdetifierIFRAME",t.name="DMGlobalUserIdetifierIFRAME",t.style.display="none",document.body.appendChild(t),t.onload=n,t},DmGlobalUserId:0,DmGlobalUserIdKey:"dmaduid",DmGlobalUserIdCookieConfirmedKey:"dmaduid_confirm",PublisherPageViewID:0,PublisherPageViewGuid:""};if(typeof DMAds.CreateAds!="function"&&(DMAds.CreateAds=function(){var s,h,v;if(!DMAds.CreateAdsCalled){DMAds.CreateAdsCalled=!0;s=this;h=1e3;this.PageRandomNumber=this.GetRandom(32,16);var it=function(n,t,i,r){var o=[],u,e;i==null&&(i=document);r==null&&(r="*");var f=i.getElementsByTagName(r),s=f.length,h=new RegExp("(^|\\s)"+t+"(\\s|$)");for(u=0,e=0;u<s;u++)h.test(f[u].getAttribute(n))&&(o[e]=f[u],e++);return o},rt=function(n,t,i){var e=[],r,f;t==null&&(t=document);i==null&&(i="*");var u=t.getElementsByTagName(i),o=u.length,s=new RegExp("(^|\\s)"+n+"(\\s|$)");for(r=0,f=0;r<o;r++)s.test(u[r].className)&&(e[f]=u[r],f++);return e},ut=function(n,t,i){var u=50,r=0,f=window.setInterval(function(){var e=s.GetDocHeight(n);e>0&&((--u==0||e===r)&&(window.clearInterval(f),s.HideRefs(n,i,t)),r=e)},100)},ft=function(n,t){var i=document.createElement("input"),r;i.type="image";i.src=CloseAdImageUrl;i.title="Report Ad";i.style.cssText="z-index:1000; position:relative; left:0px; top:-"+n.height+"px;margin-top: 0px; margin-left: 0px; display:block; font-size:0px; border: 0px; padding: 0px; height:14px; width:14px";r=et(n);n.closeAdButton=i;n.adContainer=t;n.closeAdDisplay=r;t.appendChild(i);t.parentNode.appendChild(r);i.onclick=n.width==125&&n.height==125?function(){return y(n),p(n),!1}:function(){return y(n),!1}},et=function(n){var i=n.width,r=n.height,t=document.createElement("div"),e="<select class='reportReason' style='max-width:150px !important'><option>Offensive<\/option><option>Abusive<\/option><option>Off topic<\/option><option>Don't like the Ad<\/option><option>Wrong language<\/option><\/select>",o="<input type='button' disabled class='reportButton' padding='0px' value='Report'><\/input>",u,f;return i==728&&r==90?(t.innerHTML="<div class='sendReportContainer' style='padding-left: 2px; display:block'><div style='display:inline-block; font:14px/18px \"Segoe UI\", Arial'><b>Don't like this Ad?<\/b><\/div><div class='dropzone' contenteditable='true' style='margin-left:10px; height:60px; width: 250px; background-color:lightgray; border-style:dotted; border-color:black; border-radius: 5px; border-width: 2px; text-align: center; display:inline-block'>1. Hit the refresh icon to show the ad again and take a screenshot. 2. Drag and Drop or paste the screenshot here<\/div><div style='margin-left:10px; display:inline-block; font:14px/18px \"Segoe UI\", Arial;'> "+e+"&nbsp;"+o+"<\/div><\/div><div class='reportSentContainer' style='display:none; padding: 15px 20px; font:14px/18px \"Segoe UI\", Arial;'><span style='color:#999'>Thank you for the report!<\/span><\/div><a href='http://www.developermedia.com/' target='_blank'><img src='"+DmLogoImageUrl+"' style='max-width:100%;position:absolute; right:10px; top:10px;'><\/a>",t.style.cssText="display:none; width:"+i+"px; height:"+r+"px; z-index:100; text-align:left;border-style:solid; border-width:1px; position:relative; background-color:white;box-sizing:border-box; top:0px; left:0px"):i==300&&r==250?(t.innerHTML="<div class='sendReportContainer' style='padding: 5px 5px; display:block'><div style='padding-bottom:5px; font:14px/18px \"Segoe UI\", Arial;'><b>Don't like this Ad?<\/b><\/div><div class='dropzone' contenteditable='true' style='height:100px; width: 290px; margin-top:20px; margin-bottom: 20px; background-color:lightgray; border-style:dotted; border-color:black; border-radius: 5px; border-width: 2px; text-align: center; box-sizing:border-box;'>1. Hit the refresh icon to show the ad again so you can take a screenshot <br/>2. Drag and Drop or paste the screenshot here<\/div><div style='margin-top:5px; font:14px/18px \"Segoe UI\", Arial; '> "+e+"<\/div><div style='margin-top:5px; display:block; font:14px/18px \"Segoe UI\", Arial;'>"+o+"<\/div><\/div><div class='reportSentContainer' style='display:none; padding: 15px 20px; font:14px/18px \"Segoe UI\", Arial; '><span style='color:#999;'>Thank you for the report!<\/span><\/div><a href='http://www.developermedia.com/' target='_blank'><img src='"+DmLogoImageUrl+"' style='max-width:100%;position:absolute; right:20px; bottom:10px;'><\/a>",t.style.cssText="display:none; width:"+i+"px; height:"+r+"px; z-index:100; text-align:left;border-style:solid; border-width:1px; position:relative; background-color:white;box-sizing:border-box; top:0px; left:0px"):i==160&&r==600?(t.innerHTML="<div class='sendReportContainer' style='padding: 20px 15px; display:block'><div style='padding-bottom:20px; display:block; font:14px/18px \"Segoe UI\", Arial;'><b>Don't like this Ad?<\/b><\/div><div class='dropzone' contenteditable='true' style='height:180px; width: 130px; margin-top:20px; margin-bottom: 20px; background-color:lightgray; border-style:dotted; border-color:black; border-radius: 5px; border-width: 2px; text-align: center; box-sizing:border-box;'>1. Hit the refresh icon to show the ad again so you can take a screenshot <br/><br/>2. Drag and Drop or paste the screenshot here<\/div><div style='padding-bottom:10px; padding-top: 10px display:block; font:14px/18px \"Segoe UI\", Arial;'> "+e+"<\/div><div style='font:14px/18px \"Segoe UI\", Arial;'>"+o+"<\/div><\/div><div class='reportSentContainer' style='display:none; padding: 15px 20px; font:14px/18px \"Segoe UI\", Arial;'><span style='color:#999;'>Thank you for the report!<\/span><\/div><a href='http://www.developermedia.com/' target='_blank'><img src='"+DmLogoImageUrl+"' style='max-width:120px;position:absolute; right:20px; bottom:20px;'><\/a>",t.style.cssText="display:none; width:"+i+"px; height:"+r+"px; z-index:100; text-align:left;border-style:solid; border-width:1px; position:relative; background-color:white;box-sizing:border-box; top:0px; left:0px"):i==125&&r==125&&(t.innerHTML="<div class='reportSentContainer' style='padding: 5px 5px;font:14px/18px \"Segoe UI\", Arial;'><span style='color:#999;'>Thank you for the report!<\/span><\/div>",t.style.cssText="display:none; width:"+i+"px; height:"+r+"px; z-index:100; text-align:left;border-style:solid; border-width:1px; position:relative; background-color:white;box-sizing:border-box; top:0px; left:0px"),(i==728&&r==90||i==300&&r==250||i==160&&r==600)&&(n.reportButton=t.getElementsByClassName("reportButton")[0],u=t.getElementsByClassName("dropzone")[0],u.adIndex=n.index,u.addEventListener("paste",st,!1),n.dropzone=u,DMAds.IsDragAndDropSupported()&&(u.ondragenter=function(n){n.currentTarget.style.backgroundColor="yellow";n.currentTarget.innerHTML="<br/><br/>Drag and Drop the ad here"},u.ondragleave=function(n){n.currentTarget.style.backgroundColor="lightgray";at(n)},u.ondragover=function(n){n.stopPropagation();n.preventDefault();n.currentTarget.style.backgroundColor="yellow"},u.ondrop=function(n){n.currentTarget.style.backgroundColor="lightgreen";ct(n)},u.onmouseover=function(n){lt(n)}),DMAds.IsFireFox()&&u.addEventListener("DOMSubtreeModified",function(){for(var n,t=this,i=0;i<t.children.length;i++)n=t.children[i],n.nodeName.toLowerCase()==="img"&&n.style.display!="none"&&n.src&&(n.style.display="none",ht(n.src,t))})),f=document.createElement("input"),f.type="image",f.src=UndoCloseAdImageUrl,f.title="Show Ad",f.style.cssText="z-index:1000; position:relative; left:0px; top:0px; width:14px; height:14px;margin-top: 0px; margin-left: 0px; font-size:0px; display:block; padding: 0px; border: 0px",t.insertBefore(f,t.children[0]),f.onclick=function(){return w(n),!1},t},ot=function(n){var r,i,u;EnableMutableAds&&n&&!n.isEmpty&&(!n.lineItemId||n.lineItemId&&n.lineItemId==0)?(i=document.getElementById(n.slot.getSlotElementId()),r=t[i.adIndex],r.contentUrl=n.slot.getContentUrl&&n.slot.getContentUrl(),ft(r,i)):n&&n.isEmpty&&(i=document.getElementById(n.slot.getSlotElementId()),i.style.width="0px",i.style.height="0px",i.style.display="none");b(document);u=document.getElementById("contentUrl");u.value=n.slot.getContentUrl()},y=function(n){w(n)},p=function(n){var t={},i,u,f;t.DmGlobalUserId=DMAds.DmGlobalUserId;i=n.closeAdDisplay.getElementsByClassName("reportReason")[0];t.reason=i&&i.options[i.selectedIndex].value||"Unspecified";u=n.closeAdDisplay.getElementsByClassName("sendReportContainer")[0];f=n.closeAdDisplay.getElementsByClassName("reportSentContainer")[0];u&&(u.style.display="none");f&&(f.style.display="block");try{var s=DownvoteUrl,e=new XMLHttpRequest,r=new FormData,o=null;n.fileList&&n.fileList.length>0&&(o=n.fileList[0]);e.open("POST",s,!0);r.append("adScreenshot",o);r.append("reason",t.reason);r.append("DmGlobalUserId",t.DmGlobalUserId);e.send(r)}catch(h){}},st=function(n){var i,u,o,r,e,s,h;if(!DMAds.IsFireFox()){if(n.preventDefault(),i=[],u=!1,window.clipboardData)i=window.clipboardData.files,u=!0;else if((n.clipboardData||n.originalEvent.clipboardData)&&(o=n.clipboardData||n.originalEvent.clipboardData,r=o&&o.items||[],i=[],r&&r.length>0))for(u=!0,e=0;e<r.length;e++)s=r[e],s.kind==="file"&&(i[i.length]=s.getAsFile());i&&i.length>0?(h=n.currentTarget.adIndex,t[h].fileList=i,f(h,n.currentTarget)):n.currentTarget.innerHTML=u?"<br/><br/> No files found on clipboard <br/>":"<br/><br/> Paste not supported on your browser <br/>"}},ht=function(n,i){if(n){var u=n.indexOf(","),r=n.substring(0,u),o=r.indexOf(":"),s=r.indexOf(";"),h=r.substring(o+1,s),c=n.substring(u+1),l=DMAds.B64toBlob(c,h,512),e=i.adIndex;t[e].fileList=[l];f(e,i)}},ct=function(n){var i;n.stopPropagation();n.preventDefault();var u=n.currentTarget.adIndex,r=[];if(n.dataTransfer.files&&n.dataTransfer.files.length>0)for(i=0;i<n.dataTransfer.files.length;i++)n.dataTransfer.files[i].type&&n.dataTransfer.files[i].type.indexOf("image")>=0&&(r[r.length]=n.dataTransfer.files[i]);r.length>0?(t[u].fileList=r,f(u,n.currentTarget)):(n.currentTarget.style.backgroundColor="yellow",n.currentTarget.innerHTML="<br/><br/>The dropped item was not an image")},lt=function(n){n.buttons>0&&f(n.currentTarget.adIndex,n.currentTarget)},at=function(n){n.x==0&&n.y==0&&f(event.currentTarget.adIndex,event.currentTarget)},f=function(n,i){var r=t[n];i.style.backgroundColor="lightgreen";i.innerHTML=r.width==160&&r.height==600?"<br/> Your screenshot has been received \u2013 thanks! <br/><br/> Next step: What\u2019s wrong with the ad":r.width==300&&r.height==250?"<br/> Your screenshot has been received \u2013 thanks! <br/><br/> Next step: What\u2019s wrong with the ad":r.width==728&&r.height==90?"Your screenshot has been received \u2013 thanks! Next step: What\u2019s wrong with the ad":"Ad info received. Press Report to submit";r.reportButton.disabled=!1;r.reportButton.onclick=function(){return p(t[n]),!1}},o=function(n,t,i,r){var u=window.XMLHttpRequest?new XMLHttpRequest:new ActiveXObject("MSXML2.XMLHTTP"),e=JSON.stringify(t),f=!1;server_write_timeout=setTimeout(function(){r?u.abort():(f=!0,i(null))},h);u.onreadystatechange=function(){try{u.readyState==4&&(clearTimeout(server_write_timeout),u.status==200?f||i(u.responseText):f||i(null))}catch(n){clearTimeout(server_write_timeout);f||i(null)}};try{u.open("POST",n,!0);u.setRequestHeader("Content-Type","application/json");u.send(e)}catch(o){clearTimeout(server_write_timeout);i(null)}},w=function(n){n.closeAdDisplay.style.display==="none"?(n.closeAdDisplay.style.display="table",n.closeAdButton.style.display="none",n.adContainer.style.display="none",n.dropzone.focus()):(n.closeAdDisplay.style.display="none",n.closeAdButton.style.display="block",n.adContainer.style.display="block")},b=function(n){setTimeout(function(){n&&n.body&&top.postMessage&&top.postMessage(n.body.innerHTML?"DM-enabled":"DM-disabled","*")},1e3)},k=function(i){var u=n[i],f=t[i];if(DMAds.GPTRenderingMode==1)DMAds.BuildGPTTag(f,u);else{var r=DMAds.BuildInlineGPTTag(f,u),e=navigator.userAgent&&navigator.userAgent.indexOf("MSIE")>=0,o=navigator.userAgent&&navigator.userAgent.indexOf("Opera")>=0;e||o||!r.close||r.close();(e||o)&&ut(r,f,u);b(r)}},vt=function(i){var c=n[i],l=t[i],e=!1,u=0,f=0,r,o,s,h;try{r=c.getBoundingClientRect();typeof innerWidth=="number"?(u=window.innerWidth,f=window.innerHeight):document.documentElement&&(document.documentElement.clientWidth||document.documentElement.clientHeight)?(u=document.documentElement.clientWidth,f=document.documentElement.clientHeight):document.body&&(document.body.clientWidth||document.body.clientHeight)&&(u=document.body.clientWidth,f=document.body.clientHeight);o=-200;r.top==r.bottom&&(o+=l.height*-1);s=r.top>=0&&r.top-200<=f||r.top<=0&&r.top>=o;h=u>r.left&&r.right>=0;e=s&&h}catch(a){e=!0}return e},r=function(){for(var i=0,t=0;t<n.length;t++)e[t]===!1&&vt(t)&&a(t)&&(e[t]=!0,k(t)),e[t]===!0&&(i++,u[t]||(u[t]=d(t)),nt(t));i==n.length&&clearInterval(tt)},yt=function(t,i,r){var b=window.XMLHttpRequest?new XMLHttpRequest:new ActiveXObject("MSXML2.XMLHTTP"),f,c,u,e,l,y,a,s,p,v,w;if(!b||!JSON)return r();f=document.URL;f.indexOf("?")>0&&(f=f.substring(0,f.indexOf("?")));try{c=top.document.title}catch(k){c="FAILED TO GET DOCUMENT TITLE"}if(u={},u.terms=t,u.title=c,u.url=f,u.publisher=i,u.DmGlobalUserId=DMAds.DmGlobalUserId,u.numberOfAdsOnPage=n!=null?n.length:0,e=function(n){if(clearTimeout(server_write_timeout),n){var i=JSON.parse(n);i?(i.SearchTerms&&(DMAds.PageSearchTerms=t+","+i.SearchTerms),i.PublisherPageViewGuid&&(DMAds.PublisherPageViewGuid=i.PublisherPageViewGuid)):DMAds.PageSearchTerms=t}r()},DMAds.CookiesEnabled())if(l=DMAds.GetCookieValue(DMAds.DmGlobalUserIdKey),y=DMAds.GetCookieValue(DMAds.DmGlobalUserIdCookieConfirmedKey),!y&&JSON){if(a=!1,s={},s.sender=location.href,s.Id=DMAds.PageRandomNumber,receiveMessageTimeout=setTimeout(function(){a=!0;o(SearchTermUrl,u,e,!1)},h),p=function(){w.contentWindow.postMessage(JSON.stringify(s),GlobalIdUrl)},v=function(n){var t=null,i;try{t=JSON.parse(n.data)}catch(r){}t&&t.Id&&t.Id===s.Id&&(clearTimeout(receiveMessageTimeout),i=DMAds.GetDomain(),DMAds.SetCookie(DMAds.DmGlobalUserIdKey,t.DmGlobalUserId,18250,"/",i,!1),DMAds.SetCookie(DMAds.DmGlobalUserIdCookieConfirmedKey,t.DmGlobalUserConfirmResponse,18250,"/",i,!1),u.DmGlobalUserId=t.DmGlobalUserId,DMAds.DmGlobalUserId=t.DmGlobalUserId,a||o(SearchTermUrl,u,e,!1))},window.addEventListener)window.addEventListener("message",v,!1);else try{window.attachEvent("message",v)}catch(d){}w=DMAds.CreateDMIFrame(p)}else u.DmGlobalUserId=l,DMAds.DmGlobalUserId=l,o(SearchTermUrl,u,e,!1);else o(SearchTermUrl,u,e,!1)},c=function(){var u=function(){typeof googletag.pubadsReady=="undefined"?(googletag.pubads().enableSingleRequest(),googletag.pubads().disableInitialLoad(),googletag.pubads().enableAsyncRendering(),googletag.enableServices(),DMAds.GPTRenderingMode=1,googletag.pubads().addEventListener("slotRenderEnded",function(n){ot(n)})):DMAds.GPTRenderingMode=2},f=function(){for(var i,f,o=n[0].getAttribute("lqm_publisher")||n[0].getAttribute("data-publisher"),u=EnableViewOnScroll,t=0;t<n.length;t++)e[t]=!1,i=n[t].getAttribute("data-display")||n[t].getAttribute("lqm_loadOnView"),i==="onscroll"||i==="true"?u=!0:(i==="always"||i==="false")&&(u=!1),!u&&a(t)&&(k(t),e[t]=!0);if(r(),window.addEventListener)window.addEventListener("resize",function(){r();g()},!1),window.addEventListener("scroll",r,!1);else try{window.attachEvent("onresize",function(){r();g()});window.attachEvent("onscroll",r)}catch(s){}for(pt(),wt(QuantScriptRequired),f=!1,t=0;t<n.length;t++)a(t)||(f=!0);f&&(tt=setInterval(r,100))},t=function(){googletag.cmd.push(u);googletag.cmd.push(f)},i;window.googletag&&googletag.apiReady?t():i=setInterval(function(){window.googletag&&googletag.apiReady&&(clearInterval(i),t())},100)},pt=function(){var n=null;typeof document.hidden!="undefined"?n="visibilitychange":typeof document.mozHidden!="undefined"?n="mozvisibilitychange":typeof document.msHidden!="undefined"?n="msvisibilitychange":typeof document.webkitHidden!="undefined"&&(n="webkitvisibilitychange");n&&document.addEventListener(n,r,!1)},wt=function(n){var t,i;n&&(t=document.createElement("script"),t.src=(document.location.protocol=="https:"?"https://secure":"http://edge")+".quantserve.com/quant.js",t.async=!0,t.type="text/javascript",i=document.getElementsByTagName("script")[0],i.parentNode.insertBefore(t,i))},d=function(t){if(n[t].children[0]){var r=l(n[t].children[0]),i={};return(i.isStickyRequired=n[t].getAttribute("data-sticky")&&n[t].getAttribute("data-sticky").toLowerCase()==="top",i.absoluteTop=r.y,i.absoluteLeft=r.x,i.absoluteTop===-1)?null:(i.originalPosition=n[t].style.position,i)}},g=function(){for(i=0;i<u.length;i++)u[i]&&n&&n.length>i&&n[i]&&n[i].children&&n[i].children.length&&n[i].children[0]&&(n[i].children[0].style.position=u[i].originalPosition,u[i]=d(i),nt(i))},nt=function(t){var i=u[t],r,s,o,h,f,e;i&&i.isStickyRequired&&n[t].children[0].getBoundingClientRect&&document.getElementsByClassName&&(r=document.getElementsByClassName("sticky-stop")[0],r&&(s=l(r),o=r.getBoundingClientRect()),h=l(n[t].children[0]),f=n[t].children[0].getBoundingClientRect(),r&&h.y+f.height+Math.abs(f.top)>s.y&&o.top<f.height?e=o.top-f.height+"px":window.pageYOffset+10>=i.absoluteTop&&(e="10px"),e?(n[t].children[0].style.position="fixed",n[t].children[0].style.top=e,n[t].children[0].style.left=i.absoluteLeft-window.pageXOffset+"px",n[t].clientHeight===0&&(n[t].style.height=n[t].children[0].clientHeight+"px"),n[t].clientWidth===0&&(n[t].style.width=n[t].children[0].clientWidth+"px")):n[t].children[0].style.position=i.originalPosition)},l=function(n){var t={};if(t.x=-1,t.y=-1,n.getBoundingClientRect){var r=n.getBoundingClientRect(),i=document.documentElement,u=window.pageYOffset||i.scrollTop||document.body.scrollTop||0,f=window.pageXOffset||i.scrollLeft||document.body.scrollLeft||0,e=i.clientTop||document.body.clientTop||0,o=i.clientLeft||document.body.clientLeft||0;t.y=r.top+u-e;t.x=r.left+f-o}return t},a=function(t){var i=n[t],r=!1,u;return i&&i.style.position!="fixed"&&i.offsetParent?r=!0:i&&i.style.position=="fixed"&&(u=window.getComputedStyle(i),r=u&&u.display!="none"),r&&bt()},bt=function(){var t=!0,n="";return typeof document.hidden!="undefined"?n="hidden":typeof document.mozHidden!="undefined"?n="mozHidden":typeof document.msHidden!="undefined"?n="msHidden":typeof document.webkitHidden!="undefined"&&(n="webkitHidden"),n!=null&&document[n]&&(t=!1),t},e=[],u=[],t=[];this.PageSearchTerms==null&&(this.PageSearchTerms=this.GetQueryTerms());var kt=this.PageSearchTerms,tt=null,n=rt("lqm_ad",document,"div");if((n==null||n.length<=0)&&(n=it("data-type","ad",document,"div")),n!=null&&n.length>0)if(t=DMAds.GetRequestData(n),v=t[0].sitename,SendSearchTermsToServer&&(!OnlySendForCodeProject||v===CodeProjectPublisherId))try{yt(kt,v,c)}catch(dt){c()}else c()}}),document.readyState==="complete")DMAds.CreateAds();else if(window.addEventListener)window.addEventListener("load",function(){DMAds.CreateAds()},!1),window.addEventListener("DOMContentLoaded",function(){DMAds.CreateAds()},!1);else try{window.attachEvent("onload",function(){DMAds.CreateAds()})}catch(e){DMAds.CreateAds()}_qevents=_qevents||[];_qevents.push({qacct:"p-g6uZkrDA2nB2y"});window.googletag||(googletag=googletag||{},googletag.cmd=googletag.cmd||[],function(){var n=document.createElement("script"),i,t;n.async=!0;n.type="text/javascript";i="https:"==document.location.protocol;n.src=(i?"https:":"http:")+"//www.googletagservices.com/tag/js/gpt.js";t=document.getElementsByTagName("script")[0];t.parentNode.insertBefore(n,t)}())}