using System;
using RestSharp;
using Gx.Atom;
using Gx.Links;
using System.Collections.Generic;

namespace Gx.Rs.Api
{
	public class GedcomxApiDescriptor
	{
		private readonly Dictionary<string, Link> links;
		private readonly Uri source;

		/// <summary>
		/// Initialize an API descriptor from the specified discovery URI.
		/// </summary>
		/// <param name='client'>
		/// The client to use to discover the API.
		/// </param>
		/// <param name='discoveryPath'>
		/// The path to the discovery resource on the host.
		/// </param>
		public GedcomxApiDescriptor (RestClient client, string discoveryPath)
		{
			var request = new RestRequest ();
			request.Resource = discoveryPath;
			request.AddHeader ("Accept", "application/atom+xml");

			this.source = client.BuildUri (request);
			var response = client.Execute<Feed> (request);
			if (response.ErrorException != null) {
				throw response.ErrorException;
			}
			Feed feed = response.Data;
			this.links = BuildLinkLookup ( feed != null ? feed.Links : null );
		}

		/// <summary>
		/// Inintialize a descriptor directly from a feed.
		/// </summary>
		/// <param name='feed'>
		/// The feed.
		/// </param>
		/// <param name='source'>
		/// The source of the feed.
		/// </param>
		public GedcomxApiDescriptor (Feed feed, Uri source) : this (feed != null ? feed.Links : null, source) {
		}

		/// <summary>
		/// Inintialize a descriptor directly from a feed.
		/// </summary>
		/// <param name='feed'>
		/// The feed.
		/// </param>
		public GedcomxApiDescriptor (Feed feed) : this (feed != null ? feed.Links : null, null)
		{
		}

		/// <summary>
		/// Initializes a descriptor from a list of links.
		/// </summary>
		/// <param name='links'>
		/// The links.
		/// </param>
		/// <param name='source'>
		/// The source of the links.
		/// </param>
		public GedcomxApiDescriptor (List<Link> links, Uri source)
		{
			this.source = source;
			this.links = BuildLinkLookup (links);
		}

		/// <summary>
		/// Initializes a descriptor from a list of links.
		/// </summary>
		/// <param name='links'>
		/// The links.
		/// </param>
		public GedcomxApiDescriptor (List<Link> links) : this( links, null )
		{
		}

		/// <summary>
		/// Builds the link lookup table.
		/// </summary>
		/// <returns>
		/// The link lookup.
		/// </returns>
		/// <param name='links'>
		/// The links to initialize.
		/// </param>
		Dictionary<string, Link> BuildLinkLookup (List<Link> links)
		{
			Dictionary<string, Link> lookup = new Dictionary<string, Link> ();
			if (links != null) {
				foreach (Link link in links) {
					if (link != null && link.Rel != null) {
						lookup.Add (link.Rel, link);
					}
				}
			}
			return lookup;
		}

		/// <summary>
		/// The links that describe the API.
		/// </summary>
		/// <value>
		/// The links.
		/// </value>
		public Dictionary<string, Link> Links {
			get {
				return this.links;
			}
		}

		/// <summary>
		/// The source of this descriptor.
		/// </summary>
		/// <value>
		/// The source.
		/// </value>
		public Uri Source {
			get {
				return this.source;
			}
		}

		/// <summary>
		/// Gets the OAuth2 authorization URI for this API.
		/// </summary>
		/// <value>
		/// The OAuth2 authorization URI.
		/// </value>
		public Uri OAuth2AuthorizationUri {
			get {
				Uri authorizationUri = null;
				Link authorizationLink;
				this.Links.TryGetValue(@"http://oauth.net/core/2.0/endpoint/authorize", out authorizationLink);
				if (authorizationLink != null && authorizationLink.Href != null) {
					TryUriResolution(authorizationLink.Href, out authorizationUri);
				}
				return authorizationUri;
			}
		}

		/// <summary>
		/// Tries to resolve a relative or absolute URI.
		/// </summary>
		/// <returns>
		/// <c>true</c>, if URI resolution was successful, <c>false</c> otherwise.
		/// </returns>
		/// <param name='relativeOrAbsoluteUri'>
		/// Relative or absolute URI.
		/// </param>
		/// <param name='result'>
		/// Result.
		/// </param>
		bool TryUriResolution(string relativeOrAbsoluteUri, out Uri result)
		{
			if (!Uri.TryCreate(relativeOrAbsoluteUri, UriKind.Absolute, out result)) {
				if (Uri.TryCreate(this.source, relativeOrAbsoluteUri, out result)) {
					return true;
				}
			}
			else {
				return true;
			}

			return false;
		}
	}
}
