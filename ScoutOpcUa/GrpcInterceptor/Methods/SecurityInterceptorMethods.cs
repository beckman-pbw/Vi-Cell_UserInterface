using Grpc.Core;
using ScoutUtilities.Helper;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using GrpcServer.GrpcInterceptor.Attributes;
using ScoutUtilities.Enums;

namespace GrpcServer.GrpcInterceptor
{
    /// <summary>
    /// This partial class has several other files to complete and segregate code.
    /// Despite being able to add multiple interceptors to the gRPC server, only
    /// having a single interceptor will work in conjunction with our Attributes.
    /// When you add more than 1 interceptor, the "continuation.Method" 
    /// parameter in the override methods turns into another method (named "<AddMethod>b__0")
    /// which makes it impossible to check for the custom attributes we use
    /// (PermissionAttribute, RequiresAutomationLockAttribute, etc).
    /// Because of this, we will only use a single interceptor as a partial class to segregate the 
    /// different checks (see ScoutInterceptor, LockInterceptor.cs, SecurityInterceptor.cs).
    /// </summary>
    public partial class ScoutInterceptor // Handles the User Role requirements for the OPC Methods
    {
        /// <summary>
        /// The prefix for basic auth as used in the authorization header. This library
        /// assumes that the both the username and password are UTF-8 encoded before
        /// being turned into a base64 string.
        /// </summary>
        public const string BASIC_AUTH = "Basic";
        public const string HTTP_AUTHORIZATION = "authorization";

        /// <summary>
        /// If there is no permission attribute on the method being invoked, then this method returns without throwing an exception.
        /// If the client provided credentials are not valid, an RpcException is thrown. If the role of the user is not within
        /// the list of the permission attributes, an exception is thrown. If the user is valid for the gRPC method, then this
        /// method returns with no exception.
        /// </summary>
        /// <param name="metadata">Dictionary of HTTP header attributes. This will contain the "authorization" entry.</param>
        /// <param name="method">The remote method being invoked by the client. This may have a PermissionAttribute on it.</param>
        /// <exception cref="RpcException">Thrown if the client request for this method does not pass the checks.</exception>>
        /// <returns></returns>
        private bool CheckHasPermission(Metadata metadata, MethodInfo method)
        {
            var methodPermissionAttr = ReflectionHelpers.GetCustomAttribute<PermissionAttribute>(method);
            var hasCredentials =
                ExtractBasicAuthCredentials(metadata, out var cnxId, out var username, out var password);
            if (methodPermissionAttr == null)
            {
                if (!hasCredentials)
                    return true; // allows user to log in to the system (ExtractBasicAuthCredentials will fail)

                // No attribute means any user role can access this method
                methodPermissionAttr = new PermissionAttribute(new[]
                {
                    UserPermissionLevel.eNormal,
                    UserPermissionLevel.eElevated, 
                    UserPermissionLevel.eAdministrator,
                    UserPermissionLevel.eService
                });
            }

            //if (hasCredentials)
            //{
            //    pushedCredentials = _securityService.PushUser(username, password);
            //}

            var userPermissionLevel =  _securityService.GetUserRole(username);
            if (!(methodPermissionAttr.Permissions.Contains(userPermissionLevel)))
            {
                // Either the credentials did not validate or the client failed to validate using provided credentials
                //var status = new Status(StatusCode.PermissionDenied, "User not valid for requested operation");
                //throw new RpcException(status, "User not valid for requested operation");
                return false;
            }
            return true;
        }

        internal static bool ExtractBasicAuthCredentials(Metadata metadata, out string cnxId, out string username, out string password)
        {
            username = string.Empty;
            password = string.Empty;
            cnxId = string.Empty;

            var authHeader = metadata.Get(HTTP_AUTHORIZATION);
            if (null == authHeader)
            {
                return false;
            }

            var authorization = authHeader.Value;
            if (string.IsNullOrWhiteSpace(authorization))
            {
                return false;
            }

            var authValue = AuthenticationHeaderValue.Parse(authorization);
            if (!authValue.Scheme.Equals(BASIC_AUTH))
            {
                return false;
            }

            var credentials = Encoding.Unicode.GetString(Convert.FromBase64String(authValue.Parameter));
            var words = credentials.Split((Char)(253));
            if (3 != words.Length)
            {
                // Invalid credential - requires a username and password separated with a colon.
                return false;
            }

            cnxId = words[0];
            username = words[1];

            if (HawkeyeCoreAPI.Facade.SystemStatusFacade.Instance.GetSecurity() == SecurityType.ActiveDirectory)
                username = username.ToLower();

            password = words[2];
            return true;
        }
    }
}