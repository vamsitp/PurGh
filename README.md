## PurGh
Purge stale GitHub Workflows-runs &amp; Artifacts

#### Installation
> **PRE-REQ**: [.NET Runtime 6.0.0](https://dotnet.microsoft.com/download/dotnet/6.0)   

**`dotnet tool install -g --no-cache --ignore-failed-sources purgh`**   
`dotnet tool update -g --no-cache --ignore-failed-sources purgh`    
`dotnet tool uninstall -g purgh`   

> If the Tool is not accessible post installation, add `%USERPROFILE%\.dotnet\tools` to the PATH env-var.   
> If you get an error stating *Failed to create shell shim for tool 'purgh': Command 'purgh' conflicts with an existing command from another tool*, run: `del %USERPROFILE%\.dotnet\tools\purgh.exe`

#### usage
> Download [GhExcel.json](GhExcel.json) to `%USERPROFILE%\Documents`   
> Update `PurGh.json`

Run `purgh` _<any alternate-path to [PurGh.json](PurGh.json)>_
