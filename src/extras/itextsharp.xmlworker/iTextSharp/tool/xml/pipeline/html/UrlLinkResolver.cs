using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace iTextSharp.tool.xml.pipeline.html
{
	class UrlLinkResolver {
	    private string rootPath;

	    public UrlLinkResolver(string localRootPath) {
	        rootPath = localRootPath;
	    }

	    public UrlLinkResolver() {
	    }

	    virtual public Uri ResolveUrl(string src) {
	        Uri url;
	        try {
	            url = new Uri(src);
	        }
	        catch (UriFormatException e) {
	            url = ResolveLocalUrl(src);
	        }

	        return url;
	    }

	    virtual public Uri ResolveLocalUrl(string src) {
	        string path;
	        if (rootPath != null) {
	            bool rootSlashed = rootPath.EndsWith("/");
	            bool srcSlashed = src.StartsWith("/");
	            if (rootSlashed && srcSlashed) {
	                rootPath = rootPath.Substring(0, rootPath.Length - 1);
	            }
	            else if (!rootSlashed && !srcSlashed) {
	                rootPath += "\\";
	            }
	            path = rootPath + src;
	        }
	        else {
	            path = src;
	        }
            
	        if (File.Exists(path)) {
	            return new Uri(new FileInfo(path).FullName);
	        }

	        return null;
	    }

	    virtual public void SetLocalRootPath(string rootPath) {
	        this.rootPath = rootPath;
	    }
	}
}
