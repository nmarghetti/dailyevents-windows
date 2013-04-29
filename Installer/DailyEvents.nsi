!define APP_NAME "Daily Events"
!define MSI_NAME "DailyEvents-windows.exe"
!define EXE_NAME "DailyEvents.exe"

Name "${APP_NAME}"

OutFile "${MSI_NAME}"
InstallDir $SMSTARTUP
 
Section
  SetOutPath $INSTDIR
  File "..\bin\Release\${EXE_NAME}"
  Exec "$SMSTARTUP\${EXE_NAME}"
SectionEnd
