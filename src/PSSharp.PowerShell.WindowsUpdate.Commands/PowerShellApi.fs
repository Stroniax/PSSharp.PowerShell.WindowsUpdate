namespace PSSharp.PowerShell.WindowsUpdate.Commands

    module PowerShellApi =
        open System
        open System.Threading
        open System.Management.Automation
        open System.Collections.ObjectModel
    
        type PSObject<'T> (t: 'T) as pso =
            inherit PSObject(t)
            do pso.TypeNames.Insert(0, typeof<'T>.FullName)
            member _.BaseObject = t

        type EmptyStringArgumentCompleter () =
            interface IArgumentCompleter with
                member _.CompleteArgument(commandName, parameterName, wordToComplete, commandAst, fakeBoundParameters) =
                    [new CompletionResult("''")]

        type WriteData =
            | Output of PSObject
            | Debug of string
            | Verbose of string
            | Warning of string
            | Error of ErrorRecord

        type IPowerShellOutput =
            abstract WriteObject: PSObject -> unit
            abstract WriteDebug: string -> unit
            abstract WriteVerbose: string -> unit
            abstract WriteWarning: string -> unit
            abstract WriteError: ErrorRecord -> unit
            
        type FSAsyncJob (
            command: string,
            jobName: string,
            fn: IPowerShellOutput -> Async<unit>
            ) as job =
            inherit Job (command, jobName)
            let _cancellationTokenSource = new CancellationTokenSource()
            do job.PSJobTypeName <- "FSAsyncJob"
            do
                let comp = async {
                    job.OnJobStarted()
                    try
                        do! fn { new IPowerShellOutput with
                            member _.WriteObject o = job.Output.Add o
                            member _.WriteDebug o = new DebugRecord(o) |> job.Debug.Add
                            member _.WriteVerbose o = new VerboseRecord(o) |> job.Verbose.Add
                            member _.WriteWarning o = new WarningRecord(o) |> job.Warning.Add
                            member _.WriteError e = job.Error.Add e
                        }
                        job.OnJobSuccess()
                    with
                    | :? OperationCanceledException ->
                        job.OnJobStopped()
                    | e ->
                        let er = new ErrorRecord(e, null, ErrorCategory.NotSpecified, null)
                        job.Error.Add(er)
                        job.OnJobFailed()
                }
                Async.Start(comp, _cancellationTokenSource.Token)

            member private this.OnJobStarted() =
                this.SetJobState(JobState.Running)

            member private this.OnJobSuccess() =
                this.SetJobState(JobState.Completed)

            member private this.OnJobFailed() =
                this.SetJobState(JobState.Failed)

            member private this.OnJobStopped() =
                this.SetJobState(JobState.Stopped)

            override _.HasMoreData =
                job.Output.Count > 0
                || job.Error.Count > 0
                || job.Warning.Count > 0
                || job.Information.Count > 0
                || job.Verbose.Count > 0
                || job.Debug.Count > 0
                || job.Progress.Count > 0
            override _.StatusMessage = System.String.Empty
            override _.Location = System.String.Empty
            override this.StopJob() =
                this.SetJobState(JobState.Stopping)
                _cancellationTokenSource.Cancel()
            override _.Dispose disposing =
                if disposing then
                    _cancellationTokenSource.Dispose()
                base.Dispose disposing

        [<AbstractClass>]
        type FSCmdlet () =
            inherit PSCmdlet ()
            
            member this.BoundParameter name =
                match this.MyInvocation.BoundParameters.TryGetValue name with
                | true, value -> Some <| unbox value
                | _ -> None

            member this.BoundParameter (name, value) =
                match this.MyInvocation.BoundParameters.TryGetValue name with
                | true, value -> unbox value
                | _ -> value

        [<AbstractClass>]
        type FSAsyncCmdlet () =
            inherit FSCmdlet  ()
            let _cancellationTokenSource = new CancellationTokenSource()
            
            [<Parameter>]
            member val AsJob = new SwitchParameter(false) with get, set


            abstract ProcessRecordAsync: IPowerShellOutput -> Async<unit>

            member private this.ProcessAsJob () =
                let boundparams = this.MyInvocation.BoundParameters
                let jobName =
                    match boundparams.TryGetValue "JobName" with
                    | true, name -> name :?> string
                    | false, _ -> null

                let job = new FSAsyncJob(
                    this.MyInvocation.Line,
                    jobName,
                    this.ProcessRecordAsync
                    )
                this.JobRepository.Add(job)
                this.WriteObject(job)

            

            member private this.ProcessSynchronously() =

                use channel = new System.Collections.Concurrent.BlockingCollection<_>()

                let op = async {
                    try
                        try
                            do! this.ProcessRecordAsync ({ new IPowerShellOutput with
                                member _.WriteObject obj = obj |> Output |> channel.Add
                                member _.WriteDebug msg = msg |> Debug |> channel.Add
                                member _.WriteVerbose msg = msg |> Verbose |> channel.Add
                                member _.WriteWarning msg = msg |> Warning |> channel.Add
                                member _.WriteError e = e |> Error |> channel.Add
                            })
                        with e ->
                            new ErrorRecord(e, "FSAsyncException", ErrorCategory.NotSpecified, null)
                            |> Error
                            |> channel.Add
                    finally
                        channel.CompleteAdding ()
                    }
                Async.Start(op, _cancellationTokenSource.Token)

                for item in channel.GetConsumingEnumerable() do
                    match item with
                    | Output o -> this.WriteObject o
                    | Debug d -> this.WriteDebug d
                    | Verbose v -> this.WriteVerbose v
                    | Warning w -> this.WriteWarning w
                    | Error e -> this.WriteError e

            override this.ProcessRecord () =
                match this.AsJob.IsPresent with
                | true -> this.ProcessAsJob()
                | false -> this.ProcessSynchronously()

            override _.StopProcessing () =
                _cancellationTokenSource.Cancel ()

            abstract GetDynamicParameters: RuntimeDefinedParameterDictionary -> RuntimeDefinedParameterDictionary
            default this.GetDynamicParameters(parameters) =
                if this.AsJob.IsPresent then
                    let parameterAttribute = new ParameterAttribute()
                    parameterAttribute.ValueFromPipelineByPropertyName <- true
                    let completerAttribute = new ArgumentCompleterAttribute(typeof<EmptyStringArgumentCompleter>)
                    let jobName = new RuntimeDefinedParameter(
                        "JobName",
                        typeof<string>,
                        new Collection<Attribute>()
                    )
                    jobName.Attributes.Add(parameterAttribute)
                    jobName.Attributes.Add(completerAttribute)
                    parameters.Add(jobName.Name, jobName)
                parameters

            interface IDisposable with
                member _.Dispose () =
                    _cancellationTokenSource.Dispose()

            interface IDynamicParameters with
                member this.GetDynamicParameters () =
                    let dynamicparams = new RuntimeDefinedParameterDictionary ()
                    this.GetDynamicParameters(dynamicparams)
