using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KSIS_5
{
    class Program
    {
        public static string ServPath = @"F:/KSIS_5/API";
        public static int BufferSize = 20000;
        static void Main(string[] args)
        {
            HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://127.0.0.1:5000/");
            httpListener.Start();
            while (true)
            {
                IHttpComand comand;
                HttpListenerContext httplistenercontext = httpListener.GetContext();
                switch(httplistenercontext.Request.HttpMethod)
                {
                    case ("GET"):
                        {
                            comand = new GetComand();
                            break;
                        }
                    case ("PUT"):
                        {
                            comand = new PutComand();
                            break;
                        }
                    case ("HEAD"):
                        {
                            comand = new HeadComand();
                            break;
                        }
                    case ("DELETE"):
                        {
                            comand = new DeleteComand();
                            break;
                        }
                    default:
                        {
                            httplistenercontext.Response.StatusCode = 500;
                            httplistenercontext.Response.OutputStream.Close();
                            continue;
                        }
                }
                HttpListenerResponse response = httplistenercontext.Response;
                comand.Comand(httplistenercontext.Request, ref response);


            }
        }


        class PutComand : IHttpComand
        {
            public void Comand(HttpListenerRequest request, ref HttpListenerResponse response)
            {

                var ListHead = request.Headers;
                try
                {
                    string dir;
                    foreach (var item in ListHead.AllKeys)
                {
                    if (item.ToString() == "Copy")
                    {
                        string[] value = ListHead.GetValues(item);
                        if ((value[0]) != "")
                        {
                            Console.WriteLine(value[0]);
                            if (File.Exists(ServPath + request.RawUrl))
                            {
                                dir = Path.GetDirectoryName(ServPath + value[0]);
                                if (!Directory.Exists(dir))
                                {
                                    var dirInfo = new DirectoryInfo(dir);
                                    dirInfo.Create();

                                }
                                File.Copy(ServPath + request.RawUrl, ServPath + value[0]);
                                File.Delete(ServPath + request.RawUrl);
                                return;
                            }
                            else
                            {
                                response.StatusCode = 404;
                                return;
                            }
                        }
                    }

                }

                var FilePath = request.RawUrl;
                if (FilePath == null)
                {
                    FilePath = "";
                }
                string FullPath = ServPath+FilePath;

              
                    dir = Path.GetDirectoryName(FullPath);

                    if (!Directory.Exists(dir))
                    {
                        var dirInfo = new DirectoryInfo(dir);
                        dirInfo.Create();
                        
                    }

                    using (var newFile = new FileStream(FullPath, FileMode.Create))
                    {
                        request.InputStream.CopyTo(newFile, BufferSize);
                    }
                    Console.WriteLine(Directory.GetCurrentDirectory());
                    Console.WriteLine(FilePath);
                    Console.WriteLine(FullPath);

                }
                catch (FileNotFoundException)
                {
                    response.StatusCode = 404;
                }
                catch (DirectoryNotFoundException)
                {
                    response.StatusCode = 404;
                }
                catch
                {
                    response.StatusCode = 400;
                }
                finally { response.OutputStream.Close(); }
            }

        }

        public class HeadComand : IHttpComand
        {

            public void Comand(HttpListenerRequest request, ref HttpListenerResponse response)
            {

                try
                {
                    var FullPath = ServPath+ request.RawUrl;
                    if (System.IO.File.Exists(FullPath))
                    {
                        response.StatusCode = 404;
                        return;
                    }
                    var fileInfo = new FileInfo(FullPath);
                    response.Headers.Add("Name", fileInfo.Name);
                    response.Headers.Add("Size", fileInfo.Length.ToString());
                    response.Headers.Add("LastWriteTime", fileInfo.LastWriteTime.ToString("dd/MM/yyyy hh:mm"));
                    response.StatusCode=(200);

                }
                catch
                {
                    response.StatusCode = 400;
                }

            }
        }

        public class DeleteComand : IHttpComand
        {
            public void Comand(HttpListenerRequest request, ref HttpListenerResponse response)
            {
                var FilePath = request.RawUrl; 
                try
                {
                    if (Directory.Exists(ServPath+FilePath))
                    {
                        Directory.Delete(ServPath + FilePath);
                        response.StatusCode=200;
                    }
                    else if (File.Exists(ServPath + FilePath))
                    {
                        System.IO.File.Delete(ServPath + FilePath);
                        response.StatusCode = 200;
                    }
                    else
                        response.StatusCode = 400;

                }
                catch (DirectoryNotFoundException)
                {
                    response.StatusCode = 404;
                }
                catch (FileNotFoundException)
                {
                    response.StatusCode = 404;
                }
                catch
                {
                    response.StatusCode = 400;
                }
            }
        }

        public class GetComand : IHttpComand
        {
            public void Comand(HttpListenerRequest request, ref HttpListenerResponse response)
            {

                var FilePath = request.RawUrl;
                Stream OutPutStream = response.OutputStream;
                try
                {
                    if (Directory.Exists(ServPath+FilePath))
                    {
                        
                        var Content = new List<GetItem>();
                        try
                        {
                            var Files = Directory.GetFiles(ServPath + FilePath);
                            foreach (var i in Files)
                            {
                                Content.Add(new GetItem("File", Path.GetFileName(i)));
                            }
                            var Dir = Directory.GetDirectories(ServPath + FilePath);
                            foreach (var i in Dir)
                            {
                                Content.Add(new GetItem("Directory", Path.GetFileName(i)));
                            }
                            var StreamWriter = new StreamWriter(OutPutStream);
                            StreamWriter.Write(JsonConvert.SerializeObject(Content));
                            StreamWriter.Flush();
                            response.StatusCode = 200;
                        }
                        catch
                        {
                            response.StatusCode = 400;
                        }
                    }
                    else
                    {
                        var FullPath = ServPath+FilePath;
                        try
                        {
                            if (File.Exists(FullPath))
                            {
                                using (var File = new FileStream(FullPath, FileMode.Open))
                                {
                                    File.CopyTo(OutPutStream, BufferSize);
                                }
                            }
                            else
                            {
                                response.StatusCode = 404;
                            }

                        }
                        catch
                        {
                            response.StatusCode = 400;
                        }

                    }
                }
                finally
                {
                    OutPutStream.Close();
                }
            }
        }
    }
}
