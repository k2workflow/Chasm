@ECHO OFF
SETLOCAL
CD /d %~dp0
SET PROTO_PATH=%USERPROFILE%\.nuget\packages\Google.Protobuf.Tools\3.4.0\tools\google\protobuf
SET TOOLS_PATH=%USERPROFILE%\.nuget\packages\Google.Protobuf.Tools\3.4.0\tools\windows_x86

	%TOOLS_PATH%\protoc.exe --proto_path .\ --proto_path %PROTO_PATH% --csharp_out .\ --csharp_opt=file_extension=.g.cs .\Wire.proto
		IF %ERRORLEVEL% NEQ 0 GOTO ERROR

ENDLOCAL
GOTO :END

:ERROR
PAUSE

:END
