namespace StorageRestApiAuth
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;


    internal static class Program
    {
        // static string StorageAccountName = "YOURSTORAGEACCOUNTNAME";
        static string StorageAccountName = "zjonestemp";
        // static string StorageAccountKey = "YOURSTORAGEACCOUNTKEY";
        static string StorageAccountKey = "9Fk+ksOlghxA19XQ3MkTCtL9PVD7qYgQqq+y0i3gae/4gdVXz9O6lsF3SJh1/QqAs7LG52YVrO8gxz9Kc2dKcQ==";
        static string VersionCurrent = "2020-04-08";

        // Alternate info below
        static string AltAccountName = "testingstoragex";
        static string AltAccountKey = "jOUV4uRMpX4Qf46v+UlA5lUpbMk1zSyTR4QSzk+BHY3NyPN7Tz5onR6zuYGLRKbXgxCJ0sElkMYGh/GQihhfOw==";

        private static void Main()
        {
            // List the containers in a storage account.
            // ListContainersAsyncREST(StorageAccountName, StorageAccountKey, CancellationToken.None).GetAwaiter().GetResult();

            while (true)
            {
                InputManager();
            }

            // Deprecated, using Input manager for basic UI.
            /*
            Console.WriteLine("Press any key to continue.");
            Console.ReadLine();
            */
        }

      
        private static void InputManager()
        {
            Console.WriteLine("What would you like to do?");
            Console.WriteLine("1: List Containers");
            Console.WriteLine("2: Export to another account");
            Console.WriteLine("3: Close program");
            Console.WriteLine("4: Create Container (test)");
            Console.WriteLine("5: Fun with authorization headers");

            string input = Console.ReadLine();

            switch (input)
            {
                case "0": Console.WriteLine("While that is a number, it's not an option.");
                    break;
                case "1": Console.WriteLine("List all containers!");
                    ListContainersAsyncREST(StorageAccountName, StorageAccountKey, CancellationToken.None).GetAwaiter().GetResult();
                    break;
                case "2": Console.WriteLine("Transfer to another account");
                    TransferManager();
                    Console.WriteLine("This functionality not yet completed.");
                    break;
                case "3": Environment.Exit(0);
                    break;
                case "4": Console.WriteLine("Creating new container (test)");
                    CreateContainer(StorageAccountName, StorageAccountKey, CancellationToken.None).GetAwaiter().GetResult();
                    break;
                case "5": Console.WriteLine("YOU CHOSE FUN WITH AUTHHEADERS!");
                    //FunWithHeaders();
                    break;
                default: Console.WriteLine("It looks like you've chosen an invalid input that isn't 0.");
                    break;
            }
        }

        private static void FunWithHeaders()
        {

        }

        private static void TransferManager()
        {
           

            // Get list of all containers.
            //var containers = ListContainersAsyncREST(StorageAccountName, StorageAccountKey, CancellationToken.None).GetAwaiter().GetResult(); // oops
            List<string> containers = ListContainers(StorageAccountName, StorageAccountKey, CancellationToken.None).GetAwaiter().GetResult();
            //Console.WriteLine(containers);
            foreach(string name in containers)
            {
                //Console.WriteLine(name); // Quick sanity check, we've got all the container names now.
                // Create new container
                // await CreateContainer(AltAccountName, AltAccountKey, CancellationToken.None, name);
                // Get all blob names in container
                List<string> allBlobs = ListBlobs(StorageAccountName, StorageAccountKey, name, CancellationToken.None).GetAwaiter().GetResult();
                foreach(string blobName in allBlobs)
                {
                    // string sourceURL = string.Format("https://{0}.blob.core.windows.net/{1}/{2}/{3}", StorageAccountName, name, blobName, "?sv=2020-08-04&ss=bfqt&srt=sco&sp=rwdlacuptfx&se=2021-07-23T08:04:35Z&st=2021-07-23T00:04:35Z&spr=https,http&sig=YQl9fdNGW%2FtZJqVqmfKJdoCK25%2BEw69%2FD8ycjltQDFk%3D");
                    string sourceURL = string.Format("https://{0}.blob.core.windows.net/{1}/{2}{3}", StorageAccountName, name, blobName, "?sv=2020-08-04&ss=bfqt&srt=sco&sp=rwdlacuptfx&se=2021-07-23T08:04:35Z&st=2021-07-23T00:04:35Z&spr=https,http&sig=YQl9fdNGW%2FtZJqVqmfKJdoCK25%2BEw69%2FD8ycjltQDFk%3D");
                    CreateContainer(AltAccountName, AltAccountKey, CancellationToken.None, name);
                    CopyBlob(AltAccountName, AltAccountKey, name, blobName, sourceURL, CancellationToken.None);
                }

            }
            Console.WriteLine("That's all for now!");
            // Loop through all container names:
            // // create new container in alt
            // // copy container properties and metadata (maybe, will try without that first
            // // get list of blobs
            // // loop through all blobs:
            // // // PUT Blob, using GET blob. (blob properties and blob metadata available from GET)
        }

        private static void TransferBlob()
        {
            Console.WriteLine("Transferring blob now");
        }

        private static async void CopyBlob(string storageAccountName, string storageAccountKey, string containerName, string blobName, string targetLink, CancellationToken cancellationToken)
        {


            // Construct the URI. PUT, /containername/myblob
            String uri = string.Format("https://{0}.blob.core.windows.net/{1}/{2}", storageAccountName, containerName, blobName);

            // Set this to whatever payload you desire. Ours is null because 
            //   we're not passing anything in.
            Byte[] requestPayload = null;

            //Instantiate the request message with a null payload.
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, uri)
            { Content = (requestPayload == null) ? null : new ByteArrayContent(requestPayload) })
            {

                // Add the request headers for x-ms-date and x-ms-version.
                DateTime now = DateTime.UtcNow;
                httpRequestMessage.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                httpRequestMessage.Headers.Add("x-ms-version", VersionCurrent);
                // If you need any additional headers, add them here before creating
                //   the authorization header. 

                httpRequestMessage.Headers.Add("x-ms-copy-source", targetLink);

                // Add the authorization header.
                httpRequestMessage.Headers.Authorization = AzureStorageAuthenticationHelper.GetAuthorizationHeader(
                   storageAccountName, storageAccountKey, now, httpRequestMessage);

                // Send the request.
                using (HttpResponseMessage httpResponseMessage = await new HttpClient().SendAsync(httpRequestMessage, cancellationToken))
                {
                    // If successful (status code = 200), 
                    //   parse the XML response for the container names.
                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        String xmlString = await httpResponseMessage.Content.ReadAsStringAsync();
                        XElement x = XElement.Parse(xmlString);
                        Console.WriteLine(x);
                    }
                    else
                    {
                        Console.WriteLine("That didn't work.");
                        Console.WriteLine(httpRequestMessage);
                        Console.WriteLine(httpResponseMessage);
                    }
                }
            }
        }

        private static async Task<List<string>> ListBlobs(string storageAccountName, string storageAccountKey, string containerName, CancellationToken cancellationToken)
        {
            // Construct the URI. This will look like this:
            //   https://myaccount.blob.core.windows.net/resource
            String uri = string.Format("https://{0}.blob.core.windows.net/"+ containerName +"?restype=container&comp=list", storageAccountName);

            // Set this to whatever payload you desire. Ours is null because 
            //   we're not passing anything in.
            Byte[] requestPayload = null;

            // Instantiate the request message with a null payload.
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri)
            { Content = (requestPayload == null) ? null : new ByteArrayContent(requestPayload) })
            {

                // Add the request headers for x-ms-date and x-ms-version.
                DateTime now = DateTime.UtcNow;
                httpRequestMessage.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                httpRequestMessage.Headers.Add("x-ms-version", VersionCurrent);
                // If you need any additional headers, add them here before creating
                //   the authorization header. 

                // Add the authorization header.
                httpRequestMessage.Headers.Authorization = AzureStorageAuthenticationHelper.GetAuthorizationHeader(
                   storageAccountName, storageAccountKey, now, httpRequestMessage);

                // Send the request.
                using (HttpResponseMessage httpResponseMessage = await new HttpClient().SendAsync(httpRequestMessage, cancellationToken))
                {
                    // If successful (status code = 200), 
                    //   parse the XML response for the container names.
                    List<string> allBlobs = new List<string>();
                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        String xmlString = await httpResponseMessage.Content.ReadAsStringAsync();
                        XElement x = XElement.Parse(xmlString);
                        foreach (XElement container in x.Element("Blobs").Elements("Blob"))
                        {
                            //Console.WriteLine("Blob name = {0}", container.Element("Name").Value);
                            try
                            {

                                //string testString = container.Element("Name").Value.Remove(0, 15);
                                //Console.WriteLine(testString);

                                allBlobs.Add(container.Element("Name").Value);
                            }
                            catch
                            {
                                Console.WriteLine("Something went wrong, but I think we did alright.");
                            }
                        }
                        
                    }
                    else
                    {
                        Console.WriteLine("That didn't work, List Blobs");
                    }
                    return allBlobs;
                }
            }
        }
        /// <summary>
            /// This is the method to call the REST API to retrieve a list of
            /// containers in the specific storage account.
            /// This will call CreateRESTRequest to create the request, 
            /// then check the returned status code. If it's OK (200), it will 
            /// parse the response and show the list of containers found.
        /// </summary>
        private static async Task ListContainersAsyncREST(string storageAccountName, string storageAccountKey, CancellationToken cancellationToken)
        {

            // Construct the URI. This will look like this:
            //   https://myaccount.blob.core.windows.net/resource
            String uri = string.Format("https://{0}.blob.core.windows.net?comp=list", storageAccountName);

            // Set this to whatever payload you desire. Ours is null because 
            //   we're not passing anything in.
            Byte[] requestPayload = null;

            //Instantiate the request message with a null payload.
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri)
            { Content = (requestPayload == null) ? null : new ByteArrayContent(requestPayload) })
            {

                // Add the request headers for x-ms-date and x-ms-version.
                DateTime now = DateTime.UtcNow;
                httpRequestMessage.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                httpRequestMessage.Headers.Add("x-ms-version", VersionCurrent);
                // If you need any additional headers, add them here before creating
                //   the authorization header. 

                // Add the authorization header.
                httpRequestMessage.Headers.Authorization = AzureStorageAuthenticationHelper.GetAuthorizationHeader(
                   storageAccountName, storageAccountKey, now, httpRequestMessage);

                // Send the request.
                using (HttpResponseMessage httpResponseMessage = await new HttpClient().SendAsync(httpRequestMessage, cancellationToken))
                {
                    // If successful (status code = 200), 
                    //   parse the XML response for the container names.
                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        String xmlString = await httpResponseMessage.Content.ReadAsStringAsync();
                        XElement x = XElement.Parse(xmlString);
                        foreach (XElement container in x.Element("Containers").Elements("Container"))
                        {
                            Console.WriteLine("Container name = {0}", container.Element("Name").Value);
                        }
                    }
                }
            }
        }

        private static async Task CreateContainer(string storageAccountName, string storageAccountKey, CancellationToken cancellationToken, string name = "storage-samplex")
        {
            // Taking the guts from the already made functions and modifying from there
            // Construct the URI. This will look like this:
            //   https://myaccount.blob.core.windows.net/resource
            //String uri = string.Format("https://{0}.blob.core.windows.net?comp=list", storageAccountName);
            String uri = string.Format("https://{0}.blob.core.windows.net/" + name + "?restype=container", storageAccountName);
            // string uri_plus = uri + SAS_Token;
            string uri_plus = uri; //For testing purposes
            //Console.WriteLine(uri_plus);

            // Set this to whatever payload you desire. Ours is null because 
            //   we're not passing anything in.
            Byte[] requestPayload = null;

            //Instantiate the request message with a null payload.
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, uri_plus)
            { Content = (requestPayload == null) ? null : new ByteArrayContent(requestPayload) })
            {

                // Add the request headers for x-ms-date and x-ms-version.
                DateTime now = DateTime.UtcNow;
                httpRequestMessage.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                httpRequestMessage.Headers.Add("x-ms-version", VersionCurrent);
                // If you need any additional headers, add them here before creating
                //   the authorization header. 
                // OPTIONAL HEADERS ZONE?
                httpRequestMessage.Headers.Add("x-ms-meta-Name", name);

                // Add the authorization header.
                httpRequestMessage.Headers.Authorization = AzureStorageAuthenticationHelper.GetAuthorizationHeader(
                   storageAccountName, storageAccountKey, now, httpRequestMessage);

                // Send the request.
                using (HttpResponseMessage httpResponseMessage = await new HttpClient().SendAsync(httpRequestMessage, cancellationToken))
                {
                    // If successful (status code = 200), 
                    //   parse the XML response for the container names.
                    if (httpResponseMessage.StatusCode == HttpStatusCode.Created)
                    {
                        Console.WriteLine("Looks like that worked");
                        Console.WriteLine(httpResponseMessage);
                    }
                    else
                    {
                        //Console.WriteLine("Well, that didn't work");
                        //Console.WriteLine(httpResponseMessage);
                    }
                }
            }

        }

        private static async Task<List<string>> ListContainers(string storageAccountName, string storageAccountKey, CancellationToken cancellationToken)
        {

            // Construct the URI. This will look like this:
            //   https://myaccount.blob.core.windows.net/resource
            String uri = string.Format("https://{0}.blob.core.windows.net?comp=list", storageAccountName);

            // Set this to whatever payload you desire. Ours is null because 
            //   we're not passing anything in.
            Byte[] requestPayload = null;

            //Instantiate the request message with a null payload.
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri)
            { Content = (requestPayload == null) ? null : new ByteArrayContent(requestPayload) })
            {

                // Add the request headers for x-ms-date and x-ms-version.
                DateTime now = DateTime.UtcNow;
                httpRequestMessage.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                httpRequestMessage.Headers.Add("x-ms-version", VersionCurrent);
                // If you need any additional headers, add them here before creating
                //   the authorization header. 

                // Add the authorization header.
                httpRequestMessage.Headers.Authorization = AzureStorageAuthenticationHelper.GetAuthorizationHeader(
                   storageAccountName, storageAccountKey, now, httpRequestMessage);

                // Send the request.
                using (HttpResponseMessage httpResponseMessage = await new HttpClient().SendAsync(httpRequestMessage, cancellationToken))
                {
                    // If successful (status code = 200), 
                    //   parse the XML response for the container names.
                    // Also, create and populate a list
                    List<string> allContainers = new List<string>(); // As this is a non primitive data type, you'll need to instantiate it like 
                                                                     //   any other object. (Teaching notes)
                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        String xmlString = await httpResponseMessage.Content.ReadAsStringAsync();
                        XElement x = XElement.Parse(xmlString);
                        foreach (XElement container in x.Element("Containers").Elements("Container"))
                        {
                            // Console.WriteLine("Container name = {0}", container.Element("Name").Value);
                            try
                            {

                            //string testString = container.Element("Name").Value.Remove(0, 15);
                            //Console.WriteLine(testString);
                            
                            allContainers.Add(container.Element("Name").Value);
                            }
                            catch
                            {
                                Console.WriteLine("Something went wrong, but I think we did alright.");
                            }
                        }
                    }
                    return allContainers;
                    
                }
            }
        }
    }
}
