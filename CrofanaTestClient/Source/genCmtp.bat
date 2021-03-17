@echo off

set v=%cd%

if exist Generated (
    rmdir Generated /s/q
)
mkdir Generated
cd Generated
mkdir Proto
mkdir Message
mkdir Controller
cd Controller
mkdir gen
cd ..\..\Cmtp\Compiler

start Crofana.CmtpCompiler.exe %v%
