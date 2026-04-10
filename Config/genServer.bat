set WORKSPACE=.

set GEN_CLIENT=%WORKSPACE%\LubanDll\Luban.dll
set CONF_ROOT=%WORKSPACE%\DataTables

dotnet %GEN_CLIENT% ^
    -t server ^
    -c cs-bin ^
    -d bin ^
    --conf %CONF_ROOT%\luban.conf ^
    -x outputCodeDir=..\Server\GameServer\Config\Scripts ^
    -x outputDataDir=..\Server\GameServer\Config\DataTable
pause