@echo off
setlocal
cd /d "%~dp0"
where dotnet >nul 2>&1
if errorlevel 1 (
  echo Instale o SDK .NET 10 antes de iniciar a Aportiva.
  pause
  exit /b 1
)
where npm >nul 2>&1
if errorlevel 1 (
  echo Instale o Node.js 24 antes de iniciar a Aportiva.
  pause
  exit /b 1
)
dotnet dev-certs https --check --trust >nul 2>&1
if errorlevel 1 (
  echo Execute uma vez: dotnet dev-certs https --trust
  echo Depois abra novamente este arquivo.
  pause
  exit /b 1
)
if not exist "frontend\investimentos-web\node_modules" (
  pushd frontend\investimentos-web
  call npm ci
  if errorlevel 1 (
    popd
    pause
    exit /b 1
  )
  popd
)
echo Iniciando API HTTPS com banco local e integracoes habilitadas.
echo Configure suas chaves em User Secrets conforme docs\local\EXECUCAO_LOCAL.md.
echo Mantenha as duas janelas abertas para consultas e agendamento automatico.
start "Aportiva API local" cmd /k "dotnet run --project Investimentos.Api --launch-profile localhost-integrado"
start "Aportiva Web local" /D "%~dp0frontend\investimentos-web" cmd /k "npm run dev -- --host localhost --port 5173 --strictPort"
echo Acesse http://localhost:5173 quando os dois servicos estiverem prontos.
endlocal
