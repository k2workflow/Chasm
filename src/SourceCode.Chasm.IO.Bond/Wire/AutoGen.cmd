@ECHO OFF
SETLOCAL
CD /d %~dp0
SET TOOLS_PATH=%USERPROFILE%\.nuget\packages\Bond.Compiler.CSharp\6.0.0\tools

	%TOOLS_PATH%\gbc.exe cs .\Sha1Wire.bond
		IF %ERRORLEVEL% NEQ 0 GOTO ERROR

	%TOOLS_PATH%\gbc.exe cs .\TreeWire.bond
		IF %ERRORLEVEL% NEQ 0 GOTO ERROR

	%TOOLS_PATH%\gbc.exe cs --using="DateTime=System.DateTime" .\CommitWire.bond
		IF %ERRORLEVEL% NEQ 0 GOTO ERROR

	%TOOLS_PATH%\gbc.exe cs .\CommitRefWire.bond
		IF %ERRORLEVEL% NEQ 0 GOTO ERROR

ENDLOCAL
GOTO :END

:ERROR
PAUSE

:END
