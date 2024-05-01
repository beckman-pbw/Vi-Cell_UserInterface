Developer Setup
-------------------

In order to run/debug the UserInterface software on a worksation with Visual Studio, a few steps must be done to prepare the system for testing.  The steps below will prepare a workstation to debug the UserInterface application:

1.  Install Git for windows: https://git-scm.com/downloads

2.  Install VisualStudio 2019
    * Make sure and also select the "Visual C++ -> Microsoft Foundation Classes for C++" feature under the Features list as well.
    * If you want to user a newer version of VisualStudio, you can but you will need to edit the pom.xml file to user your version of MSBUILD.exe.
    * You can also install useful plugins, such as the [Markdown Editor plugin from Mads Kristensen](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.MarkdownEditor)
    * If you are working with reports, install [Microsoft RDLC Report Designer](https://marketplace.visualstudio.com/items?itemName=ProBITools.MicrosoftRdlcReportDesignerforVisualStudio-18001) extension.

3.  Verify that .Net 3.5 and .Net 4.8 Developer Pack is installed

4.  Install JRE
    * Download JRE: http://www.oracle.com/technetwork/java/javase/downloads/jre8-downloads-2133155.html
    * Open Source JRE (OpenJDK 8) here: https://adoptopenjdk.net/
 
5.  Set JAVA_HOME (if the JRE package didn't set path during installation)
    * Hit ctrl-esc
    * Type: "environment variables"
    * Select tool to edit system environment variables
    * Select "environment variables"
    * Add `JAVA_HOME` as a system environment variable with the value: `C:\Program Files\Java\VERSION_INSTALLED_ABOVE\`

6. Install Maven 
    * Download: https://archive.apache.org/dist/maven/maven-3/3.3.9/binaries/apache-maven-3.3.9-bin.zip
    * Unzip to `C:\Dev` and add the directory `C:/Dev/apache-maven-3.3.9/bin` folder to your `PATH` environment variable

7. Fork the UserInterface repository using the GitHub web app: http://svusftcgit/Hawkeye/UserInterface

8. Clone your new fork:
    * Open a command prompt:
        * Navigate to the location you want your source code to be located.
        * Run: git clone https://github.com/YOUR_USERNAME/HawkeyeUserInterface.git

9. Build the software:
        * Create: C:\Instrument\Export    // This is required by one of the unit tests.
        * Run: mvn package -Drevision=1.4.<CURRENT_SPRINT_#>.<YOUR_INITIALS>-SNAPSHOT
 
10. Disable hardware interaction:
    * Edit environment.config:
        * Edit [CodeCheckout]\UserInterface\ScoutUtilities\UIConfiguration\environment.config
    * Ensure that the line containing `IsFromHardware` has it set to False:
         `isFromHardware="False"`

11. Enable simulation and dialog mode for easier debugging:
    * Edit ui.config:
        * Edit [CodeCheckout]\UserInterface\ScoutUtilities\UIConfiguration\ui.config
    * Ensure that the line containing `useWindowedMode` has it set to True:
         `useWindowedMode="False"`
    * Ensure that the line containing `overrideEssVcrControls` has it set to True:
         `overrideEssVcrControls="False"`

12. Run Maven integration test WARNING: This will destroy the \Instrument completely!!  Back it up first if you need it.
    * Run: mvn integration-test -Drevision=1.4.<CURRENT_SPRINT_#>.<YOUR_INITIALS>-SNAPSHOT
    * [This should now download a whopping boatload of stuff and then begin a compilation of the source code]
    * This will also populate a `\Instrument` directory with everything it needs to run

14. Starting w/ v1.4 to run the application the in Debug other reopsitories (HawkeyeApplicationSource,  Hawkeye_gRpc, and HawkeyeOpcUa) must be cloned and built using Maven and VS 2019 in Debug.

15. Debug (build & run)
    * Optionally set the ScoutX_Username and ScoutX_Password environment variables for your user account to autosign into the application
    * Open Visual Studio
    * Load the solution from `<clone-path>\UserInterface\ScoutUI.sln`
    * Select the 'Debug'/'AnyCPU' options
    * Set `ScoutUI` as the Startup Project (right-click it in the solution explorer)
    * Build/Run the solution
