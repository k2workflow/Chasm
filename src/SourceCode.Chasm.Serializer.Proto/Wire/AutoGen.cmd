@ECHO OFF
SETLOCAL
CD /d %~dp0
SET PROTO_PATH=%USERPROFILE%\.nuget\packages\Google.Protobuf.Tools\3.3.0\tools\google\protobuf
SET TOOLS_PATH=%USERPROFILE%\.nuget\packages\Google.Protobuf.Tools\3.3.0\tools\windows_x86

	%TOOLS_PATH%\protoc.exe --proto_path .\ --proto_path %PROTO_PATH% --csharp_out .\ --csharp_opt=file_extension=.g.cs .\Sha1Wire.proto
		IF %ERRORLEVEL% NEQ 0 GOTO ERROR

	%TOOLS_PATH%\protoc.exe --proto_path .\ --proto_path %PROTO_PATH% --csharp_out .\ --csharp_opt=file_extension=.g.cs .\TreeWire.proto
		IF %ERRORLEVEL% NEQ 0 GOTO ERROR

	%TOOLS_PATH%\protoc.exe --proto_path .\ --proto_path %PROTO_PATH% --csharp_out .\ --csharp_opt=file_extension=.g.cs .\CommitWire.proto
		IF %ERRORLEVEL% NEQ 0 GOTO ERROR

	%TOOLS_PATH%\protoc.exe --proto_path .\ --proto_path %PROTO_PATH% --csharp_out .\ --csharp_opt=file_extension=.g.cs .\CommitIdWire.proto
		IF %ERRORLEVEL% NEQ 0 GOTO ERROR

ENDLOCAL
GOTO :END

:ERROR
PAUSE

:END
