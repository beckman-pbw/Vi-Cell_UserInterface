mkdir \Instrument
mkdir \Instrument\Config
mkdir \Instrument\Data
mkdir \Instrument\Export
mkdir \Instrument\Images
mkdir \Instrument\Legal
mkdir \Instrument\Logs
mkdir \Instrument\Software
mkdir \Instrument\Software\Reports
mkdir \Instrument\Software\Target

xcopy target\Release\UIConfiguration\Debug\*.* \Instrument\Software\UIConfiguration\ /e /y
move \Instrument\Software\UIConfiguration\App.config \Instrument\Software\ViCellBLU_UI.exe.config /e
copy /y ScoutUtilities\UIConfiguration\deployment.config.template \Instrument\Software\UIConfiguration\deployment.config
rem xcopy target\dependencies\Hawkeye\*.* \Instrument\Software\ /e
xcopy target\dependencies\Hawkeye\import_data.bat \Instrument\Config\ /e /Y
xcopy target\dependencies\Hawkeye\*.bin \Instrument\Config\ /e /Y
xcopy target\dependencies\Hawkeye\*.dll \Instrument\Software\ /e /Y
xcopy target\dependencies\Hawkeye\*.einfo \Instrument\Config /e /Y
xcopy target\dependencies\Hawkeye\*.epng \Instrument\Config /e /Y
xcopy target\dependencies\Hawkeye\DataEncryptDecrypt.exe \Instrument\Software\ /e /Y
xcopy target\dependencies\Hawkeye\DataImporter.exe \Instrument\Software\ /e /Y
xcopy target\dependencies\Hawkeye\ScoutXTest.exe \Instrument\Software\ /e /Y
xcopy target\dependencies\Hawkeye\ViCellBLU_UI.exe \Instrument\Software\ /e /Y
xcopy target\dependencies\Hawkeye\Resources \Instrument\Software\ /Y

copy /y packages\nunit\3.13.0\lib\net45\nunit.framework.dll packages\nunitlite\3.13.0\tools\net45
copy /y packages\nunitlite\3.13.0\lib\net45\nunitlite.dll packages\nunitlite\3.13.0\tools\net45

copy "ScoutUI\Views\Reports\BioProcess\BioProcessReportLandscapeViewer.rdlc"    \Instrument\Software\Reports
copy "ScoutUI\Views\Reports\CellTypes\CellTypesReportLandscapeRdlcViewer.rdlc"    \Instrument\Software\Reports
copy "ScoutUI\Views\Reports\InstrumentStatus\InstrumentStatusReportRdlcViewer.rdlc"    \Instrument\Software\Reports
copy "ScoutUI\Views\Reports\QualityControls\QualityControlsReportRdlcViewer.rdlc"    \Instrument\Software\Reports
copy "ScoutUI\Views\Reports\RunReport\RunResultsReportRdlcViewer.rdlc"    \Instrument\Software\Reports
copy "ScoutUI\Views\Reports\RunSummary\RunSummaryReportLandscapeRdlcViewer.rdlc"    \Instrument\Software\Reports
copy "ScoutUI\Views\Reports\RunSummary\RunSummaryReportRdlcViewer.rdlc"    \Instrument\Software\Reports


