### Http-O-Tron

The `Http-O-Tron` is a web server written in C# to learn more about `Http`. Turns out `Http` is complicated. Please don't use this, unless you want to learn too.

### Why?

I already said why! To learn!

### What is Cool About This?

In C#, I've used both [Nancy](http://nancyfx.org/) and [WebAPI](https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api) for HTTP servers, but I wanted a greater understanding of the behind the scenes. Thus, this small web server was born. For Pete's sake, please use one of those frameworks in production, not this.

Syntax is similar to `Nancy`:

```
new WebServer(
	new[] {"http://localhost:9999/"},
	new DefaultRequestHandler(new HelloBinder())).Run();
```

And adding request is done through `RequestBinder` instances:

```
public class HelloBinder : RequestBinder
{
	public HelloBinder()
	{
		Get["/hello"] = (request, response) =>
		{
			response.StatusCode = 200;
			response.Body = "{\"success\":true, \"method\":\"GET\"}";
			response.Finish();
		};
	}
}
```

### Special Stuff

* To understand how [Access Control](https://developer.mozilla.org/en-US/docs/Web/HTTP/Access_control_CORS) worked, I implemented it.
* Multipart forms were also something I didn't understand, so this server also supports multipart requests (like file uploads).

```
POST["/upload"] = (request, response) => {
	byte[] bytes;
	if (request.Parts.Get("file", out bytes))
	{
		//
	}
}
```
