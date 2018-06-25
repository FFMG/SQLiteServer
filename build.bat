@IF "%~1" == "" GOTO Error
@IF "%~1" == "/t:Push" GOTO Push
@IF "%~1" == "/t:Pack" GOTO Pack

:Pack
.\tools\nuget\Nuget.exe pack ".\src\SQLiteServer\SQLiteServer.nuspec" -OutputDirectory ".\src\bin\."

@GOTO End

:Push
@echo You will need to push it manually or use
@ech .\tools\nuget\Nuget.exe push .\src\bin\MyOddWeb.Data.SQLiteServer.0.1.2.1.nupkg <API KEY>

@GOTO End

:Error
@echo Missing parametters, '/t:pack' or '/t:push'

:End