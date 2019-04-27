#!/bin/bash

strace dotnet bin/Debug/netcoreapp2.2/strace-concurrency.dll async &> async.log
strace dotnet bin/Debug/netcoreapp2.2/strace-concurrency.dll hopac &> hopac.log
cat async.log | grep futex | wc -l
cat hopac.log | grep futex | wc -l
