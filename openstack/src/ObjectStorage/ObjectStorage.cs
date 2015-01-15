using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Runtime.InteropServices;
using NStack.Auth;
using NStack.Exceptions;
using NStack.WebClient;


namespace NStack.ObjectStorage
{
    // API available at
    // http://docs.openstack.org/api/openstack-object-storage/1.0/content/storage_container_services.html
    public class ObjectStorage
    {
        private IClientConnection m_connection;
        private string m_endPointUri;

        private const string OBJ_STORAGE_TYPE = "object-store";


        public ObjectStorage (IClientConnection connection, KeystoneResponse.AccessInfo.ServiceInfo.EndpointType endpointType = KeystoneResponse.AccessInfo.ServiceInfo.EndpointType.Public)
        {
            var uris = connection.LastKeystoneResponse.Access.ServiceCatalog.Where (
                srv => srv.Type.Equals (OBJ_STORAGE_TYPE, StringComparison.InvariantCultureIgnoreCase)).ToArray ();
            // TODO: error messages in exceptions
            if (uris.Length == 0)
                throw new ServiceNotFoundException ();
            if (uris.Length > 1)
                throw new MoreThanOneServiceFoundException ();

            Initialize (connection, uris.Single (), endpointType);
        }


        public ObjectStorage (ClientConnection connection, KeystoneResponse.AccessInfo.ServiceInfo serviceInfo, KeystoneResponse.AccessInfo.ServiceInfo.EndpointType endpointType = KeystoneResponse.AccessInfo.ServiceInfo.EndpointType.Public)
        {
            Initialize (connection, serviceInfo, endpointType);
        }


        private void Initialize (IClientConnection connection, KeystoneResponse.AccessInfo.ServiceInfo serviceInfo, KeystoneResponse.AccessInfo.ServiceInfo.EndpointType endpointType)
        {
            if (serviceInfo == null)
                throw new ArgumentNullException ("serviceInfo");
            if (connection == null)
                throw new ArgumentNullException ("connection");

            m_connection = connection;
            //TODO: support many-endpoints services to handle redundancy
            var endPointInfo = serviceInfo.Endpoints.FirstOrDefault ();

            if (endPointInfo == null)
                throw new ServiceNotFoundException ();

            m_endPointUri = endPointInfo.GetURLForEndpoint (endpointType);
        }


        public Response<List<ContainerInfo>> ListContainers ()
        {
            var res = m_connection.JsonGetRequest<List<ContainerInfo>> (m_endPointUri + @"?format=json");
            return res;
        }


        public Response<List<ObjectInfo>> GetContainer(ContainerInfo info)
        {
            var res = m_connection.JsonGetRequest<List<ObjectInfo>>(string.Format("{0}/{1}?format=json", m_endPointUri, info.Name));
            return res;
        }


        //TODO: support for container's meta-data headers
        // see: http://docs.openstack.org/api/openstack-object-storage/1.0/content/storage_container_services.html
        public Response<ContainerInfo> CreateContainer (string containerName)
        {
            var res = m_connection.JsonPutRequest<object>(string.Format ("{0}/{1}", m_endPointUri, containerName));
            return new Response<ContainerInfo>
                   {
                       Status = res.Status,
                       StatusCode = res.StatusCode,
                       Result = new ContainerInfo { Name = containerName}
                   };
        }


        public Response<object> DeleteEmptyContainer(ContainerInfo info)
        {
            var res = m_connection.JsonDeleteRequest<object>(string.Format("{0}/{1}", m_endPointUri, info.Name));
            return res;
        }


        public Response<ObjectInfo> CreateOrReplaceObjectInContainer (ContainerInfo info, string objectName, byte[] content, string contentType)
        {
            string uri = string.Format ("{0}/{1}/{2}", m_endPointUri, info.Name, objectName);
            var res = m_connection.HttpRequest (uri, HttpMethod.PUT, content, contentType);
            return new Response<ObjectInfo>
            {
                Status = res.Status,
                StatusCode = res.StatusCode,
                Result = new ObjectInfo { Name = objectName }
            };
        }


        //TODO: method that returns a ready buffer of byte[]
        public HttpWebResponse GetObjectFromContainer (ContainerInfo containerInfo, ObjectInfo objectInfo)
        {
            string uri = string.Format("{0}/{1}/{2}", m_endPointUri, containerInfo.Name, objectInfo.Name);
            var res = m_connection.Request(uri, HttpMethod.GET);
            return res;
        }


        public Response<ObjectInfo> DeleteObjectFromContainer(ContainerInfo containerInfo, ObjectInfo objectInfo)
        {
            string uri = string.Format("{0}/{1}/{2}", m_endPointUri, containerInfo.Name, objectInfo.Name);
            var res = m_connection.HttpRequest(uri, HttpMethod.DELETE);
            return new Response<ObjectInfo>
            {
                Status = res.Status,
                StatusCode = res.StatusCode,
                Result = objectInfo
            };
        }
    }
}