# ViCell UI testing: common procedures

## Running a sample

Refer to the ViCell-BLU IFU or ask for help.

### Inducing bubbles into sample

Run a sample with an *empty* reagent pack.

## Legacy data

### Enabling legacy data

1. Exit the ViCell UI.
2. Launch the DataEncryptDecrypt app:
   ![data-encrypt-app](DataEncryptApp.jpg?raw=true)
3. Under "File to Decrypt", open the file `HawkeyeStatic.einfo`:
   ![hawkeye-static-encrypted](HawkeyeStaticEncrypted.jpg?raw=true)
4. Click "Decrypt File."
5. Click the "EditAndEncrypt" checkbox, then scroll to the "DLL" section of the
   decrypted file contents and find the key
   `generate_legacy_cellcounting_data`. Change this value to `1`.
   ![hawkeye-static-edit](HawkeyeStaticEdit.jpg?raw=true)
6. Click the "Encrypt Edit Data" button, then close the app.
7. Launch the ViCell-BLU app. Any *new* samples taken or *reanalyses* done will
   generate legacy data.

### Viewing legacy data

1. Navigate to the `LegacyCellCountingData` directory:
   ![legacy-folder](LegacyFolder.jpg?raw=true)
2. Find and open the CSV file (and/or raw image files) corresponding to the
   analysis or reanalysis you want to validate:
   
   1. The `LegacyCellCountingData` directory's subfolders are named with the
      analysis or reanalysis timestamp. The format for initial analyses is
      `YYYYMMDD_hhmmss_<sample_id>`, where `<sample_id>` is the sample ID
      as reported by the UI. The format for reanalyses is `YYYYMMDD_hhmmss_`.
   2. The timestamps for initial analysis will be slightly *after* the
      timestamp shown in the "Results" screen. It will *match* the timestamp
      shown in the "Review" screen.
   3. The CSV file will be in the subfolder `ScoutCumulativeResult`.
   4. Each image is stored under its own numbered subfolder,e.g.:

          LegacyCellCountingData/<SampleName>/42/CamBF/CamFB_42.png

3. Find the area of the spreadsheet containing the selection of data of interest:
   ![legacy-data-sections](LegacyDataSections.jpg?raw=true)
4. Use the following table to find the legacy data field corresponding to each
   data label used by the ViCell UI software:

| ViCell label                 | Legacy data field             |
| ---------------------------- | ------------------------      |
| Sample ID                    | (part of filename; see above) |
| Images for analysis          | ImageControlCount (maybe?)    |
| Images                       | TotalImages                   |
| Cell count                   | TotalCells\_GP                |
| Viable cell count            | TotalCells\_POI               |
| Total (x10^6) cells/mL       | CellConcentration\_GP         |
| Viable (x10^6) cells/mL      | CellConcentration\_POI        |
| Viability%                   | PopulationPercentage          |
| Average diameter (um)        | AvgDiameter\_GP(microns)      |
| Average circularity          | AvgCircularity\_GP            |
| Average viable circularity   | AvgCircularity\_POI           |
| Average background intensity | Avgbackground Intensity       |
| Bubble count                 | TotalBubble Count             |
| Cluster count                | nLargeClusterCount            |

The "ImageControlCount" in the legacy data appears to be 100 *regardless* of
the cell type, which may be a bug in the legacy data generation.

5. **Round** the values in the legacy data CSV **by hand** to the precision
   with which they are displayed on screen.

