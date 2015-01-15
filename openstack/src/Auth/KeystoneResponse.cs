using System;
using NStack.Exceptions;


namespace NStack.Auth
{
	public class KeystoneResponse
	{
		public AccessInfo Access { get; set;}

		public class AccessInfo
		{
			public TokenInfo Token { get; set; }

			public ServiceInfo[] ServiceCatalog { get; set; }

			public UserInfo User { get; set;}

			public MetadataInfo Metadata { get; set;}

			public class TokenInfo
			{
				public DateTimeOffset IssuedAt { get; set; }

				public DateTimeOffset Expires { get; set; }

				public string Id { get; set; }

				public TenantInfo Tenant { get; set; }

				public class TenantInfo
				{
					public string Description { get; set; }

					public bool Enaled { get; set; }

					public string Id { get; set; }

					public string Name { get; set; }
				}
			}

			public class ServiceInfo
			{
				public enum EndpointType 
				{
					Unknown = 0,
					Admin,
					Internal,
					Public
				}
				public EndpointInfo[] Endpoints { get; set; }

				public string Type { get; set; }

				public string Name { get; set; }

				public class EndpointInfo
				{
					public string AdminURL { get; set; }

					public string InternalURL { get; set; }

					public string PublicURL { get; set; }

					public string Id { get; set; }

					public string Region { get; set; }

					public string GetURLForEndpoint (EndpointType epType)
					{
						switch (epType)
						{
						case EndpointType.Admin: return AdminURL;
						case EndpointType.Internal: return InternalURL;
						case EndpointType.Public: return PublicURL;
						default: throw new UnknownEndPointTypeException ();
						}
					}
				}
			}

			public class UserInfo
			{
				public class RoleInfo
				{
					public string Name { get; set; }
				}

				public string Username { get; set; }

				public string Name { get; set; }

				public string Id { get; set; }

				public RoleInfo[] Roles { get; set; }

			}

			public class MetadataInfo
			{
				public bool IsAdmin { get; set;}
				public string[] Roles { get; set;}
			}
		}
	}
}

