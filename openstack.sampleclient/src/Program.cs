using System;
using System.IO;
using System.Linq;
using System.Text;
using NStack.Auth;


namespace NStack.SampleClient
{
    internal class MainClass
    {
        public static void Main (string[] args)
        {
            var containerName = "tescik";
            var fileName = "plik.txt";
            var fileContentType = "text/plain; charset=UTF-8";
            var fileContent = "This is a sample file";

            var auth = new AuthData ("tenant", "user", "password");
            var clientConnection = new ClientConnection ();
            var kr = clientConnection.Connect ("http://192.168.30.20:5000/v2.0/tokens", auth);
            Console.WriteLine ("{0} valid to {1}", kr.Access.Token.Id, kr.Access.Token.Expires.ToLocalTime ());

            var os = new ObjectStorage.ObjectStorage (clientConnection);

            var container = os.CreateContainer (containerName);
            Console.WriteLine ("1 " + container);

            var containers = os.ListContainers ();
            Console.WriteLine ("2 " + containers);
            containers.Result.ForEach (ci => Console.WriteLine ("\t{0}\t{1}\t{2}", ci.Name, ci.Count, ci.Bytes));
            
            var content = new UTF8Encoding ().GetBytes (fileContent);
            var object1 = os.CreateOrReplaceObjectInContainer(container.Result, fileName + "1", content, fileContentType);
            Console.WriteLine("3 " + object1);
            var object2 = os.CreateOrReplaceObjectInContainer (container.Result, fileName + "2", content, fileContentType);
            Console.WriteLine ("4 " + object2);

            var objResponse = os.GetObjectFromContainer (container.Result, object1.Result);
            var reader = new StreamReader (objResponse.GetResponseStream ());
            Console.WriteLine ("5 Content of file {0} from container {1}", object1.Result.Name, container.Result.Name);
            Console.Write (">>>>> ");
            Console.WriteLine (reader.ReadToEnd ());
            reader.Close ();
            objResponse.Close ();

            var containerResp = os.GetContainer (container.Result);
            Console.WriteLine ("6 " + containerResp);
            Console.WriteLine("Content of file container {0}", container.Result.Name);
            containerResp.Result.ForEach(oi => Console.WriteLine("\t{0}\t{1}\t{2}", oi.Name, oi.Bytes, oi.LastModified.ToLocalTime ()));

            Console.WriteLine("7 " + os.DeleteObjectFromContainer(container.Result, object1.Result));
            Console.WriteLine("8 " + os.DeleteObjectFromContainer(container.Result, object2.Result));
            Console.WriteLine("9 " + os.DeleteEmptyContainer(container.Result));

            Console.WriteLine ("Press ENTER");
            Console.ReadLine ();
        }
    }
}
