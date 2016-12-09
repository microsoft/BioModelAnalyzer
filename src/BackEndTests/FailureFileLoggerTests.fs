module FailureFileLoggerTests

open System 
open System.IO
open System.Threading.Tasks
open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
type WebApiControllersTests() = 
    
    [<TestMethod; TestCategory("CI")>]
    member x.``Multiple concurrent file-based loggers``() =
        let n = 50
        let m = 4

        let startTime = DateTime.Now
        let dir = DirectoryInfo(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "FailureFileLog"))
        
        dir.Create()
        dir.GetFiles("*", SearchOption.AllDirectories) |> Seq.iter(fun fi -> fi.Delete())
        dir.GetDirectories("*", SearchOption.AllDirectories) |> Seq.iter(fun fi -> fi.Delete())

        let log version =
            let logger = new BMAWebApi.FailureFileLogger("FailureFileLog", false)
            for i in 0..n-1 do
                let content = bma.client.LogContents([|"debug"|], [|"error"|]);
                logger.Add(DateTime.Now, version, version, content)
            logger.Close()

        let tasks =
            Array.init m (fun i -> Task.Factory.StartNew(Action(fun() -> log (sprintf "version %d" i))))

        Task.WaitAll(tasks)
        let endTime = DateTime.Now

        let files = dir.GetFiles("*.csv")
        Assert.AreEqual(1, files.Length, "Number of tables")
        
        let file = files.[0]
        let lines = File.ReadAllLines(file.FullName)
        Assert.AreEqual(n*m, lines.Length-1, "Number of entries") // without first header line

        lines
            |> Seq.skip(1) // header
            |> Seq.iter(fun line ->
                let items = line.Split([|','|])
                Assert.AreEqual(3, items.Length, "number of columns in an entry")
                let id = Guid.Parse(items.[0].Trim())
                let timestamp = DateTime.Parse(items.[1].Trim())
                let version = items.[2].Trim()
                Assert.IsTrue(timestamp > startTime && timestamp < endTime, "Timestamp is out of range")

                let reqFile = Path.Combine(Path.Combine(dir.FullName, "requests"), id.ToString() + "_request.json")
                let resFile = Path.Combine(Path.Combine(dir.FullName, "requests"), id.ToString() + "_result.json")
                Assert.IsTrue(File.Exists(reqFile), "Request not found")
                Assert.IsTrue(File.Exists(resFile), "Result not found")

                Assert.AreEqual("\"" + version + "\"", File.ReadAllText(reqFile), "Request is unexpected")
                Assert.AreEqual(@"Error messages:
error


Debug messages:
debug

", 
                    File.ReadAllText(resFile), "Result is unexpected")
                )


