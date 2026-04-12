set WORKSPACE=.

set GEN_CLIENT=%WORKSPACE%\LubanDll\Luban.dll
set CONF_ROOT=%WORKSPACE%\DataTables

dotnet %GEN_CLIENT% ^
    -t client ^
    -c cs-bin ^
    -d bin ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputCodeDir=..\Assets\HotAssets\Scripts\DataTable ^
    -x outputDataDir=..\Assets\HotAssets\DataTable
pause